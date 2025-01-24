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

namespace ThingsGateway.Extensions;

/// <summary>
///     委托拓展类
/// </summary>
internal static class DelegateExtensions
{
    /// <summary>
    ///     尝试执行异步委托
    /// </summary>
    /// <param name="func">异步委托</param>
    /// <param name="parameter1">参数 1</param>
    /// <param name="parameter2">参数 2</param>
    /// <typeparam name="T1">参数类型</typeparam>
    /// <typeparam name="T2">参数类型</typeparam>
    internal static async Task TryInvokeAsync<T1, T2>(this Func<T1, T2, Task>? func, T1 parameter1, T2 parameter2)
    {
        // 空检查
        if (func is null)
        {
            return;
        }

        try
        {
            await func(parameter1, parameter2).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // 输出调试事件
            Debugging.Error(e.Message);
        }
    }

    /// <summary>
    ///     尝试执行异步委托
    /// </summary>
    /// <param name="func">异步委托</param>
    /// <param name="parameter">参数</param>
    /// <typeparam name="T">参数类型</typeparam>
    internal static async Task TryInvokeAsync<T>(this Func<T, Task>? func, T parameter)
    {
        // 空检查
        if (func is null)
        {
            return;
        }

        try
        {
            await func(parameter).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // 输出调试事件
            Debugging.Error(e.Message);
        }
    }

    /// <summary>
    ///     尝试执行异步委托
    /// </summary>
    /// <param name="func">异步委托</param>
    internal static async Task TryInvokeAsync(this Func<Task>? func)
    {
        // 空检查
        if (func is null)
        {
            return;
        }

        try
        {
            await func().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // 输出调试事件
            Debugging.Error(e.Message);
        }
    }

    /// <summary>
    ///     尝试执行同步委托
    /// </summary>
    /// <param name="action">同步委托</param>
    /// <param name="parameter1">参数 1</param>
    /// <param name="parameter2">参数 2</param>
    /// <typeparam name="T1">参数类型</typeparam>
    /// <typeparam name="T2">参数类型</typeparam>
    internal static void TryInvoke<T1, T2>(this Action<T1, T2>? action, T1 parameter1, T2 parameter2)
    {
        // 空检查
        if (action is null)
        {
            return;
        }

        try
        {
            action(parameter1, parameter2);
        }
        catch (Exception e)
        {
            // 输出调试事件
            Debugging.Error(e.Message);
        }
    }

    /// <summary>
    ///     尝试执行同步委托
    /// </summary>
    /// <param name="action">同步委托</param>
    /// <param name="parameter">参数</param>
    /// <typeparam name="T">参数类型</typeparam>
    internal static void TryInvoke<T>(this Action<T>? action, T parameter)
    {
        // 空检查
        if (action is null)
        {
            return;
        }

        try
        {
            action(parameter);
        }
        catch (Exception e)
        {
            // 输出调试事件
            Debugging.Error(e.Message);
        }
    }

    /// <summary>
    ///     尝试执行同步委托
    /// </summary>
    /// <param name="action">同步委托</param>
    internal static void TryInvoke(this Action? action)
    {
        // 空检查
        if (action is null)
        {
            return;
        }

        try
        {
            action();
        }
        catch (Exception e)
        {
            // 输出调试事件
            Debugging.Error(e.Message);
        }
    }
}