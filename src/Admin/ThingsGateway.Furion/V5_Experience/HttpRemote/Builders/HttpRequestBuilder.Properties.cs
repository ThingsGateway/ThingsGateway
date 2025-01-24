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

using System.Net.Http.Headers;
using System.Text;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <see cref="HttpRequestMessage" /> 构建器
/// </summary>
public sealed partial class HttpRequestBuilder
{
    /// <summary>
    ///     请求地址
    /// </summary>
    public Uri? RequestUri { get; private set; }

    /// <summary>
    ///     请求方式
    /// </summary>
    public HttpMethod? Method { get; }

    /// <summary>
    ///     跟踪标识
    /// </summary>
    /// <remarks>
    ///     <para>可为每个请求指定唯一标识符，用于请求的跟踪和调试。</para>
    ///     <para>唯一标识符将在 <see cref="HttpRequestMessage" /> 类型实例的 <c>Headers</c> 属性中通过 <c>X-Trace-ID</c> 作为键指定。</para>
    /// </remarks>
    public string? TraceIdentifier { get; private set; }

    /// <summary>
    ///     内容类型
    /// </summary>
    public string? ContentType { get; private set; }

    /// <summary>
    ///     内容编码
    /// </summary>
    public Encoding? ContentEncoding { get; private set; }

    /// <summary>
    ///     原始请求内容
    /// </summary>
    /// <remarks>此属性值最终将转换为 <see cref="HttpContent" /> 类型实例。</remarks>
    public object? RawContent { get; private set; }

    /// <summary>
    ///     请求标头集合
    /// </summary>
    public IDictionary<string, List<string?>>? Headers { get; private set; }

    /// <summary>
    ///     需要从请求中移除的标头集合
    /// </summary>
    public HashSet<string>? HeadersToRemove { get; private set; }

    /// <summary>
    ///     片段标识符
    /// </summary>
    /// <remarks>请求地址中的 <c>#</c> 符号后面的部分。</remarks>
    public string? Fragment { get; private set; }

    /// <summary>
    ///     超时时间
    /// </summary>
    /// <remarks>可为单次请求设置超时时间。</remarks>
    public TimeSpan? Timeout { get; private set; }

    /// <summary>
    ///     查询参数集合
    /// </summary>
    /// <remarks>请求地址中位于 <c>?</c> 符号之后且 <c>#</c> 符号之前的部分。</remarks>
    public IDictionary<string, List<string?>>? QueryParameters { get; private set; }

    /// <summary>
    ///     需要从 URL 中移除的查询参数集合
    /// </summary>
    public HashSet<string>? QueryParametersToRemove { get; private set; }

    /// <summary>
    ///     路径参数集合
    /// </summary>
    /// <remarks>用于替换请求地址中符合 <c>\{\s*(\w+\s*(\.\s*\w+\s*)*)\s*\}</c> 正则表达式匹配的数据。</remarks>
    public IDictionary<string, string?>? PathParameters { get; private set; }

    /// <summary>
    ///     路径参数集合
    /// </summary>
    /// <remarks>支持自定义类类型。用于替换请求地址中符合 <c>\{\s*(\w+\s*(\.\s*\w+\s*)*)\s*\}</c> 正则表达式匹配的数据。</remarks>
    public IDictionary<string, object?>? ObjectPathParameters { get; private set; }

    /// <summary>
    ///     Cookies 集合
    /// </summary>
    /// <remarks>
    ///     <para>可为单次请求设置 Cookies。</para>
    ///     <para>Cookies 将在 <see cref="HttpRequestMessage" /> 类型实例的 <c>Headers</c> 属性中通过 <c>Cookie</c> 作为键指定。</para>
    ///     <para>使用该方式不会自动处理服务器返回的 <c>Set-Cookie</c> 头。</para>
    /// </remarks>
    public IDictionary<string, string?>? Cookies { get; private set; }

    /// <summary>
    ///     需要从请求中移除的 Cookie 集合
    /// </summary>
    public HashSet<string>? CookiesToRemove { get; private set; }

    /// <summary>
    ///     <see cref="HttpClient" /> 实例的配置名称
    /// </summary>
    /// <remarks>
    ///     <para>此属性用于指定 <see cref="IHttpClientFactory" /> 创建 <see cref="HttpClient" /> 实例时传递的名称。</para>
    ///     <para>该名称用于标识在服务容器中与特定 <see cref="HttpClient" /> 实例相关的配置。</para>
    /// </remarks>
    public string? HttpClientName { get; private set; }

    /// <summary>
    ///     响应内容最大缓存字节数
    /// </summary>
    /// <remarks>可为单次请求设置最大缓存字节数。</remarks>
    public long? MaxResponseContentBufferSize { get; private set; }

    /// <summary>
    ///     <see cref="HttpClient" /> 实例提供器
    /// </summary>
    /// <value>
    ///     <para>返回一个包含 <see cref="HttpClient" /> 实例及其释放方法的委托。</para>
    ///     <para>释放方法的委托用于在不再需要 <see cref="HttpClient" /> 实例时释放资源。</para>
    /// </value>
    public Func<(HttpClient Instance, Action<HttpClient>? Release)>? HttpClientProvider { get; private set; }

