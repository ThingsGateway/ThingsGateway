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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式管理器
/// </summary>
internal sealed class DeclarativeManager
{
    /// <inheritdoc cref="HttpDeclarativeBuilder" />
    internal readonly HttpDeclarativeBuilder _httpDeclarativeBuilder;

    /// <inheritdoc cref="IHttpRemoteService" />
    internal readonly IHttpRemoteService _httpRemoteService;

    /// <summary>
    ///     <inheritdoc cref="DeclarativeManager" />
    /// </summary>
    /// <param name="httpRemoteService">
    ///     <see cref="IHttpRemoteService" />
    /// </param>
    /// <param name="httpDeclarativeBuilder">
    ///     <see cref="HttpDeclarativeBuilder" />
    /// </param>
    internal DeclarativeManager(IHttpRemoteService httpRemoteService, HttpDeclarativeBuilder httpDeclarativeBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteService);
        ArgumentNullException.ThrowIfNull(httpDeclarativeBuilder);

        _httpRemoteService = httpRemoteService;
        _httpDeclarativeBuilder = httpDeclarativeBuilder;

        // 构建 HttpRequestBuilder 实例
        RequestBuilder = httpDeclarativeBuilder.Build(httpRemoteService.ServiceProvider
            .GetRequiredService<IOptions<HttpRemoteOptions>>().Value);
    }

    /// <summary>
    ///     <inheritdoc cref="HttpRequestBuilder" />
    /// </summary>
    internal HttpRequestBuilder RequestBuilder { get; }

    /// <summary>
    ///     开始请求
    /// </summary>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    internal object? Start()
    {
        // 尝试解析单个特殊类型参数
        var (completionOption, cancellationToken) = ExtractSingleSpecialArguments(_httpDeclarativeBuilder.Args);

        // 获取被调用方法返回值类型
        var method = _httpDeclarativeBuilder.Method;
        var returnType = method.ReturnType == typeof(void) ? typeof(VoidContent) : method.ReturnType;

        // 发送 HTTP 远程请求
        return _httpRemoteService.SendAs(returnType, RequestBuilder, completionOption, cancellationToken);
    }

    /// <summary>
    ///     开始请求
    /// </summary>
    /// <typeparam name="T">转换的目标类型</typeparam>
    /// <returns>
    ///     <typeparamref name="T" />
    /// </returns>
    internal async Task<T?> StartAsync<T>()
    {
        // 尝试解析单个特殊类型参数
        var (completionOption, cancellationToken) = ExtractSingleSpecialArguments(_httpDeclarativeBuilder.Args);

        // 发送 HTTP 远程请求
        return await _httpRemoteService.SendAsAsync<T>(RequestBuilder, completionOption, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     尝试解析单个特殊类型参数
    /// </summary>
    /// <param name="args">被调用方法的参数值数组</param>
    /// <returns>
    ///     <see cref="Tuple{T1, T2}" />
    /// </returns>
    internal static (HttpCompletionOption CompletionOption, CancellationToken CancellationToken)
        ExtractSingleSpecialArguments(object?[] args)
    {
        // 尝试解析单个 HttpCompletionOption 参数
        var completionOption = args.SingleOrDefault(u => u is HttpCompletionOption) as HttpCompletionOption?;

        // 尝试解析单个 CancellationToken 参数
        var cancellationToken = args.SingleOrDefault(u => u is CancellationToken) as CancellationToken?;

        return (completionOption ?? HttpCompletionOption.ResponseContentRead,
            cancellationToken ?? CancellationToken.None);
    }
}