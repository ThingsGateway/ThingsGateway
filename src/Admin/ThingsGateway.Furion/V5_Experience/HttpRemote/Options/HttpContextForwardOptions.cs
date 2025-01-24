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

using Microsoft.AspNetCore.Http;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="HttpContext" /> 转发配置选项
/// </summary>
public sealed class HttpContextForwardOptions
{
    /// <summary>
    ///     是否转发查询参数（URL 参数）
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithQueryParameters { get; set; } = true;

    /// <summary>
    ///     是否转发请求标头
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithRequestHeaders { get; set; } = true;

    /// <summary>
    ///     是否转发响应状态码
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithResponseStatusCode { get; set; } = true;

    /// <summary>
    ///     是否转发响应标头
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithResponseHeaders { get; set; } = true;

    /// <summary>
    ///     是否转发响应内容标头
    /// </summary>
    /// <remarks>默认值为：<c>true</c>。</remarks>
    public bool WithResponseContentHeaders { get; set; } = true;

    /// <summary>
    ///     是否重新设置 <c>Host</c> 请求标头
    /// </summary>
    /// <remarks>在一些目标服务器中，可能需要校验该请求标头。默认值为：<c>false</c>。</remarks>
    public bool ResetHostRequestHeader { get; set; }

    /// <summary>
    ///     忽略在转发时需要跳过的请求标头列表
    /// </summary>
    public string[]? IgnoreRequestHeaders { get; set; }

    /// <summary>
    ///     忽略在转发时需要跳过的响应标头列表
    /// </summary>
    /// <remarks>
    ///     若响应标头中包含 <c>Content-Length</c>，且其值与实际响应体大小不符，则可能引发“Error while copying content to a
    ///     stream.”。忽略此标头有助于解决因长度不匹配引起的错误。
    /// </remarks>
    public string[]? IgnoreResponseHeaders { get; set; }

    /// <summary>
    ///     用于在转发响应之前执行自定义操作
    /// </summary>
    public Action<HttpContext, HttpResponseMessage>? OnForward { get; set; }
}