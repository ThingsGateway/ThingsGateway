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

using System.Text;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="MultipartFormDataContent" /> 条目
/// </summary>
internal sealed class MultipartFormDataItem
{
    /// <summary>
    ///     <inheritdoc cref="MultipartFormDataItem" />
    /// </summary>
    /// <param name="name">表单名称</param>
    internal MultipartFormDataItem(string name)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }

    /// <summary>
    ///     表单名称
    /// </summary>
    internal string Name { get; }

    /// <summary>
    ///     内容类型
    /// </summary>
    internal string? ContentType { get; init; }

    /// <summary>
    ///     内容编码
    /// </summary>
    internal Encoding? ContentEncoding { get; init; }

    /// <summary>
    ///     原始请求内容
    /// </summary>
    /// <remarks>此属性值最终将转换为 <see cref="HttpContent" /> 类型实例。</remarks>
    internal object? RawContent { get; init; }

    /// <summary>
    ///     文件的名称
    /// </summary>
    internal string? FileName { get; init; }
}