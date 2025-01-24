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

namespace ThingsGateway.Reflection.Extensions;

/// <summary>
/// Method Info 拓展
/// </summary>
[SuppressSniffer]
public static class MethodInfoExtensions
{
    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static IEnumerable<Attribute> GetActualCustomAttributes(this MethodInfo method, object target)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes();
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static object[] GetActualCustomAttributes(this MethodInfo method, object target, bool inherit)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes(inherit);
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="attributeType"></param>
    /// <returns></returns>
    public static IEnumerable<Attribute> GetActualCustomAttributes(this MethodInfo method, object target, Type attributeType)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes(attributeType);
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="attributeType"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static object[] GetActualCustomAttributes(this MethodInfo method, object target, Type attributeType, bool inherit)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes(attributeType, inherit);
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static IEnumerable<TAttribute> GetActualCustomAttributes<TAttribute>(this MethodInfo method, object target)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes<TAttribute>();
    }

    /// <summary>
    /// 获取真实方法的特性集合
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static IEnumerable<TAttribute> GetActualCustomAttributes<TAttribute>(this MethodInfo method, object target, bool inherit)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttributes<TAttribute>(inherit);
    }

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="attributeType"></param>
    /// <returns></returns>
    public static Attribute GetActualCustomAttribute(this MethodInfo method, object target, Type attributeType)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttribute(attributeType);
    }

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="attributeType"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static Attribute GetActualCustomAttribute(this MethodInfo method, object target, Type attributeType, bool inherit)
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttribute(attributeType, inherit);
    }

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static TAttribute GetActualCustomAttribute<TAttribute>(this MethodInfo method, object target)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttribute<TAttribute>();
    }

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <param name="inherit"></param>
    /// <returns></returns>
    public static TAttribute GetActualCustomAttribute<TAttribute>(this MethodInfo method, object target, bool inherit)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(method, target)?.GetCustomAttribute<TAttribute>(inherit);
    }

    /// <summary>
    /// 获取实际方法对象
    /// </summary>
    /// <param name="method"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private static MethodInfo GetActualMethodInfo(MethodInfo method, object target)
    {
        if (target == null) return default;

        var targetType = target.GetType();
        var actualMethod = targetType.GetMethods()
                                             .FirstOrDefault(u => u.ToString().Equals(method.ToString()));

        if (actualMethod == null) return default;

        return actualMethod;
    }
}