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

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

using System.Collections.Concurrent;
using System.Reflection;

using ThingsGateway.Extensions;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式远程请求构建器
/// </summary>
/// <remarks>使用 <c>HttpRequestBuilder.Declarative(method, args)</c> 静态方法创建。</remarks>
public sealed class HttpDeclarativeBuilder
{
    /// <summary>
    ///     HTTP 声明式 <see cref="IHttpDeclarativeExtractor" /> 提取器集合
    /// </summary>
    internal static readonly ConcurrentDictionary<Type, IHttpDeclarativeExtractor> _extractors = new([
        new(typeof(BaseAddressDeclarativeExtractor), new BaseAddressDeclarativeExtractor()),
        new(typeof(ValidationDeclarativeExtractor), new ValidationDeclarativeExtractor()),
        new(typeof(AutoSetHostHeaderDeclarativeExtractor), new AutoSetHostHeaderDeclarativeExtractor()),
        new(typeof(PerformanceOptimizationDeclarativeExtractor), new PerformanceOptimizationDeclarativeExtractor()),
        new(typeof(HttpClientNameDeclarativeExtractor), new HttpClientNameDeclarativeExtractor()),
        new(typeof(TraceIdentifierDeclarativeExtractor), new TraceIdentifierDeclarativeExtractor()),
        new(typeof(ProfilerDeclarativeExtractor), new ProfilerDeclarativeExtractor()),
        new(typeof(SimulateBrowserDeclarativeExtractor), new SimulateBrowserDeclarativeExtractor()),
        new(typeof(AcceptLanguageDeclarativeExtractor), new AcceptLanguageDeclarativeExtractor()),
        new(typeof(DisableCacheDeclarativeExtractor), new DisableCacheDeclarativeExtractor()),
        new(typeof(EnsureSuccessStatusCodeDeclarativeExtractor), new EnsureSuccessStatusCodeDeclarativeExtractor()),
        new(typeof(TimeoutDeclarativeExtractor), new TimeoutDeclarativeExtractor()),
        new(typeof(QueryDeclarativeExtractor), new QueryDeclarativeExtractor()),
        new(typeof(PathDeclarativeExtractor), new PathDeclarativeExtractor()),
        new(typeof(CookieDeclarativeExtractor), new CookieDeclarativeExtractor()),
        new(typeof(HeaderDeclarativeExtractor), new HeaderDeclarativeExtractor()),
        new(typeof(PropertyDeclarativeExtractor), new PropertyDeclarativeExtractor()),
        new(typeof(BodyDeclarativeExtractor), new BodyDeclarativeExtractor())
    ]);

    /// <summary>
    ///     HTTP 声明式 <see cref="IHttpDeclarativeExtractor" /> 提取器集合（冻结）
    /// </summary>
    /// <remarks>该集合用于确保某些 HTTP 声明式提取器始终位于最后。</remarks>
    internal static readonly ConcurrentDictionary<Type, IFrozenHttpDeclarativeExtractor> _frozenExtractors = new([
        new(typeof(MultipartDeclarativeExtractor), new MultipartDeclarativeExtractor()),
        new(typeof(HttpMultipartFormDataBuilderDeclarativeExtractor),
            new HttpMultipartFormDataBuilderDeclarativeExtractor()),
        new(typeof(HttpRequestBuilderDeclarativeExtractor), new HttpRequestBuilderDeclarativeExtractor())
    ]);

    /// <summary>
    ///     标识是否已加载自定义 HTTP 声明式提取器
    /// </summary>
    internal bool _hasLoadedExtractors;

    /// <summary>
    ///     <inheritdoc cref="HttpDeclarativeBuilder" />
    /// </summary>
    /// <param name="method">被调用方法</param>
    /// <param name="args">被调用方法的参数值数组</param>
    internal HttpDeclarativeBuilder(MethodInfo method, object?[] args)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(method);
        ArgumentNullException.ThrowIfNull(args);

        Method = method;
        Args = args;
    }

    /// <summary>
    ///     被调用方法
    /// </summary>
    public MethodInfo Method { get; }

    /// <summary>
    ///     被调用方法的参数值数组
    /// </summary>
    public object?[] Args { get; }

    /// <summary>
    ///     构建 <see cref="HttpRequestBuilder" /> 实例
    /// </summary>
    /// <param name="httpRemoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpRequestBuilder" />
    /// </returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal HttpRequestBuilder Build(HttpRemoteOptions httpRemoteOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);

        // 检查被调用方法是否贴有 [HttpMethod] 特性
        if (!Method.IsDefined(typeof(HttpMethodAttribute), true))
        {
            throw new InvalidOperationException(
                $"No `[HttpMethod]` annotation was found in method `{Method.ToFriendlyString()}` of type `{Method.DeclaringType?.ToFriendlyString()}`.");
        }

        // 获取 HttpMethodAttribute 实例
        var httpMethodAttribute = Method.GetCustomAttribute<HttpMethodAttribute>(true)!;

        // 初始化 HttpRequestBuilder 实例并添加声明式方法签名
        var httpRequestBuilder = HttpRequestBuilder.Create(httpMethodAttribute.Method, httpMethodAttribute.RequestUri)
            .WithProperty(Constants.DECLARATIVE_METHOD_KEY,
                $"{Method.ToFriendlyString()} | {Method.DeclaringType.ToFriendlyString()}");

        // 初始化 HttpDeclarativeExtractorContext 实例
        var httpDeclarativeExtractorContext = new HttpDeclarativeExtractorContext(Method, Args);

        // 检查是否已加载自定义 HTTP 声明式提取器
        if (!_hasLoadedExtractors)
        {
            _hasLoadedExtractors = true;

            // 添加自定义 IHttpDeclarativeExtractor 数组
            _extractors.TryAdd(httpRemoteOptions.HttpDeclarativeExtractors?.SelectMany(u => u.Invoke()).ToArray(),
                value => value.GetType());
        }

        // 组合所有 HTTP 声明式提取器
        var extractors = _extractors.Values.Concat(_frozenExtractors.Values.OrderByDescending(e => e.Order)).ToArray();

        // 遍历 HTTP 声明式提取器集合
        foreach (var extractor in extractors)
        {
            // 提取方法信息构建 HttpRequestBuilder 实例
            extractor.Extract(httpRequestBuilder, httpDeclarativeExtractorContext);
        }

        return httpRequestBuilder;
    }
}