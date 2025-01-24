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
/// Cron 字段种类
/// </summary>
internal enum CrontabFieldKind
{
    /// <summary>
    /// 秒
    /// </summary>
    Second = 0,

    /// <summary>
    /// 分
    /// </summary>
    Minute = 1,

    /// <summary>
    /// 时
    /// </summary>
    Hour = 2,

    /// <summary>
    /// 天
    /// </summary>
    Day = 3,

    /// <summary>
    /// 月
    /// </summary>
    Month = 4,

    /// <summary>
    /// 星期
    /// </summary>
    DayOfWeek = 5,

    /// <summary>
    /// 年
    /// </summary>
    Year = 6
}