// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业调度持久化器
/// </summary>
public interface IJobPersistence
{
    /// <summary>
    /// 作业调度器预加载服务
    /// </summary>
    /// <param name="stoppingToken">取消任务 Token</param>
    /// <returns><see cref="Task"/></returns>
    Task<IEnumerable<SchedulerBuilder>> PreloadAsync(CancellationToken stoppingToken);

    /// <summary>
    /// 作业计划初始化通知
    /// </summary>
    /// <param name="builder">作业计划构建器</param>
    /// <param name="stoppingToken">取消任务 Token</param>
    /// <returns><see cref="Task"/></returns>
    Task<SchedulerBuilder> OnLoadingAsync(SchedulerBuilder builder, CancellationToken stoppingToken);

    /// <summary>
    /// 作业信息更改通知
    /// </summary>
    /// <param name="context">作业信息持久化上下文</param>
    /// <returns><see cref="Task"/></returns>
    Task OnChangedAsync(PersistenceContext context);

    /// <summary>
    /// 作业触发器更改通知
    /// </summary>
    /// <param name="context">作业触发器持久化上下文</param>
    /// <returns><see cref="Task"/></returns>
    Task OnTriggerChangedAsync(PersistenceTriggerContext context);

    /// <summary>
    /// 作业触发记录通知
    /// </summary>
    /// <param name="context">作业执行记录持久上下文</param>
    /// <returns><see cref="Task"/></returns>
    Task OnExecutionRecordAsync(PersistenceExecutionRecordContext context);
}