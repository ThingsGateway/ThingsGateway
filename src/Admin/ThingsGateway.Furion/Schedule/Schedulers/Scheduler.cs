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

using Microsoft.Extensions.Logging;

namespace ThingsGateway.Schedule;

/// <summary>
/// 作业计划
/// </summary>
internal sealed partial class Scheduler : IScheduler
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jobDetail">作业信息</param>
    /// <param name="triggers">作业触发器集合</param>
    internal Scheduler(JobDetail jobDetail, Dictionary<string, Trigger> triggers)
    {
        JobId = jobDetail.JobId;
        GroupName = jobDetail.GroupName;
        JobDetail = jobDetail;
        Triggers = triggers;
    }

    /// <summary>
    /// 作业 Id
    /// </summary>
    public string JobId { get; private set; }

    /// <summary>
    /// 作业组名称
    /// </summary>
    public string GroupName { get; private set; }

    /// <summary>
    /// 作业触发器数量
    /// </summary>
    public int TriggerCount => Triggers.Count;

    /// <summary>
    /// 作业信息
    /// </summary>
    internal JobDetail JobDetail { get; private set; }

    /// <summary>
    /// 作业触发器集合
    /// </summary>
    internal Dictionary<string, Trigger> Triggers { get; private set; } = new();

    /// <summary>
    /// 作业计划工厂
    /// </summary>
    internal ISchedulerFactory Factory { get; set; }

    /// <summary>
    /// 作业调度器日志服务
    /// </summary>
    internal IScheduleLogger Logger { get; set; }

    /// <summary>
    /// 作业处理类型日志服务
    /// </summary>
    internal ILogger JobLogger { get; set; }
}