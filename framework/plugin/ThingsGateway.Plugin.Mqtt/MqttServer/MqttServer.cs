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

using Furion;

using Mapster;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using System.Collections.Concurrent;
using System.Text;

using ThingsGateway.Admin.Application;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Foundation.Sockets;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttServer
/// </summary>
public class MqttServer : UpLoadBase
{
    private readonly MqttServerProperty _driverPropertys = new();
    private readonly MqttClientVariableProperty _variablePropertys = new();
    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();

    private MQTTnet.Server.MqttServer _mqttServer;
    private volatile bool success = true;

    private volatile bool updateRetainedSuccess = true;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => null;

    /// <inheritdoc/>
    public override DriverPropertyBase DriverPropertys => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override IReadWrite _readWrite => null;

    public override void Init(DeviceRunTime device)
    {
        base.Init(device);
        if (_driverPropertys.IsAllVariable)
        {
            device.DeviceVariableRunTimes = _globalDeviceData.AllVariables;
            CollectDevices = _globalDeviceData.CollectDevices.ToList();
        }
        else
        {
            var variables = _globalDeviceData.AllVariables.Where(a =>
  a.VariablePropertys.ContainsKey(device.Id)).ToList();
            device.DeviceVariableRunTimes = variables;
            CollectDevices = _globalDeviceData.CollectDevices.Where(a => device.DeviceVariableRunTimes.Select(b => b.DeviceId).Contains(a.Id)).ToList();
        }
    }

