// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Schedule;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 清理日志作业任务
/// </summary>
[JobDetail("job_log", Description = "清理操作日志", GroupName = "Log", Concurrent = false)]
[Daily(TriggerId = "trigger_log", Description = "清理操作日志", RunOnStart = true)]
public class LogJob : IJob
{
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var daysAgo = App.GetOptions<AdminLogOptions>()?.OperateLogDaysAgo ?? 7;
        await LogJob.DeleteSysOperateLog(daysAgo, stoppingToken).ConfigureAwait(false);
    }

    private static async Task DeleteSysOperateLog(int daysAgo, CancellationToken stoppingToken)
    {
        using var db = DbContext.Db.GetConnectionScopeWithAttr<SysOperateLog>().CopyNew();
        await db.DeleteableWithAttr<SysOperateLog>().Where(u => u.OpTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync(stoppingToken).ConfigureAwait(false); // 删除操作日志
    }
}
