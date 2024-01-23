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

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MQTTnet.AspNetCore;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttServer,RPC方法适配mqttNet
/// </summary>
public partial class MqttServer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private readonly MqttClientVariableProperty _variablePropertys = new();
    private readonly MqttServerProperty _driverPropertys = new();

    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheIntervalScript _businessPropertyWithCacheIntervalScript => _driverPropertys;

    /// <inheritdoc/>
    public override bool IsConnected() => _mqttServer?.IsStarted == true;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(MqttServer)} IP:{_driverPropertys.IP} Port:{_driverPropertys.Port} WebSocket:{_driverPropertys.WebSocketPort}";
    }

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        #region 初始化

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

        #endregion 初始化
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _ = _webHost.StartAsync(cancellationToken);
        if (_mqttServer != null)
        {
            _mqttServer.ValidatingConnectionAsync -= MqttServer_ValidatingConnectionAsync;
            _mqttServer.InterceptingPublishAsync -= MqttServer_InterceptingPublishAsync;
            _mqttServer.LoadingRetainedMessageAsync -= MqttServer_LoadingRetainedMessageAsync;
            _mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;
            _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
            _mqttServer.LoadingRetainedMessageAsync += MqttServer_LoadingRetainedMessageAsync;
            await _mqttServer.StartAsync();
        }
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        await Update(cancellationToken);

        await Delay(CurrentDevice.IntervalTime, cancellationToken);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_mqttServer != null)
        {
            _mqttServer.ValidatingConnectionAsync -= MqttServer_ValidatingConnectionAsync;
            _mqttServer.InterceptingPublishAsync -= MqttServer_InterceptingPublishAsync;
            _mqttServer.LoadingRetainedMessageAsync -= MqttServer_LoadingRetainedMessageAsync;
            _mqttServer?.SafeDispose();
        }
        _webHost?.SafeDispose();
        base.Dispose(disposing);
    }
}