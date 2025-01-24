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

using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Diagnostics;
using System.Net;

using ThingsGateway.HttpRemote.Extensions;
using ThingsGateway.Utilities;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 远程请求分析工具处理委托
/// </summary>
/// <remarks>参考文献：https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/http-requests?view=aspnetcore-8.0#outgoing-request-middleware</remarks>
/// <param name="logger">
///     <see cref="Logger{T}" />
/// </param>
/// <param name="httpRemoteOptions">
///     <see cref="IOptions{TOptions}" />
/// </param>
public sealed class ProfilerDelegatingHandler(ILogger<Logging> logger, IOptions<HttpRemoteOptions> httpRemoteOptions)
    : DelegatingHandler
{
    /// <summary>
    ///     是否启用请求分析工具
    /// </summary>
    /// <param name="httpRequestMessage">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <returns>
    ///     <see cref="bool" />
    /// </returns>
    internal static bool IsEnabled(HttpRequestMessage httpRequestMessage) =>
        !(httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<string>(Constants.DISABLED_PROFILER_KEY),
            out var value) && value == "TRUE");

    /// <inheritdoc />
    protected override HttpResponseMessage Send(HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken)
    {
        // 检查是否启用请求分析工具
        if (!IsEnabled(httpRequestMessage))
        {
            return base.Send(httpRequestMessage, cancellationToken);
        }

        // 记录请求信息
        LogRequestAsync(logger, httpRemoteOptions.Value, httpRequestMessage, cancellationToken)
            .GetAwaiter().GetResult();

        // 初始化 Stopwatch 实例并开启计时操作
        var stopwatch = Stopwatch.StartNew();

        // 发送 HTTP 远程请求
        var httpResponseMessage = base.Send(httpRequestMessage, cancellationToken);

        // 获取请求耗时
        var requestDuration = stopwatch.ElapsedMilliseconds;

        // 停止计时
        stopwatch.Stop();

        // 记录响应信息
        LogResponseAsync(logger, httpRemoteOptions.Value, httpResponseMessage, requestDuration, cancellationToken)
            .GetAwaiter().GetResult();

        // 打印 CookieContainer 内容
        LogCookieContainer(logger, httpRemoteOptions.Value, httpRequestMessage, ExtractCookieContainer());

        return httpResponseMessage;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage,
        CancellationToken cancellationToken)
    {
        // 检查是否启用请求分析工具
        if (!IsEnabled(httpRequestMessage))
        {
            return await base.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        }

        // 记录请求信息
        await LogRequestAsync(logger, httpRemoteOptions.Value, httpRequestMessage, cancellationToken).ConfigureAwait(false);

        // 初始化 Stopwatch 实例并开启计时操作
        var stopwatch = Stopwatch.StartNew();

        // 发送 HTTP 远程请求
        var httpResponseMessage = await base.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

        // 获取请求耗时
        var requestDuration = stopwatch.ElapsedMilliseconds;

        // 停止计时
        stopwatch.Stop();

        // 记录响应信息
        await LogResponseAsync(logger, httpRemoteOptions.Value, httpResponseMessage, requestDuration,
            cancellationToken).ConfigureAwait(false);

        // 打印 CookieContainer 内容
        LogCookieContainer(logger, httpRemoteOptions.Value, httpRequestMessage, ExtractCookieContainer());

        return httpResponseMessage;
    }

    /// <summary>
    ///     记录请求信息
    /// </summary>
    /// <param name="logger">
    ///     <see cref="ILogger" />
    /// </param>
    /// <param name="remoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="request">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task LogRequestAsync(ILogger logger, HttpRemoteOptions remoteOptions,
        HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        Log(logger, remoteOptions, request.ProfilerHeaders());
        Log(logger, remoteOptions, await request.Content.ProfilerAsync(cancellationToken: cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    ///     记录响应信息
    /// </summary>
    /// <param name="logger">
    ///     <see cref="ILogger" />
    /// </param>
    /// <param name="remoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="httpResponseMessage">
    ///     <see cref="HttpResponseMessage" />
    /// </param>
    /// <param name="requestDuration">请求耗时（毫秒）</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    internal static async Task LogResponseAsync(ILogger logger, HttpRemoteOptions remoteOptions,
        HttpResponseMessage httpResponseMessage, long requestDuration, CancellationToken cancellationToken = default)
    {
        Log(logger, remoteOptions,
            httpResponseMessage.ProfilerGeneralAndHeaders(generalCustomKeyValues:
                [new KeyValuePair<string, IEnumerable<string>>("Request Duration (ms)", [$"{requestDuration:N2}"])]));
        Log(logger, remoteOptions, await httpResponseMessage.Content.ProfilerAsync("Response Body", cancellationToken).ConfigureAwait(false));
    }

    /// <summary>
    ///     打印 <see cref="CookieContainer" /> 内容
    /// </summary>
    /// <param name="logger">
    ///     <see cref="ILogger" />
    /// </param>
    /// <param name="remoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="request">
    ///     <see cref="HttpRequestMessage" />
    /// </param>
    /// <param name="cookieContainer">
    ///     <see cref="CookieContainer" />
    /// </param>
    internal static void LogCookieContainer(ILogger logger, HttpRemoteOptions remoteOptions, HttpRequestMessage request,
        CookieContainer? cookieContainer)
    {
        // 空检查
        if (request.RequestUri is null || cookieContainer is null)
        {
            return;
        }

        // 获取 Cookie 集合
        var cookies = cookieContainer.GetCookies(request.RequestUri);

        // 空检查
        if (cookies is { Count: 0 })
        {
            return;
        }

        // 打印日志
        Log(logger, remoteOptions,
            StringUtility.FormatKeyValuesSummary(
                cookies.ToDictionary(u => u.Name, u => Enumerable.Empty<string>().Concat([u.Value])),
                "Cookie Container"));
    }

    /// <summary>
    ///     打印日志
    /// </summary>
    /// <param name="logger">
    ///     <see cref="ILogger" />
    /// </param>
    /// <param name="remoteOptions">
    ///     <see cref="HttpRemoteOptions" />
    /// </param>
    /// <param name="message">日志消息</param>
    internal static void Log(ILogger logger, HttpRemoteOptions remoteOptions, string? message)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(logger);

        // 空检查
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        // 检查是否配置（注册）了日志程序
        if (remoteOptions.IsLoggingRegistered)
        {
            logger.Log(remoteOptions.ProfilerLogLevel, "{message}", message);
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    /// <summary>
    ///     提取 <see cref="CookieContainer" /> 实例
    /// </summary>
    /// <returns>
    ///     <see cref="CookieContainer" />
    /// </returns>
    internal CookieContainer? ExtractCookieContainer() =>
        InnerHandler switch
        {
            LoggingHttpMessageHandler loggingHttpMessageHandler => loggingHttpMessageHandler.InnerHandler switch
            {
                SocketsHttpHandler socketsHttpHandler => socketsHttpHandler.CookieContainer,
                HttpClientHandler httpClientHandler => httpClientHandler.CookieContainer,
                _ => null
            },
            LoggingScopeHttpMessageHandler loggingScopeHttpMessageHandler => loggingScopeHttpMessageHandler.InnerHandler
                switch
            {
                SocketsHttpHandler socketsHttpHandler => socketsHttpHandler.CookieContainer,
                HttpClientHandler httpClientHandler => httpClientHandler.CookieContainer,
                _ => null
            },
            SocketsHttpHandler socketsHttpHandler => socketsHttpHandler.CookieContainer,
            HttpClientHandler httpClientHandler => httpClientHandler.CookieContainer,
            _ => null
        };
}