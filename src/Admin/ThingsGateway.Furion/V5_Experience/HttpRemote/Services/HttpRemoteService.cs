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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;

using ThingsGateway.Extensions;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <inheritdoc cref="IHttpRemoteService" />
/// </summary>
internal sealed partial class HttpRemoteService : IHttpRemoteService
{
    /// <inheritdoc cref="IHttpClientFactory" />
    internal readonly IHttpClientFactory _httpClientFactory;

    /// <inheritdoc cref="IHttpContentConverterFactory" />
    internal readonly IHttpContentConverterFactory _httpContentConverterFactory;

    /// <inheritdoc cref="IHttpContentProcessorFactory" />
    internal readonly IHttpContentProcessorFactory _httpContentProcessorFactory;

    /// <inheritdoc cref="HttpRemoteOptions" />
    internal readonly HttpRemoteOptions _httpRemoteOptions;

    /// <inheritdoc cref="ILogger{T}" />
    internal readonly ILogger<Logging> _logger;

    /// <summary>
    ///     <inheritdoc cref="HttpRemoteService" />
    /// </summary>
    /// <param name="serviceProvider">
    ///     <see cref="IServiceProvider" />
    /// </param>
    /// <param name="logger">
    ///     <see cref="Logger{T}" />
    /// </param>
    /// <param name="httpClientFactory">
    ///     <see cref="IHttpClientFactory" />
    /// </param>
    /// <param name="httpContentProcessorFactory">
    ///     <see cref="IHttpContentProcessorFactory" />
    /// </param>
    /// <param name="httpContentConverterFactory">
    ///     <see cref="IHttpContentConverterFactory" />
    /// </param>
    /// <param name="httpRemoteOptions">
    ///     <see cref="IOptions{TOptions}" />
    /// </param>
    public HttpRemoteService(IServiceProvider serviceProvider, ILogger<Logging> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContentProcessorFactory httpContentProcessorFactory,
        IHttpContentConverterFactory httpContentConverterFactory,
        IOptions<HttpRemoteOptions> httpRemoteOptions)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(httpContentProcessorFactory);
        ArgumentNullException.ThrowIfNull(httpContentConverterFactory);
        ArgumentNullException.ThrowIfNull(httpRemoteOptions);

