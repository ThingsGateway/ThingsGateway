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

namespace ThingsGateway.FriendlyException;

/// <summary>
/// 重试静态类
/// </summary>
[SuppressSniffer]
public sealed class Retry
{
    /// <summary>
    /// 重试有异常的方法，还可以指定特定异常
    /// </summary>
    /// <param name="action"></param>
    /// <param name="numRetries">重试次数</param>
    /// <param name="retryTimeout">重试间隔时间</param>
    /// <param name="finalThrow">是否最终抛异常</param>
    /// <param name="exceptionTypes">异常类型,可多个</param>
    /// <param name="fallbackPolicy">重试失败回调</param>
    /// <param name="retryAction">重试时调用方法</param>
    public static void Invoke(Action action
        , int numRetries
        , int retryTimeout = 1000
        , bool finalThrow = true
        , Type[] exceptionTypes = default
        , Action<Exception> fallbackPolicy = default
        , Action<int, int> retryAction = default)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        InvokeAsync(async () =>
        {
            action();
            await Task.CompletedTask.ConfigureAwait(false);
        }, numRetries, retryTimeout, finalThrow, exceptionTypes, fallbackPolicy == null ? null
        : async (ex) =>
        {
            fallbackPolicy?.Invoke(ex);
            await Task.CompletedTask.ConfigureAwait(false);
        }, retryAction).GetAwaiter().GetResult();
    }

    /// <summary>
    /// 重试有异常的方法，还可以指定特定异常
    /// </summary>
    /// <param name="action"></param>
    /// <param name="numRetries">重试次数</param>
    /// <param name="retryTimeout">重试间隔时间</param>
    /// <param name="finalThrow">是否最终抛异常</param>
    /// <param name="exceptionTypes">异常类型,可多个</param>
    /// <param name="fallbackPolicy">重试失败回调</param>
    /// <param name="retryAction">重试时调用方法</param>
    /// <returns><see cref="Task"/></returns>
    public static async Task InvokeAsync(Func<Task> action
        , int numRetries
        , int retryTimeout = 1000
        , bool finalThrow = true
        , Type[] exceptionTypes = default
        , Func<Exception, Task> fallbackPolicy = default
        , Action<int, int> retryAction = default)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        // 如果重试次数小于或等于 0，则直接调用
        if (numRetries <= 0)
        {
            await action().ConfigureAwait(false);
            return;
        }

        // 存储总的重试次数
        var totalNumRetries = numRetries;

        // 不断重试
        while (true)
        {
            try
            {
                await action().ConfigureAwait(false);
                break;
            }
            catch (Exception ex)
            {
                // 如果可重试次数小于或等于0，则终止重试
                if (--numRetries < 0)
                {
                    if (finalThrow)
                    {
                        if (fallbackPolicy != null) await fallbackPolicy.Invoke(ex).ConfigureAwait(false);
                        throw;
                    }
                    else return;
                }

                // 如果填写了 exceptionTypes 且异常类型不在 exceptionTypes 之内，则终止重试
                if (exceptionTypes != null && exceptionTypes.Length > 0 && !exceptionTypes.Any(u => u.IsAssignableFrom(ex.GetType())))
                {
                    if (finalThrow)
                    {
                        if (fallbackPolicy != null) await fallbackPolicy.Invoke(ex).ConfigureAwait(false);
                        throw;
                    }
                    else return;
                }

                // 重试调用委托
                retryAction?.Invoke(totalNumRetries, totalNumRetries - numRetries);

                // 如果可重试异常数大于 0，则间隔指定时间后继续执行
                if (retryTimeout > 0) await Task.Delay(retryTimeout).ConfigureAwait(false);
            }
        }
    }
}