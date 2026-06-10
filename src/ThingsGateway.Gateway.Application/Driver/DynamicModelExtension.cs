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

using ThingsGateway.Common.Extension.Generic;
using ThingsGateway.Foundation.Common.Json.Extension;

namespace ThingsGateway.Gateway.Application;

[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class DynamicModelExtension
{
    /// <summary>
    /// GetDynamicModel
    /// </summary>
    public static IEnumerable<object> GetDynamicModel<T>(this IEnumerable<T> datas, string script, TouchSocket.Core.ILog log)
    {
        if (!string.IsNullOrEmpty(script))
        {
            //执行脚本，获取新实体
            var getDeviceModel = CSharpScriptEngineExtension.Do<IDynamicModel>(script);
            if (getDeviceModel is DynamicModelBase dynamicModelBase)
            {
                dynamicModelBase.Logger = log;
            }
            return getDeviceModel.GetList(datas?.Cast<object>());
        }
        else
        {
            return datas?.Cast<object>();
        }
    }

    /// <summary>
    /// 获取变量的业务属性值
    /// </summary>
    /// <param name="variableRuntime">当前变量</param>
    /// <param name="businessId">对应业务设备Id</param>
    /// <param name="propertyName">属性名称</param>
    /// <returns>属性值，如果不存在则返回null</returns>
    public static string? GetPropertyValue(this VariableRuntime variableRuntime, long businessId, string propertyName)
    {
        if (variableRuntime == null || propertyName.IsNullOrWhiteSpace())
            return null;

        // 检查是否存在对应的业务设备Id
        if (variableRuntime.VariablePropertys?.TryGetValue(businessId, out var keyValuePairs) == true)
        {
            keyValuePairs.TryGetValue(propertyName, out var value);
            return value; // 返回属性值
        }



        if (GlobalData.TryGetDeviceRuntime(businessId, out var deviceRuntime))
        {
            if (deviceRuntime.Driver is BusinessBase businessBase)
            {
                return GetVariableProperty(variableRuntime, businessId, propertyName, businessBase);
            }
        }


        return null; // 未找到对应的业务设备Id，返回null

        static string? GetVariableProperty(VariableRuntime variableRuntime, long businessId, string propertyName, BusinessBase businessBase)
        {
            // 检查是否存在对应的业务设备Id
            if (variableRuntime.VariablePropertys?.TryGetValue(businessId, out var kv) == true)
            {
                kv.TryGetValue(propertyName, out var value);
                return value; // 返回属性值
            }
            else
            {
                return ThingsGatewayStringConverter.Default.Serialize(null, businessBase.VariablePropertys.GetValueEx(businessBase.VariablePropertys.GetType(), propertyName, false));
            }
        }
    }

    public static IEnumerable<IGrouping<object[], T>> GroupByKeys<T>(this IEnumerable<T> values, params string[] keys)
    {
        // 获取动态对象集合中指定键的属性信息
        var properties = GetProperties(values, keys);

        // 使用对象数组作为键进行分组
        return values.GroupBy(v => properties.Select(property => property.GetValue(v)).ToArray(), new ArrayEqualityComparer());
    }

    private static PropertyInfo[] GetProperties<T>(this IEnumerable<T> value, params string[] names)
    {
        // 获取动态对象集合的类型
        var type = value.GetType().GetGenericArguments().FirstOrDefault() ?? throw new ArgumentNullException(nameof(value));

        var namesStr = names.ToSystemTextJsonString(false);
        // 构建缓存键，包括属性名和类型信息
        var cacheKey = $"{nameof(GetProperties)}-{namesStr}-{type.FullName}-{type.TypeHandle.Value}";

        // 从缓存中获取属性信息，如果缓存不存在，则创建并缓存
        var result = App.CacheService.GetOrAdd(cacheKey, a =>
        {
            // 获取动态对象类型中指定名称的属性信息
            var allProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                                           .ToDictionary(pi => pi.Name, pi => pi);

            // 检查是否找到了所有指定名称的属性，如果没有找到，则抛出异常
            var properties = names.Select(name =>
            {
                if (!allProperties.TryGetValue(name, out var pi))
                    throw new InvalidOperationException($"Couldn't find property '{name}' on type: {type.Name}");

                return pi;
            }).ToArray();

            return properties; // 返回属性信息集合
        }, 3600); // 缓存有效期为3600秒

        return result; // 返回属性信息集合
    }
}

public interface IDynamicModel
{
    IEnumerable<dynamic> GetList(IEnumerable<object> datas);
}
public abstract class DynamicModelBase : IDynamicModel
{
    public TouchSocket.Core.ILog Logger { get; set; }
    public abstract IEnumerable<dynamic> GetList(IEnumerable<object> datas);
}
