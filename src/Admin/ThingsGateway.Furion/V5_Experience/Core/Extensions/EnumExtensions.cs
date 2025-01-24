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

using System.ComponentModel;
using System.Reflection;

namespace ThingsGateway.Extensions;

/// <summary>
///     枚举拓展类
/// </summary>
internal static class EnumExtensions
{
    /// <summary>
    ///     获取枚举值描述
    /// </summary>
    /// <param name="enumValue">枚举值</param>
    /// <returns>
    ///     <see cref="string" />
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    internal static string GetEnumDescription(this object enumValue)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(enumValue);

        // 获取枚举类型
        var enumType = enumValue.GetType();

        // 检查是否是枚举类型
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("The parameter is not an enumeration type.", nameof(enumValue));
        }

        // 获取枚举名称
        var enumName = Enum.GetName(enumType, enumValue);

        // 空检查
        ArgumentNullException.ThrowIfNull(enumName);

        // 获取枚举字段
        var enumField = enumType.GetField(enumName);

        // 空检查
        ArgumentNullException.ThrowIfNull(enumField);

        // 获取 [Description] 特性描述
        return enumField.GetCustomAttribute<DescriptionAttribute>(false)
            ?.Description ?? enumName;
    }
}