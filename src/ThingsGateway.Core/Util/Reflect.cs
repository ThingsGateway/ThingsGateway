
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Reflection;
using System.Runtime.Loader;

namespace ThingsGateway.Core;

/// <summary>
/// 内部反射静态类
/// </summary>
public static class Reflect
{
    /// <summary>
    /// 根据程序集名称获取运行时程序集
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    public static Assembly GetAssembly(string assemblyName)
    {
        // 加载程序集
        return AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(assemblyName));
    }

    /// <summary>
    /// 根据路径加载程序集
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Assembly LoadAssembly(string path)
    {
        if (!File.Exists(path)) return default;
        return Assembly.LoadFrom(path);
    }

    /// <summary>
    /// 通过流加载程序集
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static Assembly LoadAssembly(MemoryStream assembly)
    {
        return Assembly.Load(assembly.ToArray());
    }

    /// <summary>
    /// 根据程序集名称、类型完整限定名获取运行时类型
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <param name="typeFullName"></param>
    /// <returns></returns>
    public static Type GetType(string assemblyName, string typeFullName)
    {
        return GetAssembly(assemblyName).GetType(typeFullName);
    }

    /// <summary>
    /// 根据程序集和类型完全限定名获取运行时类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="typeFullName"></param>
    /// <returns></returns>
    public static Type GetType(Assembly assembly, string typeFullName)
    {
        return assembly.GetType(typeFullName);
    }

    /// <summary>
    /// 根据程序集和类型完全限定名获取运行时类型
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="typeFullName"></param>
    /// <returns></returns>
    public static Type GetType(MemoryStream assembly, string typeFullName)
    {
        return LoadAssembly(assembly).GetType(typeFullName);
    }

    /// <summary>
    /// 获取程序集名称
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static string GetAssemblyName(Assembly assembly)
    {
        return assembly.GetName().Name;
    }

    /// <summary>
    /// 获取程序集名称
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetAssemblyName(Type type)
    {
        return GetAssemblyName(type.GetTypeInfo());
    }

    /// <summary>
    /// 获取程序集名称
    /// </summary>
    /// <param name="typeInfo"></param>
    /// <returns></returns>
    public static string GetAssemblyName(TypeInfo typeInfo)
    {
        return GetAssemblyName(typeInfo.Assembly);
    }

    /// <summary>
    /// 加载程序集类型，支持格式：程序集;网站类型命名空间
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Type GetStringType(string str)
    {
        var typeDefinitions = str.Split(';');
        return GetType(typeDefinitions[0], typeDefinitions[1]);
    }
}