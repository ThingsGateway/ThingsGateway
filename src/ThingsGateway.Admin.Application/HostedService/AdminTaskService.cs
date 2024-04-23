
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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        var daysAgo = App.Configuration.GetSection("LogJob:DaysAgo").Get<int?>() ?? 1;
        var verificatInfoCacheService = App.RootServices.GetService<IVerificatInfoCacheService>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeleteSysOperateLog(daysAgo, stoppingToken).ConfigureAwait(false);
                verificatInfoCacheService.HashSetDB(verificatInfoCacheService.GetAll());
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

        //实现 程序退出时，持久化TokenCache
        verificatInfoCacheService.HashSetDB(verificatInfoCacheService.GetAll());
    }

    private async Task DeleteSysOperateLog(int daysAgo, CancellationToken stoppingToken)
    {
        using var db = DbContext.Db.GetConnectionScopeWithAttr<SysOperateLog>().CopyNew();
        await db.DeleteableWithAttr<SysOperateLog>().Where(u => u.OpTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync(stoppingToken); // 删除操作日志
    }
}
