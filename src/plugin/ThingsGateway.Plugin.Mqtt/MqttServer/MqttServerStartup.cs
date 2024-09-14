//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.Diagnostics;
using MQTTnet.Server;

namespace ThingsGateway.Plugin.Mqtt;

/// <summary>
/// MqttServerStartup
/// </summary>
public class MqttServerStartup
{
    /// <inheritdoc/>
    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapConnectionHandler<MqttConnectionHandler>(
                "/mqtt",
                httpConnectionDispatcherOptions => httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
                    protocolList => protocolList.FirstOrDefault() ?? string.Empty);
        });
    }

    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(s =>
        {
            var serverOptionsBuilder = new MqttServerOptionsBuilder();

            serverOptionsBuilder.WithDefaultEndpoint();

            return serverOptionsBuilder.Build();
        });

        var logger = new MqttNetEventLogger();
        services.AddSingleton<IMqttNetLogger>(logger);
        services.TryAddSingleton(new MqttFactory());
        services.AddSingleton<MqttHostedServer>();
        //services.AddSingleton<IHostedService>(s => s.GetService<MqttHostedServer>());
        ////不再注册HostService,MqttServer的生命周期由插件完成
        services.AddSingleton<MQTTnet.Server.MqttServer>(s => s.GetService<MqttHostedServer>());
        services.AddMqttConnectionHandler();
        services.AddConnections();
    }
}
