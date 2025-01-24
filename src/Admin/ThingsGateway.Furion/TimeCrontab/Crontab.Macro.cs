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

namespace ThingsGateway.TimeCrontab;

/// <summary>
/// Cron 表达式抽象类
/// </summary>
/// <remarks>主要将 Cron 表达式转换成 OOP 类进行操作</remarks>
public sealed partial class Crontab
{
    /// <summary>
    /// 表示每秒开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Secondly = Parse("* * * * * *", CronStringFormat.WithSeconds);

    /// <summary>
    /// 表示每分钟开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Minutely = Parse("* * * * *", CronStringFormat.Default);

    /// <summary>
    /// 表示每小时开始 的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Hourly = Parse("0 * * * *", CronStringFormat.Default);

    /// <summary>
    /// 表示每天（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Daily = Parse("0 0 * * *", CronStringFormat.Default);

    /// <summary>
    /// 表示每月1号（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Monthly = Parse("0 0 1 * *", CronStringFormat.Default);

    /// <summary>
    /// 表示每周日（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Weekly = Parse("0 0 * * 0", CronStringFormat.Default);

    /// <summary>
    /// 表示每年1月1号（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Yearly = Parse("0 0 1 1 *", CronStringFormat.Default);

    /// <summary>
    /// 表示每周一至周五（午夜）开始的 <see cref="Crontab"/> 对象
    /// </summary>
    public static readonly Crontab Workday = Parse("0 0 * * 1-5", CronStringFormat.Default);
}