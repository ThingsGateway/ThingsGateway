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
using System.Runtime.Loader;

using ThingsGateway.NewLife.Log;

namespace ThingsGateway.Reflection;

/// <summary>
/// 内部反射静态类
/// </summary>
internal static class Reflect
{
    /// <summary>
    /// 获取入口程序集
    /// </summary>
    /// <returns></returns>
    internal static Assembly GetEntryAssembly()
    {
        return Assembly.GetEntryAssembly();
    }

    /// <summary>
    /// 根据程序集名称获取运行时程序集
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    internal static Assembly GetAssembly(string assemblyName)
    {
        // 加载程序集
        try
        {
            return AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
            return null;
        }
    }

    /// <summary>
    /// 根据路径加载程序集
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static Assembly LoadAssembly(string path)
    {
        if (!File.Exists(path)) return default;
        return Assembly.LoadFrom(path);
    }

    /// <summary>
    /// 通过流加载程序集
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    internal static Assembly LoadAssembly(MemoryStream assembly)
    {
        return Assembly.Load(assembly.ToArray());
    }

    /// <summary>
    /// 根据程序集名称、类型完整限定名获取运行时类型
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="typeFullName"></param>
    /// <returns></returns>
    internal static Type GetType(string assemblyName, string typeFullName)
    {
        return GetAssembly(assemblyName).GetType(typeFullName);
    }

    /// <summary>
    /// 根据程序集和类型完全限定名获取运行时类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="typeFullName"></param>
    /// <returns></returns>
    internal static Type GetType(Assembly assembly, string typeFullName)
    {
        return assembly.GetType(typeFullName);
    }

    /// <summary>
    /// 根据程序集和类型完全限定名获取运行时类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="typeFullName"></param>
    /// <returns></returns>
    internal static Type GetType(MemoryStream assembly, string typeFullName)
    {
        return LoadAssembly(assembly).GetType(typeFullName);
    }

    /// <summary>
    /// 获取程序集名称
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    internal static string GetAssemblyName(Assembly assembly)
    {
        return assembly.GetName().Name;
    }

    /// <summary>
    /// 获取程序集名称
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    internal static string GetAssemblyName(Type type)
    {
        return GetAssemblyName(type.GetTypeInfo());
    }

    /// <summary>
    /// 获取程序集名称
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <returns></returns>
    internal static string GetAssemblyName(TypeInfo typeInfo)
    {
        return GetAssemblyName(typeInfo.Assembly);
    }

    /// <summary>
    /// 加载程序集类型，支持格式：程序集;完全限定的类型名称
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static Type GetStringType(string str)
    {
        var typeDefinitions = str.Split(';');

        // 类型格式必须以分号作为分隔程序集名称和完全限定的类型名称
        if (typeDefinitions.Length != 2)
        {
            throw new InvalidOperationException("The type format must use a semicolon as the separator between the assembly name and the fully qualified type name.");
        }

        return GetType(typeDefinitions[0], typeDefinitions[1]);
    }
}