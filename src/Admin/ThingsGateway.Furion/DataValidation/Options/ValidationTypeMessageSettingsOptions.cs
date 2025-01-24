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

using ThingsGateway.ConfigurableOptions;

namespace ThingsGateway.DataValidation;

/// <summary>
/// 验证消息配置选项
/// </summary>
public sealed class ValidationTypeMessageSettingsOptions : IConfigurableOptions
{
    /// <summary>
    /// 验证消息配置表
    /// </summary>
    public object[][] Definitions { get; set; }
}