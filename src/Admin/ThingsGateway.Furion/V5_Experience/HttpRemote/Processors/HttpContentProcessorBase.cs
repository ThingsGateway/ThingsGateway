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

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="IHttpContentProcessor" /> 内容处理器基类
/// </summary>
public abstract class HttpContentProcessorBase : IHttpContentProcessor
{
    /// <inheritdoc />
    public IServiceProvider? ServiceProvider { get; set; }

    /// <inheritdoc />
    public abstract bool CanProcess(object? rawContent, string contentType);

    /// <inheritdoc />
    public abstract HttpContent? Process(object? rawContent, string contentType, Encoding? encoding);

    /// <summary>
    ///     尝试解析 <see cref="HttpContent" /> 类型
    /// </summary>
    /// <param name="rawContent">原始请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="encoding">内容编码</param>
    /// <param name="httpContent">
    ///     <see cref="HttpContent" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpContent" />
    /// </returns>
    public virtual bool TryProcess([NotNullWhen(false)] object? rawContent, string contentType, Encoding? encoding,
        out HttpContent? httpContent)
    {
        switch (rawContent)
        {
            case null:
                httpContent = null;
                return true;
            case HttpContent content:
                // 设置 Content-Type
                content.Headers.ContentType ??=
                    new MediaTypeHeaderValue(contentType) { CharSet = encoding?.BodyName };

                httpContent = content;
                return true;
            default:
                httpContent = null;
                return false;
        }
    }
}