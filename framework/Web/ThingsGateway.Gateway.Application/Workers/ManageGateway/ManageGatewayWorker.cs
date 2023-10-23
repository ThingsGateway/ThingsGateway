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

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;

using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ManageGatewayWorker
/// </summary>
public class ManageGatewayWorker : BackgroundService
{
    private readonly ILogger _clientLogger;
    private readonly ILogger _logger;
    private readonly ILogger _manageLogger;
    /// <summary>
    /// 全部重启锁
    /// </summary>
    private readonly EasyLock restartLock = new();

    private IMqttClient _mqttClient;

    private MqttServer _mqttServer;

    private MqttClientSubscribeOptions _mqttSubscribeOptions;

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
    public OperResult ClientStatuString { get; set; } = new OperResult("初始化");

    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult ManageStatuString { get; set; } = new OperResult("初始化");

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
                        _clientLogger.LogDebug($"连接正常：{result.Message}");
                        ClientStatuString.ErrorCode = 0;
                        ClientStatuString.Message = "连接正常：" + result.Message;

                    }
                    else
                    {
                        if (ClientStatuString.IsSuccess)
                        {
                            _clientLogger.LogWarning($"连接错误：{result.Message}");
                        }
                        ClientStatuString.ErrorCode = 999;
                        ClientStatuString.Message = $"连接错误：{result.Message}";
                    }
                }

                await Task.Delay(10000, stoppingToken);

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

    #region public
    /// <summary>
    /// 获取子网关的配置信息
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<MqttDBUploadRpcResult>> GetClientGatewayDBAsync(string gatewayId, int timeOut = 3000, CancellationToken token = default)
    {
        try
        {
            var buffer = Encoding.UTF8.GetBytes(string.Empty);
            var response = await RpcDataExecuteAsync(gatewayId, ClientGatewayConfig.DBUploadTopic, buffer, timeOut, MqttQualityOfServiceLevel.AtMostOnce, token);
            var data = Encoding.UTF8.GetString(response).FromJsonString<MqttDBUploadRpcResult>();
            return OperResult.CreateSuccessResult(data);
        }
        catch (Exception ex)
        {
            return new OperResult<MqttDBUploadRpcResult>(ex);
        }

    }

    /// <summary>
    /// 重启
    /// </summary>
    /// <returns></returns>
    public async Task RestartAsync()
    {
        await StopAsync();
        await StartAsync();
    }

    /// <summary>
    /// 下载配置信息到子网关
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult> SetClientGatewayDBAsync(string gatewayId, MqttDBDownRpc mqttDBRpc, int timeOut = 3000, CancellationToken token = default)
    {
        try
        {
            var buffer = Encoding.UTF8.GetBytes(mqttDBRpc?.ToJsonString() ?? string.Empty);
            var response = await RpcDataExecuteAsync(gatewayId, ClientGatewayConfig.DBDownTopic, buffer, timeOut, MqttQualityOfServiceLevel.AtMostOnce, token);
            var data = Encoding.UTF8.GetString(response).FromJsonString<OperResult>();
            return data;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }

    }

    /// <summary>
    /// 写入变量到子网关
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<ManageMqttRpcResult>> WriteVariableAsync(ManageMqttRpcFrom manageMqttRpcFrom, int timeOut = 3000, CancellationToken token = default)
    {
        try
        {
            var payload = Encoding.UTF8.GetBytes(manageMqttRpcFrom?.ToJsonString() ?? string.Empty);
            var requestTopic = ManageGatewayConfig.WriteRpcTopic;
            var responseTopic = GetRpcReturnTopic(ManageGatewayConfig.WriteRpcTopic);
            var key = GetRpcReturnIdTopic(manageMqttRpcFrom.GatewayId, requestTopic, manageMqttRpcFrom.RpcId);

            ManageMqttRpcResult result = await RpcWriteExecuteAsync(timeOut, payload, requestTopic, key, token);

            return OperResult.CreateSuccessResult(result);
        }
        catch (Exception ex)
        {
            return new OperResult<ManageMqttRpcResult>(ex);
        }

    }

    /// <summary>
    /// 获取子网关列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<MqttClientStatus>> GetClientGatewayAsync()
    {
        if (_mqttServer != null)
        {
            var data = await _mqttServer.GetClientsAsync();
            return data.ToList();
        }
        else
        {
            return new List<MqttClientStatus>();
        }
    }


    #endregion




    #region RPC实现

    readonly ConcurrentDictionary<string, WaitDataAsync<byte[]>> _waitingCalls = new();
    readonly ConcurrentDictionary<string, WaitDataAsync<ManageMqttRpcResult>> _writerRpcResultWaitingCalls = new();
    private readonly EasyLock clientLock = new();


    private async Task<ManageMqttRpcResult> RpcWriteExecuteAsync(int timeOut, byte[] payload, string requestTopic, string key, CancellationToken token)
    {
        try
        {
            using WaitDataAsync<ManageMqttRpcResult> waitDataAsync = new();
            if (!_writerRpcResultWaitingCalls.TryAdd(key, waitDataAsync))
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
                    throw new Exception("未知错误");
            }
        }
        finally
        {
            _writerRpcResultWaitingCalls.Remove(key);

        }
    }


    /// <summary>
    /// RPC请求子网关并返回，需要传入子网关ID，作为Topic参数一部分
    /// </summary>
    /// <returns></returns>
    private async Task<byte[]> RpcDataExecuteAsync(string gatewayId, string topic, byte[] payload, int timeOut, MqttQualityOfServiceLevel qualityOfServiceLevel, CancellationToken token = default)
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
                    throw new Exception("未知错误");
            }
        }
        finally
        {
            _waitingCalls.Remove(responseTopic);

        }
    }


    #endregion

    #region 核心实现

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
    /// <summary>
    /// 初始化
    /// </summary>
    private async Task InitAsync()
    {
        try
        {
            ManageGatewayConfig = App.GetConfig<ManageGatewayConfig>("ManageGatewayConfig");
            if (ManageGatewayConfig?.Enable != true)
            {
                ManageStatuString = new OperResult($"已退出：不启用管理功能");
                _manageLogger.LogWarning("已退出：不启用管理功能");
            }
            else
            {
                var log = new MqttNetEventLogger();
                log.LogMessagePublished += ServerLog_LogMessagePublished;
                var mqttFactory = new MqttFactory(log);
                var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
                    .WithDefaultEndpointBoundIPAddress(string.IsNullOrEmpty(ManageGatewayConfig.MqttBrokerIP) ? null : IPAddress.Parse(ManageGatewayConfig.MqttBrokerIP))
                    .WithDefaultEndpointPort(ManageGatewayConfig.MqttBrokerPort)
                    .WithDefaultEndpoint()
                    .Build();
                _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
                if (_mqttServer != null)
                {
                    _mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;//认证
                    _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;//消息

                    await _mqttServer.StartAsync();
                }
                ManageStatuString = OperResult.CreateSuccessResult();

            }
        }
        catch (Exception ex)
        {
            _manageLogger.LogError(ex, "初始化失败");
            ManageStatuString = new($"初始化失败-{ex.Message}");
        }

        try
        {
            ClientGatewayConfig = App.GetConfig<ClientGatewayConfig>("ClientGatewayConfig");
            if (ClientGatewayConfig?.Enable != true)
            {
                ClientStatuString = new OperResult($"已退出：不启用子网关功能");
                _clientLogger.LogWarning("已退出：不启用子网关功能");
            }
            else
            {
                var log = new MqttNetEventLogger();
                log.LogMessagePublished += Log_LogMessagePublished;
                var mqttFactory = new MqttFactory(log);
                _mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
                  .WithCredentials(ClientGatewayConfig.UserName, ClientGatewayConfig.Password)//账密
                  .WithTcpServer(ClientGatewayConfig.MqttBrokerIP, ClientGatewayConfig.MqttBrokerPort)//服务器
                  .WithClientId(ClientGatewayConfig.GatewayId)
                  .WithCleanSession(true)
                  .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
                  .WithoutThrowOnNonSuccessfulConnectResponse()
                  .Build();
                _mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic(ClientGatewayConfig.WriteRpcTopic);
                            f.WithAtMostOnceQoS();
                        })
                      .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic(GetRpcTopic(ClientGatewayConfig.GatewayId, ClientGatewayConfig.DBDownTopic));
                            f.WithAtMostOnceQoS();
                        })
                                        .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic(GetRpcTopic(ClientGatewayConfig.GatewayId, ClientGatewayConfig.DBUploadTopic));
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
    private void Log_LogMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
    {
        _clientLogger.LogOut(e.LogMessage.Level, e.LogMessage.Source, e.LogMessage.Message, e.LogMessage.Exception);
    }
    private void ServerLog_LogMessagePublished(object sender, MqttNetLogMessagePublishedEventArgs e)
    {
        _manageLogger.LogOut(e.LogMessage.Level, e.LogMessage.Source, e.LogMessage.Message, e.LogMessage.Exception);
    }

    /// <summary>
    /// ClientGatewayConfig
    /// </summary>
    public ClientGatewayConfig ClientGatewayConfig;
    /// <summary>
    /// ManageGatewayConfig
    /// </summary>
    public ManageGatewayConfig ManageGatewayConfig;
    private MqttClientOptions _mqttClientOptions;
    RpcSingletonService _rpcCore;

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
                }
                var collectDevices = new List<CollectDevice>();


                if (mqttDBRpc.CollectDevices != null && mqttDBRpc.CollectDevices.Length > 0)
                {
                    using MemoryStream stream = new(mqttDBRpc.CollectDevices);
                    var previewResult = await collectDeviceService.PreviewAsync(stream);
                    if (previewResult.FirstOrDefault().Value.HasError)
                    {
                        throw new(previewResult.Select(a => a.Value.Results.Where(a => !a.isSuccess).ToList()).ToList().ToJsonString());
                    }
                    foreach (var item in previewResult)
                    {
                        if (item.Key == ExportHelpers.CollectDeviceSheetName)
                        {
                            var collectDeviceImports = ((ImportPreviewOutput<CollectDevice>)item.Value).Data;
                            collectDevices = collectDeviceImports.Values.Adapt<List<CollectDevice>>();
                            break;
                        }
                    }
                    await collectDeviceService.ImportAsync(previewResult);

                }

                if (mqttDBRpc.IsUploadDevicesFullUp)
                {
                    await uploadDeviceService.AsDeleteable().ExecuteCommandAsync();

                }
                var uploadDevices = new List<UploadDevice>();

                if (mqttDBRpc.UploadDevices != null && mqttDBRpc.UploadDevices.Length > 0)
                {
                    using MemoryStream stream1 = new(mqttDBRpc.UploadDevices);
                    var previewResult1 = await uploadDeviceService.PreviewAsync(stream1);
                    if (previewResult1.FirstOrDefault().Value.HasError)
                    {
                        throw new(previewResult1.Select(a => a.Value.Results.Where(a => !a.isSuccess).ToList()).ToList().ToJsonString());
                    }
                    foreach (var item in previewResult1)
                    {
                        if (item.Key == ExportHelpers.UploadDeviceSheetName)
                        {
                            var uploadDeviceImports = ((ImportPreviewOutput<UploadDevice>)item.Value).Data;
                            uploadDevices = uploadDeviceImports.Values.Adapt<List<UploadDevice>>();
                            break;
                        }
                    }
                    await uploadDeviceService.ImportAsync(previewResult1);

                }

                if (mqttDBRpc.IsDeviceVariablesFullUp)
                {
                    await variableService.AsDeleteable().ExecuteCommandAsync();
                }

                if (mqttDBRpc.DeviceVariables != null && mqttDBRpc.DeviceVariables.Length > 0)
                {
                    using MemoryStream stream2 = new(mqttDBRpc.DeviceVariables);
                    var previewResult2 = await variableService.PreviewAsync(stream2, collectDevices, uploadDevices);
                    if (previewResult2.FirstOrDefault().Value.HasError)
                    {
                        throw new(previewResult2.Select(a => a.Value.Results.Where(a => !a.isSuccess).ToList()).ToList().ToJsonString());
                    }
                    await variableService.ImportAsync(previewResult2);
                }
            });
            Cache.SysMemoryCache.Remove(ThingsGatewayCacheConst.Cache_CollectDevice);//cache删除
            Cache.SysMemoryCache.Remove(ThingsGatewayCacheConst.Cache_UploadDevice);//cache删除

            if (dbResult.IsSuccess)//如果成功了
            {
                _clientLogger.LogInformation("子网关接收配置，并保存至数据库-执行成功");
                result = OperResult.CreateSuccessResult();
                if (mqttDBRpc.IsRestart)
                {
                    _clientLogger.LogInformation("子网关接收配置，并重启");
                    await BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>().RestartDeviceThreadAsync();
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

    private string GetRpcReturnIdTopic(string gatewayId, string topic, string rpcId)
    {
        var responseTopic = $"{gatewayId}/{topic}/rpc/Return/rpcId";
        return responseTopic;
    }

    private string GetRpcReturnTopic(string gatewayId, string topic)
    {
        var responseTopic = $"{gatewayId}/{topic}/rpc/Return";
        return responseTopic;
    }

    private string GetRpcReturnTopic(string requestTopic)
    {
        var responseTopic = $"{requestTopic}/Return";
        return responseTopic;
    }

    private string GetRpcTopic(string gatewayId, string topic)
    {
        var requestTopic = $"{gatewayId}/{topic}/rpc";
        return requestTopic;
    }


    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        if (args.ApplicationMessage.Topic == GetRpcTopic(ClientGatewayConfig.GatewayId, ClientGatewayConfig.DBUploadTopic))
        {
            _clientLogger.LogInformation("子网关配置上传");
            await DBUploadTopicMethod(args);
            return;
        }
        if (args.ApplicationMessage.Topic == GetRpcTopic(ClientGatewayConfig.GatewayId, ClientGatewayConfig.DBDownTopic))
        {

            _clientLogger.LogInformation("子网关接收配置，并保存至数据库");
            await DBDownTopicMethod(args);

            return;
        }
        if (args.ApplicationMessage.Topic == ClientGatewayConfig.WriteRpcTopic)
        {

            await WriteRpcTopicMethod(args);

            return;
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

    private Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs eventArgs)
    {
        if (eventArgs.ApplicationMessage.Topic == GetRpcReturnTopic(ManageGatewayConfig.WriteRpcTopic))
        {
            if (!_writerRpcResultWaitingCalls.IsEmpty)
            {
                var payloadBuffer = eventArgs.ApplicationMessage.PayloadSegment.ToArray();
                var manageMqttRpcResult = Encoding.UTF8.GetString(payloadBuffer).FromJsonString<ManageMqttRpcResult>();
                var key = GetRpcReturnIdTopic(manageMqttRpcResult.GatewayId, ManageGatewayConfig.WriteRpcTopic, manageMqttRpcResult.RpcId);
                if (!_writerRpcResultWaitingCalls.TryRemove(key, out var writeRpcResultAsync))
                {
                    return CompletedTask.Instance;
                }
                writeRpcResultAsync.Set(manageMqttRpcResult);

            }

        }
        else
        {

            if (!_waitingCalls.TryRemove(eventArgs.ApplicationMessage.Topic, out var awaitable))
            {
                return CompletedTask.Instance;
            }

            var payloadBuffer = eventArgs.ApplicationMessage.PayloadSegment.ToArray();
            awaitable.Set(payloadBuffer);
        }

        return CompletedTask.Instance;
    }

    private Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
        if (ManageGatewayConfig.UserName != arg.UserName)
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return CompletedTask.Instance;
        }
        if (ManageGatewayConfig.Password != arg.Password)
        {
            arg.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return CompletedTask.Instance;
        }
        _manageLogger?.LogInformation($"{ToString()}-{arg.ClientId}-客户端已连接成功");
        return CompletedTask.Instance;
    }

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
    private async Task WriteRpcTopicMethod(MqttApplicationMessageReceivedEventArgs args)
    {
        var manageMqttRpcFrom = args.ApplicationMessage.PayloadSegment.Count > 0 ? Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment).FromJsonString<ManageMqttRpcFrom>() : null;
        if (manageMqttRpcFrom != null && manageMqttRpcFrom.GatewayId == ClientGatewayConfig.GatewayId)
        {
            ManageMqttRpcResult mqttRpcResult = new() { RpcId = manageMqttRpcFrom.RpcId, GatewayId = manageMqttRpcFrom.GatewayId };
            _rpcCore ??= App.GetService<RpcSingletonService>();
            var result = await _rpcCore.InvokeDeviceMethodAsync($"子网关RPC-{args.ClientId}",
    manageMqttRpcFrom.WriteInfos.Where(
    a => !mqttRpcResult.Message.Any(b => b.Key == a.Key)).ToDictionary(a => a.Key, a => a.Value));
            mqttRpcResult.Message.AddRange(result);
            mqttRpcResult.Success = !mqttRpcResult.Message.Any(a => !a.Value.IsSuccess);

            var variableMessage = new MqttApplicationMessageBuilder()
    .WithTopic(GetRpcReturnTopic(args.ApplicationMessage.Topic))
    .WithPayload(mqttRpcResult.ToJsonString()).Build();
            if (_mqttClient.IsConnected)
                await _mqttClient.PublishAsync(variableMessage);
        }
    }



    #endregion
}
