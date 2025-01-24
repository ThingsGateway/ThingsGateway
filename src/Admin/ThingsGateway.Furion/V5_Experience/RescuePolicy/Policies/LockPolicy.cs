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
///     并发锁策略
/// </summary>
public sealed class LockPolicy : LockPolicy<object>
{
    /// <summary>
    ///     <inheritdoc cref="LockPolicy" />
    /// </summary>
    public LockPolicy()
    {
    }
}

/// <summary>
///     并发锁策略
/// </summary>
/// <typeparam name="TResult">操作返回值类型</typeparam>
public class LockPolicy<TResult> : PolicyBase<TResult>
{
    /// <summary>
    ///     异步锁对象
    /// </summary>
    internal readonly SemaphoreSlim _asyncLock = new(1);

    /// <summary>
    ///     同步锁对象
    /// </summary>
    internal readonly object _syncLock = new();

    /// <summary>
    ///     <inheritdoc cref="LockPolicy{TResult}" />
    /// </summary>
    public LockPolicy()
    {
    }

    /// <inheritdoc />
    public override TResult? Execute(Func<TResult?> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 对同步锁对象进行加锁，确保同一时间只有一个线程可以进入同步代码块
        lock (_syncLock)
        {
            // 执行操作方法并返回
            return operation();
        }
    }

    /// <inheritdoc />
    public override TResult? Execute(Func<CancellationToken, TResult?> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 对同步锁对象进行加锁，确保同一时间只有一个线程可以进入同步代码块
        lock (_syncLock)
        {
            // 执行操作方法并返回
            return operation(cancellationToken);
        }
    }

    /// <inheritdoc />
    public override async Task<TResult?> ExecuteAsync(Func<CancellationToken, Task<TResult?>> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 获取异步锁，确保同一时间只有一个异步操作可以进入异步代码块
        await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            // 执行操作方法并返回
            return await operation(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // 释放异步锁
            _asyncLock.Release();
        }
    }
}