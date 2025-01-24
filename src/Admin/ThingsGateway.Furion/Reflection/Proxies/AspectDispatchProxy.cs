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

namespace ThingsGateway.Reflection;

/// <summary>
/// 异步代理分发类
/// </summary>
public abstract class AspectDispatchProxy
{
    /// <summary>
    /// 创建代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProxy"></typeparam>
    /// <returns></returns>
    public static T Create<T, TProxy>() where TProxy : AspectDispatchProxy
    {
        return (T)AspectDispatchProxyGenerator.CreateProxyInstance(typeof(TProxy), typeof(T));
    }

    /// <summary>
    /// 执行同步代理
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract object Invoke(MethodInfo method, object[] args);

    /// <summary>
    /// 执行异步代理
    /// </summary>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract Task InvokeAsync(MethodInfo method, object[] args);

    /// <summary>
    /// 执行异步返回 Task{T} 代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public abstract Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args);
}