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

namespace ThingsGateway.Mqtt
{
    /// <summary>
    /// 参考IotSharpClient.SDK.MQTT
    /// </summary>
    public class IotSharpClient : UpLoadBase
    {
        public IotSharpClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
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

        [DeviceProperty("Accesstoken", "")] public string Accesstoken { get; set; } = "Accesstoken";
        [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";
        [DeviceProperty("端口", "")] public int Port { get; set; } = 1883;
        [DeviceProperty("连接超时时间", "")] public int ConnectTimeOut { get; set; } = 3000;
        [DeviceProperty("允许Rpc写入", "")] public bool DeviceRpcEnable { get; set; }

        [DeviceProperty("循环间隔", "最小500ms")] public int CycleInterval { get; set; } = 1000;


        public override async Task BeforStart()
        {
            if (_mqttClient != null)
            {
                var result = await TryMqttClient();
                if (!result.IsSuccess)
                {
                    _logger?.LogWarning(ToString() + $"-连接MqttServer失败：{result.Message}");
                }
            }
        }
        public override void Dispose()
        {
            _globalCollectDeviceData?.CollectVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);

            _globalCollectDeviceData?.CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusCahnge -= DeviceStatusCahnge;
            });
            _mqttClient?.Dispose();
            _mqttClient = null;
        }


        private UploadDevice _curDevice { get; set; }
        RpcCore _rpcCore { get; set; }
        private IMqttClient _mqttClient;
        private MqttClientOptions _mqttClientOptions;
        private MqttClientSubscribeOptions _mqttSubscribeOptions;
        CollectDeviceHostService collectDeviceHostService;
        protected override void Init(UploadDevice device)
        {
            _curDevice = device;
            var mqttFactory = new MqttFactory(new PrivateLogger(_logger));
            _mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
               .WithClientId(Guid.NewGuid().ToString())
               .WithCredentials(Accesstoken)//账密
               .WithTcpServer(IP, Port)//服务器
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
            using var serviceScope = _scopeFactory.CreateScope();
            _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
            _rpcCore = serviceScope.ServiceProvider.GetService<RpcCore>();
            collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceHostService>();

            _globalCollectDeviceData.CollectDevices.ForEach(a =>
            {
                a.DeviceStatusCahnge += DeviceStatusCahnge;
            });
            _globalCollectDeviceData.CollectVariables.ForEach(a =>
            {
                a.VariableValueChange += VariableValueChange;
            });

            _ = Task.Run(
              async () =>
              {
                  await Task.Delay(ConnectTimeOut * 20);
                  bool lastIsSuccess = _mqttClient?.IsConnected == true;
                  while (_mqttClient != null)
                  {
                      try
                      {
                          var result = await TryMqttClient();

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
                          await Task.Delay(ConnectTimeOut * 10);
                      }
                  }
              });
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

        public override string ToString()
        {
            return $" {nameof(IotSharpClient)}-IP:{IP}-Port:{Port}-Accesstoken:{Accesstoken}";
        }


        private GlobalCollectDeviceData _globalCollectDeviceData;

        private ConcurrentQueue<VariableData> CollectVariableRunTimes { get; set; } = new();
        private ConcurrentQueue<DeviceData> CollectDeviceRunTimes { get; set; } = new();

        private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
        {
            CollectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
        }

        private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
        {
            CollectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
        }
        private EasyLock lockobj { get; set; } = new();
        private async Task<OperResult> TryMqttClient(bool reconnect = false)
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
                    lockobj.Lock();
                    if (_mqttClient?.IsConnected == true)
                        return OperResult.CreateSuccessResult();
                    using (var timeoutToken = new CancellationTokenSource(TimeSpan.FromSeconds(ConnectTimeOut)))
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

        /// <summary>
        /// rpcmethodname存疑，定为自定义方法，在ThingsGateway上写入变量的方法固定为"Write"
        /// </summary>
        private const string WriteMethod = "WRITE";
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
                    await SendResponse(rpcResponse);
                    return;
                }
                if (!DeviceRpcEnable)
                {
                    var rpcResponse = new RpcResponse()
                    {
                        DeviceId = rpcdevicename,
                        ResponseId = rpcrequestid,
                        Method = rpcmethodname,
                        Success = false,
                        Data = "不允许写入"
                    };
                    await SendResponse(rpcResponse);
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
                    await SendResponse(rpcResponse);
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
                            var result = await _rpcCore.InvokeDeviceMethod(ToString() + "-" + rpcrequestid, item);
                            results.Add(item.Name, result);
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
                            Data = "负载参数无法解析"
                        };
                    }

                    await SendResponse(rpcResponse);

                }


            }

            async Task SendResponse(RpcResponse rpcResponse)
            {
                try
                {
                    var topic = $"devices/{rpcResponse.DeviceId}/rpc/response/{rpcResponse.Method}/{rpcResponse.ResponseId}";

                    var variableMessage = new MqttApplicationMessageBuilder()
    .WithTopic($"{topic}")
    .WithPayload(rpcResponse.ToJson()).Build();
                    if (_mqttClient.IsConnected)
                        await _mqttClient.PublishAsync(variableMessage);
                }
                catch
                {
                }
            }

        }

        private static DateTime timeSpan = new DateTime(1970, 1, 1, 0, 0, 0);
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                ////变化推送
                var varList = CollectVariableRunTimes.ToListWithDequeue();
                if (varList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制.ChunkTrivialBetter(500)
                    var varData = varList.GroupBy(a=>a.deviceName).ToList();
                    foreach (var item in varData)
                    {
                        try
                        {
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                var variableMessage = new MqttApplicationMessageBuilder()
                                .WithTopic($"devices/{item.Key}/telemetry")
                                .WithPayload(item.ToDictionary(o => o.name, o => o.value).ToJson()).Build();
                                if (_mqttClient.IsConnected)
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
                var devList = CollectDeviceRunTimes.ToListWithDequeue();
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

            if (CycleInterval > 500 + 50)
            {
                await Task.Delay(CycleInterval - 500);
            }
            else
            {

            }

        }
    }


}
