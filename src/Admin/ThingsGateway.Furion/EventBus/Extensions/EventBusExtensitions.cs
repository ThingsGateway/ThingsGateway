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

namespace ThingsGateway.Extensitions.EventBus;

/// <summary>
/// 事件总线拓展类
/// </summary>
[SuppressSniffer]
public static class EventBusExtensitions
{
    /// <summary>
    /// 将事件枚举 Id 转换成字符串对象
    /// </summary>
    /// <param name="em"></param>
    /// <returns></returns>
    public static string ParseToString(this Enum em)
    {
        var enumType = em.GetType();
        return $"{enumType.Assembly.GetName().Name};{enumType.FullName}.{em}";
    }

    /// <summary>
    /// 将事件枚举字符串转换成枚举对象
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Enum ParseToEnum(this string str)
    {
        var assemblyName = str[..str.IndexOf(';')];
        var fullName = str[(str.IndexOf(';') + 1)..str.LastIndexOf('.')];
        var name = str[(str.LastIndexOf('.') + 1)..];

        return Enum.Parse(Assembly.Load(assemblyName).GetType(fullName), name) as Enum;
    }
}