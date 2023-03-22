using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using NewLife.Serialization;


using System.Collections.Concurrent;
using System.Net;
using System.Text;

using ThingsGateway.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Mqtt
{
    public class MqttServer : UpLoadBase
    {

        public MqttServer(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        [DeviceProperty("允许连接的ID(前缀)", "")] public string StartWithId { get; set; } = "ThingsGatewayId";
        [DeviceProperty("IP", "留空则全部监听")] public string IP { get; set; } = "";
        [DeviceProperty("端口", "")] public int Port { get; set; } = 1883;
        [DeviceProperty("允许Rpc写入", "")] public bool DeviceRpcEnable { get; set; }
        [DeviceProperty("Rpc写入Topic", "不允许订阅")] public string RpcWriteTopic { get; set; } = "ThingsGateway/RpcWrite";
        [DeviceProperty("Rpc返回Topic", "")] public string RpcSubTopic { get; set; } = "ThingsGateway/RpcSub";

        [DeviceProperty("变量Topic", "")] public string VariableTopic { get; set; } = "ThingsGateway/Variable";
        [DeviceProperty("设备Topic", "")] public string DeviceTopic { get; set; } = "ThingsGateway/Device";
        [DeviceProperty("循环间隔", "最小500ms")] public int CycleInterval { get; set; } = 1000;

        public override string ToString()
        {
            return $" {nameof(MqttServer)} IP:{IP} Port:{Port}";
        }

        public override async Task BeforStart()
        {
            if (_mqttServer != null)
            {
                _mqttServer.ValidatingConnectionAsync += _mqttServer_ValidatingConnectionAsync;
                _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
                _mqttServer.LoadingRetainedMessageAsync += _mqttServer_LoadingRetainedMessageAsync;
                _mqttServer.InterceptingSubscriptionAsync += _mqttServer_InterceptingSubscriptionAsync; ;
                await _mqttServer.StartAsync();

            }
        }

        private Task _mqttServer_InterceptingSubscriptionAsync(InterceptingSubscriptionEventArgs arg)
        {
            if (arg.TopicFilter.Topic == RpcWriteTopic)
            {
                arg.Response.ReasonCode = MqttSubscribeReasonCode.UnspecifiedError;
            }
            return CompletedTask.Instance;
        }

        private Task _mqttServer_LoadingRetainedMessageAsync(LoadingRetainedMessagesEventArgs arg)
        {
            //首次连接时的保留消息
            //分解List，避免超出mqtt字节大小限制
            var varData = _globalCollectDeviceData.CollectVariables.Adapt<List<VariableData>>().ChunkTrivialBetter(500);
            var devData = _globalCollectDeviceData.CollectVariables.Adapt<List<DeviceData>>().ChunkTrivialBetter(500);
            List<MqttApplicationMessage> Messages = new();
            foreach (var item in varData)
            {
                Messages.Add(new MqttApplicationMessageBuilder()
            .WithTopic($"{VariableTopic}")
            .WithPayload(item.ToJson()).Build());
            }
            foreach (var item in devData)
            {
                Messages.Add(new MqttApplicationMessageBuilder()
.WithTopic($"{DeviceTopic}")
.WithPayload(item.ToJson()).Build());
            }
            arg.LoadedRetainedMessages = Messages;
            return CompletedTask.Instance;
        }

        private GlobalCollectDeviceData _globalCollectDeviceData;
        public override void Dispose()
        {
            _globalCollectDeviceData?.CollectVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);
            _globalCollectDeviceData?.CollectDevices?.ForEach(a =>
            {
                a.DeviceStatusCahnge -= DeviceStatusCahnge;
            });
            _mqttServer?.Dispose();
        }
        private UploadDevice _curDevice { get; set; }
        RpcCore _rpcCore { get; set; }
        private MQTTnet.Server.MqttServer _mqttServer;
        protected override void Init(UploadDevice device)
        {
            _curDevice = device;
            var mqttFactory = new MqttFactory(new PrivateLogger(_logger));
            var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
                .WithDefaultEndpointBoundIPAddress(IP.IsNullOrEmpty() ? null : IPAddress.Parse(IP))
                .WithDefaultEndpointPort(Port)
                .WithDefaultEndpoint()
                .Build();
            _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);

            using var serviceScope = _scopeFactory.CreateScope();
            _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
            _rpcCore = serviceScope.ServiceProvider.GetService<RpcCore>();

            _globalCollectDeviceData.CollectDevices.ForEach(a =>
            {
                a.DeviceStatusCahnge += DeviceStatusCahnge;
            });
            _globalCollectDeviceData.CollectVariables.ForEach(a =>
            {
                a.VariableValueChange += VariableValueChange;
            });
        }
        private IntelligentConcurrentQueue<VariableData> CollectVariableRunTimes { get; set; } = new(10000);
        private IntelligentConcurrentQueue<DeviceData> CollectDeviceRunTimes { get; set; } = new(10000);

        private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
        {
            CollectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
        }

        private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
        {
            CollectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
        }
        ConcurrentDictionary<string, string> IdWithName = new();
        private async Task _mqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
        {
            if (!arg.ClientId.StartsWith(StartWithId))
            {
                arg.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                return;
            }
            using var serviceScope = _scopeFactory.CreateScope();
            var _openApiUserService = serviceScope.ServiceProvider.GetService<IOpenApiUserService>();
            var userInfo = await _openApiUserService.GetUserByAccount(arg.UserName);//获取用户信息
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
            IdWithName.AddOrUpdate(arg.ClientId, (a) => arg.UserName, (a, b) => arg.UserName);
            _logger?.LogInformation(ToString() + "-" + IdWithName[arg.ClientId] + "-客户端已连接成功");
        }
        private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
        {
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
                var result = await _rpcCore.InvokeDeviceMethod(ToString() + "-" + IdWithName[arg.ClientId], rpcData.Adapt<NameVaue>());

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
                await _mqttServer.InjectApplicationMessage(
                        new InjectedMqttApplicationMessage(variableMessage));
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
                var varList = CollectVariableRunTimes.ToListWithDequeue(10000);

                if (varList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var varData = varList.ChunkTrivialBetter(500);
                    foreach (var item in varData)
                    {
                        try
                        {
                            var message = new MqttApplicationMessageBuilder()
.WithTopic($"{VariableTopic}")
.WithPayload(item.ToJson()).Build();
                            await _mqttServer.InjectApplicationMessage(
                                    new InjectedMqttApplicationMessage(message));
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
                _logger.LogWarning(ex, ToString());
            }

            try
            {
                ////变化推送
                var devList = CollectDeviceRunTimes.ToListWithDequeue(10000);
                if (devList?.Count != 0)
                {
                    //分解List，避免超出mqtt字节大小限制
                    var varData = devList.ChunkTrivialBetter(500);
                    foreach (var item in varData)
                    {
                        try
                        {
                            var message = new MqttApplicationMessageBuilder()
                            .WithTopic($"{DeviceTopic}")
                            .WithPayload(item.ToJson()).Build();
                            await _mqttServer.InjectApplicationMessage(
                                    new InjectedMqttApplicationMessage(message));
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
                _logger.LogWarning(ex, ToString());
            }
            if (CycleInterval > 500 + 50)
            {
                await Task.Delay(CycleInterval - 500);
            }
            else
            {

            }
        }

        public override OperResult Success()
        {
            if (_mqttServer?.IsStarted == true)
            {
                return OperResult.CreateSuccessResult();
            }
            else
            {
                return new OperResult();
            }
        }
    }

}