    /// <summary>
    ///     <see cref="IHttpContentProcessor" /> 集合提供器
    /// </summary>
    /// <value>返回多个包含实现 <see cref="IHttpContentProcessor" /> 集合的集合。</value>
    public IList<Func<IEnumerable<IHttpContentProcessor>>>? HttpContentProcessorProviders { get; private set; }

    /// <summary>
    ///     <see cref="IHttpContentConverter" /> 集合提供器
    /// </summary>
    /// <value>返回多个包含实现 <see cref="IHttpContentConverter" /> 集合的集合。</value>
    public IList<Func<IEnumerable<IHttpContentConverter>>>? HttpContentConverterProviders { get; private set; }

    /// <summary>
    ///     用于处理在设置 <see cref="HttpRequestMessage" /> 的请求消息的内容时的操作
    /// </summary>
    public Action<HttpContent?>? OnPreSetContent { get; private set; }

    /// <summary>
    ///     用于处理在发送 HTTP 请求之前的操作
    /// </summary>
    public Action<HttpRequestMessage>? OnPreSendRequest { get; private set; }

    /// <summary>
    ///     用于处理在收到 HTTP 响应之后的操作
    /// </summary>
    public Action<HttpResponseMessage>? OnPostReceiveResponse { get; private set; }

    /// <summary>
    ///     用于处理在发送 HTTP 请求发生异常时的操作
    /// </summary>
    public Action<Exception, HttpResponseMessage?>? OnRequestFailed { get; private set; }

    /// <summary>
    ///     身份验证凭据请求授权标头
    /// </summary>
    /// <remarks>可为单次请求设置身份验证凭据请求授权标头。</remarks>
    public AuthenticationHeaderValue? AuthenticationHeader { get; private set; }

    /// <summary>
    ///     <see cref="HttpRequestMessage" /> 请求属性集合
    /// </summary>
    /// <remarks>用于添加 <see cref="HttpRequestMessage" /> 请求属性。该值将合并到 <c>HttpRequestMessage.Options</c> 属性中。</remarks>
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    /// <summary>
    ///     请求基地址
    /// </summary>
    public Uri? BaseAddress { get; private set; }

    /// <summary>
    ///     <inheritdoc cref="HttpMultipartFormDataBuilder" />
    /// </summary>
    internal HttpMultipartFormDataBuilder? MultipartFormDataBuilder { get; private set; }

    /// <summary>
    ///     如果 HTTP 响应的 <c>IsSuccessStatusCode</c> 属性是 <c>false</c>，则引发异常。
    /// </summary>
    /// <remarks>默认值为 <c>false</c>。</remarks>
    internal bool EnsureSuccessStatusCodeEnabled { get; private set; }

    /// <summary>
    ///     是否禁用 HTTP 缓存
    /// </summary>
    /// <remarks>可为单次请求设置禁用 HTTP 缓存。默认值为：<c>false</c>。</remarks>
    internal bool DisableCacheEnabled { get; private set; }

    /// <summary>
    ///     实现 <see cref="IHttpRequestEventHandler" /> 的类型
    /// </summary>
    internal Type? RequestEventHandlerType { get; private set; }

    /// <summary>
    ///     用于请求结束时需要释放的对象集合
    /// </summary>
    internal List<IDisposable>? Disposables { get; private set; }

    /// <summary>
    ///     <see cref="HttpClient" /> 实例管理器
    /// </summary>
    internal HttpClientPooling? HttpClientPooling { get; set; }

    /// <summary>
    ///     是否启用 <see cref="HttpClient" /> 的池化管理
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    internal bool HttpClientPoolingEnabled { get; private set; }

    /// <summary>
    ///     是否启用请求分析工具
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    internal bool ProfilerEnabled { get; private set; }

    /// <summary>
    ///     是否启用性能优化
    /// </summary>
    /// <remarks>默认值为：<c>false</c>。</remarks>
    internal bool PerformanceOptimizationEnabled { get; private set; }

    /// <summary>
    ///     是否自动设置 <c>Host</c> 标头
    /// </summary>
    /// <remarks><c>Host</c> 标头是 <c>HTTP/1.1</c> 协议中的一个必需标头。默认值为：<c>false</c>，表示不默认添加 <c>Host</c> 标头。</remarks>
    internal bool AutoSetHostHeaderEnabled { get; private set; }

    /// <summary>
    ///     表示禁用请求分析工具标识
    /// </summary>
    /// <remarks>用于禁用全局请求分析工具。</remarks>
    internal bool __Disabled_Profiler__ { get; private set; }

    /// <summary>
    ///     状态码处理程序
    /// </summary>
    internal IDictionary<IEnumerable<object>, Func<HttpResponseMessage, CancellationToken, Task>>? StatusCodeHandlers
    {
        get;
        private set;
    }
}