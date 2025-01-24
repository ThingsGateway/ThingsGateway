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

using ThingsGateway.Extensions;
using ThingsGateway.Utilities;

namespace ThingsGateway.HttpRemote;

/// <summary>
///     HTTP 声明式 <see cref="PropertyAttribute" /> 特性提取器
/// </summary>
internal sealed class PropertyDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeExtractorContext context)
    {
        /* 情况一：当特性作用于方法或接口时 */

        // 获取 PropertyAttribute 特性集合
        var propertyAttributes = context.GetMethodDefinedCustomAttributes<PropertyAttribute>(true, false)?.ToArray();

        // 空检查
        if (propertyAttributes is { Length: > 0 })
        {
            // 遍历所有 [Property] 特性并添加到 HttpRequestBuilder 中
            foreach (var propertyAttribute in propertyAttributes)
            {
                // 获取 HttpRequestMessage 请求属性键
                var propertyName = propertyAttribute.Name;

                // 空检查
                ArgumentException.ThrowIfNullOrEmpty(propertyName);

                // 设置 HttpRequestMessage 请求属性
                httpRequestBuilder.WithProperty(propertyName, propertyAttribute.Value);
            }
        }

        /* 情况二：当特性作用于参数时 */

        // 查找所有贴有 [Property] 特性的参数集合
        var propertyParameters = context.UnFrozenParameters.Where(u => u.Key.IsDefined(typeof(PropertyAttribute), true))
            .ToArray();

        // 空检查
        if (propertyParameters.Length == 0)
        {
            return;
        }

        // 遍历所有贴有 [Property] 特性的参数
        foreach (var (parameter, value) in propertyParameters)
        {
            // 获取 PropertyAttribute 特性集合
            var parameterPropertyAttributes = parameter.GetCustomAttributes<PropertyAttribute>(true);

            // 获取参数名
            var parameterName = AliasAsUtility.GetParameterName(parameter, out var aliasAsDefined);

            // 遍历所有 [Property] 特性并添加到 HttpRequestBuilder 中
            foreach (var propertyAttribute in parameterPropertyAttributes)
            {
                // 检查参数是否贴了 [AliasAs] 特性
                if (!aliasAsDefined)
                {
                    parameterName = string.IsNullOrWhiteSpace(propertyAttribute.AliasAs)
                        ? string.IsNullOrWhiteSpace(propertyAttribute.Name)
                            ? parameterName
                            : propertyAttribute.Name.Trim()
                        : propertyAttribute.AliasAs.Trim();
                }

                // 检查类型是否是基本类型或枚举类型或由它们组成的数组或集合类型或 AsItem 属性为真
                if (parameter.ParameterType.IsBaseTypeOrEnumOrCollection() || propertyAttribute.AsItem)
                {
                    httpRequestBuilder.WithProperty(parameterName, value ?? propertyAttribute.Value);

                    continue;
                }

                // 空检查
                if (value is not null)
                {
                    httpRequestBuilder.WithProperties(value);
                }
            }
        }
    }
}