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

namespace ThingsGateway.Mqtt;

public class MqttServerProperty : DriverPropertyBase
{


    [DeviceProperty("IP", "留空则全部监听")] public string IP { get; set; } = "";
    [DeviceProperty("端口", "")] public int Port { get; set; } = 1883;
    [DeviceProperty("允许连接的ID(前缀)", "")] public string StartWithId { get; set; } = "ThingsGatewayId";
    [DeviceProperty("允许Rpc写入", "")] public bool DeviceRpcEnable { get; set; }
    [DeviceProperty("循环间隔", "最小500ms")] public int CycleInterval { get; set; } = 1000;
    [DeviceProperty("设备Topic", "")] public string DeviceTopic { get; set; } = "ThingsGateway/Device";
    [DeviceProperty("变量Topic", "")] public string VariableTopic { get; set; } = "ThingsGateway/Variable";
    [DeviceProperty("Rpc返回Topic", "")] public string RpcSubTopic { get; set; } = "ThingsGateway/RpcSub";
    [DeviceProperty("Rpc写入Topic", "不允许订阅")] public string RpcWriteTopic { get; set; } = "ThingsGateway/RpcWrite";
    [DeviceProperty("设备实体脚本", "查看文档说明，为空时不起作用")] public string BigTextScriptDeviceModel { get; set; }
    [DeviceProperty("变量实体脚本", "查看文档说明，为空时不起作用")] public string BigTextScriptVariableModel { get; set; }

}
public class MqttServerVariableProperty : VariablePropertyBase
{
    [VariableProperty("启用", "")]
    public bool Enable { get; set; } = true;
    [VariableProperty("允许写入", "")]
    public bool VariableRpcEnable { get; set; } = true;
}
public class MqttServer : UpLoadBase
{

    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private UploadDevice _curDevice;
    private GlobalCollectDeviceData _globalCollectDeviceData;

    private MQTTnet.Server.MqttServer _mqttServer;

    private RpcSingletonService _rpcCore;
    private List<CollectVariableRunTime> _uploadVariables = new();

    private MqttServerProperty driverPropertys = new();

    private ConcurrentDictionary<string, string> IdWithName = new();

    private MqttServerVariableProperty variablePropertys = new();

    public MqttServer(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override DriverPropertyBase DriverPropertys => driverPropertys;
    public override List<CollectVariableRunTime> UploadVariables => _uploadVariables;
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override async Task BeforStartAsync()
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

    public override void Dispose()
    {
        _globalCollectDeviceData?.CollectVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        _globalCollectDeviceData?.CollectDevices?.ForEach(a =>
        {
            a.DeviceStatusCahnge -= DeviceStatusCahnge;
        });
        _mqttServer?.Dispose();
    }

    public override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            ////变化推送
            var varList = _collectVariableRunTimes.ToListWithDequeue();

            if (varList?.Count != 0)
            {
                //分解List，避免超出mqtt字节大小限制
                var varData = varList.ChunkTrivialBetter(500);
                foreach (var item in varData)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var message = new MqttApplicationMessageBuilder()
.WithTopic($"{driverPropertys.VariableTopic}")
.WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel)).Build();
                            await _mqttServer.InjectApplicationMessage(
                                    new InjectedMqttApplicationMessage(message));
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
            _logger.LogWarning(ex, ToString());
        }

        try
        {
            ////变化推送
            var devList = _collectDeviceRunTimes.ToListWithDequeue();
            if (devList?.Count != 0)
            {
                //分解List，避免超出mqtt字节大小限制
                var varData = devList.ChunkTrivialBetter(500);
                foreach (var item in varData)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            var message = new MqttApplicationMessageBuilder()
                        .WithTopic($"{driverPropertys.DeviceTopic}")
                        .WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel)).Build();
                            await _mqttServer.InjectApplicationMessage(
                                    new InjectedMqttApplicationMessage(message));
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
            _logger.LogWarning(ex, ToString());
        }
        if (driverPropertys.CycleInterval > 500 + 50)
        {
            await Task.Delay(driverPropertys.CycleInterval - 500);
        }
        else
        {

        }
    }

    public override OperResult IsConnected()
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

    public override string ToString()
    {
        return $" {nameof(MqttServer)} IP:{driverPropertys.IP} Port:{driverPropertys.Port}";
    }
    protected override void Init(UploadDevice device)
    {
        _curDevice = device;
        var mqttFactory = new MqttFactory(new PrivateLogger(_logger));
        var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
            .WithDefaultEndpointBoundIPAddress(driverPropertys.IP.IsNullOrEmpty() ? null : IPAddress.Parse(driverPropertys.IP))
            .WithDefaultEndpointPort(driverPropertys.Port)
            .WithDefaultEndpoint()
            .Build();
        _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);

        using var serviceScope = _scopeFactory.CreateScope();
        _globalCollectDeviceData = serviceScope.ServiceProvider.GetService<GlobalCollectDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();

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

    }

    private Task _mqttServer_InterceptingSubscriptionAsync(InterceptingSubscriptionEventArgs arg)
    {
        if (arg.TopicFilter.Topic == driverPropertys.RpcWriteTopic)
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
        .WithTopic($"{driverPropertys.VariableTopic}")
        .WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptVariableModel)).Build());
        }
        foreach (var item in devData)
        {
            Messages.Add(new MqttApplicationMessageBuilder()
.WithTopic($"{driverPropertys.DeviceTopic}")
.WithPayload(item.GetSciptListValue(driverPropertys.BigTextScriptDeviceModel)).Build());
        }
        arg.LoadedRetainedMessages = Messages;
        return CompletedTask.Instance;
    }
    private async Task _mqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        if (!arg.ClientId.StartsWith(driverPropertys.StartWithId))
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

    private void DeviceStatusCahnge(CollectDeviceRunTime collectDeviceRunTime)
    {
        _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
    {
        if (!driverPropertys.DeviceRpcEnable || arg.ClientId.IsNullOrEmpty())
            return;
        if (arg.ApplicationMessage.Topic != driverPropertys.RpcWriteTopic)
            return;
        var rpcData = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload).ToJsonEntity<MqttRpcNameVaueWithId>();
        if (rpcData == null)
            return;
        MqttRpcResult mqttRpcResult = new();
        try
        {
            var nv = rpcData.Adapt<NameValue>();
            var tag = _uploadVariables.FirstOrDefault(a => a.Name == nv.Name);
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
                    var result = await _rpcCore.InvokeDeviceMethodAsync(ToString() + "-" + IdWithName[arg.ClientId], nv);

                    mqttRpcResult = new() { Message = result.Message, RpcId = rpcData.RpcId, Success = result.IsSuccess };

                }
                else
                {
                    mqttRpcResult = new() { Message = "权限不足，变量不支持写入", RpcId = rpcData.RpcId, Success = false };
                }

            }
            else
            {
                mqttRpcResult = new() { Message = "不存在该变量", RpcId = rpcData.RpcId, Success = false };
            }
        }

        catch (Exception ex)
        {
            _logger?.LogWarning(ex, ToString());
            mqttRpcResult = new() { Message = "Failed", RpcId = rpcData.RpcId, Success = false };

        }
        try
        {
            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{driverPropertys.RpcSubTopic}")
.WithPayload(mqttRpcResult.ToJson()).Build();
            await _mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(variableMessage));
        }
        catch
        {
        }
    }

    private void VariableValueChange(CollectVariableRunTime collectVariableRunTime)
    {
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}
