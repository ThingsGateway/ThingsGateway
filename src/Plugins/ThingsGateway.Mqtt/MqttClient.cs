using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;

using NewLife.Serialization;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Mqtt
{
    public class MqttClient : UpLoadBase
    {

        public MqttClient(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }
        public override OperResult Success()
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
        [DeviceProperty("连接Id", "")] public string ConnectId { get; set; } = "ThingsGatewayId";
        [DeviceProperty("账号", "")] public string UserName { get; set; } = "admin";
        [DeviceProperty("密码", "")] public string Password { get; set; } = "123456";
        [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";
        [DeviceProperty("端口", "")] public int Port { get; set; } = 1883;
        [DeviceProperty("连接超时时间", "")] public int ConnectTimeOut { get; set; } = 3000;
        [DeviceProperty("允许Rpc写入", "")] public bool DeviceRpcEnable { get; set; }
        [DeviceProperty("Rpc写入Topic", "")] public string RpcWriteTopic { get; set; } = "ThingsGateway/RpcWrite";
        [DeviceProperty("Rpc返回Topic", "")] public string RpcSubTopic { get; set; } = "ThingsGateway/RpcSub";

        [DeviceProperty("数据请求RpcTopic", "这个主题接收到任何数据都会把全部的信息发送到变量/设备主题中")] public string QuestRpcTopic { get; set; } = "ThingsGateway/Quest";

        [DeviceProperty("变量Topic", "")] public string VariableTopic { get; set; } = "ThingsGateway/Variable";
        [DeviceProperty("设备Topic", "")] public string DeviceTopic { get; set; } = "ThingsGateway/Device";

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
                            .WithClientId(ConnectId)
               .WithCredentials(UserName, Password)//账密
               .WithTcpServer(IP, Port)//服务器
               .WithCleanSession(true)
               .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
               .Build();
            _mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(RpcWriteTopic);
                    })
                         .WithTopicFilter(
                    f =>
                    {
                        f.WithTopic(QuestRpcTopic);
                    })
                .Build();
            _mqttClient = mqttFactory.CreateMqttClient();
            _mqttClient.ConnectedAsync += _mqttClient_ConnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += _mqttClient_ApplicationMessageReceivedAsync;
            using var serviceScope = _scopeFactory.CreateScope();
            _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
            _rpcCore = serviceScope.ServiceProvider.GetService<RpcCore>();
            collectDeviceHostService = serviceScope.ServiceProvider.GetBackgroundService<CollectDeviceHostService>();

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
            return $" {nameof(MqttClient)} IP:{IP} Port:{Port}";
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

        private async Task AllPublish()
        {
            //保留消息
            //分解List，避免超出mqtt字节大小限制
            var varData = _globalCollectDeviceData.CollectVariables.Adapt<List<VariableData>>().ChunkTrivialBetter(500);
            var devData = _globalCollectDeviceData.CollectVariables.Adapt<List<DeviceData>>().ChunkTrivialBetter(500);
            foreach (var item in devData)
            {
                var devMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{DeviceTopic}")
.WithPayload(item.ToJson()).Build();
                await _mqttClient.PublishAsync(devMessage);
            }

            foreach (var item in varData)
            {
                var varMessage = new MqttApplicationMessageBuilder()
                .WithTopic($"{VariableTopic}")
                .WithPayload(item.ToJson()).Build();
                await _mqttClient.PublishAsync(varMessage);
            }
        }

        private async Task _mqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            if (arg.ApplicationMessage.Topic == QuestRpcTopic && arg.ApplicationMessage.Payload?.Length > 0)
            {
                await AllPublish();
                return;
            }

            if (!DeviceRpcEnable || arg.ClientId.IsNullOrEmpty())
                return;
            if (arg.ApplicationMessage.Topic != RpcWriteTopic)
                return;
            var rpcData = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload).ToJsonEntity<MqttRpcNameVaueWithId>();
            if (rpcData == null)
                return;

            MqttRpcResult mqttRpcResult = new();
            try
            {
                var result = await _rpcCore.InvokeDeviceMethod(ToString() + "-" + arg.ClientId, rpcData.Adapt<NameVaue>());

                mqttRpcResult = new() { Message = result.Message, RpcId = rpcData.RpcId, Success = result.IsSuccess };

            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, ToString());
                mqttRpcResult = new() { Message = "Failed", RpcId = rpcData.RpcId, Success = false };
            }
            try
            {
                var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{RpcSubTopic}")
.WithPayload(mqttRpcResult.ToJson()).Build();
                if (_mqttClient.IsConnected)
                    await _mqttClient.PublishAsync(variableMessage);
            }
            catch
            {
            }
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                ////变化推送
                var varList = CollectVariableRunTimes.ToListWithDequeue();
                if (varList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var varData = varList.ChunkTrivialBetter(500);
                    foreach (var item in varData)
                    {
                        try
                        {
                            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{VariableTopic}")
.WithPayload(item.ToJson()).Build();
                            if (_mqttClient.IsConnected)
                                await _mqttClient.PublishAsync(variableMessage);
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
                            var variableMessage = new MqttApplicationMessageBuilder()
                        .WithTopic($"{DeviceTopic}")
                        .WithPayload(item.ToJson()).Build();
                            if (_mqttClient.IsConnected)
                                await _mqttClient.PublishAsync(variableMessage);
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

            if (CycleInterval > 100 + 50)
            {
                await Task.Delay(CycleInterval - 100);
            }
            else
            {

            }

        }
    }


}
