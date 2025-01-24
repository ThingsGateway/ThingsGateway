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

namespace ThingsGateway.Utilities;

/// <summary>
///     提供别名获取实用方法
/// </summary>
public static class AliasAsUtility
{
    /// <summary>
    ///     获取属性名
    /// </summary>
    /// <param name="property">
    ///     <see cref="PropertyInfo" />
    /// </param>
    /// <param name="isDefined">是否定义特性</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string GetPropertyName(PropertyInfo property, out bool isDefined)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(property);

        // 获取属性名
        var name = property.Name;

        // 检查属性是否贴有 [AliasAs] 特性
        if (!property.IsDefined(typeof(AliasAsAttribute)))
        {
            isDefined = false;
            return name;
        }

        // 获取 AliasAsAttribute 实例的 AliasAs 属性值
        var aliasAs = property.GetCustomAttribute<AliasAsAttribute>()!.AliasAs;

        // 空检查
        if (!string.IsNullOrWhiteSpace(aliasAs))
        {
            name = aliasAs.Trim();
        }

        isDefined = true;
        return name;
    }

    /// <summary>
    ///     获取参数名
    /// </summary>
    /// <param name="parameter">
    ///     <see cref="ParameterInfo" />
    /// </param>
    /// <param name="isDefined">是否定义特性</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    public static string GetParameterName(ParameterInfo parameter, out bool isDefined)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(parameter);

        // 获取参数名
        var name = parameter.Name!;

        // 检查参数是否贴有 [AliasAs] 特性
        if (!parameter.IsDefined(typeof(AliasAsAttribute)))
        {
            isDefined = false;
            return name;
        }

        // 获取 AliasAsAttribute 实例的 AliasAs 属性值
        var aliasAs = parameter.GetCustomAttribute<AliasAsAttribute>()!.AliasAs;

        // 空检查
        if (!string.IsNullOrWhiteSpace(aliasAs))
        {
            name = aliasAs.Trim();
        }

        isDefined = true;
        return name;
    }
}