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
///     <see cref="IHttpContentConverter{TResult}" /> 内容处理器基类
/// </summary>
/// <typeparam name="TResult">转换的目标类型</typeparam>
public abstract class HttpContentConverterBase<TResult> : IHttpContentConverter<TResult>
{
    /// <inheritdoc />
    public IServiceProvider? ServiceProvider { get; set; }

    /// <inheritdoc />
    public abstract TResult? Read(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract Task<TResult?> ReadAsync(HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual object? Read(Type resultType, HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default) =>
        Read(httpResponseMessage, cancellationToken);

    /// <inheritdoc />
    public virtual async Task<object?> ReadAsync(Type resultType, HttpResponseMessage httpResponseMessage,
        CancellationToken cancellationToken = default) =>
        await ReadAsync(httpResponseMessage, cancellationToken).ConfigureAwait(false);
}