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

namespace ThingsGateway.Gateway.Application;

internal sealed class RedundancyHostedService : BackgroundService, IRedundancyHostedService
{
    private readonly ILogger _logger;
    /// <inheritdoc cref="RedundancyHostedService"/>
    public RedundancyHostedService(ILogger<RedundancyHostedService> logger)
    {
        _logger = logger;
        RedundancyTask = new RedundancyTask(_logger);
    }
    private RedundancyTask RedundancyTask;

    protected override  Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await RedundancyTask.StartRedundancyTaskAsync().ConfigureAwait(false);
    }
    public Task StartRedundancyTaskAsync() => RedundancyTask.StartRedundancyTaskAsync();

    public Task StopRedundancyTaskAsync() => RedundancyTask.StopRedundancyTaskAsync();

    public Task RedundancyForcedSync() => RedundancyTask.RedundancyForcedSync();

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await RedundancyTask.DisposeAsync().ConfigureAwait(false);
        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }

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
}
