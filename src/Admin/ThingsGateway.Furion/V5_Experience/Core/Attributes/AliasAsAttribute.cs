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

namespace System;

/// <summary>
///     设置别名特性
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class AliasAsAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="AliasAsAttribute" />
    /// </summary>
    /// <param name="aliasAs">别名</param>
    public AliasAsAttribute(string aliasAs) => AliasAs = aliasAs;

    /// <summary>
    ///     别名
    /// </summary>
    public string? AliasAs { get; set; }
}