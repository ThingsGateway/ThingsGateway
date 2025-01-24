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
/// 作业持久化（数据库）
/// </summary>
public class JobPersistence : IJobPersistence
{

    /// <summary>
    /// 作业调度服务启动时
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public async Task<IEnumerable<SchedulerBuilder>> PreloadAsync(CancellationToken stoppingToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        // 获取所有定义的作业
        var allJobs = App.EffectiveTypes.ScanToBuilders().ToList();
        return allJobs;
    }

    /// <summary>
    /// 作业计划初始化通知
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    public Task<SchedulerBuilder> OnLoadingAsync(SchedulerBuilder builder, CancellationToken stoppingToken)
    {
        return Task.FromResult(builder);
    }

    /// <summary>
    /// 作业计划Scheduler的JobDetail变化时
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task OnChangedAsync(PersistenceContext context)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// 作业计划Scheduler的触发器Trigger变化时
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task OnTriggerChangedAsync(PersistenceTriggerContext context)
    {
        await Task.CompletedTask.ConfigureAwait(false);

    }

    /// <summary>
    /// 作业触发器运行记录
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task OnExecutionRecordAsync(PersistenceExecutionRecordContext context)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
