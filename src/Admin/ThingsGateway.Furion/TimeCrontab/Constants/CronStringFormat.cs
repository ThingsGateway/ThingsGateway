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
/// Cron 表达式格式化类型
/// </summary>
public enum CronStringFormat
{
    /// <summary>
    /// 默认格式
    /// </summary>
    /// <remarks>书写顺序：分 时 天 月 周</remarks>
    Default = 0,

    /// <summary>
    /// 带年份格式
    /// </summary>
    /// <remarks>书写顺序：分 时 天 月 周 年</remarks>
    WithYears = 1,

    /// <summary>
    /// 带秒格式
    /// </summary>
    /// <remarks>书写顺序：秒 分 时 天 月 周</remarks>
    WithSeconds = 2,

    /// <summary>
    /// 带秒和年格式
    /// </summary>
    /// <remarks>书写顺序：秒 分 时 天 月 周 年</remarks>
    WithSecondsAndYears = 3
}