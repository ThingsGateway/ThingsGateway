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

using System.Reflection;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <inheritdoc cref="IHttpRemoteService" />
/// </summary>
internal sealed partial class HttpRemoteService
{
    /// <inheritdoc />
    public void DownloadFile(string? requestUri, string? destinationPath,
        Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        CancellationToken cancellationToken = default) =>
        Send(
            HttpRequestBuilder.DownloadFile(requestUri, destinationPath, onProgressChanged, fileExistsBehavior,
                configure),
            requestConfigure, cancellationToken);

    /// <inheritdoc />
    public Task DownloadFileAsync(string? requestUri, string? destinationPath,
        Func<FileTransferProgress, Task>? onProgressChanged = null,
        FileExistsBehavior fileExistsBehavior = FileExistsBehavior.CreateNew,
        Action<HttpFileDownloadBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            HttpRequestBuilder.DownloadFile(requestUri, destinationPath, onProgressChanged, fileExistsBehavior,
                configure),
            requestConfigure, cancellationToken);

    /// <inheritdoc />
    public void Send(HttpFileDownloadBuilder httpFileDownloadBuilder, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        new FileDownloadManager(this, httpFileDownloadBuilder, configure).Start(cancellationToken);

    /// <inheritdoc />
    public Task SendAsync(HttpFileDownloadBuilder httpFileDownloadBuilder, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        new FileDownloadManager(this, httpFileDownloadBuilder, configure).StartAsync(cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage UploadFile(string? requestUri, string filePath, string name = "file",
        Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        CancellationToken cancellationToken = default) =>
        Send(HttpRequestBuilder.UploadFile(requestUri, filePath, name, onProgressChanged, fileName, configure),
            requestConfigure,
            cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> UploadFileAsync(string? requestUri, string filePath, string name = "file",
        Func<FileTransferProgress, Task>? onProgressChanged = null, string? fileName = null,
        Action<HttpFileUploadBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpRequestBuilder.UploadFile(requestUri, filePath, name, onProgressChanged, fileName, configure),
            requestConfigure,
            cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Send(HttpFileUploadBuilder httpFileUploadBuilder,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        new FileUploadManager(this, httpFileUploadBuilder, configure).Start(cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> SendAsync(HttpFileUploadBuilder httpFileUploadBuilder,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        new FileUploadManager(this, httpFileUploadBuilder, configure).StartAsync(cancellationToken);

    /// <inheritdoc />
    public void ServerSentEvents(string? requestUri, Func<ServerSentEventsData, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        CancellationToken cancellationToken = default) =>
        Send(HttpRequestBuilder.ServerSentEvents(requestUri, onMessage, configure), requestConfigure,
            cancellationToken);

    /// <inheritdoc />
    public Task ServerSentEventsAsync(string? requestUri, Func<ServerSentEventsData, Task> onMessage,
        Action<HttpServerSentEventsBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpRequestBuilder.ServerSentEvents(requestUri, onMessage, configure), requestConfigure,
            cancellationToken);

    /// <inheritdoc />
    public void Send(HttpServerSentEventsBuilder httpServerSentEventsBuilder,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        new ServerSentEventsManager(this, httpServerSentEventsBuilder, configure).Start(cancellationToken);

    /// <inheritdoc />
    public Task SendAsync(HttpServerSentEventsBuilder httpServerSentEventsBuilder,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        new ServerSentEventsManager(this, httpServerSentEventsBuilder, configure).StartAsync(cancellationToken);

    /// <inheritdoc />
    public StressTestHarnessResult StressTestHarness(string? requestUri, int numberOfRequests = 100,
        Action<HttpStressTestHarnessBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) =>
        Send(HttpRequestBuilder.StressTestHarness(requestUri, numberOfRequests, configure), requestConfigure,
            completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<StressTestHarnessResult> StressTestHarnessAsync(string? requestUri, int numberOfRequests = 100,
        Action<HttpStressTestHarnessBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpRequestBuilder.StressTestHarness(requestUri, numberOfRequests, configure), requestConfigure,
            completionOption, cancellationToken);

    /// <inheritdoc />
    public StressTestHarnessResult Send(HttpStressTestHarnessBuilder httpStressTestHarnessBuilder,
        Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) =>
        new StressTestHarnessManager(this, httpStressTestHarnessBuilder, configure).Start(completionOption,
            cancellationToken);

    /// <inheritdoc />
    public Task<StressTestHarnessResult> SendAsync(HttpStressTestHarnessBuilder httpStressTestHarnessBuilder,
        Action<HttpRequestBuilder>? configure = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default) =>
        new StressTestHarnessManager(this, httpStressTestHarnessBuilder, configure).StartAsync(completionOption,
            cancellationToken);

    /// <inheritdoc />
    public void LongPolling(string? requestUri, Func<HttpResponseMessage, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        CancellationToken cancellationToken = default) =>
        Send(HttpRequestBuilder.LongPolling(requestUri, onDataReceived, configure), requestConfigure,
            cancellationToken);

    /// <inheritdoc />
    public Task LongPollingAsync(string? requestUri, Func<HttpResponseMessage, Task> onDataReceived,
        Action<HttpLongPollingBuilder>? configure = null, Action<HttpRequestBuilder>? requestConfigure = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(HttpRequestBuilder.LongPolling(requestUri, onDataReceived, configure), requestConfigure,
            cancellationToken);

    /// <inheritdoc />
    public void Send(HttpLongPollingBuilder httpLongPollingBuilder, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        new LongPollingManager(this, httpLongPollingBuilder, configure).Start(cancellationToken);

    /// <inheritdoc />
    public Task SendAsync(HttpLongPollingBuilder httpLongPollingBuilder, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        new LongPollingManager(this, httpLongPollingBuilder, configure).StartAsync(cancellationToken);

    /// <inheritdoc />
    public object? Declarative(MethodInfo method, object[] args) =>
        SendAs(HttpRequestBuilder.Declarative(method, args));

    /// <inheritdoc />
    public Task<T?> DeclarativeAsync<T>(MethodInfo method, object[] args) =>
        SendAsAsync<T>(HttpRequestBuilder.Declarative(method, args));

    /// <inheritdoc />
    public object? SendAs(HttpDeclarativeBuilder httpDeclarativeBuilder) =>
        new DeclarativeManager(this, httpDeclarativeBuilder).Start();

    /// <inheritdoc />
    public Task<T?> SendAsAsync<T>(HttpDeclarativeBuilder httpDeclarativeBuilder) =>
        new DeclarativeManager(this, httpDeclarativeBuilder).StartAsync<T>();
}