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

namespace ThingsGateway.Configuration;

/// <summary>
/// Configuration 模块常量
/// </summary>
internal static class Constants
{
    /// <summary>
    /// 正则表达式常量
    /// </summary>
    internal static class Patterns
    {
        /// <summary>
        /// 配置文件名
        /// </summary>
        internal const string ConfigurationFileName = @"(?<fileName>(?<realName>.+?)(\.(?<environmentName>\w+))?(?<extension>\.(json|xml|ini)))";

        /// <summary>
        /// 配置文件参数
        /// </summary>
        internal const string ConfigurationFileParameter = @"\s+(?<parameter>\b\w+\b)\s*=\s*(?<value>\btrue\b|\bfalse\b)";
    }
}