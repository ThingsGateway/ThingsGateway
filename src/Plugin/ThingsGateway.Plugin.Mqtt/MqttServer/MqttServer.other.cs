//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using MQTTnet;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using Newtonsoft.Json.Linq;

using System.Text;

using ThingsGateway.Admin.Application;
using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttServer
/// </summary>
public partial class MqttServer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private static readonly CompositeFormat RpcTopic = CompositeFormat.Parse("{0}/+");
    private MQTTnet.Server.MqttServer _mqttServer;
    private IWebHost _webHost { get; set; }

    protected override void AlarmChange(AlarmVariable alarmVariable)
    {
        if (!_businessPropertyWithCacheIntervalScript.AlarmTopic.IsNullOrWhiteSpace())
            AddQueueAlarmModel(new(alarmVariable));
        base.AlarmChange(alarmVariable);
    }
    protected override void DeviceTimeInterval(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {

        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrWhiteSpace())
            AddQueueDevModel(new(deviceData));

        base.DeviceChange(deviceRunTime, deviceData);
    }
    protected override void DeviceChange(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {
        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrEmpty())
            AddQueueDevModel(new(deviceData));
        base.DeviceChange(deviceRunTime, deviceData);
    }

    protected override ValueTask<OperResult> UpdateAlarmModel(IEnumerable<CacheDBItem<AlarmVariable>> item, CancellationToken cancellationToken)
    {
        return UpdateAlarmModel(item.Select(a => a.Value), cancellationToken);
    }

    protected override ValueTask<OperResult> UpdateDevModel(IEnumerable<CacheDBItem<DeviceData>> item, CancellationToken cancellationToken)
    {
        return UpdateDevModel(item.Select(a => a.Value), cancellationToken);
    }

    protected override ValueTask<OperResult> UpdateVarModel(IEnumerable<CacheDBItem<VariableData>> item, CancellationToken cancellationToken)
    {
        return UpdateVarModel(item.Select(a => a.Value), cancellationToken);
    }
    protected override void VariableTimeInterval(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        if (!_businessPropertyWithCacheIntervalScript.VariableTopic.IsNullOrWhiteSpace())
            AddQueueVarModel(new(variable));
        base.VariableChange(variableRuntime, variable);
    }
    protected override void VariableChange(VariableRuntime variableRuntime, VariableBasicData variable)
    {
        if (!_businessPropertyWithCacheIntervalScript.VariableTopic.IsNullOrEmpty())
            AddQueueVarModel(new(variable));
        base.VariableChange(variableRuntime, variable);
    }

    #region private

    private async ValueTask<OperResult> Update(List<TopicJson> topicJsonList, int count, CancellationToken cancellationToken)
    {
        foreach (var topicJson in topicJsonList)
        {
            var result = await MqttUpAsync(topicJson.Topic, topicJson.Json, count, cancellationToken).ConfigureAwait(false);
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
        return OperResult.Success;
    }

    private ValueTask<OperResult> UpdateAlarmModel(IEnumerable<AlarmVariable> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetAlarms(item);

        return Update(topicJsonList, item.Count(), cancellationToken);
    }

    private ValueTask<OperResult> UpdateDevModel(IEnumerable<DeviceData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetDeviceData(item);
        return Update(topicJsonList, item.Count(), cancellationToken);
    }

    private ValueTask<OperResult> UpdateVarModel(IEnumerable<VariableData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetVariable(item);
        return Update(topicJsonList, item.Count(), cancellationToken);
    }

    #endregion private

    private async ValueTask<Dictionary<string, OperResult>> GetResult(InterceptingPublishEventArgs args, Dictionary<string, JToken> rpcDatas)
    {
        var mqttRpcResult = new Dictionary<string, OperResult>();
        try
        {
            foreach (var rpcData in rpcDatas)
            {
                VariableRuntimes.TryGetValue(rpcData.Key, out var tag);
                if (tag != null)
                {
                    var rpcEnable = tag.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable))?.ToBoolean();
                    if (rpcEnable == false)
                    {
                        mqttRpcResult.Add(rpcData.Key, new OperResult("RPCEnable is False"));
                    }
                }
                else
                {
                    mqttRpcResult.Add(rpcData.Key, new OperResult("The variable does not exist"));
                }
            }

            var result = await GlobalData.RpcService.InvokeDeviceMethodAsync(ToString() + "-" + args.ClientId,
                rpcDatas.Where(
                a => !mqttRpcResult.Any(b => b.Key == a.Key)).ToDictionary(a => a.Key, a => a.Value.ToString())).ConfigureAwait(false);

            mqttRpcResult.AddRange(result);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        return mqttRpcResult;
    }

    private List<MqttApplicationMessage> GetRetainedMessages()
    {
        //首次连接时的保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = VariableRuntimes.Select(a => a.Value).Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Select(a => a.Value).Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        var alramData = GlobalData.ReadOnlyRealAlarmVariables.Select(a => a.Value).Adapt<List<AlarmVariable>>().ChunkBetter(_driverPropertys.SplitSize);
        List<MqttApplicationMessage> Messages = new();

        if (!_businessPropertyWithCacheIntervalScript.VariableTopic.IsNullOrEmpty())
        {
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
        }
        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrEmpty())
        {
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
        }
        if (!_businessPropertyWithCacheIntervalScript.AlarmTopic.IsNullOrEmpty())
        {
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
        }
        return Messages;
    }

    private Task MqttServer_ClientDisconnectedAsync(MQTTnet.Server.ClientDisconnectedEventArgs arg)
    {
        LogMessage?.LogInformation($"{ToString()}-{arg.ClientId}-Client DisConnected-{arg.DisconnectType}");
        return Task.CompletedTask;
    }

    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs args)
    {
#if NET8_0_OR_GREATER

        var payload = args.ApplicationMessage.Payload;
#else

        var payload = args.ApplicationMessage.PayloadSegment;

#endif
        if (!_driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(args.ClientId))
            return;

        if (_driverPropertys.RpcWriteTopic.IsNullOrWhiteSpace()) return;

        var t = string.Format(null, RpcTopic, _driverPropertys.RpcWriteTopic);
        if (MqttTopicFilterComparer.Compare(args.ApplicationMessage.Topic, t) != MqttTopicFilterCompareResult.IsMatch)
            return;
        var rpcDatas = Encoding.UTF8.GetString(payload).FromJsonNetString<Dictionary<string, JToken>>();
        if (rpcDatas == null)
            return;
        Dictionary<string, OperResult> mqttRpcResult = await GetResult(args, rpcDatas).ConfigureAwait(false);

        try
        {
            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{args.ApplicationMessage.Topic}/Response")
.WithPayload(mqttRpcResult.ToJsonNetString()).Build();
            await _mqttServer.InjectApplicationMessage(
                     new InjectedMqttApplicationMessage(variableMessage)).ConfigureAwait(false);
        }
        catch
        {
        }
    }

    private Task MqttServer_LoadingRetainedMessageAsync(LoadingRetainedMessagesEventArgs arg)
    {
        //List<MqttApplicationMessage> Messages = GetRetainedMessages();
        //arg.LoadedRetainedMessages = Messages;
        return CompletedTask.Instance;
    }

    private async Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        if (!arg.ClientId.StartsWith(_driverPropertys.StartWithId))
        {
            arg.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
            return;
        }

        var _userService = App.RootServices.GetRequiredService<ISysUserService>();
        var userInfo = await _userService.GetUserByAccountAsync(arg.UserName, null).ConfigureAwait(false);//获取用户信息
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
        LogMessage?.LogInformation($"{ToString()}-{arg.ClientId}-Client Connected");
        //   _ = Task.Run(async () =>
        //   {
        //       //延时发送
        //       await Task.Delay(1000).ConfigureAwait(false);
        //       List<MqttApplicationMessage> data = GetRetainedMessages();
        //       foreach (var item in data)
        //       {
        //           await _mqttServer.InjectApplicationMessage(
        //new InjectedMqttApplicationMessage(item)).ConfigureAwait(false);
        //       }
        //   });
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    private async ValueTask<OperResult> MqttUpAsync(string topic, string payLoad, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MqttApplicationMessageBuilder()
.WithTopic(topic)
.WithPayload(payLoad).Build();
            await _mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message), cancellationToken).ConfigureAwait(false);

            if (_driverPropertys.DetailLog)
            {
                if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                    LogMessage.LogTrace($"Topic：{topic}{Environment.NewLine}PayLoad：{payLoad} {Environment.NewLine} VarModelQueue:{_memoryVarModelQueue.Count} ");
            }
            else
            {
                LogMessage.LogTrace($"Topic：{topic}{Environment.NewLine}Count：{count} {Environment.NewLine} VarModelQueue:{_memoryVarModelQueue.Count}");

            }
            return OperResult.Success;
        }
        catch (Exception ex)
        {
            return new OperResult("Upload fail", ex);
        }
    }
}
