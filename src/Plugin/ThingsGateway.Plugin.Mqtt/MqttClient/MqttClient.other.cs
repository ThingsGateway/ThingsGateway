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

using MQTTnet;

#if NET6_0
using MQTTnet.Client;
#endif

using Newtonsoft.Json.Linq;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Extension.Generic;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.NewLife.Extension;
using ThingsGateway.NewLife.Json.Extension;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttClient
/// </summary>
public partial class MqttClient : BusinessBaseWithCacheIntervalScript<VariableData, DeviceBasicData, AlarmVariable>
{
    private static readonly CompositeFormat RpcTopic = CompositeFormat.Parse("{0}/+");
    public const string ThingsBoardRpcTopic = "v1/gateway/rpc";
    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private WaitLock ConnectLock = new();

    protected override void AlarmChange(AlarmVariable alarmVariable)
    {
        if (!_businessPropertyWithCacheIntervalScript.AlarmTopic.IsNullOrWhiteSpace())
            AddQueueAlarmModel(new(alarmVariable));
        base.AlarmChange(alarmVariable);
    }

    private ConcurrentQueue<DeviceBasicData> ThingsBoardDeviceConnectQueue { get; set; } = new();

    private async ValueTask<OperResult> UpdateThingsBoardDeviceConnect(DeviceBasicData deviceData)
    {
        List<TopicJson> topicJsonTBList = new();

        {
            if (deviceData.DeviceStatus == DeviceStatusEnum.OnLine)
            {
                var topicJson = new TopicJson()
                {
                    Topic = "v1/gateway/connect",
                    Json = new
                    {
                        device = deviceData.Name,
                    }.ToJsonNetString()
                };
                topicJsonTBList.Add(topicJson);
            }
            else
            {
                var topicJson = new TopicJson()
                {
                    Topic = "v1/gateway/disconnect",
                    Json = new
                    {
                        device = deviceData.Name,
                    }.ToJsonNetString()
                };
                topicJsonTBList.Add(topicJson);
            }

        }
        var result = await Update(topicJsonTBList, 1, default).ConfigureAwait(false);
        if (success != result.IsSuccess)
        {
            if (!result.IsSuccess)
            {
                LogMessage.LogWarning(result.ToString());
            }
            success = result.IsSuccess;
        }
        return result;
    }

    protected override void DeviceTimeInterval(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {

        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrWhiteSpace())
            AddQueueDevModel(new(deviceData));

        base.DeviceChange(deviceRunTime, deviceData);
    }
    protected override void DeviceChange(DeviceRuntime deviceRunTime, DeviceBasicData deviceData)
    {
        if (_driverPropertys.RpcWriteTopic == ThingsBoardRpcTopic)
        {
            ThingsBoardDeviceConnectQueue.Enqueue(deviceData);
        }

        if (!_businessPropertyWithCacheIntervalScript.DeviceTopic.IsNullOrWhiteSpace())
            AddQueueDevModel(new(deviceData));

        base.DeviceChange(deviceRunTime, deviceData);
    }

    protected override ValueTask<OperResult> UpdateAlarmModel(IEnumerable<CacheDBItem<AlarmVariable>> item, CancellationToken cancellationToken)
    {
        return UpdateAlarmModel(item.Select(a => a.Value), cancellationToken);
    }

