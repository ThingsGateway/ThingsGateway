#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using Mapster;

using MQTTnet;
using MQTTnet.Client;

using Newtonsoft.Json.Linq;

using System.Text;

using ThingsGateway.Cache;
using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Extension.Json;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttClient
/// </summary>
public partial class MqttClient : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private EasyLock ConnectLock = new();

    protected override void VariableChange(VariableRunTime variableRunTime)
    {
        AddQueueT(new(variableRunTime.Adapt<VariableData>()));
        base.VariableChange(variableRunTime);
    }

    protected override void DeviceChange(DeviceRunTime deviceRunTime)
    {
        AddQueueT2(new(deviceRunTime.Adapt<DeviceData>()));
        base.DeviceChange(deviceRunTime);
    }

    protected override void AlarmChange(AlarmVariable alarmVariable)
    {
        AddQueueT3(new(alarmVariable.Adapt<AlarmVariable>()));
        base.AlarmChange(alarmVariable);
    }

    protected override Task<OperResult> UpdateT3(IEnumerable<LiteDBDefalutCacheItem<AlarmVariable>> item, CancellationToken cancellationToken)
    {
        return UpdateT3(item.Select(a => a.Data), cancellationToken);
    }

    protected override Task<OperResult> UpdateT2(IEnumerable<LiteDBDefalutCacheItem<DeviceData>> item, CancellationToken cancellationToken)
    {
        return UpdateT2(item.Select(a => a.Data), cancellationToken);
    }

    protected override Task<OperResult> UpdateT(IEnumerable<LiteDBDefalutCacheItem<VariableData>> item, CancellationToken cancellationToken)
    {
        return UpdateT(item.Select(a => a.Data), cancellationToken);
    }

    #region mqtt方法

    #region private

    private async Task<OperResult> UpdateT3(IEnumerable<AlarmVariable> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetAlarms(item);
        return await Update(topicJsonList, cancellationToken);
    }

    private async Task<OperResult> Update(List<TopicJson> topicJsonList, CancellationToken cancellationToken)
    {
        foreach (var topicJson in topicJsonList)
        {
            var result = await MqttUpAsync(topicJson.Topic, topicJson.Json, cancellationToken);
            if (success != result.IsSuccess)
            {
                if (!result.IsSuccess)
                {
                    LogMessage.LogWarning(result.ToString());
                }
                success = result.IsSuccess;
            }
            if (!result.IsSuccess)
            {
                return result;
            }
        }
        return new();
    }

    private async Task<OperResult> UpdateT2(IEnumerable<DeviceData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetDeviceData(item);
        return await Update(topicJsonList, cancellationToken);
    }

    private async Task<OperResult> UpdateT(IEnumerable<VariableData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetVariable(item);
        return await Update(topicJsonList, cancellationToken);
    }

    #endregion private

    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = CurrentDevice.VariableRunTimes.Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        var alramData = WorkerUtil.GetWoker<AlarmWorker>().RealAlarmVariables.Adapt<List<AlarmVariable>>().ChunkBetter(_driverPropertys.SplitSize);
        foreach (var item in varData)
        {
            if (!success)
                break;
            await UpdateT(item, cancellationToken);
        }

        foreach (var item in devData)
        {
            if (!success)
                break;
            await UpdateT2(item, cancellationToken);
        }

        foreach (var item in alramData)
        {
            if (!success)
                break;
            await UpdateT3(item, cancellationToken);
        }
    }

    private async Task MqttClient_ApplicationMessageReceivedAsync(MQTTnet.Client.MqttApplicationMessageReceivedEventArgs args)
    {
        if (args.ApplicationMessage.Topic == _driverPropertys.RpcQuestTopic && args.ApplicationMessage.PayloadSegment.Count > 0)
        {
            await AllPublishAsync(CancellationToken.None);
            return;
        }

        if (!_driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(args.ClientId))
            return;
        var t = string.Format(TgMqttRpcClientTopicGenerationStrategy.RpcTopic, _driverPropertys.RpcWriteTopic);
        if (MqttTopicFilterComparer.Compare(args.ApplicationMessage.Topic, t) != MqttTopicFilterCompareResult.IsMatch)
            return;
        var rpcDatas = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).FromJsonString<Dictionary<string, JToken>>();
        if (rpcDatas == null)
            return;
        Dictionary<string, OperResult> mqttRpcResult = await GetResult(args, rpcDatas);
        try
        {
            var isConnect = await TryMqttClientAsync(CancellationToken.None);
            if (isConnect.IsSuccess)
            {
                var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{args.ApplicationMessage.Topic}/Response")
.WithPayload(mqttRpcResult.ToJsonString(true)).Build();
                await _mqttClient.PublishAsync(variableMessage);
            }
        }
        catch
        {
        }
    }

    private async Task<Dictionary<string, OperResult>> GetResult(MqttApplicationMessageReceivedEventArgs args, Dictionary<string, JToken> rpcDatas)
    {
        var mqttRpcResult = new Dictionary<string, OperResult>();
        try
        {
            foreach (var rpcData in rpcDatas)
            {
                var tag = CurrentDevice.VariableRunTimes.FirstOrDefault(a => a.Name == rpcData.Key);
                if (tag != null)
                {
                    var rpcEnable = tag.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable))?.Value?.ToBoolean();
                    if (rpcEnable == false)
                    {
                        mqttRpcResult.Add(rpcData.Key, new("权限不足，变量不支持写入"));
                    }
                }
                else
                {
                    mqttRpcResult.Add(rpcData.Key, new("不存在该变量"));
                }
            }

            var result = await RpcService.InvokeDeviceMethodAsync(ToString() + "-" + args.ClientId,
                rpcDatas.Where(
                a => !mqttRpcResult.Any(b => b.Key == a.Key)).ToDictionary(a => a.Key, a => a.Value.ToString()));

            mqttRpcResult.AddRange(result);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        return mqttRpcResult;
    }

    private async Task MqttClient_ConnectedAsync(MQTTnet.Client.MqttClientConnectedEventArgs args)
    {
        //连接成功后订阅相关主题
        var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions);
        if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
        {
            LogMessage?.LogWarning($"订阅失败  {subResult.Items
                .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                .Select(a =>
                new
                {
                    Topic = a.TopicFilter.Topic,
                    ResultCode = a.ResultCode.ToString()
                }
                )
                .ToJsonString(true)}");
        }
    }

    private async Task<OperResult> TryMqttClientAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient?.IsConnected == true)
            return new();
        return await Cilent();

        async Task<OperResult> Cilent()
        {
            if (_mqttClient?.IsConnected == true)
                return new();
            try
            {
                await ConnectLock.WaitAsync();
                if (_mqttClient?.IsConnected == true)
                    return new();
                using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(_driverPropertys.ConnectTimeout));
                using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);
                if (_mqttClient?.IsConnected == true)
                    return new();
                if (_mqttClient == null)
                {
                    return new OperResult("未初始化");
                }
                var result = await _mqttClient?.ConnectAsync(_mqttClientOptions, stoppingToken.Token);
                if (_mqttClient.IsConnected)
                {
                    return new();
                }
                else
                {
                    if (timeoutToken.IsCancellationRequested)
                        return new OperResult($"连接失败：超时");
                    else
                        return new OperResult($"连接失败{result.ReasonString}");
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                ConnectLock.Release();
            }
        }
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    private async Task<OperResult> MqttUpAsync(string topic, string payLoad, CancellationToken cancellationToken)
    {
        try
        {
            var isConnect = await TryMqttClientAsync(cancellationToken);
            if (isConnect.IsSuccess)
            {
                var variableMessage = new MqttApplicationMessageBuilder()
    .WithTopic(topic).WithRetainFlag(true)
    .WithPayload(payLoad).Build();
                var result = await _mqttClient.PublishAsync(variableMessage, cancellationToken);
                if (result.IsSuccess)
                {
                    LogMessage.LogTrace($"主题：{topic}{Environment.NewLine}负载：{payLoad}");
                    return new();
                }
                else
                {
                    return new($"上传失败{result.ReasonString}");
                }
            }
            else
            {
                return isConnect;
            }
        }
        catch (Exception ex)
        {
            return new($"上传失败", ex);
        }
    }

    #endregion mqtt方法
}