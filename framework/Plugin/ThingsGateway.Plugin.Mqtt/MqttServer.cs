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

using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using System.Collections.Concurrent;
using System.Net;
using System.Text;

using ThingsGateway.Admin.Application;
using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Plugin.Mqtt;
/// <summary>
/// MqttServer
/// </summary>
public class MqttServer : UpLoadBase
{

    private readonly MqttServerProperty driverPropertys = new();
    private readonly MqttClientVariableProperty variablePropertys = new();
    private ConcurrentQueue<DeviceData> _collectDeviceRunTimes = new();
    private ConcurrentQueue<VariableData> _collectVariableRunTimes = new();
    private GlobalDeviceData _globalDeviceData;

    private MQTTnet.Server.MqttServer _mqttServer;
    private RpcSingletonService _rpcCore;
    private List<DeviceVariableRunTime> _uploadVariables = new();
    /// <inheritdoc/>
    public override Type DriverDebugUIType => null;
    /// <inheritdoc/>
    public override UpDriverPropertyBase DriverPropertys => driverPropertys;
    /// <inheritdoc/>
    public override List<DeviceVariableRunTime> UploadVariables => _uploadVariables;
    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => variablePropertys;
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    public override Task AfterStopAsync()
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task BeforStartAsync(CancellationToken cancellationToken)
    {
        if (_mqttServer != null)
        {
            _mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;
            _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
            _mqttServer.LoadingRetainedMessageAsync += MqttServer_LoadingRetainedMessageAsync;
            _mqttServer.InterceptingSubscriptionAsync += MqttServer_InterceptingSubscriptionAsync; ;
            await _mqttServer.StartAsync();

        }
    }

    /// <inheritdoc/>
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
                                    new InjectedMqttApplicationMessage(message), cancellationToken);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }

                }

            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
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
                                    new InjectedMqttApplicationMessage(message), cancellationToken);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage?.LogWarning(ex);
                    }

                }


            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
        }

        try
        {
            var data = GetRetainedMessages();
            foreach (var item in data)
            {
                await _mqttServer.UpdateRetainedMessageAsync(item);
            }
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex);
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

    /// <inheritdoc/>
    public override bool IsConnected() => _mqttServer?.IsStarted == true;


    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(MqttServer)} IP:{driverPropertys.IP} Port:{driverPropertys.Port}";
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
        _globalDeviceData?.AllVariables?.ForEach(a => a.VariableValueChange -= VariableValueChange);
        _globalDeviceData?.CollectDevices?.ForEach(a =>
        {
            a.DeviceStatusChange -= DeviceStatusChange;
        });
        _uploadVariables = null;
        _collectDeviceRunTimes.Clear();
        _collectVariableRunTimes.Clear();
        _collectDeviceRunTimes = null;
        _collectVariableRunTimes = null;
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    protected override void Init(UploadDeviceRunTime device)
    {
        var log = new MqttNetEventLogger();
        log.LogMessagePublished += Log_LogMessagePublished;
        var mqttFactory = new MqttFactory(log);
        var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
            .WithDefaultEndpointBoundIPAddress(string.IsNullOrEmpty(driverPropertys.IP) ? null : IPAddress.Parse(driverPropertys.IP))
            .WithDefaultEndpointPort(driverPropertys.Port)
            .WithDefaultEndpoint()
            .Build();
        _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);

        _globalDeviceData = App.GetService<GlobalDeviceData>();
        _rpcCore = App.GetService<RpcSingletonService>();

        var tags = _globalDeviceData.AllVariables.Where(a => a.VariablePropertys.ContainsKey(device.Id))
                .Where(b => GetPropertyValue(b, nameof(variablePropertys.Enable)).GetBoolValue())
                .ToList();

        _uploadVariables = tags;

        _globalDeviceData.CollectDevices.Where(a => _uploadVariables.Select(b => b.DeviceId).Contains(a.Id)).ForEach(a =>
        {
            a.DeviceStatusChange += DeviceStatusChange;
        });
        _uploadVariables.ForEach(a =>
        {
            a.VariableValueChange += VariableValueChange;
        });

    }
    private void Log_LogMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
    {
        LogMessage.LogOut(e.LogMessage.Level, e.LogMessage.Source, e.LogMessage.Message, e.LogMessage.Exception);
    }
    private void DeviceStatusChange(CollectDeviceRunTime collectDeviceRunTime)
    {
        _collectDeviceRunTimes.Enqueue(collectDeviceRunTime.Adapt<DeviceData>());
    }

    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
    {
        if (!driverPropertys.DeviceRpcEnable || string.IsNullOrEmpty(arg.ClientId))
            return;
        if (arg.ApplicationMessage.Topic != driverPropertys.RpcWriteTopic)
            return;
        var rpcDatas = Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment).FromJsonString<MqttRpcNameVaueWithId>();
        if (rpcDatas == null)
            return;
        MqttRpcResult mqttRpcResult = new() { RpcId = rpcDatas.RpcId, Success = true };
        try
        {
            foreach (var rpcData in rpcDatas.WriteInfos)
            {

                var tag = _uploadVariables.FirstOrDefault(a => a.Name == rpcData.Key);
                if (tag != null)
                {
                    var rpcEnable = GetPropertyValue(tag, nameof(variablePropertys.VariableRpcEnable)).ToBoolean();
                    if (rpcEnable == true)
                    {

                    }
                    else
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

            var result = await _rpcCore.InvokeDeviceMethodAsync(ToString() + "-" + arg.ClientId,
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
.WithTopic($"{driverPropertys.RpcSubTopic}")
.WithPayload(mqttRpcResult.ToJsonString()).Build();
            await _mqttServer.InjectApplicationMessage(
                     new InjectedMqttApplicationMessage(variableMessage));
        }
        catch
        {
        }
    }

    private Task MqttServer_InterceptingSubscriptionAsync(InterceptingSubscriptionEventArgs arg)
    {
        if (arg.TopicFilter.Topic == driverPropertys.RpcWriteTopic)
        {
            arg.Response.ReasonCode = MqttSubscribeReasonCode.UnspecifiedError;
        }
        return CompletedTask.Instance;
    }

    private Task MqttServer_LoadingRetainedMessageAsync(LoadingRetainedMessagesEventArgs arg)
    {
        List<MqttApplicationMessage> Messages = GetRetainedMessages();
        arg.LoadedRetainedMessages = Messages;
        return CompletedTask.Instance;
    }

    private List<MqttApplicationMessage> GetRetainedMessages()
    {
        //首次连接时的保留消息
        //分解List，避免超出mqtt字节大小限制
        var varData = _globalDeviceData.AllVariables.Adapt<List<VariableData>>().ChunkTrivialBetter(driverPropertys.SplitSize);
        var devData = _globalDeviceData.CollectDevices.Adapt<List<DeviceData>>().ChunkTrivialBetter(driverPropertys.SplitSize);
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

        return Messages;
    }

    private async Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        if (!arg.ClientId.StartsWith(driverPropertys.StartWithId))
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
        _collectVariableRunTimes.Enqueue(collectVariableRunTime.Adapt<VariableData>());
    }
}
