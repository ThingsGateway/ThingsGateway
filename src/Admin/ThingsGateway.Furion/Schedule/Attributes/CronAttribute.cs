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

using ThingsGateway.TimeCrontab;

namespace ThingsGateway.Schedule;

/// <summary>
/// Cron 表达式作业触发器特性
/// </summary>
[SuppressSniffer, AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CronAttribute : TriggerAttribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="schedule">Cron 表达式</param>
    /// <param name="format">Cron 表达式格式化类型</param>
    public CronAttribute(string schedule, CronStringFormat format = CronStringFormat.Default)
        : base(typeof(CronTrigger)
            , schedule, format)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="schedule">Cron 表达式</param>
    /// <param name="args">动态参数类型，支持 <see cref="int"/>，<see cref="CronStringFormat"/> 和 object[]</param>
    internal CronAttribute(string schedule, object args)
        : base(typeof(CronTrigger)
            , schedule, args)
    {
    }
}