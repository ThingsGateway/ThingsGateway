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

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式如果 HTTP 响应的 IsSuccessStatusCode 属性是 <c>false</c>，则引发异常特性
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public sealed class EnsureSuccessStatusCodeAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="EnsureSuccessStatusCodeAttribute" />
    /// </summary>
    public EnsureSuccessStatusCodeAttribute()
        : this(true)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="EnsureSuccessStatusCodeAttribute" />
    /// </summary>
    /// <param name="enabled">是否启用</param>
    public EnsureSuccessStatusCodeAttribute(bool enabled) => Enabled = enabled;

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; }
}