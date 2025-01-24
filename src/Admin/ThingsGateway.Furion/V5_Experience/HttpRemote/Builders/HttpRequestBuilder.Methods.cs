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

using Microsoft.Net.Http.Headers;

using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

using ThingsGateway.Extensions;
using ThingsGateway.Utilities;

using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     线程锁
    /// </summary>
    /// <remarks>用于保证 <see cref="AddStringContentForFormUrlEncodedContentProcessor" /> 方法调用是线程安全的。</remarks>
    internal readonly object _lock = new();

    /// <summary>
    ///     表示是否已添加了 <see cref="StringContentForFormUrlEncodedContentProcessor" /> 处理器
    /// </summary>
    internal bool _isAddedStringContentForFormUrlEncodedContentProcessor;

    /// <summary>
    ///     设置跟踪标识
    /// </summary>
    /// <param name="traceIdentifier">设置跟踪标识</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetTraceIdentifier(string traceIdentifier, bool escape = false)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(traceIdentifier);

        TraceIdentifier = traceIdentifier.EscapeDataString(escape);

        return this;
    }

    /// <summary>
    ///     设置内容类型
    /// </summary>
    /// <param name="contentType">内容类型</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetContentType(string contentType)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        // 解析内容类型字符串
        var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);

        ContentType = mediaTypeHeaderValue.MediaType;

        // 检查是否包含 charset 设置
        if (!string.IsNullOrWhiteSpace(mediaTypeHeaderValue.CharSet))
        {
            SetContentEncoding(mediaTypeHeaderValue.CharSet);
        }

        return this;
    }

    /// <summary>
    ///     设置内容编码
    /// </summary>
    /// <param name="encoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetContentEncoding(Encoding encoding)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(encoding);

        ContentEncoding = encoding;

        return this;
    }

    /// <summary>
    ///     设置内容编码
    /// </summary>
    /// <param name="encodingName">内容编码名</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetContentEncoding(string encodingName)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(encodingName);

        SetContentEncoding(Encoding.GetEncoding(encodingName));

        return this;
    }

    /// <summary>
    ///     设置 JSON 内容
    /// </summary>
    /// <param name="rawJson">JSON 字符串/原始对象</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="JsonException"></exception>
    public HttpRequestBuilder SetJsonContent(object? rawJson, Encoding? contentEncoding = null)
    {
        // 检查是否是字符串类型
        if (rawJson is not string rawString)
        {
            return SetContent(rawJson, MediaTypeNames.Application.Json, contentEncoding);
        }

        // 尝试验证并获取 JsonDocument 实例（需 using）
        var jsonDocument = JsonUtility.Parse(rawString);

        // 添加请求结束时需要释放的对象
        AddDisposable(jsonDocument);

        return SetContent(jsonDocument, MediaTypeNames.Application.Json, contentEncoding);
    }

    /// <summary>
    ///     设置 HTML 内容
    /// </summary>
    /// <param name="htmlString">HTML 字符串</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetHtmlContent(string? htmlString, Encoding? contentEncoding = null) =>
        SetContent(htmlString, MediaTypeNames.Text.Html, contentEncoding);

    /// <summary>
    ///     设置 XML 内容
    /// </summary>
    /// <param name="xmlString">XML 字符串</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetXmlContent(string? xmlString, Encoding? contentEncoding = null) =>
        SetContent(xmlString, MediaTypeNames.Application.Xml, contentEncoding);

    /// <summary>
    ///     设置文本内容
    /// </summary>
    /// <param name="text">文本</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetTextContent(string? text, Encoding? contentEncoding = null) =>
        SetContent(text, MediaTypeNames.Text.Plain, contentEncoding);

    /// <summary>
    ///     设置原始字符串内容
    /// </summary>
    /// <remarks>字符串内容将被双引号包围并发送，格式如下：<c>"内容"</c>。</remarks>
    /// <param name="text">文本</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetRawStringContent(string text, string contentType, Encoding? contentEncoding = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        return SetContent(text.AddQuotes(), contentType, contentEncoding);
    }

    /// <summary>
    ///     设置 URL 编码表单内容
    /// </summary>
    /// <param name="rawObject">原始对象</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <param name="useStringContent">
    ///     是否使用 <see cref="StringContent" /> 构建
    ///     <see cref="FormUrlEncodedContent" />。默认 <c>false</c>。
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetFormUrlEncodedContent(object? rawObject, Encoding? contentEncoding = null,
        bool useStringContent = false)
    {
        SetContent(rawObject, MediaTypeNames.Application.FormUrlEncoded, contentEncoding);

        // 检查是否启用 StringContent 方式构建 application/x-www-form-urlencoded 请求内容
        if (useStringContent)
        {
            AddStringContentForFormUrlEncodedContentProcessor();
        }

        return this;
    }

    /// <summary>
    ///     设置请求内容
    /// </summary>
    /// <param name="rawContent">原始请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="contentEncoding">内容编码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetContent(object? rawContent, string? contentType = null,
        Encoding? contentEncoding = null)
    {
        // 空检查
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            // 解析内容类型字符串
            var mediaTypeHeaderValue = MediaTypeHeaderValue.Parse(contentType);

            // 禁止使用该方法设置 multipart/form-data 类型内容
            if (mediaTypeHeaderValue.MediaType == MediaTypeNames.Multipart.FormData &&
                rawContent is not MultipartContent)
            {
                throw new NotSupportedException(
                    $"The method does not support setting the request content type to `{MediaTypeNames.Multipart.FormData}`. Please use the `{nameof(SetMultipartContent)}` method instead. If you are using an HTTP declarative requests, define the parameter with the `Action<HttpMultipartFormDataBuilder>` type or annotate the parameter with the `MultipartAttribute`.");
            }
        }

        RawContent = rawContent;

        // 空检查
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            SetContentType(contentType);
        }

        // 空检查
        if (contentEncoding is not null)
        {
            SetContentEncoding(contentEncoding);
        }

        return this;
    }

    /// <summary>
    ///     设置多部分表单内容，请求类型为 <c>multipart/form-data</c>
    /// </summary>
    /// <remarks>
    ///     该操作将强制覆盖 <see cref="SetContent" />、<see cref="SetContentEncoding(System.Text.Encoding)" /> 和
    ///     <see cref="SetContentType" /> 设置的内容。
    /// </remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetMultipartContent(Action<HttpMultipartFormDataBuilder> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 初始化 HttpMultipartFormDataBuilder 实例
        var httpMultipartFormDataBuilder = new HttpMultipartFormDataBuilder(this);

        // 调用自定义配置委托
        configure.Invoke(httpMultipartFormDataBuilder);

        MultipartFormDataBuilder = httpMultipartFormDataBuilder;

        return this;
    }

    /// <summary>
    ///     设置多部分表单内容，请求类型为 <c>multipart/form-data</c>
    /// </summary>
    /// <remarks>
    ///     该操作将强制覆盖 <see cref="SetContent" />、<see cref="SetContentEncoding(System.Text.Encoding)" /> 和
    ///     <see cref="SetContentType" /> 设置的内容。
    /// </remarks>
    /// <param name="httpMultipartFormDataBuilder">
    ///     <see cref="HttpMultipartFormDataBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    internal HttpRequestBuilder SetMultipartContent(HttpMultipartFormDataBuilder httpMultipartFormDataBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpMultipartFormDataBuilder);

        MultipartFormDataBuilder = httpMultipartFormDataBuilder;

        return this;
    }

    /// <summary>
    ///     设置请求标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <param name="replace">是否替换已存在的请求标头。默认值为 <c>false</c>。</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithHeader(string key, object? value, bool escape = false, CultureInfo? culture = null,
        IEqualityComparer<string>? comparer = null, bool replace = false)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return WithHeaders(new Dictionary<string, object?> { { key, value } }, escape, culture, comparer, replace);
    }

    /// <summary>
    ///     设置请求标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="headers">请求标头集合</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <param name="replace">是否替换已存在的请求标头。默认值为 <c>false</c>。</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithHeaders(IDictionary<string, object?> headers, bool escape = false,
        CultureInfo? culture = null, IEqualityComparer<string>? comparer = null, bool replace = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(headers);

        // 初始化请求标头
        Headers ??= new Dictionary<string, List<string?>>(comparer);
        var objectHeaders = new Dictionary<string, List<object?>>(comparer);

        // 存在则合并否则添加
        objectHeaders.AddOrUpdate(Headers.ToDictionary(u => u.Key, object? (u) => u.Value), false);
        objectHeaders.AddOrUpdate(headers, false, replace);

        // 设置请求标头
        Headers = objectHeaders.ToDictionary(kvp => kvp.Key,
            kvp => kvp.Value.Select(u =>
                u.ToCultureString(culture ?? CultureInfo.InvariantCulture)?.EscapeDataString(escape)).ToList(),
            comparer);

        return this;
    }

    /// <summary>
    ///     设置请求标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="headerSource">请求标头源对象</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <param name="replace">是否替换已存在的请求标头。默认值为 <c>false</c>。</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithHeaders(object headerSource, bool escape = false, CultureInfo? culture = null,
        IEqualityComparer<string>? comparer = null, bool replace = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(headerSource);

        return WithHeaders(
            headerSource.ObjectToDictionary()!.ToDictionary(
                u => u.Key.ToCultureString(culture ?? CultureInfo.InvariantCulture)!, u => u.Value), escape, culture,
            comparer, replace);
    }

    /// <summary>
    ///     设置需要从请求中移除的标头
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="headerNames">请求标头名集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder RemoveHeaders(params string[] headerNames)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(headerNames);

        // 检查是否为空元素数组
        if (headerNames.Length == 0)
        {
            return this;
        }

        HeadersToRemove ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 逐条添加到集合中
        foreach (var headerName in headerNames)
        {
            if (!string.IsNullOrWhiteSpace(headerName))
            {
                HeadersToRemove.Add(headerName);
            }
        }

        return this;
    }

    /// <summary>
    ///     设置片段标识符
    /// </summary>
    /// <param name="fragment">片段标识符</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetFragment(string fragment, bool escape = false)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(fragment);

        Fragment = fragment.EscapeDataString(escape);

        return this;
    }

    /// <summary>
    ///     设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetTimeout(TimeSpan timeout)
    {
        Timeout = timeout;

        return this;
    }

    /// <summary>
    ///     设置超时时间
    /// </summary>
    /// <param name="timeoutMilliseconds">超时时间（毫秒）</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetTimeout(double timeoutMilliseconds)
    {
        // 检查参数是否小于 0
        if (timeoutMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds), "Timeout value must be non-negative.");
        }

        Timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

        return this;
    }

    /// <summary>
    ///     设置查询参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <param name="replace">是否替换已存在的查询参数。默认值为 <c>false</c>。</param>
    /// <param name="ignoreNullValues">是否忽略空值。默认值为 <c>false</c>。</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithQueryParameter(string key, object? value, bool escape = false,
        CultureInfo? culture = null, IEqualityComparer<string>? comparer = null, bool replace = false,
        bool ignoreNullValues = false)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return WithQueryParameters(new Dictionary<string, object?> { { key, value } }, escape, culture, comparer,
            replace, ignoreNullValues);
    }

    /// <summary>
    ///     设置查询参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameters">查询参数集合</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <param name="replace">是否替换已存在的查询参数。默认值为 <c>false</c>。</param>
    /// <param name="ignoreNullValues">是否忽略空值。默认值为 <c>false</c>。</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithQueryParameters(IDictionary<string, object?> parameters, bool escape = false,
        CultureInfo? culture = null, IEqualityComparer<string>? comparer = null, bool replace = false,
        bool ignoreNullValues = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameters);

        // 初始化查询参数
        QueryParameters ??= new Dictionary<string, List<string?>>(comparer);
        var objectQueryParameters = new Dictionary<string, List<object?>>(comparer);

        // 存在则合并否则添加
        objectQueryParameters.AddOrUpdate(QueryParameters.ToDictionary(u => u.Key, object? (u) => u.Value), false);
        objectQueryParameters.AddOrUpdate(parameters.WhereIf(ignoreNullValues, u => u.Value is not null).ToDictionary(),
            false, replace);

        // 设置查询参数
        QueryParameters = objectQueryParameters.ToDictionary(kvp => kvp.Key,
            kvp => kvp.Value.Select(u =>
                u.ToCultureString(culture ?? CultureInfo.InvariantCulture)?.EscapeDataString(escape)).ToList(),
            comparer);

        return this;
    }

    /// <summary>
    ///     设置查询参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameterSource">查询参数集合</param>
    /// <param name="prefix">参数前缀。对于对象类型可生成如 <c>prefix.Name=furion</c> 与 <c>prefix.Age=30</c> 参数格式。</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <param name="replace">是否替换已存在的查询参数。默认值为 <c>false</c>。</param>
    /// <param name="ignoreNullValues">是否忽略空值。默认值为 <c>false</c>。</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithQueryParameters(object parameterSource, string? prefix = null, bool escape = false,
        CultureInfo? culture = null, IEqualityComparer<string>? comparer = null, bool replace = false,
        bool ignoreNullValues = false)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameterSource);

        return WithQueryParameters(
            parameterSource.ObjectToDictionary()!.ToDictionary(
                u =>
                    $"{(string.IsNullOrWhiteSpace(prefix) ? null : $"{prefix}.")}{u.Key.ToCultureString(culture ?? CultureInfo.InvariantCulture)!}",
                u => u.Value), escape, culture, comparer, replace, ignoreNullValues);
    }

    /// <summary>
    ///     设置需要从 URL 中移除的查询参数集合
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameterNames">查询参数键集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder RemoveQueryParameters(params string[] parameterNames)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameterNames);

        // 检查是否为空元素数组
        if (parameterNames.Length == 0)
        {
            return this;
        }

        QueryParametersToRemove ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 逐条添加到集合中
        foreach (var parameterName in parameterNames)
        {
            if (!string.IsNullOrWhiteSpace(parameterName))
            {
                QueryParametersToRemove.Add(parameterName);
            }
        }

        return this;
    }

    /// <summary>
    ///     设置路径参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithPathParameter(string key, object? value, bool escape = false,
        CultureInfo? culture = null, IEqualityComparer<string>? comparer = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return WithPathParameters(new Dictionary<string, object?> { { key, value } }, escape, culture, comparer);
    }

    /// <summary>
    ///     设置路径参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameters">路径参数集合</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithPathParameters(IDictionary<string, object?> parameters,
        bool escape = false,
        CultureInfo? culture = null,
        IEqualityComparer<string>? comparer = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameters);

        PathParameters ??= new Dictionary<string, string?>(comparer);

        // 存在则更新否则添加
        PathParameters.AddOrUpdate(parameters.ToDictionary(u => u.Key,
            u => u.Value?.ToCultureString(culture ?? CultureInfo.InvariantCulture)?.EscapeDataString(escape),
            comparer));

        return this;
    }

    /// <summary>
    ///     设置路径参数
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="parameterSource">路径参数源对象</param>
    /// <param name="prefix">模板字符串前缀。若该参数值不为空，则支持 <c>{prefix.Prop.SubProp}</c> 对象路径方式。</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithPathParameters(object? parameterSource, string? prefix = null, bool escape = false,
        CultureInfo? culture = null,
        IEqualityComparer<string>? comparer = null)
    {
        // 检查是否设置了模板字符串前缀
        if (string.IsNullOrWhiteSpace(prefix))
        {
            // 空检查
            ArgumentNullException.ThrowIfNull(parameterSource);

            return WithPathParameters(
                parameterSource.ObjectToDictionary()!.ToDictionary(
                    u => u.Key.ToCultureString(culture ?? CultureInfo.InvariantCulture)!, u => u.Value), escape,
                culture, comparer);
        }

        ObjectPathParameters ??= new Dictionary<string, object?>();

        // 存在则更新否则添加
        ObjectPathParameters[prefix] = parameterSource;

        return this;
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookieHeaderValue">Cookie 标头值格式化字符串</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithCookie(string cookieHeaderValue)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(cookieHeaderValue);

        return WithCookies(cookieHeaderValue.ParseFormatKeyValueString([';']));
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithCookie(string key, object? value, bool escape = false, CultureInfo? culture = null,
        IEqualityComparer<string>? comparer = null)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return WithCookies(new Dictionary<string, object?> { { key, value } }, escape, culture, comparer);
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookies">Cookies 集合</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithCookies(IDictionary<string, object?> cookies,
        bool escape = false,
        CultureInfo? culture = null,
        IEqualityComparer<string>? comparer = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(cookies);

        Cookies ??= new Dictionary<string, string?>(comparer);

        // 存在则更新否则添加
        Cookies.AddOrUpdate(cookies.ToDictionary(u => u.Key,
            u => u.Value?.ToCultureString(culture ?? CultureInfo.InvariantCulture)?.EscapeDataString(escape),
            comparer));

        return this;
    }

    /// <summary>
    ///     设置 Cookies
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookieSource">Cookie 参数源对象</param>
    /// <param name="escape">是否转义字符串，默认 <c>false</c></param>
    /// <param name="culture">
    ///     <see cref="CultureInfo" />
    /// </param>
    /// <param name="comparer">
    ///     <see cref="IEqualityComparer{T}" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithCookies(object cookieSource, bool escape = false,
        CultureInfo? culture = null,
        IEqualityComparer<string>? comparer = null)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(cookieSource);

        // 存在则更新否则添加
        return WithCookies(
            cookieSource.ObjectToDictionary()!.ToDictionary(
                u => u.Key.ToCultureString(culture ?? CultureInfo.InvariantCulture)!, u => u.Value), escape, culture,
            comparer);
    }

    /// <summary>
    ///     需要从请求中移除的 Cookie 集合
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="cookieNames">Cookie 键集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder RemoveCookies(params string[] cookieNames)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(cookieNames);

        // 检查是否为空元素数组
        if (cookieNames.Length == 0)
        {
            return this;
        }

        CookiesToRemove ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 逐条添加到集合中
        foreach (var cookieName in cookieNames)
        {
            if (!string.IsNullOrWhiteSpace(cookieName))
            {
                CookiesToRemove.Add(cookieName);
            }
        }

        return this;
    }

    /// <summary>
    ///     设置 <see cref="HttpClient" /> 实例的配置名称
    /// </summary>
    /// <param name="httpClientName"><see cref="HttpClient" /> 实例的配置名称</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetHttpClientName(string? httpClientName)
    {
        HttpClientName = httpClientName;

        return this;
    }

    /// <summary>
    ///     设置响应内容最大缓存字节数
    /// </summary>
    /// <param name="maxResponseContentBufferSize">响应内容最大缓存字节数</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetMaxResponseContentBufferSize(long maxResponseContentBufferSize)
    {
        // 小于或等于 0 检查
        if (maxResponseContentBufferSize <= 0)
        {
            throw new ArgumentException("Max response content buffer size must be greater than 0.",
                nameof(maxResponseContentBufferSize));
        }

        MaxResponseContentBufferSize = maxResponseContentBufferSize;

        return this;
    }

    /// <summary>
    ///     设置 <see cref="HttpClient" /> 实例提供器
    /// </summary>
    /// <param name="configure"><inheritdoc cref="HttpClient" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetHttpClientProvider(Func<(HttpClient Instance, Action<HttpClient>? Release)> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        HttpClientProvider = configure;

        return this;
    }

    /// <summary>
    ///     添加 <see cref="IHttpContentProcessor" /> 请求内容处理器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure"><see cref="IHttpContentProcessor" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AddHttpContentProcessors(Func<IEnumerable<IHttpContentProcessor>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        HttpContentProcessorProviders ??= new List<Func<IEnumerable<IHttpContentProcessor>>>();

        HttpContentProcessorProviders.Add(configure);

        return this;
    }

    /// <summary>
    ///     添加 <see cref="IHttpContentConverter" /> 响应内容转换器
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure"><see cref="IHttpContentConverter" /> 实例提供器</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AddHttpContentConverters(Func<IEnumerable<IHttpContentConverter>> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        HttpContentConverterProviders ??= new List<Func<IEnumerable<IHttpContentConverter>>>();

        HttpContentConverterProviders.Add(configure);

        return this;
    }

    /// <summary>
    ///     设置用于处理在设置 <see cref="HttpRequestMessage" /> 的 <c>Content</c> 时的操作
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnPreSetContent(Action<HttpContent?> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        // 如果 OnPreSetContent 未设置则直接赋值
        if (OnPreSetContent is null)
        {
            OnPreSetContent = configure;
        }
        // 否则创建级联调用委托
        else
        {
            // 复制一个新的委托避免死循环
            var originalOnPreSetContent = OnPreSetContent;

            OnPreSetContent = content =>
            {
                originalOnPreSetContent.Invoke(content);
                configure.Invoke(content);
            };
        }

        return this;
    }

    /// <summary>
    ///     设置在发送 HTTP 请求之前执行的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnPreSendRequest(Action<HttpRequestMessage> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnPreSendRequest = configure;

        return this;
    }

    /// <summary>
    ///     设置在收到 HTTP 响应之后执行的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnPostReceiveResponse(Action<HttpResponseMessage> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnPostReceiveResponse = configure;

        return this;
    }

    /// <summary>
    ///     设置在发送 HTTP 请求发生异常时的操作
    /// </summary>
    /// <param name="configure">自定义配置委托</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetOnRequestFailed(Action<Exception, HttpResponseMessage?> configure)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(configure);

        OnRequestFailed = configure;

        return this;
    }

    /// <summary>
    ///     如果 HTTP 响应的 IsSuccessStatusCode 属性是 <c>false</c>，则引发异常。
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder EnsureSuccessStatusCode() => EnsureSuccessStatusCode(true);

    /// <summary>
    ///     设置是否如果 HTTP 响应的 IsSuccessStatusCode 属性是 <c>false</c>，则引发异常
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder EnsureSuccessStatusCode(bool enabled)
    {
        EnsureSuccessStatusCodeEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置 Basic 身份验证凭据请求授权标头
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AddBasicAuthentication(string username, string password)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // 将用户名和密码转换为 Base64 字符串
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

        AddAuthentication(new AuthenticationHeaderValue(Constants.BASIC_AUTHENTICATION_SCHEME, base64Credentials));

        return this;
    }

    /// <summary>
    ///     设置 JWT (JSON Web Token) 身份验证凭据请求授权标头
    /// </summary>
    /// <param name="jwtToken">JWT 字符串</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AddJwtBearerAuthentication(string jwtToken)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtToken);

        AddAuthentication(new AuthenticationHeaderValue(Constants.JWT_BEARER_AUTHENTICATION_SCHEME, jwtToken));

        return this;
    }

    /// <summary>
    ///     设置 Digest 摘要身份验证凭据请求授权标头
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">密码</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AddDigestAuthentication(string username, string password)
    {
        // 空检查
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        // 设置预设授权凭证
        AddAuthentication(new AuthenticationHeaderValue(Constants.DIGEST_AUTHENTICATION_SCHEME,
            $"{username}|:|{password}"));

        return this;
    }

    /// <summary>
    ///     设置身份验证凭据请求授权标头
    /// </summary>
    /// <param name="authenticationHeader">
    ///     <see cref="AuthenticationHeaderValue" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AddAuthentication(AuthenticationHeaderValue authenticationHeader)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(authenticationHeader);

        AuthenticationHeader = authenticationHeader;

        return this;
    }

    /// <summary>
    ///     设置禁用 HTTP 缓存
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder DisableCache() => DisableCache(true);

    /// <summary>
    ///     设置禁用 HTTP 缓存
    /// </summary>
    /// <param name="disabled">是否禁用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder DisableCache(bool disabled)
    {
        DisableCacheEnabled = disabled;

        return this;
    }

    /// <summary>
    ///     设置 HTTP 远程请求事件处理程序
    /// </summary>
    /// <param name="requestEventHandlerType">实现 <see cref="IHttpRequestEventHandler" /> 接口的类型</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    public HttpRequestBuilder SetEventHandler(Type requestEventHandlerType)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(requestEventHandlerType);

        // 检查类型是否实现了 IHttpRequestEventHandler 接口
        if (!typeof(IHttpRequestEventHandler).IsAssignableFrom(requestEventHandlerType))
        {
            throw new ArgumentException(
                $"`{requestEventHandlerType}` type is not assignable from `{typeof(IHttpRequestEventHandler)}`.",
                nameof(requestEventHandlerType));
        }

        RequestEventHandlerType = requestEventHandlerType;

        return this;
    }

    /// <summary>
    ///     设置 HTTP 远程请求事件处理程序
    /// </summary>
    /// <typeparam name="TRequestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </typeparam>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetEventHandler<TRequestEventHandler>()
        where TRequestEventHandler : IHttpRequestEventHandler =>
        SetEventHandler(typeof(TRequestEventHandler));

    /// <summary>
    ///     设置是否启用 <see cref="HttpClient" /> 的池化管理
    /// </summary>
    /// <remarks>
    ///     <para>用于在并发请求中复用同一个 <see cref="HttpClient" /> 实例。</para>
    ///     <para>注意：启用池化管理后，在请求完成之后需手动调用 <see cref="ReleaseResources" /> 方法释放资源。</para>
    /// </remarks>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder UseHttpClientPool()
    {
        HttpClientPoolingEnabled = true;

        return this;
    }

    /// <summary>
    ///     添加请求结束时需要释放的对象
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="disposable">
    ///     <see cref="IDisposable" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AddDisposable(IDisposable disposable)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(disposable);

        Disposables ??= [];
        Disposables.Add(disposable);

        return this;
    }

    /// <summary>
    ///     释放资源集合
    /// </summary>
    /// <remarks>包含自定义 <see cref="HttpClient" /> 实例和其他可释放对象集合。</remarks>
    public void ReleaseResources()
    {
        // 空检查
        if (HttpClientPooling is not null)
        {
            HttpClientPooling.Release?.Invoke(HttpClientPooling.Instance);
            HttpClientPooling = null;
        }

        // 释放可释放的对象集合
        ReleaseDisposables();
    }

    /// <summary>
    ///     设置模拟浏览器环境
    /// </summary>
    /// <remarks>设置此配置后，将在单次请求标头中添加主流浏览器的 <c>User-Agent</c> 值。</remarks>
    /// <param name="simulateMobile">是否模拟移动端，默认值为：<c>false</c>（即模拟桌面端）。</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SimulateBrowser(bool simulateMobile = false) =>
        WithHeader(HeaderNames.UserAgent,
            !simulateMobile ? Constants.USER_AGENT_OF_BROWSER : Constants.USER_AGENT_OF_MOBILE_BROWSER, replace: true);

    /// <summary>
    ///     添加状态码处理程序
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <param name="handler">自定义处理程序</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithStatusCodeHandler(object statusCode,
        Func<HttpResponseMessage, CancellationToken, Task> handler) =>
        WithStatusCodeHandler([statusCode], handler);

    /// <summary>
    ///     添加任何状态码处理程序
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="handler">自定义处理程序</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithAnyStatusCodeHandler(Func<HttpResponseMessage, CancellationToken, Task> handler) =>
        WithStatusCodeHandler(["*"], handler);

    /// <summary>
    ///     添加状态码处理程序
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="statusCodes">HTTP 状态码集合</param>
    /// <param name="handler">自定义处理程序</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithStatusCodeHandler(IEnumerable<object> statusCodes,
        Func<HttpResponseMessage, CancellationToken, Task> handler)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(statusCodes);

        // 检查数量是否为空
        if (statusCodes.TryGetCount(out var count) && count == 0)
        {
            throw new ArgumentException(
                "The status codes array cannot be empty. At least one status code must be provided.",
                nameof(statusCodes));
        }

        // 空检查
        ArgumentNullException.ThrowIfNull(handler);

        StatusCodeHandlers ??=
            new Dictionary<IEnumerable<object>, Func<HttpResponseMessage, CancellationToken, Task>>();

        StatusCodeHandlers[statusCodes] = handler;

        return this;
    }

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder Profiler() => Profiler(true);

    /// <summary>
    ///     设置是否启用请求分析工具
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder Profiler(bool enabled)
    {
        ProfilerEnabled = enabled;
        __Disabled_Profiler__ = !enabled;

        return this;
    }

    /// <summary>
    ///     设置客户端所偏好的自然语言和区域设置
    /// </summary>
    /// <remarks>设置此配置后，将在单次请求标头中添加 <c>Accept-Language</c> 值。</remarks>
    /// <param name="language">自然语言和区域设置</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AcceptLanguage(string? language) =>
        WithHeader(HeaderNames.AcceptLanguage, language, replace: true);

    /// <summary>
    ///     设置 <see cref="HttpRequestMessage" /> 请求属性
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithProperty(string key, object? value)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(key);

        return WithProperties(new Dictionary<string, object?> { { key, value } });
    }

    /// <summary>
    ///     设置 <see cref="HttpRequestMessage" /> 请求属性集合
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="properties">请求的属性集合</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithProperties(IDictionary<string, object?> properties)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(properties);

        Properties.AddOrUpdate(properties);

        return this;
    }

    /// <summary>
    ///     设置 <see cref="HttpRequestMessage" /> 请求属性集合
    /// </summary>
    /// <remarks>支持多次调用。</remarks>
    /// <param name="propertySource"><see cref="HttpRequestMessage" /> 请求的属性源对象</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder WithProperties(object? propertySource)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(propertySource);

        return WithProperties(
            propertySource.ObjectToDictionary()!.ToDictionary(u => u.Key.ToCultureString(CultureInfo.InvariantCulture)!,
                u => u.Value));
    }

    /// <summary>
    ///     设置是否启用性能优化
    /// </summary>
    /// <remarks>当需要返回 <see cref="Stream" /> 内容或进行 <c>HttpContext</c> 网页转发时，请勿启用此配置，因为流会因压缩而变得不可读，同时该配置也不适用于网页转发的场景。</remarks>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder PerformanceOptimization() => PerformanceOptimization(true);

    /// <summary>
    ///     设置是否启用性能优化
    /// </summary>
    /// <remarks>当需要返回 <see cref="Stream" /> 内容或进行 <c>HttpContext</c> 网页转发时，请勿启用此配置，因为流会因压缩而变得不可读，同时该配置也不适用于网页转发的场景。</remarks>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder PerformanceOptimization(bool enabled)
    {
        PerformanceOptimizationEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置是否自动设置 <c>Host</c> 标头
    /// </summary>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AutoSetHostHeader() => AutoSetHostHeader(true);

    /// <summary>
    ///     设置是否自动设置 <c>Host</c> 标头
    /// </summary>
    /// <param name="enabled">是否启用</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder AutoSetHostHeader(bool enabled)
    {
        AutoSetHostHeaderEnabled = enabled;

        return this;
    }

    /// <summary>
    ///     设置请求基地址
    /// </summary>
    /// <param name="baseAddress">基地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetBaseAddress(Uri? baseAddress)
    {
        // 检查基地址是否是绝对路径地址
        if (baseAddress is not null && !baseAddress.IsAbsoluteUri)
        {
            throw new ArgumentException("The base address must be absolute.", nameof(baseAddress));
        }

        BaseAddress = baseAddress;

        return this;
    }

    /// <summary>
    ///     设置请求基地址
    /// </summary>
    /// <param name="baseAddress">基地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    public HttpRequestBuilder SetBaseAddress(string? baseAddress) =>
        SetBaseAddress(string.IsNullOrWhiteSpace(baseAddress)
            ? null
            : new Uri(baseAddress, UriKind.RelativeOrAbsolute));

    /// <summary>
    ///     释放可释放的对象集合
    /// </summary>
    internal void ReleaseDisposables()
    {
        // 空检查
        if (Disposables.IsNullOrEmpty())
        {
            return;
        }

        // 逐条遍历进行释放
        foreach (var disposable in Disposables)
        {
            disposable.Dispose();
        }

        // 清空集合
        Disposables.Clear();
    }

    /// <summary>
    ///     添加 <see cref="StringContentForFormUrlEncodedContentProcessor" /> 处理器
    /// </summary>
    internal void AddStringContentForFormUrlEncodedContentProcessor()
    {
        lock (_lock)
        {
            // 检查是否已添加 StringContentForFormUrlEncodedContentProcessor 处理器
            if (_isAddedStringContentForFormUrlEncodedContentProcessor)
            {
                return;
            }

            _isAddedStringContentForFormUrlEncodedContentProcessor = true;
            AddHttpContentProcessors(() => [_stringContentForFormUrlEncodedContentProcessorInstance.Value]);
        }
    }

    /// <summary>
    ///     重写请求地址
    /// </summary>
    /// <param name="newRequestUri">新的请求地址</param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    internal HttpRequestBuilder RewriteRequestUri(Uri? newRequestUri)
    {
        RequestUri = newRequestUri;

        // 解决重定向时重复拼接查询参数问题
        QueryParameters?.Clear();
        QueryParametersToRemove?.Clear();

        return this;
    }
}