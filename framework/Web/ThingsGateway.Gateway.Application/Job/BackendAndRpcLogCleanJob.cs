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

using Furion;
using Furion.Schedule;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 清理日志作业任务
/// </summary>
[JobDetail("job_tglog", Description = "清理网关后台/Rpc日志", GroupName = "default", Concurrent = false)]
[Daily(TriggerId = "trigger_tglog", Description = "清理网关后台/Rpc日志", RunOnStart = true)]
public class BackendAndRpcLogCleanJob : IJob
{
    /// <inheritdoc/>
    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        var db = DbContext.Db.CopyNew();
        var daysAgo = App.GetConfig<int?>("Logging:LogJob:DaysAgo") ?? 30;
        await db.DeleteableWithAttr<BackendLog>().Where(u => u.LogTime < DateTimeExtensions.CurrentDateTime.AddDays(-daysAgo)).ExecuteCommandAsync();
        await db.DeleteableWithAttr<RpcLog>().Where(u => u.LogTime < DateTimeExtensions.CurrentDateTime.AddDays(-daysAgo)).ExecuteCommandAsync();
    }
}