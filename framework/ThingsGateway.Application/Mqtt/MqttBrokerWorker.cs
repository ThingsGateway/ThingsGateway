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

using System.Net;

using ThingsGateway.Foundation;

namespace ThingsGateway.Application;

/// <summary>
/// MqttBroker
/// </summary>
public class MqttBrokerWorker : BackgroundService
{
    private readonly ILogger<MqttBrokerWorker> _logger;
    /// <inheritdoc/>
    public MqttBrokerWorker(ILogger<MqttBrokerWorker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
    }


    #region worker服务

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken token)
    {
        await base.StartAsync(token);
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken token)
    {
        await base.StopAsync(token);
    }
    private MQTTnet.Server.MqttServer _mqttServer;

    /// <inheritdoc/>
    protected void Init()
    {
        var mqttBrokerConfig = App.GetConfig<MqttBrokerConfig>("MqttBrokerConfig");
        var mqttFactory = new MqttFactory(new MqttNetLogger(_logger));
        var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
            .WithDefaultEndpointBoundIPAddress(string.IsNullOrEmpty(mqttBrokerConfig.MqttBrokerIP) ? null : IPAddress.Parse(mqttBrokerConfig.MqttBrokerIP))
            .WithDefaultEndpointPort(mqttBrokerConfig.MqttBrokerPort)
            .WithDefaultEndpoint()
            .Build();
        _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);

    }


    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(300000, stoppingToken);
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



}

