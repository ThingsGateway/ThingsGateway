using IoTSharp.Data;

using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;

using NewLife.Serialization;

using System.Collections.Concurrent;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Mqtt;

/// <summary>
/// 参考IotSharpClient.SDK.MQTT
/// </summary>
public class IotSharpClient : UpLoadBase
{
    /// <summary>
    /// rpcmethodname存疑，定为自定义方法，在ThingsGateway上写入变量的方法固定为"Write"
    /// </summary>
    private const string WriteMethod = "WRITE";

    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private UploadDevice _curDevice;
    private GlobalCollectDeviceData _globalCollectDeviceData;

    private IMqttClient _mqttClient;

    private MqttClientOptions _mqttClientOptions;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

    private RpcSingletonService _rpcCore;
    private List<CollectVariableRunTime> _uploadVariables = new();
    private CollectDeviceWorker collectDeviceHostService;

    private IotSharpClientProperty driverPropertys = new();

    private EasyLock lockobj = new();
    private IotSharpClientVariableProperty variablePropertys = new();

    public IotSharpClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }

    public override UpDriverPropertyBase DriverPropertys => driverPropertys;
    public override List<CollectVariableRunTime> UploadVariables => _uploadVariables;
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override async Task BeforStartAsync()
    {
        if (_mqttClient != null)
        {
            var result = await TryMqttClientAsync();
            if (!result.IsSuccess)
            {
                _logger?.LogWarning(ToString() + $"-连接MqttServer失败：{result.Message}");
            }
        }
    }

    public override void Dispose()
    {
        try
        {
            lockobj.Lock();
            _globalCollectDeviceData?.CollectVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);

            _globalCollectDeviceData?.CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusCahnge -= DeviceStatusCahnge;
            });
            _mqttClient?.Dispose();
            _mqttClient = null;
        }
        finally
        {
            lockobj.UnLock();
        }

    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            ////变化推送
            var varList = _collectVariableRunTimes.ToListWithDequeue();
            if (varList?.Count != 0)
            {
                //分解List，避免超出mqtt字节大小限制.ChunkTrivialBetter(500)
                var varData = varList.GroupBy(a => a.deviceName).ToList();
                foreach (var item in varData)
                {
                    try
                    {
                        Dictionary<string, object> nameValueDict = new();
                        foreach (var pair in item)
                        {
                            //只用最新的变量值
                            nameValueDict.AddOrUpdate(pair.name, pair.value);
                        }
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var variableMessage = new MqttApplicationMessageBuilder()
                            .WithTopic($"devices/{item.Key}/telemetry")
                            .WithPayload(nameValueDict.ToJson()).Build();
                            var isConnect = await TryMqttClientAsync();
                            if (isConnect.IsSuccess)
                                await _mqttClient.PublishAsync(variableMessage, cancellationToken);
                        }
                        else
                        {
                            break;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, ToString());
                    }

                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }
        try
        {
            ////变化推送
            var devList = _collectDeviceRunTimes.ToListWithDequeue();
            if (devList?.Count != 0)
            {
                //分解List，避免超出mqtt字节大小限制
                var devData = devList.ChunkTrivialBetter(500);
                foreach (var item in devData)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            //        var variableMessage = new MqttApplicationMessageBuilder()
                            //.WithTopic($"{DeviceTopic}")
                            //.WithPayload(item.GetSciptListValue(BigTextScriptDeviceModel)).Build();
                            //        if (_mqttClient.IsConnected)
                            //            await _mqttClient.PublishAsync(variableMessage);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, ToString());
                    }
                }

            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
        }

        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval);
        }
        else
        {

        }

    }

    public override OperResult IsConnected()
    {
        if (_mqttClient?.IsConnected == true)
        {
            return OperResult.CreateSuccessResult();
        }
        else
        {
            return new OperResult();
        }
    }
    public override string ToString()
    {
        return $" {nameof(IotSharpClient)}-IP:{driverPropertys.IP}-Port:{driverPropertys.Port}-Accesstoken:{driverPropertys.Accesstoken}";
    }

    protected override void Init(UploadDevice device)
    {
        _curDevice = device;
        var mqttFactory = new MqttFactory(new PrivateLogger(_logger));
        _mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
           .WithClientId(Guid.NewGuid().ToString())
           .WithCredentials(driverPropertys.Accesstoken)//账密
           .WithTcpServer(driverPropertys.IP, driverPropertys.Port)//服务器
           .WithCleanSession(true)
           .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
           .Build();
        _mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic($"devices/+/rpc/request/+/+");//RPC控制请求，需要订阅
                })

            .Build();
        _mqttClient = mqttFactory.CreateMqttClient();
        _mqttClient.ConnectedAsync += _mqttClient_ConnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += _mqttClient_ApplicationMessageReceivedAsync;
        var serviceScope = _scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceWorker>();


        var tags = _globalCollectDeviceData.CollectVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
           .Where(b => b.VariablePropertys[device.Id].Any(c =>
           {
               if (c.PropertyName == nameof(variablePropertys.Enable))
               {
                   if (c.Value?.GetBoolValue() == true)
                       return true;
                   else
                       return false;
               }
               else
                   return false;
           }))
           .ToList();

        _uploadVariables = tags;

        _globalCollectDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusCahnge += DeviceStatusCahnge;
        });
        _uploadVariables.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
        });

        _ = Task.Run(
          async () =>
          {
              await Task.Delay(driverPropertys.ConnectTimeOut * 20);
              bool lastIsSuccess = _mqttClient?.IsConnected == true;
              while (_mqttClient != null)
              {
                  try
                  {
                      var result = await TryMqttClientAsync();

                      if (!result.IsSuccess && lastIsSuccess)
                      {
                          lastIsSuccess = false;
                          _logger?.LogWarning(ToString() + $"-连接MqttServer失败：{result.Message}");
                      }
                      else if (result.IsSuccess && !lastIsSuccess)
                      {
                          lastIsSuccess = true;
                          _logger?.LogInformation(ToString() + $"-连接MqttServer成功：{result.Message}");
                      }
                  }
                  finally
                  {
                      await Task.Delay(driverPropertys.ConnectTimeOut * 10);
                  }
              }
          });
    }

    private async Task _mqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {

        if (e.ApplicationMessage.Topic.StartsWith($"devices/") && e.ApplicationMessage.Topic.Contains("/rpc/request/"))
        {
            var tps = e.ApplicationMessage.Topic.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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
            if (!driverPropertys.DeviceRpcEnable)
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
                var nameValue = e.ApplicationMessage.ConvertPayloadToString().ToJsonEntity<List<NameValue>>();
                Dictionary<string, OperResult> results = new();
                if (nameValue?.Count > 0)
                {
                    foreach (var item in nameValue)
                    {
                        var tag = _uploadVariables.FirstOrDefault(a => a.Name == item.Name);
                        if (tag != null)
                        {
                            var rpcEnable = tag.VariablePropertys[_curDevice.Id]?.Any(c =>
                            {
                                if (c.PropertyName == nameof(variablePropertys.VariableRpcEnable))
                                {
                                    if (c.Value?.GetBoolValue() == true)
                                        return true;
                                    else
                                        return false;
                                }
                                else
                                    return false;
                            });
                            if (rpcEnable == true)
                            {
                                var result = await _rpcCore.InvokeDeviceMethodAsync(ToString() + "-" + rpcrequestid, item);

                                results.Add(item.Name, result);

                            }
                            else
                            {
                                results.Add(item.Name, new("权限不足，变量不支持写入"));
                            }

                        }
                        else
                        {
                            results.Add(item.Name, new("不存在该变量"));
                        }
                    }
                    rpcResponse = new()
                    {
                        DeviceId = rpcdevicename,
                        ResponseId = rpcrequestid,
                        Method = rpcmethodname,
                        Success = !results.Any(a => !a.Value.IsSuccess),
                        Data = results.ToJson()
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
.WithPayload(rpcResponse.ToJson()).Build();
                var isConnect = await TryMqttClientAsync();
                if (isConnect.IsSuccess)
                    await _mqttClient.PublishAsync(variableMessage);
            }
            catch
            {
            }
        }

    }

    private async Task _mqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions);

        if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
        {
            _logger.LogWarning(subResult.Items
                .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                .Select(a => a.ToString()).ToJson());
        }
    }
    private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
    {
        _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task<OperResult> TryMqttClientAsync(bool reconnect = false)
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
                await lockobj.LockAsync();
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(driverPropertys.ConnectTimeOut)))
                {
                    if (_mqttClient?.IsConnected == true)
                        return OperResult.CreateSuccessResult();
                    if (_mqttClient == null)
                        return new OperResult("未初始化");
                    var result = await _mqttClient?.ConnectAsync(_mqttClientOptions, timeoutToken.Token);
                    if (result.ResultCode == MqttClientConnectResultCode.Success)
                    {

                        return OperResult.CreateSuccessResult();

                    }
                    else
                    {
                        return new OperResult(result.ReasonString);
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                lockobj.UnLock();
            }
        }
    }

    private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
    {
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}

public class IotSharpClientProperty : UpDriverPropertyBase
{
    [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";
    [DeviceProperty("端口", "")] public int Port { get; set; } = 1883;
    [DeviceProperty("Accesstoken", "")] public string Accesstoken { get; set; } = "Accesstoken";
    [DeviceProperty("连接超时时间", "")] public int ConnectTimeOut { get; set; } = 3000;
    [DeviceProperty("允许Rpc写入", "")] public bool DeviceRpcEnable { get; set; }
    [DeviceProperty("线程循环间隔", "最小500ms")] public int CycleInterval { get; set; } = 1000;
}
public class IotSharpClientVariableProperty : VariablePropertyBase
{
    [VariableProperty("启用", "")]
    public bool Enable { get; set; } = true;
    [VariableProperty("允许写入", "")]
    public bool VariableRpcEnable { get; set; } = true;
}