    /// <inheritdoc/>
    public override bool IsConnected() => _mqttServer?.IsStarted == true;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(MqttServer)} IP:{_driverPropertys.IP} Port:{_driverPropertys.Port}";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_mqttServer != null)
        {
            _mqttServer.ValidatingConnectionAsync -= MqttServer_ValidatingConnectionAsync;
            _mqttServer.InterceptingPublishAsync -= MqttServer_InterceptingPublishAsync;
            _mqttServer.LoadingRetainedMessageAsync -= MqttServer_LoadingRetainedMessageAsync;
            _mqttServer.InterceptingSubscriptionAsync -= MqttServer_InterceptingSubscriptionAsync; ;
            _mqttServer?.SafeDispose();
        }
        _webHost?.Dispose();
        _globalDeviceData?.AllVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        _globalDeviceData?.CollectDevices?.ForEach(a =>
        {
            a.DeviceStatusChange -= DeviceStatusChange;
        });
        _collectDeviceRunTimes.Clear();
        _collectVariableRunTimes.Clear();
        base.Dispose(disposing);
    }

    private IWebHost _webHost { get; set; }

    protected override void Init(ISenderClient client = null)
    {
        //var mqttFactory = new MqttFactory();
        //var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
        //    .WithDefaultEndpointBoundIPAddress(string.IsNullOrEmpty(_driverPropertys.IP) ? null : IPAddress.Parse(_driverPropertys.IP))
        //    .WithDefaultEndpointPort(_driverPropertys.Port)
        //    .WithDefaultEndpoint()
        //    .Build();
        //_mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
        var configuration = new ConfigurationBuilder()
          .SetBasePath(Directory)
          .Build();
        var webBuilder = new WebHostBuilder()
     .UseKestrel(
                             o =>
                             {
                                 o.ListenAnyIP(_driverPropertys.Port, l => l.UseMqtt());
                                 o.ListenAnyIP(_driverPropertys.WebSocketPort);
                             });
        webBuilder.UseStartup<MqttServerStartup>();
        _webHost = webBuilder.UseConfiguration(configuration)
           .Build();

        _mqttServer = _webHost.Services.GetService<MqttHostedServer>();

        CollectDevices.Where(a => CurrentDevice.DeviceVariableRunTimes.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusChange += DeviceStatusChange;
        });
        CurrentDevice.DeviceVariableRunTimes.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
        });
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _ = _webHost.StartAsync(cancellationToken);
        if (_mqttServer != null)
        {
            _mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;
            _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
            _mqttServer.LoadingRetainedMessageAsync += MqttServer_LoadingRetainedMessageAsync;
            _mqttServer.InterceptingSubscriptionAsync += MqttServer_InterceptingSubscriptionAsync; ;
            await _mqttServer.StartAsync();
        }
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        //获取设备连接状态
        if (IsConnected())
        {
            //更新设备活动时间
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 0);
        }
        else
        {
            CurrentDevice.SetDeviceStatus(DateTimeExtensions.CurrentDateTime, 999);
        }

        ////变化推送
        var varList = _collectVariableRunTimes.ToListWithDequeue();

        if (varList?.Count != 0)
        {
            //分解List，避免超出mqtt字节大小限制
            var varData = varList.ChunkBetter(_driverPropertys.SplitSize);
            foreach (var item in varData)
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var message = new MqttApplicationMessageBuilder()
.WithTopic($"{_driverPropertys.VariableTopic}")
.WithPayload(item.GetSciptListValue(_driverPropertys.BigTextScriptVariableModel)).Build();
                        await _mqttServer.InjectApplicationMessage(
                                new InjectedMqttApplicationMessage(message), cancellationToken);
                    }
                    else
                    {
                        break;
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }
        }

        ////变化推送
        var devList = _collectDeviceRunTimes.ToListWithDequeue();
        if (devList?.Count != 0)
        {
            //分解List，避免超出mqtt字节大小限制
            var varData = devList.ChunkBetter(_driverPropertys.SplitSize);
            foreach (var item in varData)
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var message = new MqttApplicationMessageBuilder()
                    .WithTopic($"{_driverPropertys.DeviceTopic}")
                    .WithPayload(item.GetSciptListValue(_driverPropertys.BigTextScriptDeviceModel)).Build();
                        await _mqttServer.InjectApplicationMessage(
                                new InjectedMqttApplicationMessage(message), cancellationToken);
                    }
                    else
                    {
                        break;
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    if (success)
                        LogMessage?.LogWarning(ex);
                    success = false;
                }
            }
        }

        try
        {
            var data = GetRetainedMessages();
            foreach (var item in data)
            {
                await _mqttServer.UpdateRetainedMessageAsync(item);
            }
            updateRetainedSuccess = true;
        }
        catch (Exception ex)
        {
            if (updateRetainedSuccess)
                LogMessage?.LogWarning(ex);
            updateRetainedSuccess = false;
        }
        await Delay(_driverPropertys.CycleInterval, cancellationToken);
    }

    private void DeviceStatusChange(DeviceRunTime collectDeviceRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private List<MqttApplicationMessage> GetRetainedMessages()
    {
        //首次连接时的保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = _globalDeviceData.AllVariables.Adapt<List<VariableData>>().ChunkBetter(_driverPropertys.SplitSize);
        var devData = _globalDeviceData.CollectDevices.Adapt<List<DeviceData>>().ChunkBetter(_driverPropertys.SplitSize);
        List<MqttApplicationMessage> Messages = new();
        foreach (var item in varData)
        {
            Messages.Add(new MqttApplicationMessageBuilder()
        .WithTopic($"{_driverPropertys.VariableTopic}")
        .WithPayload(item.GetSciptListValue(_driverPropertys.BigTextScriptVariableModel)).Build());
        }
        foreach (var item in devData)
        {
            Messages.Add(new MqttApplicationMessageBuilder()
.WithTopic($"{_driverPropertys.DeviceTopic}")
.WithPayload(item.GetSciptListValue(_driverPropertys.BigTextScriptDeviceModel)).Build());
        }

        return Messages;
    }

    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
    {
        if (!_driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(arg.ClientId))
            return;
        if (arg.ApplicationMessage.Topic != _driverPropertys.RpcWriteTopic)
            return;
        var rpcDatas = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment).FromJsonString<MqttRpcNameVaueWithId>();
        if (rpcDatas == null)
            return;
        MqttRpcResult mqttRpcResult = new() { RpcId = rpcDatas.RpcId, Success = true };
        try
        {
            foreach (var rpcData in rpcDatas.WriteInfos)
            {
                var tag = CurrentDevice.DeviceVariableRunTimes.FirstOrDefault(a => a.Name == rpcData.Key);
                if (tag != null)
                {
                    var rpcEnable = tag.GetPropertyValue(DeviceId, nameof(_variablePropertys.VariableRpcEnable))?.Value?.ToBool() == true;
                    if (!rpcEnable)
                    {
                        mqttRpcResult.Success = false;
                        mqttRpcResult.Message.Add(rpcData.Key, new OperResult("权限不足，变量不支持写入"));
                    }
                }
                else
                {
                    mqttRpcResult.Success = false;
                    mqttRpcResult.Message.Add(rpcData.Key, new OperResult("不存在该变量"));
                }
            }

            var result = await RpcSingletonService.InvokeDeviceMethodAsync(ToString() + "-" + arg.ClientId,
                rpcDatas.WriteInfos.Where(
                a => !mqttRpcResult.Message.Any(b => b.Key == a.Key)).ToDictionary(a => a.Key, a => a.Value));

            mqttRpcResult.Message.AddRange(result);
            mqttRpcResult.Success = !mqttRpcResult.Message.Any(a => !a.Value.IsSuccess);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }
        try
        {
            var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic($"{_driverPropertys.RpcSubTopic}")
.WithPayload(mqttRpcResult.ToJsonString()).Build();
            await _mqttServer.InjectApplicationMessage(
                     new InjectedMqttApplicationMessage(variableMessage));
        }
        catch
        {
        }
    }

    private async Task MqttServer_InterceptingSubscriptionAsync(InterceptingSubscriptionEventArgs arg)
    {
        if (arg.TopicFilter.Topic == _driverPropertys.RpcWriteTopic)
        {
            arg.Response.ReasonCode = MqttSubscribeReasonCode.UnspecifiedError;
        }
        await CompletedTask.Instance;
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
        var _openApiUserService = App.GetService<IOpenApiUserService>();
        var userInfo = await _openApiUserService.GetUserByAccountAsync(arg.UserName);//获取用户信息
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
        LogMessage?.LogInformation($"{ToString()}-{arg.ClientId}-客户端已连接成功");
    }

    private void VariableValueChange(DeviceVariableRunTime collectVariableRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}