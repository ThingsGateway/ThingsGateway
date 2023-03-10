using Furion.Schedule;

using System.Threading;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 清理日志作业任务
/// </summary>
[JobDetail("job_tglog", Description = "清理日志", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_tglog", Description = "清理日志", RunOnStart = true)]
public class TGLogJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public TGLogJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var db = DbContext.Db.CopyNew();
        var daysAgo = 30; // 删除30天以前
        await db.Deleteable<RuntimeLog>().Where(u => (DateTime)u.LogTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync();
        await db.Deleteable<RpcLog>().Where(u => (DateTime)u.LogTime < DateTime.Now.AddDays(-daysAgo)).ExecuteCommandAsync();
    }
}