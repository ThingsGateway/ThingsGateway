#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Mapster;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;




using System.Collections.Concurrent;
using System.Net;
using System.Text;

using ThingsGateway.Application;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Enumerator;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Mqtt;
public class MqttServer : UpLoadBase
{

    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private GlobalDeviceData _globalDeviceData;

    private MQTTnet.Server.MqttServer _mqttServer;
    public override Type DriverDebugUIType => null;

    private RpcSingletonService _rpcCore;
    private List<DeviceVariableRunTime> _uploadVariables = new();

    private MqttServerProperty driverPropertys = new();

    private ConcurrentDictionary<string, string> IdWithName = new();

    private MqttServerVariableProperty variablePropertys = new();

    public MqttServer(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;
    public override List<DeviceVariableRunTime> UploadVariables => _uploadVariables;
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
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

    protected override void Dispose(bool disposing)
    {
        if (_mqttServer != null)
        {
            _mqttServer.ValidatingConnectionAsync -= _mqttServer_ValidatingConnectionAsync;
            _mqttServer.InterceptingPublishAsync -= MqttServer_InterceptingPublishAsync;
            _mqttServer.LoadingRetainedMessageAsync -= _mqttServer_LoadingRetainedMessageAsync;
            _mqttServer.InterceptingSubscriptionAsync -= _mqttServer_InterceptingSubscriptionAsync; ;
            _mqttServer?.SafeDispose();
        }
        _globalDeviceData?.AllVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        _globalDeviceData?.CollectDevices?.ForEach(a =>
        {
            a.DeviceStatusCahnge -= DeviceStatusCahnge;
        });
        _uploadVariables = null;
        _collectDeviceRunTimes.Clear();
        _collectVariableRunTimes.Clear();
        _collectDeviceRunTimes = null;
        _collectVariableRunTimes = null;
        base.Dispose(disposing);
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
                var varData = varList.ChunkTrivialBetter(driverPropertys.SplitSize);
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
                var varData = devList.ChunkTrivialBetter(driverPropertys.SplitSize);
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
        if (driverPropertys.CycleInterval > UploadDeviceThread.CycleInterval + 50)
        {
            try
            {
                await Task.Delay(driverPropertys.CycleInterval - UploadDeviceThread.CycleInterval, cancellationToken);
            }
            catch
            {
            }
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
    protected override void Init(UploadDeviceRunTime device)
    {
        var mqttFactory = new MqttFactory(new PrivateLogger(_logger));
        var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
            .WithDefaultEndpointBoundIPAddress(driverPropertys.IP.IsNullOrEmpty() ? null : IPAddress.Parse(driverPropertys.IP))
            .WithDefaultEndpointPort(driverPropertys.Port)
            .WithDefaultEndpoint()
            .Build();
        _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);

        var serviceScope = _scopeFactory.CreateScope();
        _globalDeviceData = serviceScope.ServiceProvider.GetService<GlobalDeviceData>();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();

        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
                .Where(b => GetPropertyValue(b, nameof(variablePropertys.Enable)).GetBoolValue())
                .ToList();

        _uploadVariables = tags;

        _globalDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
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
        var varData = _globalDeviceData.AllVariables.Adapt<List<VariableData>>().ChunkTrivialBetter(driverPropertys.SplitSize);
        var devData = _globalDeviceData.AllVariables.Adapt<List<DeviceData>>().ChunkTrivialBetter(driverPropertys.SplitSize);
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
        var serviceScope = _scopeFactory.CreateScope();
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
        var rpcData = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment).FromJson<MqttRpcNameVaueWithId>();
        if (rpcData == null)
            return;
        MqttRpcResult mqttRpcResult = new();
        try
        {
            var nv = rpcData.Adapt<KeyValuePair<string, string>>();
            var tag = _uploadVariables.FirstOrDefault(a => a.Name == nv.Key);
            if (tag != null)
            {
                var rpcEnable = GetPropertyValue(tag, nameof(variablePropertys.VariableRpcEnable)).ToBoolean();
                if (rpcEnable == true)
                {
                    var result = await _rpcCore.InvokeDeviceMethodAsync(ToString() + "-" + IdWithName[arg.ClientId], nv, CancellationToken.None);

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

    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}
