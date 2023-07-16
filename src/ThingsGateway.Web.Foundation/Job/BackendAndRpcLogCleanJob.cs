#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.Schedule;

using System.Threading;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 清理日志作业任务
/// </summary>
[JobDetail("job_tglog", Description = "清理网关后台/Rpc日志", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_tglog", Description = "清理网关后台/Rpc日志", RunOnStart = true)]
public class BackendAndRpcLogCleanJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc cref="BackendAndRpcLogCleanJob"/>
    public BackendAndRpcLogCleanJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    /// <inheritdoc/>
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var db = DbContext.Db.CopyNew();
        var daysAgo = 30; // 删除30天以前
        await db.Deleteable<BackendLog>().Where(u => (DateTime)u.LogTime < DateTime.UtcNow.AddDays(-daysAgo)).ExecuteCommandAsync();
        await db.Deleteable<RpcLog>().Where(u => (DateTime)u.LogTime < DateTime.UtcNow.AddDays(-daysAgo)).ExecuteCommandAsync();
    }
}