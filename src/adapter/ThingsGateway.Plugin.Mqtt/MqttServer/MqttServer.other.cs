//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MQTTnet;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using Newtonsoft.Json.Linq;

using System.Text;

using ThingsGateway.Admin.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Collection;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Json.Extension;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttServer
/// </summary>
public partial class MqttServer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private MQTTnet.Server.MqttServer _mqttServer;
    private IWebHost _webHost { get; set; }

    protected override void VariableChange(VariableRunTime variableRunTime, VariableData variable)
    {
        AddQueueVarModel(new(variable));
        base.VariableChange(variableRunTime, variable);
    }

    protected override void DeviceChange(DeviceRunTime deviceRunTime, DeviceData deviceData)
    {
        AddQueueDevModel(new(deviceData));
        base.DeviceChange(deviceRunTime, deviceData);
    }

    protected override void AlarmChange(AlarmVariable alarmVariable)
    {
        AddQueueAlarmModel(new(alarmVariable));
        base.AlarmChange(alarmVariable);
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

    #region private

    private ValueTask<OperResult> UpdateAlarmModel(IEnumerable<AlarmVariable> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetAlarms(item);
        return Update(topicJsonList, cancellationToken);
    }

    private async ValueTask<OperResult> Update(List<TopicJson> topicJsonList, CancellationToken cancellationToken)
    {
        foreach (var topicJson in topicJsonList)
        {
            var result = await MqttUpAsync(topicJson.Topic, topicJson.Json, cancellationToken).ConfigureAwait(false);
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

    private ValueTask<OperResult> UpdateDevModel(IEnumerable<DeviceData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetDeviceData(item);
        return Update(topicJsonList, cancellationToken);
    }

    private ValueTask<OperResult> UpdateVarModel(IEnumerable<VariableData> item, CancellationToken cancellationToken)
    {
        List<TopicJson> topicJsonList = GetVariable(item);
        return Update(topicJsonList, cancellationToken);
    }

    #endregion private

    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs args)
    {
        if (!_driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(args.ClientId))
            return;
        var t = string.Format(TgMqttRpcClientTopicGenerationStrategy.RpcTopic, _driverPropertys.RpcWriteTopic);
        if (MqttTopicFilterComparer.Compare(args.ApplicationMessage.Topic, t) != MqttTopicFilterCompareResult.IsMatch)
            return;
        var rpcDatas = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).FromJsonNetString<Dictionary<string, JToken>>();
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

    private async ValueTask<Dictionary<string, OperResult>> GetResult(InterceptingPublishEventArgs args, Dictionary<string, JToken> rpcDatas)
    {
        var mqttRpcResult = new Dictionary<string, OperResult>();
        try
        {
            foreach (var rpcData in rpcDatas)
            {
                CurrentDevice.VariableRunTimes.TryGetValue(rpcData.Key, out var tag);
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

            var result = await RpcService.InvokeDeviceMethodAsync(ToString() + "-" + args.ClientId,
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

        var _userService = App.RootServices.GetRequiredService<ISysUserService>();
        var userInfo = await _userService.GetUserByAccountAsync(arg.UserName).ConfigureAwait(false);//获取用户信息
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
        _ = Task.Run(async () =>
        {
            //延时发送
            await Task.Delay(1000).ConfigureAwait(false);
            List<MqttApplicationMessage> data = GetRetainedMessages();
            foreach (var item in data)
            {
                await _mqttServer.InjectApplicationMessage(
     new InjectedMqttApplicationMessage(item)).ConfigureAwait(false);
                //await _mqttServer.UpdateRetainedMessageAsync(item);
            }
        });
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    private async ValueTask<OperResult> MqttUpAsync(string topic, string payLoad, CancellationToken cancellationToken)
    {
        try
        {
            var message = new MqttApplicationMessageBuilder()
.WithTopic(topic)
.WithPayload(payLoad).Build();
            await _mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message), cancellationToken).ConfigureAwait(false);
            if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                LogMessage.LogTrace($"Topic：{topic}{Environment.NewLine}PayLoad：{payLoad}");
            return OperResult.Success;
        }
        catch (Exception ex)
        {
            return new OperResult("Upload fail", ex);
        }
    }

    private List<MqttApplicationMessage> GetRetainedMessages()
    {
        //首次连接时的保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = CurrentDevice.VariableRunTimes.Select(a => a.Value).Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Select(a => a.Value).Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        var alramData = GlobalData.ReadOnlyRealAlarmVariables.Adapt<List<AlarmVariable>>().ChunkBetter(_driverPropertys.SplitSize);
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
