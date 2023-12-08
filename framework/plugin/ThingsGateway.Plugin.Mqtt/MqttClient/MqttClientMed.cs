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

using LiteDB;

using Mapster;

using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;

using System.Text;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttClient
/// </summary>
public partial class MqttClient : UpLoadBaseWithCacheT<DeviceData, VariableData>
{
    private const string devType = "dev";
    private const string varType = "var";
    private readonly MqttClientVariableProperty _variablePropertys = new();
    private readonly MqttClientProperty _driverPropertys = new();
    private readonly EasyLock easyLock = new();

    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private volatile bool success = true;

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<VariableData> dev)
    {
        AddVarCahce(cacheItems, $"{_driverPropertys.VariableTopic}", dev.GetSciptListValue(_driverPropertys.BigTextScriptVariableModel));
    }

    private void AddVarCahce(List<CacheItem> cacheItems, string key, string data)
    {
        var cacheItem = new CacheItem()
        {
            Id = YitIdHelper.NextId(),
            Key = key,
            Type = varType,
            Value = data
        };
        cacheItems.Add(cacheItem);
    }

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<DeviceData> dev)
    {
        AddDevCache(cacheItems, $"{_driverPropertys.DeviceTopic}", dev.GetSciptListValue(_driverPropertys.BigTextScriptDeviceModel));
    }

    private void AddDevCache(List<CacheItem> cacheItems, string key, string data)
    {
        var cacheItem = new CacheItem()
        {
            Id = YitIdHelper.NextId(),
            Key = key,
            Type = devType,
            Value = data
        };
        cacheItems.Add(cacheItem);
    }

    private async Task AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = CurrentDevice.DeviceVariableRunTimes.Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        var isConnect = await TryMqttClientAsync(cancellationToken);
        foreach (var item in devData)
        {
            var devMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{_driverPropertys.DeviceTopic}")
.WithPayload(item.GetSciptListValue(_driverPropertys.BigTextScriptDeviceModel)).Build();
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(devMessage, cancellationToken);
        }

        foreach (var item in varData)
        {
            var varMessage = new MqttApplicationMessageBuilder()
            .WithTopic($"{_driverPropertys.VariableTopic}")
            .WithPayload(item.GetSciptListValue(_driverPropertys.BigTextScriptVariableModel)).Build();
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(varMessage, cancellationToken);
        }
    }

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        if (args.ApplicationMessage.Topic == _driverPropertys.QuestRpcTopic && args.ApplicationMessage.PayloadSegment.Count > 0)
        {
            await AllPublishAsync(CancellationToken.None);
            return;
        }

        if (!_driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(args.ClientId))
            return;
        if (args.ApplicationMessage.Topic != _driverPropertys.RpcWriteTopic)
            return;
        var rpcDatas = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).FromJsonString<MqttRpcNameVaueWithId>();
        if (rpcDatas == null)
            return;

        MqttRpcResult mqttRpcResult = new() { RpcId = rpcDatas.RpcId, Success = true };
        try
        {
            foreach (var rpcData in rpcDatas.WriteInfos)
            {
                var tag = CurrentDevice.DeviceVariableRunTimes.FirstOrDefault(a => a.Name == rpcData.Key);
                if (tag != null)
                {
                    var rpcEnable = tag.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable))?.Value?.ToBool();
                    if (rpcEnable == true)
                    {
                    }
                    else
                    {
                        mqttRpcResult.Success = false;
                        mqttRpcResult.Message.Add(rpcData.Key, new("权限不足，变量不支持写入"));
                    }
                }
                else
                {
                    mqttRpcResult.Success = false;
                    mqttRpcResult.Message.Add(rpcData.Key, new("不存在该变量"));
                }
            }

            var result = await RpcSingletonService.InvokeDeviceMethodAsync(ToString() + "-" + args.ClientId,
                rpcDatas.WriteInfos.Where(
                a => !mqttRpcResult.Message.Any(b => b.Key == a.Key)).ToDictionary(a => a.Key, a => a.Value));

            mqttRpcResult.Message.AddRange(result);
            mqttRpcResult.Success = !mqttRpcResult.Message.Any(a => !a.Value.IsSuccess);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        try
        {
            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{_driverPropertys.RpcSubTopic}")
.WithPayload(mqttRpcResult.ToJsonString()).Build();
            var isConnect = await TryMqttClientAsync(CancellationToken.None);
            if (isConnect.IsSuccess)
                await _mqttClient.PublishAsync(variableMessage);
        }
        catch
        {
        }
    }

    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions);
        if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
        {
            LogMessage?.Warning($"订阅失败-{subResult.Items
                .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                .Select(a =>
                new
                {
                    Topic = a.TopicFilter.Topic,
                    ResultCode = a.ResultCode.ToString()
                }
                )
                .ToJsonString()}");
        }
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    private async Task<OperResult> MqttUpAsync(string topic, string payLoad, CancellationToken cancellationToken)
    {
        var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic(topic)
.WithPayload(payLoad).Build();
        var isConnect = await TryMqttClientAsync(cancellationToken);
        if (isConnect.IsSuccess)
        {
            var result = await _mqttClient.PublishAsync(variableMessage, cancellationToken);
            if (result.IsSuccess)
            {
                LogMessage.Trace($"{FoundationConst.LogMessageHeader}主题：{topic}{Environment.NewLine}负载：{payLoad}");
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);

                return OperResult.CreateSuccessResult();
            }
            else
            {
                CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1);
                return new($"上传失败{result.ReasonString}");
            }
        }
        else
        {
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, CurrentDevice.ErrorCount + 1);
            return isConnect;
        }
    }

    private async Task<OperResult> TryMqttClientAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient?.IsConnected == true)
            return OperResult.CreateSuccessResult();
        return await Cilent();

        async Task<OperResult> Cilent()
        {
            if (_mqttClient?.IsConnected == true)
                return OperResult.CreateSuccessResult();
            try
            {
                await easyLock.WaitAsync();
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(_driverPropertys.ConnectTimeOut));
                using CancellationTokenSource StoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                if (_mqttClient == null)
                {
                    return new OperResult("未初始化");
                }
                var result = await _mqttClient?.ConnectAsync(_mqttClientOptions, StoppingToken.Token);
                if (_mqttClient.IsConnected)
                {
                    return OperResult.CreateSuccessResult();
                }
                else
                {
                    return new OperResult($"连接失败{result.ReasonString}");
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                easyLock.Release();
            }
        }
    }
}