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
///     异常策略服务
/// </summary>
/// <typeparam name="TResult">操作返回值类型</typeparam>
public interface IExceptionPolicy<TResult>
{
    /// <summary>
    ///     策略名称
    /// </summary>
    string? PolicyName { get; set; }

    /// <summary>
    ///     执行同步操作方法
    /// </summary>
    /// <param name="operation">操作方法</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    void Execute(Action operation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行同步操作方法
    /// </summary>
    /// <param name="operation">操作方法</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    void Execute(Action<CancellationToken> operation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行异步操作方法
    /// </summary>
    /// <param name="operation">操作方法</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行异步操作方法
    /// </summary>
    /// <param name="operation">操作方法</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行同步操作方法
    /// </summary>
    /// <param name="operation">操作方法</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    TResult? Execute(Func<TResult?> operation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行同步操作方法
    /// </summary>
    /// <param name="operation">操作方法</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <typeparamref name="TResult" />
    /// </returns>
    TResult? Execute(Func<CancellationToken, TResult?> operation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行异步操作方法
    /// </summary>
    /// <param name="operation">操作方法</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task<TResult?> ExecuteAsync(Func<Task<TResult?>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     执行异步操作方法
    /// </summary>
    /// <param name="operation">操作方法</param>
    /// <param name="cancellationToken">
    ///     <see cref="CancellationToken" />
    /// </param>
    /// <returns>
    ///     <see cref="Task{TResult}" />
    /// </returns>
    Task<TResult?> ExecuteAsync(Func<CancellationToken, Task<TResult?>> operation,
        CancellationToken cancellationToken = default);
}