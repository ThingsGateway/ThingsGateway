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

namespace ThingsGateway.Admin.Application;

internal class AdminTaskService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //实现 删除过期日志 功能，不需要精确的时间
        var daysAgo = App.Configuration.GetSection("LogJob:DaysAgo").Get<int?>() ?? 30;

        while (!stoppingToken.IsCancellationRequested)
        {
            await DeleteSysOperateLog(daysAgo, stoppingToken);
            await Task.Delay(TimeSpan.FromDays(1));
        }
    }

    private async Task DeleteSysOperateLog(int daysAgo, CancellationToken stoppingToken)
    {
        using var db = DbContext.Db.CopyNew();
        await db.DeleteableWithAttr<SysOperateLog>().Where(u => u.OpTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync(stoppingToken); // 删除操作日志
    }
}