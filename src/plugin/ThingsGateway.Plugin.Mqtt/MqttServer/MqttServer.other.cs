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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using MQTTnet;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using System.Text;

using ThingsGateway.Admin.Application;
using ThingsGateway.Cache;
using ThingsGateway.Core.Extension;
using ThingsGateway.Core.Extension.Json;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttServer
/// </summary>
public partial class MqttServer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private MQTTnet.Server.MqttServer _mqttServer;
    private IWebHost _webHost { get; set; }

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

    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs args)
    {
        if (!_driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(args.ClientId))
            return;
        var t = string.Format(TgMqttRpcClientTopicGenerationStrategy.RpcTopic, _driverPropertys.RpcWriteTopic);
        if (MqttTopicFilterComparer.Compare(args.ApplicationMessage.Topic, t) != MqttTopicFilterCompareResult.IsMatch)
            return;
        var rpcDatas = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).FromJsonString<Dictionary<string, string>>();
        if (rpcDatas == null)
            return;
        Dictionary<string, OperResult> mqttRpcResult = await GetResult(args, rpcDatas);

        try
        {
            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{args.ApplicationMessage.Topic}/Response")
.WithPayload(mqttRpcResult.ToJsonString(true)).Build();
            await _mqttServer.InjectApplicationMessage(
                     new InjectedMqttApplicationMessage(variableMessage));
        }
        catch
        {
        }
    }

    private async Task<Dictionary<string, OperResult>> GetResult(InterceptingPublishEventArgs args, Dictionary<string, string> rpcDatas)
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
                a => !mqttRpcResult.Any(b => b.Key == a.Key)).ToDictionary(a => a.Key, a => a.Value));

            mqttRpcResult.AddRange(result);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        return mqttRpcResult;
    }

    private async Task MqttServer_LoadingRetainedMessageAsync(LoadingRetainedMessagesEventArgs arg)
    {
        List<MqttApplicationMessage> Messages = GetRetainedMessages();
        arg.LoadedRetainedMessages = Messages;
        await CompletedTask.Instance;
    }

    private async Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        if (!arg.ClientId.StartsWith(_driverPropertys.StartWithId))
        {
            arg.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
            return;
        }

        var _openApiUserService = ServiceScope.ServiceProvider.GetService<ISysUserService>();
        var userInfo = await _openApiUserService.GetUserByAccountAsync(arg.UserName);//获取用户信息
        if (userInfo == null)
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return;
        }
        if (userInfo.Password != arg.Password)
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return;
        }

        List<MqttApplicationMessage> data = GetRetainedMessages();
        foreach (var item in data)
        {
            await _mqttServer.UpdateRetainedMessageAsync(item);
        }
        LogMessage?.LogInformation($"{ToString()}-{arg.ClientId}-客户端已连接成功");
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    private async Task<OperResult> MqttUpAsync(string topic, string payLoad, CancellationToken cancellationToken)
    {
        try
        {
            var message = new MqttApplicationMessageBuilder()
.WithTopic(topic)
.WithPayload(payLoad).Build();
            await _mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message), cancellationToken);
            LogMessage.LogTrace($"主题：{topic}{Environment.NewLine}负载：{payLoad}");
            return new();
        }
        catch (Exception ex)
        {
            return new("上传失败", ex);
        }
    }

    private List<MqttApplicationMessage> GetRetainedMessages()
    {
        //首次连接时的保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = CurrentDevice.VariableRunTimes.Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        var alramData = WorkerUtil.GetWoker<AlarmWorker>().RealAlarmVariables.Adapt<List<AlarmVariable>>().ChunkBetter(_driverPropertys.SplitSize);
        List<MqttApplicationMessage> Messages = new();
        foreach (var item in varData)
        {
            List<TopicJson> topicJsonList = GetVariable(item);
            foreach (var topicJson in topicJsonList)
            {
                Messages.Add(new MqttApplicationMessageBuilder()
.WithTopic(topicJson.Topic)
.WithPayload(topicJson.Json).Build());
            }
        }
        foreach (var item in devData)
        {
            List<TopicJson> topicJsonList = GetDeviceData(item);
            foreach (var topicJson in topicJsonList)
            {
                Messages.Add(new MqttApplicationMessageBuilder()
.WithTopic(topicJson.Topic)
.WithPayload(topicJson.Json).Build());
            }
        }
        foreach (var item in alramData)
        {
            List<TopicJson> topicJsonList = GetAlarms(item);
            foreach (var topicJson in topicJsonList)
            {
                Messages.Add(new MqttApplicationMessageBuilder()
.WithTopic(topicJson.Topic)
.WithPayload(topicJson.Json).Build());
            }
        }
        return Messages;
    }
}