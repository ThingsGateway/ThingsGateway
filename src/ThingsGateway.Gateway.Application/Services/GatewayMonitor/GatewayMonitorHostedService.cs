//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Common.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 通道后台服务
/// </summary>
internal sealed class GatewayMonitorHostedService : BackgroundService,IRedundancyHostedService
{
    public ILogger Logger { get; }
    /// <inheritdoc cref="AlarmHostedService"/>
    public GatewayMonitorHostedService(ILogger<GatewayMonitorHostedService> logger, IChannelThreadManage channelThreadManage)
    {
        Logger = logger;
        ChannelThreadManage = channelThreadManage;
        RedundancyTask = new RedundancyTask(Logger);
    }

    private IChannelThreadManage ChannelThreadManage { get; }
    private RedundancyTask RedundancyTask;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await RestartAll().ConfigureAwait(false);
        await RedundancyTask.StartRedundancyTaskAsync().ConfigureAwait(false);

    }
    public Task StartRedundancyTaskAsync() => RedundancyTask.StartRedundancyTaskAsync();

    public Task StopRedundancyTaskAsync() => RedundancyTask.StopRedundancyTaskAsync();

    public Task RedundancyForcedSync() => RedundancyTask.RedundancyForcedSync();
    public Task<TouchSocket.Core.LogLevel> RedundancyLogLevelAsync()
    {
        return Task.FromResult(RedundancyTask.TextLogger.LogLevel);
    }

    public Task SetRedundancyLogLevelAsync(TouchSocket.Core.LogLevel logLevel)
    {
        RedundancyTask.TextLogger.LogLevel = logLevel;
        return Task.CompletedTask;
    }

    public Task<string> RedundancyLogPathAsync()
    {
        return Task.FromResult(RedundancyTask.LogPath);
    }

    private async Task RestartAll()
    {
        try
        {
            //网关启动时，获取所有通道
            var channelRuntimes = (await GlobalData.ChannelService.GetFromDBAsync().ConfigureAwait(false)).AdaptListChannelRuntime();
            var deviceRuntimes = (await GlobalData.DeviceService.GetFromDBAsync().ConfigureAwait(false)).AdaptListDeviceRuntime();

            var variableRuntimes = GlobalData.VariableService.GetAllVariableRuntime();

            channelRuntimes.ParallelForEach(channelRuntime =>
            {
                try
                {
                    channelRuntime.Init();
                    var devRuntimes = deviceRuntimes.Where(x => x.ChannelId == channelRuntime.Id);
                    foreach (var item in devRuntimes)
                    {
                        item.Init(channelRuntime);

                        var varRuntimes = variableRuntimes.Where(x => x.DeviceId == item.Id);

                        varRuntimes.ParallelForEach(varItem => varItem.Init(item));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Init Channel");
                }
            });

            GlobalData.ChannelDeviceRuntimeDispatchService.Dispatch(null);
            GlobalData.VariableRuntimeDispatchService.Dispatch(null);

            await ChannelThreadManage.RestartChannelAsync(channelRuntimes).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Start error");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await ChannelThreadManage.SafeDisposeAsync().ConfigureAwait(false);
        await RedundancyTask.DisposeAsync().ConfigureAwait(false);
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}
