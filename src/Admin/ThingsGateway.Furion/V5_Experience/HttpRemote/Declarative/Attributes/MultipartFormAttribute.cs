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
///     HTTP 声明式多部分表单内容特性
/// </summary>
/// <remarks>需配合 <see cref="MultipartAttribute" /> 使用。</remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class MultipartFormAttribute : Attribute
{
    /// <summary>
    ///     <inheritdoc cref="MultipartFormAttribute" />
    /// </summary>
    public MultipartFormAttribute()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="MultipartFormAttribute" />
    /// </summary>
    /// <param name="boundary">多部分表单内容的边界</param>
    public MultipartFormAttribute(string? boundary) => Boundary = boundary;

    /// <summary>
    ///     多部分表单内容的边界
    /// </summary>
    public string? Boundary { get; set; } = $"--------------------------{DateTime.Now.Ticks:x}";

    /// <summary>
    ///     是否移除默认的多部分内容的 <c>Content-Type</c>
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool OmitContentType { get; set; } = true;
}