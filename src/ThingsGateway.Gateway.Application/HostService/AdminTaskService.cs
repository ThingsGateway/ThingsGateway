
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Gateway.Application;

using LoggerExtensions = ThingsGateway.Foundation.LoggerExtensions;

namespace ThingsGateway.Admin.Application;

internal class AdminTaskService : BackgroundService
{
    private readonly ILogger _logger;

    public AdminTaskService(ILogger<AdminTaskService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //实现 删除过期日志 功能，不需要精确的时间
        var daysAgo = App.Configuration.GetSection("LogJob:DaysAgo").Get<int?>() ?? 30;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeleteBackendLog(daysAgo, stoppingToken).ConfigureAwait(false);
                await DeleteRpcLog(daysAgo, stoppingToken).ConfigureAwait(false);
                await DeleteTextLog(stoppingToken).ConfigureAwait(false);
                await DeleteLocalDB(stoppingToken).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Execute waining");
            }
        }
    }

    private async Task DeleteBackendLog(int daysAgo, CancellationToken stoppingToken)
    {
        using var db = DbContext.Db.GetConnectionScopeWithAttr<BackendLog>().CopyNew();
        await db.DeleteableWithAttr<BackendLog>().Where(u => u.LogTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync(stoppingToken); // 删除操作日志
    }

    private async Task DeleteRpcLog(int daysAgo, CancellationToken stoppingToken)
    {
        using var db = DbContext.Db.GetConnectionScopeWithAttr<RpcLog>().CopyNew();
        await db.DeleteableWithAttr<RpcLog>().Where(u => u.LogTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync(stoppingToken); // 删除操作日志
    }

    private Task DeleteTextLog(CancellationToken stoppingToken)
    {
        //网关调试日志以通道id命名
        var channelService = App.RootServices.GetService<IChannelService>();
        var channelIds = channelService.GetAll().Select(a => a.Id.ToString());

        var baseDir = LoggerExtensions.GetLogBasePath();
        Directory.CreateDirectory(baseDir);
        string[] dirs = Directory.GetDirectories(baseDir)
.Select(a => Path.GetFileName(a))
.ToArray();
        foreach (var dir in dirs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }
            //删除文件夹
            try
            {
                if (!channelIds.Contains(dir))
                {
                    Directory.Delete(baseDir.CombinePathWithOs(dir), true);
                }
            }
            catch { }
        }

        //底层调试
        var debugDir = LoggerExtensions.GetDebugLogBasePath();
        Directory.CreateDirectory(debugDir);
        string[] debugDirs = Directory.GetDirectories(debugDir)
    .Select(a => Path.GetFileName(a))
    .ToArray();

        ChannelConfig channelConfig = TouchSocket.Core.AppConfigBase.GetNewDefault<ChannelConfig>();
        foreach (var item in debugDirs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }
            //删除文件夹
            try
            {
                if (!channelConfig.ChannelDatas.Select(a => a.Id.ToString()).Contains(item))
                {
                    Directory.Delete(debugDir.CombinePathWithOs(item), true);
                }
            }
            catch { }
        }

        return Task.CompletedTask;
    }

    public Task DeleteLocalDB(CancellationToken stoppingToken)
    {
        var deviceService = App.RootServices.GetService<IDeviceService>();
        var data = deviceService.GetAll().Where(a => a.PluginType == PluginTypeEnum.Business).Select(a => a.Id);
        var dir = CacheDBUtil.GetFileBasePath();
        string[] dirs = Directory.GetDirectories(dir);
        foreach (var item in dirs)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }
            //删除文件夹
            try
            {
                var id = Path.GetFileName(item).ToLong();
                if (id > 0)
                {
                    if (!data.Contains(id))
                    {
                        Directory.Delete(item, true);
                    }
                }
            }
            catch { }
        }

        return Task.CompletedTask;
    }
}
