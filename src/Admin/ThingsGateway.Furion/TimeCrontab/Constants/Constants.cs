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
/// TimeCrontab 模块常量
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Cron 字段种类最大值
    /// </summary>
    internal static readonly Dictionary<CrontabFieldKind, int> MaximumDateTimeValues = new()
    {
        { CrontabFieldKind.Second, 59 },
        { CrontabFieldKind.Minute, 59 },
        { CrontabFieldKind.Hour, 23 },
        { CrontabFieldKind.DayOfWeek, 7 },
        { CrontabFieldKind.Day, 31 },
        { CrontabFieldKind.Month, 12 },
        { CrontabFieldKind.Year, 9999 },
    };

    /// <summary>
    /// Cron 字段种类最大值
    /// </summary>
    internal static readonly Dictionary<CrontabFieldKind, int> MinimumDateTimeValues = new()
    {
        { CrontabFieldKind.Second, 0 },
        { CrontabFieldKind.Minute, 0 },
        { CrontabFieldKind.Hour, 0 },
        { CrontabFieldKind.DayOfWeek, 0 },
        { CrontabFieldKind.Day, 1 },
        { CrontabFieldKind.Month, 1 },
        { CrontabFieldKind.Year, 1 },
    };

    /// <summary>
    /// Cron 不同格式化类型字段数量
    /// </summary>
    internal static readonly Dictionary<CronStringFormat, int> ExpectedFieldCounts = new()
    {
        { CronStringFormat.Default, 5 },
        { CronStringFormat.WithYears, 6 },
        { CronStringFormat.WithSeconds, 6 },
        { CronStringFormat.WithSecondsAndYears, 7 },
    };

    /// <summary>
    /// 配置 C# 中 <see cref="DayOfWeek"/> 枚举元素值
    /// </summary>
    /// <remarks>主要解决 C# 中该类型和 Cron 星期字段域不对应问题</remarks>
    internal static readonly Dictionary<DayOfWeek, int> CronDays = new()
    {
        { DayOfWeek.Sunday, 0 },
        { DayOfWeek.Monday, 1 },
        { DayOfWeek.Tuesday, 2 },
        { DayOfWeek.Wednesday, 3 },
        { DayOfWeek.Thursday, 4 },
        { DayOfWeek.Friday, 5 },
        { DayOfWeek.Saturday, 6 },
    };

    /// <summary>
    /// 定义 Cron 星期字段域值支持的星期英文缩写
    /// </summary>
    internal static readonly Dictionary<string, int> Days = new()
    {
        { "SUN", 0 },
        { "MON", 1 },
        { "TUE", 2 },
        { "WED", 3 },
        { "THU", 4 },
        { "FRI", 5 },
        { "SAT", 6 },
    };

    /// <summary>
    /// 定义 Cron 月字段域值支持的星期英文缩写
    /// </summary>
    internal static readonly Dictionary<string, int> Months = new()
    {
        { "JAN", 1 },
        { "FEB", 2 },
        { "MAR", 3 },
        { "APR", 4 },
        { "MAY", 5 },
        { "JUN", 6 },
        { "JUL", 7 },
        { "AUG", 8 },
        { "SEP", 9 },
        { "OCT", 10 },
        { "NOV", 11 },
        { "DEC", 12 },
    };
}