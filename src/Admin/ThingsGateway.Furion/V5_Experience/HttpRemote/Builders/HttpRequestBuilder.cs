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

using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;

using ThingsGateway.Extensions;

using CacheControlHeaderValue = System.Net.Http.Headers.CacheControlHeaderValue;
using StringWithQualityHeaderValue = System.Net.Http.Headers.StringWithQualityHeaderValue;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     <see cref="StringContentForFormUrlEncodedContentProcessor" /> 实例
    /// </summary>
    internal static readonly Lazy<StringContentForFormUrlEncodedContentProcessor>
        _stringContentForFormUrlEncodedContentProcessorInstance =
            new(() => new StringContentForFormUrlEncodedContentProcessor());

    /// <summary>
    ///     <inheritdoc cref="HttpRequestBuilder" />
    /// </summary>
    /// <param name="httpMethod">请求方式</param>
    /// <param name="requestUri">请求地址</param>
    internal HttpRequestBuilder(HttpMethod httpMethod, Uri? requestUri)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMethod);

        Method = httpMethod;
        RequestUri = requestUri;
    }

    /// <summary>
    ///     构建 <see cref="HttpRequestMessage" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="httpContentProcessorFactory">
    ///     <see cref="IHttpContentProcessorFactory" />
    /// </param>
    /// <param name="clientBaseAddress">客户端基地址</param>
    /// <returns>
    ///     <see cref="HttpRequestMessage" />
    /// </returns>
    internal HttpRequestMessage Build(HttpRemoteOptions httpRemoteOptions,
        IHttpContentProcessorFactory httpContentProcessorFactory, Uri? clientBaseAddress)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);
        ArgumentNullException.ThrowIfNull(httpContentProcessorFactory);
        ArgumentNullException.ThrowIfNull(Method);

        // 构建最终的请求地址
        var finalRequestUri = BuildFinalRequestUri(clientBaseAddress, httpRemoteOptions.Configuration);

        // 初始化 HttpRequestMessage 实例
        var httpRequestMessage = new HttpRequestMessage(Method, finalRequestUri);

        // 启用性能优化
        EnablePerformanceOptimization(httpRequestMessage);

        // 追加请求标头
        AppendHeaders(httpRequestMessage);

        // 追加 Cookies
        AppendCookies(httpRequestMessage);

        // 移除 Cookies
        RemoveCookies(httpRequestMessage);

        // 移除请求标头
        RemoveHeaders(httpRequestMessage);

        // 构建并设置指定的 HttpRequestMessage 请求消息的内容
        BuildAndSetContent(httpRequestMessage, httpContentProcessorFactory, httpRemoteOptions);

        // 追加 HttpRequestMessage 请求属性集合
        AppendProperties(httpRequestMessage);

        return httpRequestMessage;
    }

    /// <summary>
    ///     构建最终的请求地址
    /// </summary>
    /// <param name="clientBaseAddress">客户端基地址</param>
    /// <param name="configuration">
    ///     <see cref="IConfiguration" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal string BuildFinalRequestUri(Uri? clientBaseAddress, IConfiguration? configuration)
    {
        // 替换路径或配置参数，处理非标准 HTTP URI 的应用场景（如 {url}），此时需优先解决路径或配置参数问题
        var newRequestUri = RequestUri is null or { OriginalString: null }
            ? RequestUri
            : new Uri(ReplacePlaceholders(RequestUri.OriginalString, configuration), UriKind.RelativeOrAbsolute);

        // 初始化带局部 BaseAddress 的请求地址
        var requestUriWithBaseAddress = BaseAddress is null
            ? newRequestUri!
            : new Uri(BaseAddress, newRequestUri!);

        // 初始化 UriBuilder 实例
        var uriBuilder = new UriBuilder(clientBaseAddress is null
            ? requestUriWithBaseAddress
            : new Uri(clientBaseAddress, requestUriWithBaseAddress));

        // 追加片段标识符
        AppendFragment(uriBuilder);

        // 追加查询参数
        AppendQueryParameters(uriBuilder);

        // 替换路径或配置参数
        var finalRequestUri = ReplacePlaceholders(uriBuilder.Uri.ToString(), configuration);

        return finalRequestUri;
    }

    /// <summary>
    ///     追加片段标识符
    /// </summary>
    /// <param name="uriBuilder">
    ///     <see cref="UriBuilder" />
    /// </param>
    internal void AppendFragment(UriBuilder uriBuilder)
    {
        // 空检查
        if (string.IsNullOrWhiteSpace(Fragment))
        {
            return;
        }

        uriBuilder.Fragment = Fragment;
    }

    /// <summary>
    ///     追加查询参数
    /// </summary>
    /// <param name="uriBuilder">
    ///     <see cref="UriBuilder" />
    /// </param>
    internal void AppendQueryParameters(UriBuilder uriBuilder)
    {
        // 空检查
        if (QueryParameters.IsNullOrEmpty())
        {
            return;
        }

        // 解析 URL 中的查询字符串为键值对列表
        var queryParameters = uriBuilder.Query.ParseFormatKeyValueString(['&'], '?');

        // 追加查询参数
        foreach (var (key, values) in QueryParameters)
        {
            queryParameters.AddRange(values.Select(value =>
                new KeyValuePair<string, string?>(key, value)));
        }

        // 构建查询字符串赋值给 UriBuilder 的 Query 属性
        uriBuilder.Query =
            "?" + string.Join('&',
                queryParameters
                    // 过滤已标记为移除的查询参数
                    .WhereIf(QueryParametersToRemove is { Count: > 0 },
                        u => QueryParametersToRemove?.TryGetValue(u.Key, out _) == false)
                    .Select(u => $"{u.Key}={u.Value}"));
    }

    /// <summary>
    ///     替换路径或配置参数
    /// </summary>
    /// <param name="originalUri">源请求地址</param>
    /// <param name="configuration">
    ///     <see cref="IConfiguration" />
    /// </param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    internal string ReplacePlaceholders(string originalUri, IConfiguration? configuration)
    {
        var newUri = originalUri;

        // 空检查
        if (!PathParameters.IsNullOrEmpty())
        {
            newUri = newUri.ReplacePlaceholders(PathParameters);
        }

        // 空检查
        if (!ObjectPathParameters.IsNullOrEmpty())
        {
            newUri = ObjectPathParameters.Aggregate(newUri,
                (current, objectPathParameter) =>
                    current.ReplacePlaceholders(objectPathParameter.Value, objectPathParameter.Key));
        }

        // 替换配置参数
        newUri = newUri.ReplacePlaceholders(configuration);

        return newUri!;
    }

    /// <summary>
    ///     追加请求标头
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void AppendHeaders(HttpRequestMessage httpRequestMessage)
    {
        // 添加 Host 标头
        if (AutoSetHostHeaderEnabled)
        {
            httpRequestMessage.Headers.Host =
                $"{httpRequestMessage.RequestUri?.Host}{(httpRequestMessage.RequestUri?.IsDefaultPort != true ? $":{httpRequestMessage.RequestUri?.Port}" : string.Empty)}";
        }

        // 添加跟踪标识
        if (!string.IsNullOrWhiteSpace(TraceIdentifier))
        {
            httpRequestMessage.Headers.TryAddWithoutValidation(Constants.X_TRACE_ID_HEADER, TraceIdentifier);
        }

        // 添加身份认证
        AppendAuthentication(httpRequestMessage);

        // 设置禁用 HTTP 缓存
        if (DisableCacheEnabled)
        {
            httpRequestMessage.Headers.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
        }

        // 空检查
        if (Headers.IsNullOrEmpty())
        {
            return;
        }

        // 遍历请求标头集合并追加到 HttpRequestMessage.Headers 中
        foreach (var (key, values) in Headers)
        {
            httpRequestMessage.Headers.TryAddWithoutValidation(key, values);
        }
    }

    /// <summary>
    ///     添加身份认证
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void AppendAuthentication(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (AuthenticationHeader is null)
        {
            return;
        }

        // 检查是否是 Digest 摘要认证
        if (AuthenticationHeader.Scheme != Constants.DIGEST_AUTHENTICATION_SCHEME)
        {
            httpRequestMessage.Headers.Authorization = AuthenticationHeader;

            return;
        }

        // 检查参数是否包含预设的 Digest 授权凭证
        const string separator = "|:|";
        if (AuthenticationHeader.Parameter?.Contains(separator) != true)
        {
            return;
        }

        // 分割预设的用户名和密码
        var parts = AuthenticationHeader.Parameter.Split(separator);

        // 获取 Digest 摘要认证授权凭证
        var digestCredentials =
            DigestCredentials.GetDigestCredentials(httpRequestMessage.RequestUri?.OriginalString, parts[0], parts[1],
                Method!);

        // 设置身份验证凭据请求授权标头
        httpRequestMessage.Headers.Authorization =
            new AuthenticationHeaderValue(Constants.DIGEST_AUTHENTICATION_SCHEME, digestCredentials);
    }

    /// <summary>
    ///     移除请求标头
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void RemoveHeaders(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (HeadersToRemove.IsNullOrEmpty())
        {
            return;
        }

        // 遍历请求标头集合并从 HttpRequestMessage.Headers 中移除
        foreach (var headerName in HeadersToRemove)
        {
            httpRequestMessage.Headers.Remove(headerName);
        }
    }

    /// <summary>
    ///     启用性能优化
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void EnablePerformanceOptimization(HttpRequestMessage httpRequestMessage)
    {
        if (!PerformanceOptimizationEnabled)
        {
            return;
        }

        // 设置 Accept 头，表示可以接受任何类型的内容
        httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

        // 添加 Accept-Encoding 头，支持 gzip、deflate 以及 Brotli 压缩算法
        // 这样服务器可以根据情况选择最合适的压缩方式发送响应，从而减少传输的数据量
        httpRequestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        httpRequestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        httpRequestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

        // 设置 Connection 头为 keep-alive，允许重用 TCP 连接，避免每次请求都重新建立连接带来的开销
        httpRequestMessage.Headers.ConnectionClose = false;
    }

    /// <summary>
    ///     追加 Cookies
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void AppendCookies(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (Cookies.IsNullOrEmpty())
        {
            return;
        }

        httpRequestMessage.Headers.TryAddWithoutValidation(HeaderNames.Cookie,
            string.Join("; ", Cookies.Select(u => $"{u.Key}={u.Value.EscapeDataString(true)}")));
    }

    /// <summary>
    ///     移除 Cookies
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void RemoveCookies(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (CookiesToRemove.IsNullOrEmpty())
        {
            return;
        }

        // 获取已经设置的 Cookies
        if (!httpRequestMessage.Headers.TryGetValues(HeaderNames.Cookie, out var cookies))
        {
            return;
        }

        // 解析 Cookies 标头值
        var cookieList = CookieHeaderValue.ParseList(cookies.ToList());

        // 空检查
        if (cookieList.Count == 0)
        {
            return;
        }

        // 重新设置 Cookies
        httpRequestMessage.Headers.Remove(HeaderNames.Cookie);
        httpRequestMessage.Headers.TryAddWithoutValidation(HeaderNames.Cookie,
            // 过滤已标记为移除的 Cookie 键
            string.Join("; ", cookieList.WhereIf(CookiesToRemove is { Count: > 0 },
                    u => CookiesToRemove?.TryGetValue(u.Name.ToString(), out _) == false)
                .Select(u => $"{u.Name}={u.Value}")));
    }

    /// <summary>
    ///     构建并设置指定的 <see cref="HttpRequestMessage" /> 请求消息的内容
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="httpContentProcessorFactory">
    ///     <see cref="IHttpContentProcessorFactory" />
    /// </param>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    internal void BuildAndSetContent(HttpRequestMessage httpRequestMessage,
        IHttpContentProcessorFactory httpContentProcessorFactory, HttpRemoteOptions httpRemoteOptions)
    {
        // 获取自定义的 IHttpContentProcessor 集合
        var processors = HttpContentProcessorProviders?.SelectMany(u => u.Invoke()).ToArray();

        // 构建 MultipartFormDataContent 请求内容
        if (MultipartFormDataBuilder is not null)
        {
            ContentType = MediaTypeNames.Multipart.FormData;
            RawContent = MultipartFormDataBuilder.Build(httpRemoteOptions, httpContentProcessorFactory, processors);
        }

        // 检查是否设置了内容
        if (RawContent is null)
        {
            return;
        }

        // 设置默认的内容类型
        SetDefaultContentType(httpRemoteOptions.DefaultContentType);

        // 构建 HttpContent 实例
        var httpContent = httpContentProcessorFactory.Build(RawContent, ContentType!, ContentEncoding, processors);

        // 调用用于处理在设置请求消息的内容时的操作
        OnPreSetContent?.Invoke(httpContent);

        // 设置 HttpRequestMessage 请求消息的内容
        httpRequestMessage.Content = httpContent;
    }

    /// <summary>
    ///     追加 <see cref="HttpRequestMessage" /> 请求属性集合
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal void AppendProperties(HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (Properties.Count > 0)
        {
            // 注意：httpRequestMessage.Properties 已过时，使用 Options 替代
            httpRequestMessage.Options.AddOrUpdate(Properties);
        }

        // 检查是否禁用全局请求分析工具
        if (__Disabled_Profiler__)
        {
            httpRequestMessage.Options.AddOrUpdate(Constants.DISABLED_PROFILER_KEY, "TRUE");
        }
    }

    /// <summary>
    ///     设置默认的内容类型
    /// </summary>
    /// <param name="defaultContentType">默认请求内容类型</param>
    internal void SetDefaultContentType(string? defaultContentType)
    {
        // 空检查
        if (!string.IsNullOrWhiteSpace(ContentType))
        {
            return;
        }

        ContentType = RawContent switch
        {
            JsonContent => MediaTypeNames.Application.Json,
            FormUrlEncodedContent => MediaTypeNames.Application.FormUrlEncoded,
            (byte[] or Stream or ByteArrayContent or StreamContent or ReadOnlyMemoryContent or ReadOnlyMemory<byte>)
                and not StringContent => MediaTypeNames.Application
                    .Octet,
            MultipartContent => MediaTypeNames.Multipart.FormData,
            _ => defaultContentType ?? Constants.TEXT_PLAIN_MIME_TYPE
        };
    }
}