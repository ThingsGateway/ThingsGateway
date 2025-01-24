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
///     HTTP 声明式 <see cref="QueryAttribute" /> 特性提取器
/// </summary>
internal sealed class QueryDeclarativeExtractor : IHttpDeclarativeExtractor
{
    /// <inheritdoc />
    public void Extract(HttpRequestBuilder httpRequestBuilder, HttpDeclarativeExtractorContext context)
    {
        /* 情况一：当特性作用于方法或接口时 */

        // 获取 QueryAttribute 特性集合
        var queryAttributes = context.GetMethodDefinedCustomAttributes<QueryAttribute>(true, false)?.ToArray();

        // 空检查
        if (queryAttributes is { Length: > 0 })
        {
            // 遍历所有 [Query] 特性并添加到 HttpRequestBuilder 中
            foreach (var queryAttribute in queryAttributes)
            {
                // 获取查询参数键
                var queryName = queryAttribute.Name;

                // 空检查
                ArgumentException.ThrowIfNullOrEmpty(queryName);

                // 设置查询参数
                if (queryAttribute.HasSetValue)
                {
                    httpRequestBuilder.WithQueryParameter(queryName, queryAttribute.Value, queryAttribute.Escape,
                        replace: queryAttribute.Replace, ignoreNullValues: queryAttribute.IgnoreNullValues);
                }
                // 移除查询参数
                else
                {
                    httpRequestBuilder.RemoveQueryParameters(queryName);
                }
            }
        }

        /* 情况二：当特性作用于参数时 */

        // 查找所有贴有 [Query] 特性的参数集合
        var queryParameters = context.UnFrozenParameters.Where(u => u.Key.IsDefined(typeof(QueryAttribute), true))
            .ToArray();

        // 空检查
        if (queryParameters.Length == 0)
        {
            return;
        }

        // 遍历所有贴有 [Query] 特性的参数
        foreach (var (parameter, value) in queryParameters)
        {
            // 获取 QueryAttribute 特性集合
            var parameterQueryAttributes = parameter.GetCustomAttributes<QueryAttribute>(true);

            // 获取参数名
            var parameterName = AliasAsUtility.GetParameterName(parameter, out var aliasAsDefined);

            // 遍历所有 [Query] 特性并添加到 HttpRequestBuilder 中
            foreach (var queryAttribute in parameterQueryAttributes)
            {
                // 检查参数是否贴了 [AliasAs] 特性
                if (!aliasAsDefined)
                {
                    parameterName = string.IsNullOrWhiteSpace(queryAttribute.AliasAs)
                        ? string.IsNullOrWhiteSpace(queryAttribute.Name) ? parameterName : queryAttribute.Name.Trim()
                        : queryAttribute.AliasAs.Trim();
                }

                // 检查类型是否是基本类型或枚举类型或由它们组成的数组或集合类型
                if (parameter.ParameterType.IsBaseTypeOrEnumOrCollection())
                {
                    httpRequestBuilder.WithQueryParameter(parameterName, value ?? queryAttribute.Value,
                        queryAttribute.Escape, replace: queryAttribute.Replace,
                        ignoreNullValues: queryAttribute.IgnoreNullValues);

                    continue;
                }

                // 空检查
                if (value is not null)
                {
                    httpRequestBuilder.WithQueryParameters(value, queryAttribute.Prefix, queryAttribute.Escape,
                        replace: queryAttribute.Replace, ignoreNullValues: queryAttribute.IgnoreNullValues);
                }
            }
        }
    }
}