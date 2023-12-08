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

using IoTSharp.Data;

using LiteDB;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;

using ThingsGateway.Foundation.Extension.String;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// IotSharpClient
/// </summary>
public partial class IotSharpClient : UpLoadBaseWithCacheT<DeviceData, VariableData>
{
    /// <summary>
    /// rpcmethodname存疑，定为自定义方法，在ThingsGateway上写入变量的方法固定为"Write"
    /// </summary>
    private const string WriteMethod = "WRITE";

    private const string devType = "dev";
    private const string varType = "var";
    private readonly MqttClientVariableProperty _variablePropertys = new();
    private readonly IotSharpClientProperty _driverPropertys = new();
    private readonly EasyLock easyLock = new();

    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private volatile bool success = true;

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        if (!_driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(args.ClientId))
            return;

        if (args.ApplicationMessage.Topic.StartsWith($"devices/") && args.ApplicationMessage.Topic.Contains("/rpc/request/"))
        {
            var tps = args.ApplicationMessage.Topic.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var rpcmethodname = tps[4];
            var rpcdevicename = tps[1];
            var rpcrequestid = tps[5];
            if (!string.IsNullOrEmpty(rpcmethodname) && !string.IsNullOrEmpty(rpcdevicename) && !string.IsNullOrEmpty(rpcrequestid))
            {
                var rpcResponse = new RpcResponse()
                {
                    DeviceId = rpcdevicename,
                    ResponseId = rpcrequestid,
                    Method = rpcmethodname,
                    Success = false,
                    Data = "参数为空"
                };
                await SendResponseAsync(rpcResponse);
                return;
            }
            if (!_driverPropertys.DeviceRpcEnable)
            {
                var rpcResponse = new RpcResponse()
                {
                    DeviceId = rpcdevicename,
                    ResponseId = rpcrequestid,
                    Method = rpcmethodname,
                    Success = false,
                    Data = "不允许写入"
                };
                await SendResponseAsync(rpcResponse);
                return;
            }
            //rpcmethodname定为自定义方法，在ThingsGateway上写入变量的方法固定为"Write"
            if (rpcmethodname.ToUpper() != WriteMethod)
            {
                var rpcResponse = new RpcResponse()
                {
                    DeviceId = rpcdevicename,
                    ResponseId = rpcrequestid,
                    Method = rpcmethodname,
                    Success = false,
                    Data = "不支持的方法"
                };
                await SendResponseAsync(rpcResponse);
                return;
            }
            else
            {
                RpcResponse rpcResponse = new();
                var nameValue = args.ApplicationMessage.ConvertPayloadToString().FromJsonString<List<KeyValuePair<string, string>>>();
                Dictionary<string, OperResult> results = new();
                if (nameValue?.Count > 0)
                {
                    foreach (var item in nameValue)
                    {
                        var tag = CurrentDevice.DeviceVariableRunTimes.FirstOrDefault(a => a.Name == item.Key);
                        if (tag != null)
                        {
                            var rpcEnable = tag.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable))?.Value?.ToBool() == true && _driverPropertys.DeviceRpcEnable;
                            if (!rpcEnable)
                            {
                                results.Add(item.Key, new OperResult("权限不足，变量不支持写入"));
                            }
                        }
                        else
                        {
                            results.Add(item.Key, new OperResult("不存在该变量"));
                        }
                    }

                    var result = await RpcSingletonService.InvokeDeviceMethodAsync(ToString() + "-" + rpcrequestid, nameValue
                        .Where(a => !results.Any(b => b.Key == a.Key))
                        .ToDictionary(a => a.Key, a => a.Value));

                    results.AddRange(result);
                    rpcResponse = new()
                    {
                        DeviceId = rpcdevicename,
                        ResponseId = rpcrequestid,
                        Method = rpcmethodname,
                        Success = !results.Any(a => !a.Value.IsSuccess),
                        Data = results.ToJsonString()
                    };
                }
                else
                {
                    rpcResponse = new()
                    {
                        DeviceId = rpcdevicename,
                        ResponseId = rpcrequestid,
                        Method = rpcmethodname,
                        Success = false,
                        Data = "消息体参数无法解析"
                    };
                }

                await SendResponseAsync(rpcResponse);
            }
        }

        async Task SendResponseAsync(RpcResponse rpcResponse)
        {
            try
            {
                var topic = $"devices/{rpcResponse.DeviceId}/rpc/response/{rpcResponse.Method}/{rpcResponse.ResponseId}";

                var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{topic}")
.WithPayload(rpcResponse.ToJsonString()).Build();
                var isConnect = await TryMqttClientAsync(CancellationToken.None);
                if (isConnect.IsSuccess)
                    await _mqttClient.PublishAsync(variableMessage);
            }
            catch
            {
            }
        }
    }

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<VariableData> dev)
    {
        Dictionary<string, object> nameValueDict = new();
        foreach (var pair in dev)
        {
            nameValueDict.AddOrUpdate(pair.Name, pair.Value);
        }
        var json = nameValueDict.ToJsonString();
        AddVarCahce(cacheItems, $"{dev.FirstOrDefault()?.Name ?? ""}", json);
    }

    private static void AddVarCahce(List<CacheItem> cacheItems, string key, string data)
    {
        var cacheItem = new CacheItem()
        {
            Id = YitIdHelper.NextId(),
            Key = key,
            Type = varType,
            Value = data,
        };
        cacheItems.Add(cacheItem);
    }

    protected override void AddCache(List<CacheItem> cacheItems, IEnumerable<DeviceData> dev)
    {
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