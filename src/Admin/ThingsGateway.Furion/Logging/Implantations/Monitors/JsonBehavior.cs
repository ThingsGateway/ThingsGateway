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

using System.ComponentModel;

namespace ThingsGateway.Logging;

/// <summary>
/// LoggingMonitor JSON 输出行为
/// </summary>
[SuppressSniffer]
public enum JsonBehavior
{
    /// <summary>
    /// 不输出 JSON 格式
    /// </summary>
    /// <remarks>默认值，输出文本日志</remarks>
    [Description("不输出 JSON 格式")]
    None = 0,

    /// <summary>
    /// 只输出 JSON 格式
    /// </summary>
    [Description("只输出 JSON 格式")]
    OnlyJson = 1,

    /// <summary>
    /// 输出 JSON 格式和文本日志
    /// </summary>
    [Description("输出 JSON 格式和文本日志")]
    All = 2
}