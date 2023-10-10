#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Reflection;

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// Method Info 拓展
/// </summary>
public static class MethodInfoExtensions
{

    /// <summary>
    /// 获取真实方法的特性
    /// </summary>
    /// <returns></returns>
    public static TAttribute GetCustomAttribute<TAttribute>(this object target, string methodStr, bool inherit)
        where TAttribute : Attribute
    {
        return GetActualMethodInfo(methodStr, target)?.GetCustomAttribute<TAttribute>(inherit);
    }

    /// <summary>
    /// 获取实际方法对象
    /// </summary>
    /// <returns></returns>
    private static MethodInfo GetActualMethodInfo(string methodStr, object target)
    {
        if (target == null) return default;

        var actualMethod = target.GetType().GetMethods()
                                             .FirstOrDefault(u => u.ToString().Equals(methodStr));

        return actualMethod;
    }
}