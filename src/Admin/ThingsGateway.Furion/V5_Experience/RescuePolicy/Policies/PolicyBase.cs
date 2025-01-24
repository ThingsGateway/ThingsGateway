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

namespace ThingsGateway.RescuePolicy;

/// <summary>
///     策略抽象基类
/// </summary>
/// <typeparam name="TResult">操作返回值类型</typeparam>
public abstract class PolicyBase<TResult> : IExceptionPolicy<TResult>
{
    /// <inheritdoc />
    public string? PolicyName { get; set; }

    /// <inheritdoc />
    public virtual void Execute(Action operation, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 执行同步操作方法
        Execute(() =>
        {
            operation();

            return default;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Execute(Action<CancellationToken> operation, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 执行同步操作方法
        Execute(token =>
        {
            operation(token);

            return default;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 执行异步操作方法
        await ExecuteAsync(async () =>
        {
            await operation().ConfigureAwait(false);

            return default;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual async Task ExecuteAsync(Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 执行异步操作方法
        await ExecuteAsync(async token =>
        {
            await operation(token).ConfigureAwait(false);

            return default;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public virtual TResult? Execute(Func<TResult?> operation, CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        return ExecuteAsync(() => Task.FromResult(operation()), cancellationToken)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc />
    public virtual TResult? Execute(Func<CancellationToken, TResult?> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        return ExecuteAsync(token => Task.FromResult(operation(token)), cancellationToken)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc />
    public virtual async Task<TResult?> ExecuteAsync(Func<Task<TResult?>> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 执行异步操作方法
        return await ExecuteAsync(async _ => await operation().ConfigureAwait(false), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public abstract Task<TResult?> ExecuteAsync(Func<CancellationToken, Task<TResult?>> operation,
        CancellationToken cancellationToken = default);
}