        ServiceProvider = serviceProvider;
        _httpRemoteOptions = httpRemoteOptions.Value;

        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _httpContentProcessorFactory = httpContentProcessorFactory;
        _httpContentConverterFactory = httpContentConverterFactory;
    }

    /// <inheritdoc />
    public IServiceProvider ServiceProvider { get; }

    /// <inheritdoc />
    public HttpResponseMessage Send(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        Send(httpRequestBuilder, HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Send(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, _) = SendCoreAsync(httpRequestBuilder, completionOption, default,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.Send(httpRequestMessage, option, token), cancellationToken).GetAwaiter().GetResult();

        return httpResponseMessage;
    }

    /// <inheritdoc />
    public Task<HttpResponseMessage> SendAsync(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAsync(httpRequestBuilder, HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpResponseMessage> SendAsync(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, _) = await SendCoreAsync(httpRequestBuilder, completionOption,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.SendAsync(httpRequestMessage, option, token), default, cancellationToken).ConfigureAwait(false);

        return httpResponseMessage;
    }

    /// <inheritdoc />
    public TResult? SendAs<TResult>(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAs<TResult>(httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public TResult? SendAs<TResult>(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = SendCoreAsync(httpRequestBuilder, completionOption, default,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.Send(httpRequestMessage, option, token), cancellationToken).GetAwaiter().GetResult();

        // 获取结果类型
        var resultType = typeof(TResult);

        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(resultType))
        {
            // 将 HttpResponseMessage 转换为 TResult 实例
            return _httpContentConverterFactory.Read<TResult>(httpResponseMessage,
                httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
                cancellationToken);
        }

        // 将 HttpResponseMessage 转换为 HttpRemoteResult<T> 泛型类型 T 的实例
        var result = _httpContentConverterFactory.Read(resultType.GetGenericArguments()[0],
            httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
            cancellationToken);

        // 动态创建 HttpRemoteResult<TResult> 实例并转换为 TResult 实例
        return (TResult)DynamicCreateHttpRemoteResult(resultType, httpResponseMessage, result, requestDuration);
    }

    /// <inheritdoc />
    public string? SendAsString(HttpRequestBuilder httpRequestBuilder, CancellationToken cancellationToken = default) =>
        SendAs<string>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public string? SendAsString(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAs<string>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public byte[]? SendAsByteArray(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAs<byte[]>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public byte[]? SendAsByteArray(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAs<byte[]>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public Stream? SendAsStream(HttpRequestBuilder httpRequestBuilder, CancellationToken cancellationToken = default) =>
        SendAs<Stream>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public Stream? SendAsStream(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAs<Stream>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> SendAsAsync<TResult>(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAsAsync<TResult>(httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public async Task<TResult?> SendAsAsync<TResult>(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = await SendCoreAsync(httpRequestBuilder, completionOption,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.SendAsync(httpRequestMessage, option, token), default, cancellationToken).ConfigureAwait(false);

        // 获取结果类型
        var resultType = typeof(TResult);

        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(resultType))
        {
            // 将 HttpResponseMessage 转换为 TResult 实例
            return await _httpContentConverterFactory.ReadAsync<TResult>(httpResponseMessage,
                httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
                cancellationToken).ConfigureAwait(false);
        }

        // 将 HttpResponseMessage 转换为 HttpRemoteResult<T> 泛型类型 T 的实例
        var result = await _httpContentConverterFactory.ReadAsync(resultType.GetGenericArguments()[0],
            httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
            cancellationToken).ConfigureAwait(false);

        // 动态创建 HttpRemoteResult<TResult> 实例并转换为 TResult 实例
        return (TResult)DynamicCreateHttpRemoteResult(resultType, httpResponseMessage, result, requestDuration);
    }

    /// <inheritdoc />
    public Task<string?> SendAsStringAsync(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<string>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public Task<string?> SendAsStringAsync(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<string>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> SendAsByteArrayAsync(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<byte[]>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> SendAsByteArrayAsync(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default) =>
        SendAsAsync<byte[]>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> SendAsStreamAsync(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<Stream>(httpRequestBuilder, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> SendAsStreamAsync(HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default) =>
        SendAsAsync<Stream>(httpRequestBuilder, completionOption, cancellationToken);

    /// <inheritdoc />
    public object? SendAs(Type resultType, HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAs(resultType, httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public object? SendAs(Type resultType, HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = SendCoreAsync(httpRequestBuilder, completionOption, default,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.Send(httpRequestMessage, option, token), cancellationToken).GetAwaiter().GetResult();

        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(resultType))
        {
            // 将 HttpResponseMessage 转换为 resultType 类型实例
            return _httpContentConverterFactory.Read(resultType, httpResponseMessage,
                httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
                cancellationToken);
        }

        // 将 HttpResponseMessage 转换为 HttpRemoteResult<T> 泛型类型 T 的实例
        var result = _httpContentConverterFactory.Read(resultType.GetGenericArguments()[0],
            httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
            cancellationToken);

        // 动态创建 HttpRemoteResult<TResult> 实例并转换为 resultType 类型实例
        return DynamicCreateHttpRemoteResult(resultType, httpResponseMessage, result, requestDuration);
    }

    /// <inheritdoc />
    public Task<object?> SendAsAsync(Type resultType, HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAsAsync(resultType, httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public async Task<object?> SendAsAsync(Type resultType, HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = await SendCoreAsync(httpRequestBuilder, completionOption,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.SendAsync(httpRequestMessage, option, token), default, cancellationToken).ConfigureAwait(false);

        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(resultType))
        {
            // 将 HttpResponseMessage 转换为 resultType 类型实例
            return await _httpContentConverterFactory.ReadAsync(resultType, httpResponseMessage,
                httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
                cancellationToken).ConfigureAwait(false);
        }

        // 将 HttpResponseMessage 转换为 HttpRemoteResult<T> 泛型类型 T 的实例
        var result = await _httpContentConverterFactory.ReadAsync(resultType.GetGenericArguments()[0],
            httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(),
            cancellationToken).ConfigureAwait(false);

        // 动态创建 HttpRemoteResult<TResult> 实例并转换为 resultType 类型实例
        return DynamicCreateHttpRemoteResult(resultType, httpResponseMessage, result, requestDuration);
    }

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Send<TResult>(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) =>
        Send<TResult>(httpRequestBuilder, HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Send<TResult>(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = SendCoreAsync(httpRequestBuilder, completionOption, default,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.Send(httpRequestMessage, option, token), cancellationToken).GetAwaiter().GetResult();

        // 将 HttpResponseMessage 转换为 TResult 实例
        var result = _httpContentConverterFactory.Read<TResult>(httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(), cancellationToken);

        // 初始化 HttpRemoteResult 实例
        var httpRemoteResult = new HttpRemoteResult<TResult>(httpResponseMessage)
        {
            Result = result,
            RequestDuration = requestDuration
        };

        return httpRemoteResult;
    }

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> SendAsync<TResult>(HttpRequestBuilder httpRequestBuilder,
        CancellationToken cancellationToken = default) => SendAsync<TResult>(httpRequestBuilder,
        HttpCompletionOption.ResponseContentRead, cancellationToken);

    /// <inheritdoc />
    public async Task<HttpRemoteResult<TResult>> SendAsync<TResult>(HttpRequestBuilder httpRequestBuilder,
        HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
    {
        // 发送 HTTP 远程请求
        var (httpResponseMessage, requestDuration) = await SendCoreAsync(httpRequestBuilder, completionOption,
            (httpClient, httpRequestMessage, option, token) =>
                httpClient.SendAsync(httpRequestMessage, option, token), default, cancellationToken).ConfigureAwait(false);

        // 将 HttpResponseMessage 转换为 TResult 实例
        var result = await _httpContentConverterFactory.ReadAsync<TResult>(httpResponseMessage,
            httpRequestBuilder.HttpContentConverterProviders?.SelectMany(u => u.Invoke()).ToArray(), cancellationToken).ConfigureAwait(false);

        // 初始化 HttpRemoteResult 实例
        var httpRemoteResult = new HttpRemoteResult<TResult>(httpResponseMessage)
        {
            Result = result,
            RequestDuration = requestDuration
        };

        return httpRemoteResult;
    }

    /// <summary>
    ///     发送 HTTP 远程请求并处理 <see cref="HttpResponseMessage" /> 实例
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="completionOption">
    ///     <see cref="HttpCompletionOption" />
    /// </param>
    /// <param name="sendAsyncMethod">异步发送 HTTP 请求的委托</param>
    /// <param name="sendMethod">同步发送 HTTP 请求的委托</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Tuple{T1, T2}" />
    /// </returns>
    internal async Task<(HttpResponseMessage ResponseMessage, long RequestDuration)> SendCoreAsync(
        HttpRequestBuilder httpRequestBuilder, HttpCompletionOption completionOption,
        Func<HttpClient, HttpRequestMessage, HttpCompletionOption, CancellationToken, Task<HttpResponseMessage>>?
            sendAsyncMethod,
        Func<HttpClient, HttpRequestMessage, HttpCompletionOption, CancellationToken, HttpResponseMessage>? sendMethod,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);

        // 空检查
        if (sendAsyncMethod is null && sendMethod is null)
        {
            throw new InvalidOperationException("Both `sendAsyncMethod` and `sendMethod` cannot be null.");
        }

        // 解析 IHttpRequestEventHandler 事件处理程序
        var requestEventHandler =
            (httpRequestBuilder.RequestEventHandlerType is not null
                ? ServiceProvider.GetService(httpRequestBuilder.RequestEventHandlerType)
                : null) as IHttpRequestEventHandler;

        // 创建带有默认值的 HttpClient 实例
        var httpClientPooling = CreateHttpClientWithDefaults(httpRequestBuilder);
        var httpClient = httpClientPooling.Instance;

        // 构建 HttpRequestMessage 实例
        var httpRequestMessage =
            httpRequestBuilder.Build(_httpRemoteOptions, _httpContentProcessorFactory, httpClient.BaseAddress);

        // 处理发送 HTTP 请求之前
        HandlePreSendRequest(httpRequestBuilder, requestEventHandler, httpRequestMessage);

        // 检查是否启用请求分析工具
        if (httpRequestBuilder.ProfilerEnabled)
        {
            await ProfilerDelegatingHandler.LogRequestAsync(_logger, _httpRemoteOptions, httpRequestMessage,
                cancellationToken).ConfigureAwait(false);
        }

        // 创建关联的超时 Token 标识
        using var timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // 设置单次请求超时时间控制
        if (httpRequestBuilder.Timeout is not null && httpRequestBuilder.Timeout.Value != TimeSpan.Zero)
        {
            timeoutCancellationTokenSource.CancelAfter(httpRequestBuilder.Timeout.Value);
        }

        HttpResponseMessage? httpResponseMessage = null;

        // 初始化 Stopwatch 实例并开启计时操作
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // 调用发送 HTTP 请求委托
            httpResponseMessage = sendAsyncMethod is not null
                ? await sendAsyncMethod(httpClient, httpRequestMessage, completionOption,
                    timeoutCancellationTokenSource.Token).ConfigureAwait(false)
                : sendMethod!(httpClient, httpRequestMessage, completionOption, timeoutCancellationTokenSource.Token);

            // 处理重定向问题
            var redirections = 0;
            while (Helpers.IsRedirectStatusCode(httpResponseMessage.StatusCode) &&
                   _httpRemoteOptions.AllowAutoRedirect &&
                   redirections < _httpRemoteOptions.MaximumAutomaticRedirections)
            {
                // 获取重定向地址
                var redirectUrl = httpResponseMessage.Headers.Location;

                // 空检查
                if (redirectUrl is null)
                {
                    break;
                }

                // 构建新的 HttpRequestMessage 实例（TODO：未来考虑克隆新的 HttpRequestBuilder 实例）
                var newHttpRequestMessage = httpRequestBuilder
                    // 处理相对地址
                    .RewriteRequestUri(redirectUrl.IsAbsoluteUri
                        ? redirectUrl
                        : new Uri(Helpers.ParseBaseAddress(httpRequestMessage.RequestUri), redirectUrl))
                    .Build(_httpRemoteOptions, _httpContentProcessorFactory, httpClient.BaseAddress);

                // 释放前一个 HttpResponseMessage 实例
                httpResponseMessage.Dispose();

                // 重新调用发送 HTTP 请求委托
                httpResponseMessage = sendAsyncMethod is not null
                    ? await sendAsyncMethod(httpClient, newHttpRequestMessage, completionOption,
                        timeoutCancellationTokenSource.Token).ConfigureAwait(false)
                    : sendMethod!(httpClient, newHttpRequestMessage, completionOption,
                        timeoutCancellationTokenSource.Token);

                // 递增重定向次数
                redirections++;
            }

            // 获取请求耗时
            var requestDuration = stopwatch.ElapsedMilliseconds;

            // 调用状态码处理程序
            if (sendAsyncMethod is not null)
            {
                await InvokeStatusCodeHandlersAsync(httpRequestBuilder, httpResponseMessage,
                    timeoutCancellationTokenSource.Token).ConfigureAwait(false);
            }
            else
            {
                // ReSharper disable once MethodHasAsyncOverload
                InvokeStatusCodeHandlers(httpRequestBuilder, httpResponseMessage, timeoutCancellationTokenSource.Token);
            }

            // 检查是否启用请求分析工具
            if (httpRequestBuilder.ProfilerEnabled)
            {
                await ProfilerDelegatingHandler.LogResponseAsync(_logger, _httpRemoteOptions, httpResponseMessage,
                    requestDuration, cancellationToken).ConfigureAwait(false);
            }

            // 检查 HTTP 响应内容长度是否在设定的最大缓冲区大小限制内
            CheckContentLengthWithinLimit(httpRequestBuilder, httpResponseMessage);

            // 如果 HTTP 响应的 IsSuccessStatusCode 属性是 false，则引发异常
            if (httpRequestBuilder.EnsureSuccessStatusCodeEnabled)
            {
                httpResponseMessage.EnsureSuccessStatusCode();
            }

            return (httpResponseMessage, requestDuration);
        }
        catch (Exception e)
        {
            // 处理发送 HTTP 请求发生异常
            HandleRequestFailed(httpRequestBuilder, requestEventHandler, e, httpResponseMessage);

            throw;
        }
        finally
        {
            // 停止计时
            stopwatch.Stop();

            // 处理收到 HTTP 响应之后
            HandlePostReceiveResponse(httpRequestBuilder, requestEventHandler, httpResponseMessage);

            // 释放资源集合
            if (!httpRequestBuilder.HttpClientPoolingEnabled)
            {
                httpRequestBuilder.ReleaseResources();
            }
        }
    }

    /// <summary>
    ///     处理发送 HTTP 请求之前
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="requestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </param>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    internal static void HandlePreSendRequest(HttpRequestBuilder httpRequestBuilder,
        IHttpRequestEventHandler? requestEventHandler, HttpRequestMessage httpRequestMessage)
    {
        // 空检查
        if (requestEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(requestEventHandler.OnPreSendRequest, httpRequestMessage);
        }

        httpRequestBuilder.OnPreSendRequest.TryInvoke(httpRequestMessage);
    }

    /// <summary>
    ///     处理收到 HTTP 响应之后
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="requestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    internal static void HandlePostReceiveResponse(HttpRequestBuilder httpRequestBuilder,
        IHttpRequestEventHandler? requestEventHandler, HttpResponseMessage? httpResponseMessage)
    {
        // 空检查
        if (httpResponseMessage is null)
        {
            return;
        }

        // 空检查
        if (requestEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(requestEventHandler.OnPostReceiveResponse, httpResponseMessage);
        }

        httpRequestBuilder.OnPostReceiveResponse.TryInvoke(httpResponseMessage);
    }

    /// <summary>
    ///     处理发送 HTTP 请求发生异常
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="requestEventHandler">
    ///     <see cref="IHttpRequestEventHandler" />
    /// </param>
    /// <param name="e">
    ///     <see cref="Exception" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    internal static void HandleRequestFailed(HttpRequestBuilder httpRequestBuilder,
        IHttpRequestEventHandler? requestEventHandler, Exception e, HttpResponseMessage? httpResponseMessage)
    {
        // 空检查
        if (requestEventHandler is not null)
        {
            DelegateExtensions.TryInvoke(requestEventHandler.OnRequestFailed, e, httpResponseMessage);
        }

        httpRequestBuilder.OnRequestFailed.TryInvoke(e, httpResponseMessage);
    }

    /// <summary>
    ///     创建带有默认值的 <see cref="HttpClient" /> 实例
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpClientPooling" />
    /// </returns>
    internal HttpClientPooling CreateHttpClientWithDefaults(HttpRequestBuilder httpRequestBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);

        // 检查是否已经存在 HttpClientPooling 实例
        if (httpRequestBuilder.HttpClientPooling is not null)
        {
            return httpRequestBuilder.HttpClientPooling;
        }

        // 使用锁确保线程安全
        lock (httpRequestBuilder)
        {
            return httpRequestBuilder.HttpClientPooling ?? CreateHttpClientPooling(httpRequestBuilder);
        }
    }

    /// <summary>
    ///     创建 <see cref="HttpClient" /> 实例管理器
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <returns>
    ///     <see cref="HttpClientPooling" />
    /// </returns>
    internal HttpClientPooling CreateHttpClientPooling(HttpRequestBuilder httpRequestBuilder)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);

        Action<HttpClient>? release = null;
        HttpClient httpClient;

        // 检查是否设置了 HttpClient 实例提供器
        if (httpRequestBuilder.HttpClientProvider is null)
        {
            httpClient = string.IsNullOrWhiteSpace(httpRequestBuilder.HttpClientName)
                ? _httpClientFactory.CreateClient()
                : _httpClientFactory.CreateClient(httpRequestBuilder.HttpClientName);
        }
        else
        {
            // 调用 HttpClient 实例提供器
            var provider = httpRequestBuilder.HttpClientProvider();
            httpClient = provider.Instance;
            release = provider.Release;
        }

        // 空检查
        ArgumentNullException.ThrowIfNull(httpClient);

        // 添加默认的 User-Agent 标头
        AddDefaultUserAgentHeader(httpClient);

        // 存储 HttpClientPooling 实例并返回
        return httpRequestBuilder.HttpClientPooling = new HttpClientPooling(httpClient, release);
    }

    /// <summary>
    ///     向 <see cref="HttpClient" /> 添加默认的 <c>User-Agent</c> 标头
    /// </summary>
    /// <remarks>解决某些服务器可能需要这个头部信息才能正确响应请求。</remarks>
    /// <param name="httpClient">
    ///     <see cref="HttpClient" />
    /// </param>
    internal static void AddDefaultUserAgentHeader(HttpClient httpClient)
    {
        // 空检查
        if (httpClient.DefaultRequestHeaders.UserAgent.Count != 0)
        {
            return;
        }

        // User-Agent 默认格式为：程序集名称/程序集版本号
        httpClient.DefaultRequestHeaders.UserAgent.Add(typeof(HttpRemoteService).Assembly.ConvertTo(ass =>
            new ProductInfoHeaderValue(ass.GetName().Name!,
                ass.GetVersion()?.ToString() ?? Constants.UNKNOWN_USER_AGENT_VERSION)));
    }

    /// <summary>
    ///     检查 HTTP 响应内容长度是否在设定的最大缓冲区大小限制内
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <exception cref="HttpRequestException"></exception>
    internal static void CheckContentLengthWithinLimit(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage)
    {
        // 空检查
        if (httpRequestBuilder.MaxResponseContentBufferSize is null)
        {
            return;
        }

        // 检查响应内容长度
        if (httpResponseMessage.Content.Headers.ContentLength is { } contentLength &&
            contentLength > httpRequestBuilder.MaxResponseContentBufferSize)
        {
            throw new HttpRequestException(
                $"Cannot write more bytes to the buffer than the configured maximum buffer size: `{httpRequestBuilder.MaxResponseContentBufferSize}`.");
        }
    }

    /// <summary>
    ///     调用状态码处理程序
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static void InvokeStatusCodeHandlers(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 后台线程中启动异步任务
        Task.Run(
            async () => await InvokeStatusCodeHandlersAsync(httpRequestBuilder, httpResponseMessage, cancellationToken).ConfigureAwait(false),
            cancellationToken);
    }

    /// <summary>
    ///     调用状态码处理程序
    /// </summary>
    /// <param name="httpRequestBuilder">
    ///     <see cref="HttpRequestBuilder" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task InvokeStatusCodeHandlersAsync(HttpRequestBuilder httpRequestBuilder,
        HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(httpRequestBuilder);
        ArgumentNullException.ThrowIfNull(httpResponseMessage);

        // 空检查
        if (httpRequestBuilder.StatusCodeHandlers is null || httpRequestBuilder.StatusCodeHandlers.Count == 0)
        {
            return;
        }

        // 获取响应状态码
        var statusCode = (int)httpResponseMessage.StatusCode;

        // 查找响应状态码所有处理程序
        var statusCodeHandlers = httpRequestBuilder.StatusCodeHandlers
            .Where(u => u.Key.Any(code => IsMatchedStatusCode(code, statusCode)))
            .Select(u => u.Value).ToList();

        // 空检查
        if (statusCodeHandlers.Count == 0)
        {
            return;
        }

        // 并行执行所有的处理程序，并等待所有任务完成
        await Task.WhenAll(statusCodeHandlers.Select(handler =>
            handler.TryInvokeAsync(httpResponseMessage, cancellationToken))).ConfigureAwait(false);
    }

    /// <summary>
    ///     检查状态码代码是否匹配响应状态码
    /// </summary>
    /// <param name="code">状态码代码</param>
    /// <param name="statusCode">响应状态码</param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsMatchedStatusCode(object code, int statusCode)
    {
        switch (code)
        {
            // 处理正整数类型
            case int intStatusCode when intStatusCode == statusCode:
                return true;
            // 处理 HttpStatusCode 枚举类型
            case HttpStatusCode httpStatusCode when (int)httpStatusCode == statusCode:
                return true;
            // 处理特殊字符串
            case "*" or '*':
                return true;
            // 处理字符串类型
            case string stringStatusCode when !stringStatusCode.Contains('+') &&
                                              int.TryParse(stringStatusCode, out var intStatusCodeResult) &&
                                              intStatusCodeResult == statusCode:
                return true;
            // 处理字符串区间类型，如 200-500
            case string stringStatusCode when StatusCodeRangeRegex().IsMatch(stringStatusCode):
                // 根据 - 符号切割
                var parts = stringStatusCode.Split('-', StringSplitOptions.RemoveEmptyEntries);

                // 比较状态码区间
                if (parts.Length == 2 && int.TryParse(parts[0], out var start) && int.TryParse(parts[1], out var end))
                {
                    return statusCode >= start && statusCode <= end;
                }

                break;
            // 处理包含比较符号的类型：如：>=200, <=300, <100, =100, >100
            case string compareStatusCode when StatusCodeCompareRegex().IsMatch(compareStatusCode):
                // 提取正则表达式内容并获取符号和数字部分
                var match = StatusCodeCompareRegex().Match(compareStatusCode);
                var symbolPart = match.Groups[1].Value;
                var numberPart = match.Groups[2].Value;

                // 获取状态码
                if (!int.TryParse(numberPart, out var number))
                {
                    return false;
                }

                return symbolPart switch
                {
                    ">=" => statusCode >= number,
                    "<=" => statusCode <= number,
                    ">" => statusCode > number,
                    "<" => statusCode < number,
                    "=" => statusCode == number,
                    _ => false
                };
            default:
                return false;
        }

        return false;
    }

    /// <summary>
    ///     动态创建 <see cref="HttpRemoteResult{TResult}" /> 实例
    /// </summary>
    /// <param name="httpRemoteResultType"><see cref="HttpRemoteResult{TResult}" /> 类型</param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="result"><see cref="HttpRemoteResult{TResult}" /> 泛型类型的实例</param>
    /// <param name="requestDuration">请求耗时（毫秒）</param>
    /// <returns>
    ///     <see cref="object" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static object DynamicCreateHttpRemoteResult(Type httpRemoteResultType,
        HttpResponseMessage httpResponseMessage,
        object? result, long requestDuration)
    {
        // 检查类型是否是 HttpRemoteResult<TResult> 类型
        if (!typeof(HttpRemoteResult<>).IsDefinitionEqual(httpRemoteResultType))
        {
            throw new ArgumentException(
                $"`{httpRemoteResultType}` type is not assignable from `{typeof(HttpRemoteResult<>)}`.",
                nameof(httpRemoteResultType));
        }

        // 反射创建 HttpRemoteResult<TResult> 实例
        var httpRemoteResult = Activator.CreateInstance(httpRemoteResultType, httpResponseMessage);

        // 空检查
        ArgumentNullException.ThrowIfNull(httpRemoteResult);

        // 初始化反射搜索成员方式
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // 获取 Result 和 RequestDuration 属性设置器
        var setResultDelegate =
            httpRemoteResultType.CreatePropertySetter(httpRemoteResultType.GetProperty(
                nameof(HttpRemoteResult<object>.Result),
                bindingFlags)!);
        var setRequestDurationDelegate =
            httpRemoteResultType.CreatePropertySetter(httpRemoteResultType.GetProperty(
                nameof(HttpRemoteResult<object>.RequestDuration),
                bindingFlags)!);

        // 设置 Result 和 RequestDuration 属性值
        setResultDelegate(httpRemoteResult, result);
        setRequestDurationDelegate(httpRemoteResult, requestDuration);

        return httpRemoteResult;
    }

    /// <summary>
    ///     状态码区间正则表达式
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^\d+-\d+$")]
    private static partial Regex StatusCodeRangeRegex();

    /// <summary>
    ///     状态码比较正则表达式
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"^([<>]=?|=|>|<)(\d+)$")]
    private static partial Regex StatusCodeCompareRegex();
}