    protected override ValueTask<OperResult> UpdateDevModel(IEnumerable<CacheDBItem<DeviceBasicData>> item, CancellationToken cancellationToken)
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
        if (!_businessPropertyWithCacheIntervalScript.VariableTopic.IsNullOrWhiteSpace())
            AddQueueVarModel(new(variable));
        base.VariableChange(variableRuntime, variable);
    }

    #region mqtt方法

    #region private

    private async ValueTask<OperResult> Update(List<TopicJson> topicJsonList, int count, CancellationToken cancellationToken)
    {
        foreach (var topicJson in topicJsonList)
        {
            var result = await MqttUpAsync(topicJson.Topic, topicJson.Json, count, cancellationToken).ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
                return result;
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

    private ValueTask<OperResult> UpdateDevModel(IEnumerable<DeviceBasicData> item, CancellationToken cancellationToken)
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

    private async ValueTask AllPublishAsync(CancellationToken cancellationToken)
    {
        //保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = VariableRuntimes.Select(a => a.Value).Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = CollectDevices.Select(a => a.Value).Adapt<List<DeviceBasicData>>().ChunkBetter(_driverPropertys.SplitSize);
        var alramData = GlobalData.ReadOnlyRealAlarmVariables.Select(a => a.Value).Adapt<List<AlarmVariable>>().ChunkBetter(_driverPropertys.SplitSize);
        foreach (var item in varData)
        {
            if (!success)
                break;
            await UpdateVarModel(item, cancellationToken).ConfigureAwait(false);
        }

        foreach (var item in devData)
        {
            if (!success)
                break;
            await UpdateDevModel(item, cancellationToken).ConfigureAwait(false);
        }

        foreach (var item in alramData)
        {
            if (!success)
                break;
            await UpdateAlarmModel(item, cancellationToken).ConfigureAwait(false);
        }
    }

    private async ValueTask<Dictionary<string, OperResult>> GetResult(MqttApplicationMessageReceivedEventArgs args, Dictionary<string, JToken> rpcDatas)
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

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
#if NET8_0_OR_GREATER

        var payload = args.ApplicationMessage.Payload;
        var payloadCount = payload.Length;
#else

        var payload = args.ApplicationMessage.PayloadSegment;
        var payloadCount = payload.Count;

#endif


        if (args.ApplicationMessage.Topic == _driverPropertys.RpcQuestTopic && payloadCount > 0)
        {
            await AllPublishAsync(CancellationToken.None).ConfigureAwait(false);
            return;
        }

        if (!_driverPropertys.DeviceRpcEnable)
            return;

        Dictionary<string, JToken> rpcDatas = null;

        //适配 ThingsBoardRp
        if (args.ApplicationMessage.Topic == ThingsBoardRpcTopic)
        {
            var thingsBoardRpcData = Encoding.UTF8.GetString(payload).FromJsonNetString<ThingsBoardRpcData>();
            if (thingsBoardRpcData == null)
                return;
            rpcDatas = thingsBoardRpcData.data.@params.ToDictionary(a => a.Key, a => JToken.Parse(a.Value));
            if (rpcDatas == null)
                return;

            Dictionary<string, OperResult> mqttRpcResult = await GetResult(args, rpcDatas).ConfigureAwait(false);
            try
            {
                var isConnect = await TryMqttClientAsync(CancellationToken.None).ConfigureAwait(false);
                if (isConnect.IsSuccess)
                {
                    ThingsBoardRpcResponseData thingsBoardRpcResponseData = new();
                    thingsBoardRpcResponseData.device = thingsBoardRpcData.device;
                    thingsBoardRpcResponseData.id = thingsBoardRpcData.data.id;
                    thingsBoardRpcResponseData.data.success = mqttRpcResult.All(a => a.Value.IsSuccess);
                    thingsBoardRpcResponseData.data.message = mqttRpcResult.Select(a => a.Value.ErrorMessage).ToJsonNetString();

                    var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{args.ApplicationMessage.Topic}")
.WithPayload(thingsBoardRpcResponseData.ToJsonNetString()).Build();
                    await _mqttClient.PublishAsync(variableMessage).ConfigureAwait(false);


                }
            }
            catch
            {
            }
        }
        else
        {
            var t = string.Format(null, RpcTopic, _driverPropertys.RpcWriteTopic);
            if (MqttTopicFilterComparer.Compare(args.ApplicationMessage.Topic, t) != MqttTopicFilterCompareResult.IsMatch)
                return;
            rpcDatas = Encoding.UTF8.GetString(payload).FromJsonNetString<Dictionary<string, JToken>>();
            if (rpcDatas == null)
                return;

            Dictionary<string, OperResult> mqttRpcResult = await GetResult(args, rpcDatas).ConfigureAwait(false);
            try
            {
                var isConnect = await TryMqttClientAsync(CancellationToken.None).ConfigureAwait(false);
                if (isConnect.IsSuccess)
                {

                    var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{args.ApplicationMessage.Topic}/Response")
.WithPayload(mqttRpcResult.ToJsonNetString()).Build();
                    await _mqttClient.PublishAsync(variableMessage).ConfigureAwait(false);


                }
            }
            catch
            {
            }
        }


    }

    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs args)
    {
        //连接成功后订阅相关主题
        if (_mqttSubscribeOptions != null)
        {
            var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions).ConfigureAwait(false);
            if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
            {
                LogMessage?.LogWarning($"Subscribe fail  {subResult.Items
                    .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                    .Select(a =>
                    new
                    {
                        Topic = a.TopicFilter.Topic,
                        ResultCode = a.ResultCode.ToString()
                    }
                    )
                    .ToJsonNetString()}");
            }
        }
    }

    /// <summary>
    /// 上传mqtt，返回上传结果
    /// </summary>
    public async ValueTask<OperResult> MqttUpAsync(string topic, string payLoad, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            var isConnect = await TryMqttClientAsync(cancellationToken).ConfigureAwait(false);
            if (isConnect.IsSuccess)
            {
                var variableMessage = new MqttApplicationMessageBuilder()
    .WithTopic(topic).WithRetainFlag(true)
    .WithPayload(payLoad).Build();
                var result = await _mqttClient.PublishAsync(variableMessage, cancellationToken).ConfigureAwait(false);
                if (result.IsSuccess)
                {
                    if (_driverPropertys.DetailLog)
                    {
                        if (LogMessage.LogLevel <= TouchSocket.Core.LogLevel.Trace)
                            LogMessage.LogTrace($"Topic：{topic}{Environment.NewLine}PayLoad：{payLoad} {Environment.NewLine} VarModelQueue:{_memoryVarModelQueue.Count}");
                    }
                    else
                    {
                        LogMessage.LogTrace($"Topic：{topic}{Environment.NewLine}Count：{count} {Environment.NewLine} VarModelQueue:{_memoryVarModelQueue.Count}");

                    }
                    return OperResult.Success;
                }
                else
                {
                    return new OperResult($"Upload fail{result.ReasonString}");
                }
            }
            else
            {
                return isConnect;
            }
        }
        catch (Exception ex)
        {
            return new OperResult($"Upload fail", ex);
        }
    }

    private async ValueTask<OperResult> TryMqttClientAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient?.IsConnected == true)
            return OperResult.Success;
        return await Cilent().ConfigureAwait(false);

        async ValueTask<OperResult> Cilent()
        {
            if (_mqttClient?.IsConnected == true)
                return OperResult.Success;
            try
            {
                await ConnectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                if (_mqttClient?.IsConnected == true)
                    return OperResult.Success;
                using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(_driverPropertys.ConnectTimeout));
                using CancellationTokenSource stoppingToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);
                if (_mqttClient?.IsConnected == true)
                    return OperResult.Success;
                if (_mqttClient == null)
                {
                    return new OperResult("mqttClient is null");
                }
                var result = await _mqttClient.ConnectAsync(_mqttClientOptions, stoppingToken.Token).ConfigureAwait(false);
                if (_mqttClient.IsConnected)
                {
                    return OperResult.Success;
                }
                else
                {
                    if (timeoutToken.IsCancellationRequested)
                        return new OperResult($"Connect timeout");
                    else
                        return new OperResult($"Connect fail {result.ReasonString}");
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

    #endregion mqtt方法
}
