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
using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using System.Collections.Concurrent;
using System.Net;
using System.Text;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Application;

/// <summary>
/// ManageGatewayWorker
/// </summary>
public class ManageGatewayWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly ILogger _manageLogger;
    private readonly ILogger _clientLogger;
    /// <inheritdoc cref="ManageGatewayWorker"/>
    public ManageGatewayWorker(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("ManageGatewayWorker");
        _manageLogger = loggerFactory.CreateLogger("管理网关(mqttBroker)");
        _clientLogger = loggerFactory.CreateLogger("子网关(mqttClient)");
    }
    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult ManageStatuString { get; set; } = new OperResult("初始化");
    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult ClientStatuString { get; set; } = new OperResult("初始化");

    private MqttServer _mqttServer;
    private IMqttClient _mqttClient;
    private MqttClientSubscribeOptions _mqttSubscribeOptions;


    #region worker服务
    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken token)
    {
        _logger?.LogInformation("ManageGatewayWorker启动");
        await RestartAsync();
        await base.StartAsync(token);
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken token)
    {
        _logger?.LogInformation("ManageGatewayWorker停止");
        await StopAsync();
        await base.StopAsync(token);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_mqttClient != null)
                {
                    //持续重连
                    var result = await TryMqttClientAsync(stoppingToken);
                    if (result.IsSuccess)
                    {
                        _clientLogger.LogDebug("连接正常：" + result.Message);
                    }
                    else
                    {
                        _clientLogger.LogWarning("连接错误：" + result.Message);
                    }
                }

                await Task.Delay(10000, stoppingToken);


                if (_mqttServer != null)
                {
                    //TODO:test code
                    var mqttClientStatuses = await _mqttServer.GetClientsAsync();

                    if (mqttClientStatuses.FirstOrDefault() is MqttClientStatus mqttClientStatus)
                    {
                        //获取子网关信息
                        var getClientGatewayDBResult = await GetClientGatewayDB(mqttClientStatus.Id);

                        //下发子网关配置
                        var mqttDBDownRpc = new MqttDBDownRpc();
                        mqttDBDownRpc.IsRestart = true;
                        var setClientGatewayDBResult = await SetClientGatewayDB(mqttClientStatus.Id, mqttDBDownRpc);

                    }

                }



            }
            catch (TaskCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ToString());
            }
        }
    }


    #endregion


    /// <summary>
    /// 全部重启锁
    /// </summary>
    private readonly EasyLock restartLock = new();

    /// <summary>
    /// 重启
    /// </summary>
    /// <returns></returns>
    public async Task RestartAsync()
    {
        await StopAsync();
        await StartAsync();
    }

    internal async Task StartAsync()
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (restartLock.IsWaitting)
            {
                return;
            }
            await restartLock.WaitAsync();

            await InitAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动错误");
        }
        finally
        {
            restartLock.Release();
        }
    }
    internal async Task StopAsync()
    {
        try
        {
            //重启操作在未完全之前直接取消
            if (restartLock.IsWaitting)
            {
                return;
            }
            await restartLock.WaitAsync();
            _mqttClient?.SafeDispose();
            _mqttServer?.SafeDispose();
            _mqttClient = null;
            _mqttServer = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止错误");
        }
        finally
        {
            restartLock.Release();
        }
    }


    #region 核心实现
    ManageGatewayConfig _manageGatewayConfig;
    ClientGatewayConfig _clientGatewayConfig;

    private MqttClientOptions _mqttClientOptions;
    /// <summary>
    /// 初始化
    /// </summary>
    private async Task InitAsync()
    {
        try
        {
            _manageGatewayConfig = App.GetConfig<ManageGatewayConfig>("ManageGatewayConfig");
            if (_manageGatewayConfig?.Enable != true)
            {
                ManageStatuString = new OperResult($"已退出：不启用管理功能");
                _manageLogger.LogWarning("已退出：不启用管理功能");
            }
            else
            {
                var mqttFactory = new MqttFactory(new MqttNetLogger(_manageLogger));
                var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
                    .WithDefaultEndpointBoundIPAddress(string.IsNullOrEmpty(_manageGatewayConfig.MqttBrokerIP) ? null : IPAddress.Parse(_manageGatewayConfig.MqttBrokerIP))
                    .WithDefaultEndpointPort(_manageGatewayConfig.MqttBrokerPort)
                    .WithDefaultEndpoint()
                    .Build();
                _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
                if (_mqttServer != null)
                {
                    _mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;//认证
                    _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;//认证

                    await _mqttServer.StartAsync();
                }

            }
        }
        catch (Exception ex)
        {
            _manageLogger.LogError(ex, "初始化失败");
        }

        try
        {
            _clientGatewayConfig = App.GetConfig<ClientGatewayConfig>("ClientGatewayConfig");
            if (_clientGatewayConfig?.Enable != true)
            {
                ClientStatuString = new OperResult($"已退出：不启用子网关功能");
                _clientLogger.LogWarning("已退出：不启用子网关功能");
            }
            else
            {
                var mqttFactory = new MqttFactory(new MqttNetLogger(_clientLogger));
                _mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
                  .WithCredentials(_clientGatewayConfig.UserName, _clientGatewayConfig.Password)//账密
                  .WithTcpServer(_clientGatewayConfig.MqttBrokerIP, _clientGatewayConfig.MqttBrokerPort)//服务器
                  .WithClientId(_clientGatewayConfig.GatewayId)
                  .WithCleanSession(true)
                  .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
                  .WithoutThrowOnNonSuccessfulConnectResponse()
                  .Build();
                _mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic(GetRpcTopic(_clientGatewayConfig.GatewayId, _clientGatewayConfig.PrivateWriteRpcTopic));
                            f.WithAtMostOnceQoS();
                        })
                      .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic(GetRpcTopic(_clientGatewayConfig.GatewayId, _clientGatewayConfig.DBDownTopic));
                            f.WithAtMostOnceQoS();
                        })
                                        .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic(GetRpcTopic(_clientGatewayConfig.GatewayId, _clientGatewayConfig.DBUploadTopic));
                            f.WithAtMostOnceQoS();
                        })
                    .Build();
                _mqttClient = mqttFactory.CreateMqttClient();
                _mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
                _mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
                await TryMqttClientAsync(CancellationToken.None);

            }
        }
        catch (Exception ex)
        {
            _clientLogger.LogError(ex, "初始化失败");
        }

    }

    private EasyLock clientLock = new();
    private async Task<OperResult> TryMqttClientAsync(CancellationToken token)
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
                await clientLock.WaitAsync();
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                using var timeoutToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(5000));
                using CancellationTokenSource StoppingToken = CancellationTokenSource.CreateLinkedTokenSource(token, timeoutToken.Token);
                if (_mqttClient?.IsConnected == true)
                    return OperResult.CreateSuccessResult();
                if (_mqttClient == null)
                {
                    return new OperResult("未初始化");
                }
                var result = await _mqttClient?.ConnectAsync(_mqttClientOptions, StoppingToken.Token);
                if (result.ResultCode == MqttClientConnectResultCode.Success)
                {
                    return OperResult.CreateSuccessResult();
                }
                else
                {
                    return new OperResult(result.ReasonString);
                }
            }
            catch (Exception ex)
            {
                return new OperResult(ex);
            }
            finally
            {
                clientLock.Release();
            }
        }
    }

    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        if (args.ApplicationMessage.Topic == GetRpcTopic(_clientGatewayConfig.GatewayId, _clientGatewayConfig.DBUploadTopic))
        {

            await DBUploadTopicMethod(args);
            return;
        }
        if (args.ApplicationMessage.Topic == GetRpcTopic(_clientGatewayConfig.GatewayId, _clientGatewayConfig.DBDownTopic))
        {

            await DBDownTopicMethod(args);

            return;
        }
    }

    private async Task DBUploadTopicMethod(MqttApplicationMessageReceivedEventArgs args)
    {
        MqttDBUploadRpcResult result = new();
        var collectDeviceService = App.GetService<CollectDeviceService>();
        var variableService = App.GetService<VariableService>();
        var uploadDeviceService = App.GetService<UploadDeviceService>();
        result.CollectDevices = collectDeviceService.GetCacheList(false);
        result.DeviceVariables = await variableService.GetListAsync();
        result.UploadDevices = uploadDeviceService.GetCacheList(false);

        var variableMessage = new MqttApplicationMessageBuilder()
.WithTopic(GetRpcReturnTopic(args.ApplicationMessage.Topic))
.WithPayload(result.ToJsonString()).Build();
        if (_mqttClient.IsConnected)
            await _mqttClient.PublishAsync(variableMessage);
    }
    private async Task DBDownTopicMethod(MqttApplicationMessageReceivedEventArgs args)
    {
        var mqttDBRpc = args.ApplicationMessage.PayloadSegment.Count > 0 ? Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).FromJsonString<MqttDBDownRpc>() : null;
        if (mqttDBRpc != null)
        {
            OperResult result = new();
            var collectDeviceService = App.GetService<CollectDeviceService>();
            var variableService = App.GetService<VariableService>();
            var uploadDeviceService = App.GetService<UploadDeviceService>();

            collectDeviceService.Context = variableService.Context = uploadDeviceService.Context;
            var itenant = collectDeviceService.Context.AsTenant();
            //事务
            var dbResult = await itenant.UseTranAsync(async () =>
            {
                if (mqttDBRpc.IsCollectDevicesFullUp)
                {
                    await collectDeviceService.AsDeleteable().ExecuteCommandAsync();
                    await collectDeviceService.InsertRangeAsync(mqttDBRpc.CollectDevices);
                }
                else
                {
                    await collectDeviceService.Context.Storageable(mqttDBRpc.CollectDevices).ExecuteCommandAsync();
                }
                if (mqttDBRpc.IsUploadDevicesFullUp)
                {
                    await uploadDeviceService.AsDeleteable().ExecuteCommandAsync();
                    await uploadDeviceService.InsertRangeAsync(mqttDBRpc.UploadDevices);
                }
                else
                {
                    await uploadDeviceService.Context.Storageable(mqttDBRpc.UploadDevices).ExecuteCommandAsync();
                }
                if (mqttDBRpc.IsDeviceVariablesFullUp)
                {
                    await variableService.AsDeleteable().ExecuteCommandAsync();
                    await variableService.InsertRangeAsync(mqttDBRpc.DeviceVariables);
                }
                else
                {
                    await variableService.Context.Storageable(mqttDBRpc.DeviceVariables).ExecuteCommandAsync();
                }
            });
            if (dbResult.IsSuccess)//如果成功了
            {
                result = OperResult.CreateSuccessResult();
                if (mqttDBRpc.IsRestart)
                {
                    await ServiceHelper.GetBackgroundService<CollectDeviceWorker>().RestartDeviceThreadAsync();
                }
            }
            else
            {
                //写日志
                result.Message = dbResult.ErrorMessage;
            }

            var variableMessage = new MqttApplicationMessageBuilder()
    .WithTopic(GetRpcReturnTopic(args.ApplicationMessage.Topic))
    .WithPayload(result.ToJsonString()).Build();
            if (_mqttClient.IsConnected)
                await _mqttClient.PublishAsync(variableMessage);
        }
    }

    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs args)
    {
        var subResult = await _mqttClient.SubscribeAsync(_mqttSubscribeOptions);
        if (subResult.Items.Any(a => a.ResultCode > (MqttClientSubscribeResultCode)10))
        {
            _clientLogger?.LogWarning("订阅失败-" + subResult.Items
                .Where(a => a.ResultCode > (MqttClientSubscribeResultCode)10)
                .Select(a =>
                new
                {
                    Topic = a.TopicFilter.Topic,
                    ResultCode = a.ResultCode.ToString()
                }
                )
                .ToJsonString()
                );
        }
    }

    readonly ConcurrentDictionary<string, WaitDataAsync<byte[]>> _waitingCalls = new();


    private string GetRpcReturnTopic(string gatewayId, string topic)
    {
        var responseTopic = $"{gatewayId}/{topic}/rpcReturn";
        return responseTopic;
    }
    private string GetRpcReturnTopic(string requestTopic)
    {
        var responseTopic = $"{requestTopic}Return";
        return responseTopic;
    }
    private string GetRpcTopic(string gatewayId, string topic)
    {
        var requestTopic = $"{gatewayId}/{topic}/rpc";
        return requestTopic;
    }


    private Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs eventArgs)
    {
        if (!_waitingCalls.TryRemove(eventArgs.ApplicationMessage.Topic, out var awaitable))
        {
            return CompletedTask.Instance;
        }

        var payloadBuffer = eventArgs.ApplicationMessage.PayloadSegment.ToArray();
        awaitable.Set(payloadBuffer);

        return CompletedTask.Instance;
    }

    private Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        if (_manageGatewayConfig.UserName != arg.UserName)
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return CompletedTask.Instance;
        }
        if (_manageGatewayConfig.Password != arg.Password)
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return CompletedTask.Instance;
        }
        _manageLogger?.LogInformation(ToString() + "-" + arg.ClientId + "-客户端已连接成功");
        return CompletedTask.Instance;
    }
    #endregion


    /// <summary>
    /// RPC请求子网关并返回，需要传入子网关ID，作为Topic参数一部分
    /// </summary>
    /// <returns></returns>
    public async Task<byte[]> RpcDataExecuteAsync(string gatewayId, string topic, byte[] payload, int timeOut, MqttQualityOfServiceLevel qualityOfServiceLevel, CancellationToken token = default)
    {
        var responseTopic = GetRpcReturnTopic(gatewayId, topic);
        var requestTopic = GetRpcTopic(gatewayId, topic);

        try
        {
            using WaitDataAsync<byte[]> waitDataAsync = new();
            if (!_waitingCalls.TryAdd(responseTopic, waitDataAsync))
            {
                throw new InvalidOperationException();
            }
            waitDataAsync.SetCancellationToken(token);

            //请求子网关的数据
            var message = new MqttApplicationMessageBuilder().WithTopic(requestTopic).WithPayload(payload).Build();
            await _mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(message), token);

            var result = await waitDataAsync.WaitAsync(timeOut);
            switch (result)
            {
                case WaitDataStatus.SetRunning:
                    return waitDataAsync.WaitResult;
                case WaitDataStatus.Overtime:
                    throw new TimeoutException();
                case WaitDataStatus.Canceled:
                    {
                        throw new Exception("等待已终止。可能是客户端已掉线，或者被注销。");
                    }
                case WaitDataStatus.Default:
                case WaitDataStatus.Disposed:
                default:
                    throw new Exception(ThingsGatewayStatus.UnknownError.GetDescription());
            }
        }
        finally
        {
            _waitingCalls.Remove(responseTopic);

        }
    }

    /// <summary>
    /// 获取子网关的配置信息
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<MqttDBUploadRpcResult>> GetClientGatewayDB(string gatewayId, int timeOut = 3000, CancellationToken token = default)
    {
        try
        {
            var buffer = Encoding.UTF8.GetBytes(string.Empty);
            var response = await RpcDataExecuteAsync(gatewayId, _clientGatewayConfig.DBUploadTopic, buffer, timeOut, MqttQualityOfServiceLevel.AtMostOnce, token);
            var data = Encoding.UTF8.GetString(response).FromJsonString<MqttDBUploadRpcResult>();
            return OperResult.CreateSuccessResult(data);
        }
        catch (Exception ex)
        {
            return new OperResult<MqttDBUploadRpcResult>(ex);
        }

    }

    /// <summary>
    /// 下载配置信息到子网关
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<OperResult>> SetClientGatewayDB(string gatewayId, MqttDBDownRpc mqttDBRpc, int timeOut = 3000, CancellationToken token = default)
    {
        try
        {
            var buffer = Encoding.UTF8.GetBytes(mqttDBRpc?.ToJsonString() ?? string.Empty);
            var response = await RpcDataExecuteAsync(gatewayId, _clientGatewayConfig.DBDownTopic, buffer, timeOut, MqttQualityOfServiceLevel.AtMostOnce, token);
            var data = Encoding.UTF8.GetString(response).FromJsonString<OperResult>();
            return OperResult.CreateSuccessResult(data);
        }
        catch (Exception ex)
        {
            return new OperResult<OperResult>(ex);
        }

    }

}
