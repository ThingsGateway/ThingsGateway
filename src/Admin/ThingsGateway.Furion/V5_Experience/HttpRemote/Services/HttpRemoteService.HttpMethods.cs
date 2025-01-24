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

namespace ThingsGateway.HttpRemote;

/// <summary>
///     <inheritdoc cref="IHttpRemoteService" />
/// </summary>
internal sealed partial class HttpRemoteService
{
    /// <inheritdoc />
    public HttpResponseMessage Get(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Get(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Get(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send(
        HttpRequestBuilder.Create(HttpMethod.Get, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> GetAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAsync(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> GetAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAsync(
        HttpRequestBuilder.Create(HttpMethod.Get, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public TResult? GetAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAs<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public TResult? GetAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAs<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Get, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> GetAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAsAsync<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> GetAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Get, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Get<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Get<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Get<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Get, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> GetAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        GetAsync<TResult>(requestUri, HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> GetAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Get, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public string? GetAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAs<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? GetAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAs<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? GetAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAs<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public string? GetAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        GetAs<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? GetAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        GetAs<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? GetAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        GetAs<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> GetAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAsAsync<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> GetAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAsAsync<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> GetAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => GetAsAsync<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> GetAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        GetAsAsync<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> GetAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        GetAsAsync<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> GetAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        GetAsAsync<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Put(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Put(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Put(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send(
        HttpRequestBuilder.Create(HttpMethod.Put, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> PutAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAsync(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> PutAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAsync(
        HttpRequestBuilder.Create(HttpMethod.Put, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public TResult? PutAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAs<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public TResult? PutAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAs<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Put, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> PutAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAsAsync<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> PutAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Put, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Put<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Put<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Put<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Put, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> PutAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PutAsync<TResult>(requestUri, HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> PutAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Put, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public string? PutAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAs<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? PutAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAs<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? PutAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAs<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public string? PutAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PutAs<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? PutAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PutAs<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? PutAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PutAs<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> PutAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAsAsync<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> PutAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAsAsync<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> PutAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PutAsAsync<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> PutAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PutAsAsync<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> PutAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PutAsAsync<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> PutAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PutAsAsync<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Post(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Post(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Post(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send(
        HttpRequestBuilder.Create(HttpMethod.Post, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> PostAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAsync(requestUri,
        HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> PostAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAsync(
        HttpRequestBuilder.Create(HttpMethod.Post, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public TResult? PostAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAs<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public TResult? PostAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAs<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Post, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> PostAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAsAsync<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> PostAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Post, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Post<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Post<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Post<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Post, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> PostAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PostAsync<TResult>(requestUri, HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> PostAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Post, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public string? PostAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAs<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? PostAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAs<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? PostAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAs<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public string? PostAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PostAs<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? PostAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PostAs<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? PostAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PostAs<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> PostAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAsAsync<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> PostAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAsAsync<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> PostAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PostAsAsync<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> PostAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PostAsAsync<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> PostAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PostAsAsync<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> PostAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PostAsAsync<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Delete(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Delete(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Delete(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send(
        HttpRequestBuilder.Create(HttpMethod.Delete, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> DeleteAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => DeleteAsync(requestUri,
        HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> DeleteAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAsync(
        HttpRequestBuilder.Create(HttpMethod.Delete, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public TResult? DeleteAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => DeleteAs<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public TResult? DeleteAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAs<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Delete, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> DeleteAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => DeleteAsAsync<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> DeleteAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Delete, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Delete<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Delete<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Delete<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Delete, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> DeleteAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        DeleteAsync<TResult>(requestUri, HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> DeleteAsync<TResult>(string? requestUri,
        HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Delete, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public string? DeleteAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => DeleteAs<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? DeleteAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => DeleteAs<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? DeleteAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => DeleteAs<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public string? DeleteAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        DeleteAs<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? DeleteAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        DeleteAs<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? DeleteAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        DeleteAs<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> DeleteAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        DeleteAsAsync<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> DeleteAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        DeleteAsAsync<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> DeleteAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        DeleteAsAsync<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> DeleteAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        DeleteAsAsync<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> DeleteAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        DeleteAsAsync<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> DeleteAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        DeleteAsAsync<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Head(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Head(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Head(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send(
        HttpRequestBuilder.Create(HttpMethod.Head, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> HeadAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAsync(requestUri,
        HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> HeadAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAsync(
        HttpRequestBuilder.Create(HttpMethod.Head, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public TResult? HeadAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAs<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public TResult? HeadAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAs<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Head, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> HeadAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAsAsync<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> HeadAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Head, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Head<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Head<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Head<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Head, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> HeadAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        HeadAsync<TResult>(requestUri, HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> HeadAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Head, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public string? HeadAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAs<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? HeadAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAs<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? HeadAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAs<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public string? HeadAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        HeadAs<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? HeadAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        HeadAs<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? HeadAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        HeadAs<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> HeadAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAsAsync<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> HeadAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAsAsync<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> HeadAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => HeadAsAsync<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> HeadAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        HeadAsAsync<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> HeadAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        HeadAsAsync<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> HeadAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        HeadAsAsync<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Options(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Options(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Options(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send(
        HttpRequestBuilder.Create(HttpMethod.Options, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> OptionsAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => OptionsAsync(requestUri,
        HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> OptionsAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAsync(
        HttpRequestBuilder.Create(HttpMethod.Options, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public TResult? OptionsAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => OptionsAs<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public TResult? OptionsAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAs<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Options, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> OptionsAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => OptionsAsAsync<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> OptionsAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Options, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Options<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Options<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Options<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Options, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> OptionsAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        OptionsAsync<TResult>(requestUri, HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> OptionsAsync<TResult>(string? requestUri,
        HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Options, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public string? OptionsAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => OptionsAs<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? OptionsAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => OptionsAs<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? OptionsAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => OptionsAs<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public string? OptionsAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        OptionsAs<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? OptionsAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        OptionsAs<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? OptionsAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        OptionsAs<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> OptionsAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        OptionsAsAsync<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> OptionsAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        OptionsAsAsync<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> OptionsAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        OptionsAsAsync<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> OptionsAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        OptionsAsAsync<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> OptionsAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        OptionsAsAsync<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> OptionsAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        OptionsAsAsync<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Trace(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Trace(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Trace(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send(
        HttpRequestBuilder.Create(HttpMethod.Trace, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> TraceAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => TraceAsync(requestUri,
        HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> TraceAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAsync(
        HttpRequestBuilder.Create(HttpMethod.Trace, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public TResult? TraceAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => TraceAs<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public TResult? TraceAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAs<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Trace, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> TraceAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => TraceAsAsync<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> TraceAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Trace, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Trace<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Trace<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Trace<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Trace, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> TraceAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        TraceAsync<TResult>(requestUri, HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> TraceAsync<TResult>(string? requestUri,
        HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Trace, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public string? TraceAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => TraceAs<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? TraceAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => TraceAs<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? TraceAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => TraceAs<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public string? TraceAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        TraceAs<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? TraceAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        TraceAs<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? TraceAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        TraceAs<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> TraceAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        TraceAsAsync<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> TraceAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        TraceAsAsync<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> TraceAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        TraceAsAsync<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> TraceAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        TraceAsAsync<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> TraceAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        TraceAsAsync<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> TraceAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        TraceAsAsync<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Patch(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Patch(requestUri, HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public HttpResponseMessage Patch(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send(
        HttpRequestBuilder.Create(HttpMethod.Patch, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> PatchAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PatchAsync(requestUri,
        HttpCompletionOption.ResponseContentRead,
        configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpResponseMessage> PatchAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAsync(
        HttpRequestBuilder.Create(HttpMethod.Patch, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public TResult? PatchAs<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PatchAs<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public TResult? PatchAs<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => SendAs<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Patch, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> PatchAsAsync<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PatchAsAsync<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<TResult?> PatchAsAsync<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Patch, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Patch<TResult>(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => Patch<TResult>(requestUri,
        HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public HttpRemoteResult<TResult> Patch<TResult>(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) => Send<TResult>(
        HttpRequestBuilder.Create(HttpMethod.Patch, requestUri, configure), completionOption, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> PatchAsync<TResult>(string? requestUri,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PatchAsync<TResult>(requestUri, HttpCompletionOption.ResponseContentRead, configure, cancellationToken);

    /// <inheritdoc />
    public Task<HttpRemoteResult<TResult>> PatchAsync<TResult>(string? requestUri,
        HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        SendAsync<TResult>(HttpRequestBuilder.Create(HttpMethod.Patch, requestUri, configure), completionOption,
            cancellationToken);

    /// <inheritdoc />
    public string? PatchAsString(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PatchAs<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? PatchAsStream(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PatchAs<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? PatchAsByteArray(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) => PatchAs<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public string? PatchAsString(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PatchAs<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Stream? PatchAsStream(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PatchAs<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public byte[]? PatchAsByteArray(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PatchAs<byte[]>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> PatchAsStringAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        PatchAsAsync<string>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> PatchAsStreamAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        PatchAsAsync<Stream>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> PatchAsByteArrayAsync(string? requestUri, Action<HttpRequestBuilder>? configure = null,
        CancellationToken cancellationToken = default) =>
        PatchAsAsync<byte[]>(requestUri, configure, cancellationToken);

    /// <inheritdoc />
    public Task<string?> PatchAsStringAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PatchAsAsync<string>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<Stream?> PatchAsStreamAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PatchAsAsync<Stream>(requestUri, completionOption, configure, cancellationToken);

    /// <inheritdoc />
    public Task<byte[]?> PatchAsByteArrayAsync(string? requestUri, HttpCompletionOption completionOption,
        Action<HttpRequestBuilder>? configure = null, CancellationToken cancellationToken = default) =>
        PatchAsAsync<byte[]>(requestUri, completionOption, configure, cancellationToken);
}