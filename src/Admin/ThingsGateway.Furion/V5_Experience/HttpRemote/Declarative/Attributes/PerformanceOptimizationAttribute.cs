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
///     HTTP 声明式启用性能优化特性
/// </summary>
/// <remarks>当需要返回 <see cref="Stream" /> 内容或进行 <c>HttpContext</c> 网页转发时，请勿启用此配置，因为流会因压缩而变得不可读，同时该配置也不适用于网页转发的场景。</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
public sealed class PerformanceOptimizationAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="PerformanceOptimizationAttribute" />
    /// </summary>
    /// <remarks>当需要返回 <see cref="Stream" /> 内容或进行 <c>HttpContext</c> 网页转发时，请勿启用此配置，因为流会因压缩而变得不可读，同时该配置也不适用于网页转发的场景。</remarks>
    public PerformanceOptimizationAttribute()
        : this(true)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="PerformanceOptimizationAttribute" />
    /// </summary>
    /// <remarks>当需要返回 <see cref="Stream" /> 内容或进行 <c>HttpContext</c> 网页转发时，请勿启用此配置，因为流会因压缩而变得不可读，同时该配置也不适用于网页转发的场景。</remarks>
    /// <param name="enabled">是否启用</param>
    public PerformanceOptimizationAttribute(bool enabled) => Enabled = enabled;

    /// <summary>
    ///     是否启用
    /// </summary>
    public bool Enabled { get; set; }
}