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

using System.Diagnostics.CodeAnalysis;

namespace ThingsGateway.RescuePolicy;

/// <summary>
///     超时策略
/// </summary>
/// <remarks>
///     <para>若需要测试同步阻塞，建议使用 <c>Task.Delay(...).Wait()</c> 替代 <c>Thread.Sleep(...)</c></para>
/// </remarks>
public sealed class TimeoutPolicy : TimeoutPolicy<object>
{
    /// <summary>
    ///     <inheritdoc cref="TimeoutPolicy" />
    /// </summary>
    public TimeoutPolicy()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="TimeoutPolicy" />
    /// </summary>
    /// <param name="timeout">超时时间（毫秒）</param>
    public TimeoutPolicy(double timeout)
        : base(timeout)
    {
    }

    /// <summary>
    ///     <inheritdoc cref="TimeoutPolicy" />
    /// </summary>
    /// <param name="timeout">超时时间</param>
    public TimeoutPolicy(TimeSpan timeout)
        : base(timeout)
    {
    }
}

/// <summary>
///     超时策略
/// </summary>
/// <remarks>
///     <para>若需要测试同步阻塞，建议使用 <c>Task.Delay(...).Wait()</c> 替代 <c>Thread.Sleep(...)</c></para>
/// </remarks>
/// <typeparam name="TResult">操作返回值类型</typeparam>
public class TimeoutPolicy<TResult> : PolicyBase<TResult>
{
    /// <summary>
    ///     超时输出信息
    /// </summary>
    internal const string TIMEOUT_MESSAGE = "The operation has timed out.";

    /// <summary>
    ///     <inheritdoc cref="TimeoutPolicy{TResult}" />
    /// </summary>
    public TimeoutPolicy()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="TimeoutPolicy{TResult}" />
    /// </summary>
    /// <param name="timeout">超时时间（毫秒）</param>
    public TimeoutPolicy(double timeout) => Timeout = TimeSpan.FromMilliseconds(timeout);

    /// <summary>
    ///     <inheritdoc cref="TimeoutPolicy{TResult}" />
    /// </summary>
    /// <param name="timeout">超时时间</param>
    public TimeoutPolicy(TimeSpan timeout) => Timeout = timeout;

    /// <summary>
    ///     超时时间
    /// </summary>
    public TimeSpan Timeout { get; set; }

    /// <summary>
    ///     超时时操作方法
    /// </summary>
    public Action<TimeoutPolicyContext<TResult>>? TimeoutAction { get; set; }

    /// <summary>
    ///     添加超时时操作方法
    /// </summary>
    /// <param name="timeoutAction">超时时操作方法</param>
    /// <returns>
    ///     <see cref="RetryPolicy{TResult}" />
    /// </returns>
    public TimeoutPolicy<TResult> OnTimeout(Action<TimeoutPolicyContext<TResult>> timeoutAction)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(timeoutAction);

        TimeoutAction = timeoutAction;

        return this;
    }

    /// <inheritdoc />
    public override TResult? Execute(Func<TResult?> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        return ExecuteAsync(() => Task.Run(operation, cancellationToken), cancellationToken)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc />
    public override TResult? Execute(Func<CancellationToken, TResult?> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        return ExecuteAsync(() => Task.Run(() => operation(cancellationToken), cancellationToken), cancellationToken)
            .GetAwaiter()
            .GetResult();
    }

    /// <inheritdoc />
    public override async Task<TResult?> ExecuteAsync(Func<CancellationToken, Task<TResult?>> operation,
        CancellationToken cancellationToken = default)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(operation);

        // 检查是否配置了超时时间
        if (Timeout == TimeSpan.Zero)
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }

        // 创建关联的取消标识
        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var stoppingToken = cancellationTokenSource.Token;

        // 设置超时时间
        cancellationTokenSource.CancelAfter(Timeout);

        try
        {
            // 获取操作方法任务
            var operationTask = operation(stoppingToken);

            // 获取提前完成的任务
            var completedTask = await Task.WhenAny(operationTask,
                Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, stoppingToken)).ConfigureAwait(false);

            // 检查是否存在取消请求
            cancellationToken.ThrowIfCancellationRequested();

            // 检查提前完成的任务是否是操作方法任务
            if (completedTask == operationTask)
            {
                // 返回操作方法结果
                return await operationTask.ConfigureAwait(false);
            }

            // 抛出超时异常
            ThrowTimeoutException();
        }
        catch (OperationCanceledException exception) when (exception.CancellationToken == stoppingToken)
        {
            // 抛出超时异常
            ThrowTimeoutException();
        }

        return default;
    }

    /// <summary>
    ///     抛出超时异常
    /// </summary>
    /// <exception cref="TimeoutException"></exception>
    [DoesNotReturn]
    internal void ThrowTimeoutException()
    {
        // 输出调试事件
        Debugging.Error(TIMEOUT_MESSAGE);

        // 调用重试时操作方法
        TimeoutAction?.Invoke(new TimeoutPolicyContext<TResult> { PolicyName = PolicyName });

        // 抛出超时异常
        throw new TimeoutException(TIMEOUT_MESSAGE);
    }
}