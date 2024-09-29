// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using System.Reflection;

using ThingsGateway.Extension.Generic;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Gateway.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class DynamicModelExtension
{
    /// <summary>
    /// GetDynamicModel
    /// </summary>
    public static IEnumerable<dynamic> GetDynamicModel<T>(this IEnumerable<T> datas, string script) where T : class
    {
        if (!string.IsNullOrEmpty(script))
        {
            //执行脚本，获取新实体
            var getDeviceModel = CSharpScriptEngineExtension.Do<IDynamicModel>(script);
            return getDeviceModel.GetList(datas);
        }
        else
        {
            return datas;
        }
    }



    /// <summary>
    /// 获取变量的业务属性值
    /// </summary>
    /// <param name="variableRunTime">当前变量</param>
    /// <param name="businessId">对应业务设备Id</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>属性值，如果不存在则返回null</returns>
    public static string? GetPropertyValue(this VariableRunTime variableRunTime, long businessId, string propertyName)
    {
        if (variableRunTime == null || propertyName.IsNullOrWhiteSpace())
            return null;

        // 检查是否存在对应的业务设备Id
        if (variableRunTime.VariablePropertys?.ContainsKey(businessId) == true)
        {
            variableRunTime.VariablePropertys[businessId].TryGetValue(propertyName, out var value);
            return value; // 返回属性值
        }

        return null; // 未找到对应的业务设备Id，返回null
    }

    public static IEnumerable<IGrouping<object[], dynamic>> GroupByKeys(this IEnumerable<dynamic> values, IEnumerable<string> keys)
    {
        // 获取动态对象集合中指定键的属性信息
        var properties = GetProperties(values, keys.ToArray());

        // 使用对象数组作为键进行分组
        return values.GroupBy(v => properties.Select(property => property.GetValue(v)).ToArray(), new ArrayEqualityComparer());
    }

    private static PropertyInfo[] GetProperties(this IEnumerable<dynamic> value, params string[] names)
    {
        // 获取动态对象集合的类型
        var type = value.GetType().GetGenericArguments().LastOrDefault() ?? throw new ArgumentNullException(nameof(value));

        var namesStr = Newtonsoft.Json.JsonConvert.SerializeObject(names);
        // 构建缓存键，包括属性名和类型信息
        var cacheKey = $"{nameof(GetProperties)}-{namesStr}-{type.FullName}-{type.TypeHandle.Value}";

        // 从缓存中获取属性信息，如果缓存不存在，则创建并缓存
        var result = App.CacheService.GetOrAdd(cacheKey, a =>
        {
            // 获取动态对象类型中指定名称的属性信息
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                  .Where(pi => names.Contains(pi.Name)) // 筛选出指定属性名的属性信息
                  .Where(pi => pi != null) // 过滤空属性信息
                  .AsEnumerable();

            // 检查是否找到了所有指定名称的属性，如果没有找到，则抛出异常
            if (names.Count() != properties.Count())
            {
                throw new InvalidOperationException($"Couldn't find properties on type：{type.Name}，{Environment.NewLine}names：{namesStr}");
            }

            return properties.ToArray(); // 返回属性信息集合
        }, 3600); // 缓存有效期为3600秒

        return result; // 返回属性信息集合
    }

}
