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
using MQTTnet.Server;

using System.Net;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Application;

/// <summary>
/// 设备采集报警后台服务
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
    public OperResult RealAlarmStatuString { get; set; } = new OperResult("初始化");
    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult HisAlarmStatuString { get; set; } = new OperResult("初始化");
    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult ReadAlarmStatuString { get; set; } = new OperResult("初始化");
    private MqttServer _mqttServer;
    private IMqttClient _mqttClient;


    #region worker服务
    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken token)
    {
        _logger?.LogInformation("ManageGatewayWorker启动");
        await RestartAsync();
        await base.StartAsync(token);
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken token)
    {
        _logger?.LogInformation("ManageGatewayWorker停止");
        return base.StopAsync(token);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(60000, stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
            }
        }
    }


    #endregion

    #region 核心实现

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

    /// <summary>
    /// 初始化
    /// </summary>
    private async Task InitAsync()
    {
        try
        {
            var manageGatewayConfig = App.GetConfig<ManageGatewayConfig>("ManageGatewayConfig");
            if (manageGatewayConfig?.Enable != true)
            {
                HisAlarmStatuString = new OperResult($"已退出：不启用管理功能");
                return;
            }
            else
            {
                var mqttFactory = new MqttFactory(new MqttNetLogger(_manageLogger));
                var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
                    .WithDefaultEndpointBoundIPAddress(string.IsNullOrEmpty(manageGatewayConfig.MqttBrokerIP) ? null : IPAddress.Parse(manageGatewayConfig.MqttBrokerIP))
                    .WithDefaultEndpointPort(manageGatewayConfig.MqttBrokerPort)
                    .WithDefaultEndpoint()
                    .Build();
                _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
                if (_mqttServer != null)
                {
                    _mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;
                    _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
                    _mqttServer.LoadingRetainedMessageAsync += MqttServer_LoadingRetainedMessageAsync;
                    _mqttServer.InterceptingSubscriptionAsync += MqttServer_InterceptingSubscriptionAsync; ;
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
            var clientGatewayConfig = App.GetConfig<ManageGatewayConfig>("ClientGatewayConfig");
            if (clientGatewayConfig?.Enable != true)
            {
                RealAlarmStatuString = new OperResult($"已退出：不启用子网关功能");
                return;
            }
            else
            {
                var mqttFactory = new MqttFactory(new MqttNetLogger(_clientLogger));
                var _mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
                   .WithCredentials(clientGatewayConfig.UserName, clientGatewayConfig.Password)//账密
                   .WithTcpServer(clientGatewayConfig.MqttBrokerIP, clientGatewayConfig.MqttBrokerPort)//服务器
                   .WithCleanSession(true)
                   .WithKeepAlivePeriod(TimeSpan.FromSeconds(120.0))
                   .WithoutThrowOnNonSuccessfulConnectResponse()
                   .Build();
                var _mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                     .WithTopicFilter(
                         f =>
                         {
                             f.WithTopic(clientGatewayConfig.WriteRpcTopic);
                             f.WithAtMostOnceQoS();
                         })
                       .WithTopicFilter(
                         f =>
                         {
                             f.WithTopic(clientGatewayConfig.DBDownTopic);
                             f.WithAtMostOnceQoS();
                         })
                                         .WithTopicFilter(
                         f =>
                         {
                             f.WithTopic(clientGatewayConfig.DBUploadTopic);
                             f.WithAtMostOnceQoS();
                         })
                     .Build();
                _mqttClient = mqttFactory.CreateMqttClient();
                _mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
                _mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;

            }
        }
        catch (Exception ex)
        {
            _clientLogger.LogError(ex, "初始化失败");
        }


    }

    private Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs args)
    {
        throw new NotImplementedException();
    }

    private Task MqttServer_InterceptingSubscriptionAsync(InterceptingSubscriptionEventArgs args)
    {
        throw new NotImplementedException();
    }

    private Task MqttServer_LoadingRetainedMessageAsync(LoadingRetainedMessagesEventArgs args)
    {
        throw new NotImplementedException();
    }

    private Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs args)
    {
        throw new NotImplementedException();
    }

    private Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs args)
    {
        throw new NotImplementedException();
    }
    #endregion
}


