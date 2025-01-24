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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using System.Net.Mime;

using ThingsGateway.AspNetCore.Extensions;
using ThingsGateway.Extensions;

using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="HttpContext" /> 转发构建器
/// </summary>
public sealed class HttpContextForwardBuilder
{
    /// <summary>
    ///     <see cref="IActionResultContentConverter" /> 实例
    /// </summary>
    internal static readonly Lazy<IActionResultContentConverter> _actionResultContentConverterInstance =
        new(() => new IActionResultContentConverter());

    /// <summary>
    ///     忽略在转发时需要跳过的请求标头列表
    /// </summary>
    internal static HashSet<string> _ignoreRequestHeaders =
    [
        Constants.X_FORWARD_TO_HEADER, "Host", "Accept", "Accept-CH", "Accept-Charset", "Accept-Encoding",
        "Accept-Language", "Accept-Patch", "Accept-Post", "Accept-Ranges"
    ];

    /// <summary>
    ///     <inheritdoc cref="HttpContextForwardBuilder" />
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="httpMethod">转发方式</param>
    /// <param name="requestUri">转发地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址。</param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    internal HttpContextForwardBuilder(HttpContext? httpContext, HttpMethod httpMethod, Uri? requestUri = null,
        HttpContextForwardOptions? forwardOptions = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);
        ArgumentNullException.ThrowIfNull(httpContext);

        HttpContext = httpContext;
        Method = httpMethod;

        RequestUri = GetTargetUri(httpContext, requestUri);
        ForwardOptions = GetForwardOptions(httpContext, forwardOptions);
    }

    /// <summary>
    ///     转发地址
    /// </summary>
    public Uri? RequestUri { get; }

    /// <summary>
    ///     转发方式
    /// </summary>
    public HttpMethod Method { get; }

    /// <inheritdoc cref="Microsoft.AspNetCore.Http.HttpContext" />
    public HttpContext HttpContext { get; }

    /// <inheritdoc cref="HttpContextForwardOptions" />
    public HttpContextForwardOptions ForwardOptions { get; }

    /// <summary>
    ///     获取目标地址
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="requestUri">请求地址。若为空则尝试从请求标头 <c>X-Forward-To</c> 中获取目标地址。</param>
    /// <returns>
    ///     <see cref="Uri" />
    /// </returns>
    internal static Uri? GetTargetUri(HttpContext httpContext, Uri? requestUri = null)
    {
        // 空检查
        if (requestUri is not null)
        {
            return requestUri;
        }

        // 尝试从请求标头 X-Forward-To 中获取目标地址
        var targetUrl = httpContext.Request.Headers[Constants.X_FORWARD_TO_HEADER].ToString();

        return string.IsNullOrWhiteSpace(targetUrl) ? null : new Uri(targetUrl, UriKind.RelativeOrAbsolute);
    }

    /// <summary>
    ///     获取 <see cref="HttpContextForwardOptions" /> 实例
    /// </summary>
    /// <param name="httpContext">
    ///     <see cref="HttpContext" />
    /// </param>
    /// <param name="forwardOptions">
    ///     <see cref="HttpContextForwardOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpContextForwardOptions" />
    /// </returns>
    internal static HttpContextForwardOptions GetForwardOptions(HttpContext httpContext,
        HttpContextForwardOptions? forwardOptions) =>
        forwardOptions ??
        httpContext.RequestServices.GetService<IOptions<HttpContextForwardOptions>>()
            ?.Value ?? new HttpContextForwardOptions();

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    internal HttpRequestBuilder Build(Action<HttpRequestBuilder>? configure = null)
    {
        // 初始化 HttpRequestBuilder 实例
        var httpRequestBuilder = HttpRequestBuilder.Create(Method, RequestUri, configure)
            .AddHttpContentConverters(() => [_actionResultContentConverterInstance.Value]).DisableCache();

        // 复制查询参数和路由参数
        CopyQueryAndRouteValues(httpRequestBuilder);

        // 复制请求标头
        CopyHeaders(httpRequestBuilder);

        // 复制请求内容
        CopyBodyAsync(httpRequestBuilder).Wait(HttpContext.RequestAborted);

        return httpRequestBuilder;
    }

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    internal async Task<HttpRequestBuilder> BuildAsync(Action<HttpRequestBuilder>? configure = null)
    {
        // 初始化 HttpRequestBuilder 实例
        var httpRequestBuilder = HttpRequestBuilder.Create(Method, RequestUri, configure)
            .AddHttpContentConverters(() => [new IActionResultContentConverter()]).DisableCache();

        // 复制查询参数和路由参数
        CopyQueryAndRouteValues(httpRequestBuilder);

        // 复制请求标头
        CopyHeaders(httpRequestBuilder);

        // 复制请求内容
        await CopyBodyAsync(httpRequestBuilder).ConfigureAwait(false);

        return httpRequestBuilder;
    }

    /// <summary>
    ///     复制查询参数和路由参数
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    internal void CopyQueryAndRouteValues(HttpRequestBuilder httpRequestBuilder)
    {
        // 获取查询参数集合
        var queryValues = HttpContext.Request.Query.ToArray();

        // 空检查
        if (queryValues.Length > 0)
        {
            // 检查是否转发查询参数（URL 参数）
            if (ForwardOptions.WithQueryParameters)
            {
                // 将查询参数添加到查询参数集合中
                httpRequestBuilder.WithQueryParameters(queryValues);
            }

            // 将查询参数添加到路径参数集合中
            httpRequestBuilder.WithPathParameters(queryValues);
        }

        // 获取路由参数集合
        var routeValues = HttpContext.Request.RouteValues;

        // 空检查
        if (routeValues.Count > 0)
        {
            // 将路由参数添加到路径参数集合中
            httpRequestBuilder.WithPathParameters(routeValues);
        }
    }

    /// <summary>
    ///     复制请求标头
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    internal void CopyHeaders(HttpRequestBuilder httpRequestBuilder)
    {
        // 获取 HttpRequest 实例
        var httpRequest = HttpContext.Request;

        // 添加原始请求地址标头
        httpRequestBuilder.WithHeader(Constants.X_ORIGINAL_URL_HEADER, httpRequest.GetFullRequestUrl(), replace: true);

        // 检查是否转发请求标头
        if (!ForwardOptions.WithRequestHeaders)
        {
            return;
        }

        // 初始化忽略在转发时需要跳过的请求标头列表
        var ignoreRequestHeaders =
            _ignoreRequestHeaders.ConcatIgnoreNull(ForwardOptions.IgnoreRequestHeaders).Distinct().ToArray();

        // 忽略特定请求标头列表
        httpRequestBuilder.WithHeaders(
            httpRequest.Headers.Where(u => !u.Key.IsIn(ignoreRequestHeaders, StringComparer.OrdinalIgnoreCase)),
            replace: true);

        // 检查是否需要重新设置 Host 请求标头
        if (ForwardOptions.ResetHostRequestHeader)
        {
            httpRequestBuilder.WithHeader(HeaderNames.Host,
                $"{RequestUri?.Host}{(RequestUri?.IsDefaultPort != true ? $":{RequestUri?.Port}" : string.Empty)}",
                replace: true);
        }
    }

    /// <summary>
    ///     复制请求内容
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    internal async Task CopyBodyAsync(HttpRequestBuilder httpRequestBuilder)
    {
        // 获取 HttpRequest 实例
        var httpRequest = HttpContext.Request;

        // 检查是否包含请求内容
        if (httpRequest.ContentLength is null or 0)
        {
            return;
        }

        // 获取原始内容类型
        var rawContentType = httpRequest.ContentType;

        // 空检查
        ArgumentException.ThrowIfNullOrEmpty(rawContentType);

        // 解析原始内容类型
        var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(rawContentType);

        // 获取内容类型
        var contentType = mediaTypeHeaderValue.MediaType;

        // 空检查
        ArgumentNullException.ThrowIfNull(contentType);

        // 读取 HttpContext 请求体流
        var bodyStream = await ReadBodyAsync(httpRequestBuilder).ConfigureAwait(false);

        // 检查请求内容类型是否为 multipart/form-data
        if (!contentType.IsIn([MediaTypeNames.Multipart.FormData], StringComparer.OrdinalIgnoreCase))
        {
            // 复制非多部分表单内容
            CopyNonMultipartFormData(bodyStream, contentType, httpRequestBuilder);
        }
        else
        {
            // 复制多部分表单内容
            await CopyMultipartFormDataAsync(bodyStream, rawContentType, httpRequestBuilder,
                HttpContext.RequestAborted).ConfigureAwait(false);
        }

        // 将请求体流的位置重置回起始位置
        httpRequest.Body.Position = 0;
    }

    /// <summary>
    ///     复制非多部分表单内容
    /// </summary>
    /// <param name="bodyStream">
    ///     <see cref="Stream" />
    /// </param>
    /// <param name="contentType">内容类型</param>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    internal static void CopyNonMultipartFormData(Stream bodyStream, string contentType,
        HttpRequestBuilder httpRequestBuilder)
    {
        // 初始化 StreamContent 实例
        var streamContent = new StreamContent(bodyStream);

        // 设置请求内容
        httpRequestBuilder.SetContent(streamContent, contentType);
    }

    /// <summary>
    ///     复制多部分表单内容
    /// </summary>
    /// <param name="bodyStream">
    ///     <see cref="Stream" />
    /// </param>
    /// <param name="rawContentType">原始内容类型</param>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task CopyMultipartFormDataAsync(Stream bodyStream, string rawContentType,
        HttpRequestBuilder httpRequestBuilder, CancellationToken cancellationToken)
    {
        // 获取多部分表单内容的边界；注意：这里可能出现前后双引号问题
        var boundary = rawContentType.Split('=')[1].TrimStart('"').TrimEnd('"');

        // 初始化 HttpMultipartFormDataBuilder 实例
        var httpMultipartFormDataBuilder =
            new HttpMultipartFormDataBuilder(httpRequestBuilder) { Boundary = boundary };

        // 初始化 MultipartReader 实例
        var multipartReader = new MultipartReader(boundary, bodyStream);

        while ((await multipartReader.ReadNextSectionAsync(cancellationToken).ConfigureAwait(false))is { } multipartSection)
        {
            // 检查当前节是否为文件节
            if (multipartSection.AsFileSection() is not null)
            {
                // 复制多部分表单内容文件节内容
                await CopyFileMultipartSectionAsync(multipartSection, httpMultipartFormDataBuilder, httpRequestBuilder,
                                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // 复制多部分表单内容文本节内容
                await CopyTextMultipartSectionAsync(multipartSection, httpMultipartFormDataBuilder, cancellationToken).ConfigureAwait(false);
            }

         
        }

        // 设置多部分表单内容
        httpRequestBuilder.SetMultipartContent(httpMultipartFormDataBuilder);
    }

    /// <summary>
    ///     复制多部分表单内容文本节内容
    /// </summary>
    /// <param name="multipartSection">
    ///     <see cref="MultipartSection" />
    /// </param>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task CopyTextMultipartSectionAsync(MultipartSection multipartSection,
        HttpMultipartFormDataBuilder httpMultipartFormDataBuilder, CancellationToken cancellationToken)
    {
        // 获取 ContentDispositionHeaderValue 实例
        var contentDispositionHeaderValue = multipartSection.GetContentDispositionHeader();

        // 获取表单名称
        var name = contentDispositionHeaderValue?.Name.Value;

        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 读取文本
        var text = await multipartSection.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // 添加文本
        httpMultipartFormDataBuilder.AddText(text, name);
    }

    /// <summary>
    ///     复制多部分表单内容文件节内容
    /// </summary>
    /// <param name="multipartSection">
    ///     <see cref="MultipartSection" />
    /// </param>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task CopyFileMultipartSectionAsync(MultipartSection multipartSection,
            HttpMultipartFormDataBuilder httpMultipartFormDataBuilder, HttpRequestBuilder httpRequestBuilder,
            CancellationToken cancellationToken)
    {
        // 初始化 MemoryStream 实例
        var memoryStream = new MemoryStream();

        // 将多部分表单内容流复制到内存流
        await multipartSection.Body.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);

        // 将内存流的位置重置到起始位置
        memoryStream.Position = 0;

        // 将 multipartSection 转换为 MultipartSection 类型
        var fileMultipartSection = multipartSection.AsFileSection()!;

        // 添加文件流
        httpMultipartFormDataBuilder.AddStream(memoryStream, fileMultipartSection.Name, fileMultipartSection.FileName);

        // 添加文件流到请求结束时需要释放的集合中
        httpRequestBuilder.AddDisposable(memoryStream);
    }

    /// <summary>
    ///     读取 <see cref="HttpContext" /> 请求体流
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="Stream" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal async Task<Stream> ReadBodyAsync(HttpRequestBuilder httpRequestBuilder)
    {
        try
        {
            // 获取 HttpRequest 实例
            var httpRequest = HttpContext.Request;

            // 将请求体流的位置重置回起始位置
            httpRequest.Body.Position = 0;

            // 初始化 MemoryStream 实例
            var memoryStream = new MemoryStream();

            // 将请求体流复制到内存流
            await httpRequest.Body.CopyToAsync(memoryStream, HttpContext.RequestAborted).ConfigureAwait(false);

            // 将内存流的位置重置到起始位置
            memoryStream.Position = 0;

            // 添加内存流到请求结束时需要释放的集合中
            httpRequestBuilder.AddDisposable(memoryStream);

            return memoryStream;
        }
        // 捕获不支持 Body 流重复读异常
        catch (NotSupportedException e)
        {
            throw new InvalidOperationException(
                "Please ensure that the `app.UseEnableBuffering()` middleware is registered.", e);
        }
    }
}