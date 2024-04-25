
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using CSScriptLib;

using System.Reflection;

using ThingsGateway.Core.Json.Extension;

namespace ThingsGateway.Gateway.Application;

#if false
//变量脚本动态设置传输实体的Demo

    public class DemoScript:IDynamicModel
    {
        public IEnumerable<dynamic> GetList(IEnumerable<dynamic> datas)
        {
            List<DemoData> demoDatas = new List<DemoData>();
            foreach (var data in datas)
            {
                DemoData demoData = new DemoData();
                demoData.Value = data.Value;
                demoData.Name = data.Name;
                demoData.IsOnline = data.IsOnline;
                demoData.ChangeTime = data.ChangeTime;
                demoDatas.Add(demoData);
            }
            return demoDatas;
        }
    }
    public class DemoData
    {
        public string Name { get; set; }
        public bool IsOnline { get; set; }
        public object Value { get; set; }
        public DateTime ChangeTime { get; set; }
    }
#endif

#if false

//采集设备脚本动态设置传输实体的Demo
public class DemoScript:IDynamicModel
{
    public IEnumerable<dynamic> GetList(IEnumerable<dynamic> datas)
    {
        List<DemoData> demoDatas = new List<DemoData>();
        foreach (var data in datas)
        {
            DemoData demoData = new DemoData();
            demoData.Name = data.Name;
            demoData.ActiveTime = data.ActiveTime;
            demoData.DeviceStatus = data.DeviceStatus;
            demoDatas.Add(demoData);
        }
        return demoDatas;
    }
}
public class DemoData
{
    public string Name { get; set; }

    public DateTime ActiveTime { get; set; }

    public DeviceStatusEnum DeviceStatus { get; set; }
}
#endif

public interface IDynamicModel
{
    IEnumerable<dynamic> GetList(IEnumerable<dynamic> datas);
}

public interface IDynamicModelData
{
    dynamic GeData(dynamic datas);
}

/// <summary>
/// 脚本扩展方法
/// </summary>
public static class CSharpScriptEngineExtension
{
    /// <summary>
    /// 执行脚本获取返回值，通常用于上传实体返回脚本，参数为input
    /// </summary>
    public static T Do<T>(string _source) where T : class
    {
        var cacheKey = $"{nameof(CSharpScriptEngineExtension)}-{Do<T>}-{_source}";
        var runscript = App.CacheService.GetOrCreate(cacheKey, c =>
        {
            var eva = CSScript.Evaluator
              .LoadCode<T>(
@$"
using System;
using System.Linq;
using System.Collections.Generic;
using ThingsGateway.Core.Json.Extension;
using ThingsGateway.Gateway.Application;
{_source}
");
            return eva;
        }, 3600);
        return runscript;
    }

    /// <summary>
    /// GetDynamicModel
    /// </summary>
    public static dynamic GetDynamicModelData<T>(this T data, string script) where T : class
    {
        if (!string.IsNullOrEmpty(script))
        {
            //执行脚本，获取新实体
            var getDeviceModel = Do<IDynamicModelData>(script);
            return getDeviceModel.GeData(data);
        }
        else
        {
            return data;
        }
    }

    /// <summary>
    /// GetDynamicModel
    /// </summary>
    public static IEnumerable<dynamic> GetDynamicModel<T>(this IEnumerable<T> datas, string script) where T : class
    {
        if (!string.IsNullOrEmpty(script))
        {
            //执行脚本，获取新实体
            var getDeviceModel = Do<IDynamicModel>(script);
            return getDeviceModel.GetList(datas);
        }
        else
        {
            return datas;
        }
    }

    public static IEnumerable<PropertyInfo> GetProperties(this IEnumerable<dynamic> value, params string[] names)
    {
        // 获取动态对象集合的类型
        var type = value.GetType().GetGenericArguments().LastOrDefault() ?? throw new ArgumentNullException(nameof(value));

        // 构建缓存键，包括属性名和类型信息
        var cacheKey = $"{names.ToSystemTextJsonString()}-{type.FullName}-{type.TypeHandle.Value}";

        // 从缓存中获取属性信息，如果缓存不存在，则创建并缓存
        var result = App.CacheService.GetOrCreate(cacheKey, a =>
        {
            // 获取动态对象类型中指定名称的属性信息
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                  .Where(pi => names.Contains(pi.Name)) // 筛选出指定属性名的属性信息
                  .Where(pi => pi != null) // 过滤空属性信息
                  .AsEnumerable();

            // 检查是否找到了所有指定名称的属性，如果没有找到，则抛出异常
            if (names.Count() != properties.Count())
            {
                throw new InvalidOperationException("Couldn't find all properties on type" + type.Name);
            }

            return properties; // 返回属性信息集合
        }, 3600); // 缓存有效期为3600秒

        return result; // 返回属性信息集合
    }

    public static IEnumerable<IGrouping<object[], dynamic>> GroupByKeys(this IEnumerable<dynamic> values, IEnumerable<string> keys)
    {
        // 获取动态对象集合中指定键的属性信息
        var properties = GetProperties(values, keys.ToArray());

        // 使用对象数组作为键进行分组
        return values.GroupBy(v => properties.Select(property => property.GetValue(v)).ToArray(), new ArrayEqualityComparer());
    }
}

public class ArrayEqualityComparer : IEqualityComparer<object[]>
{
    // 判断两个对象数组是否相等
    public bool Equals(object[]? x, object[]? y)
    {
        // 如果引用相同，则返回 true
        if (ReferenceEquals(x, y)) return true;

        // 如果其中一个数组为空，则返回 false
        if (x == null || y == null) return false;

        // 如果两个数组的长度不相等，则返回 false
        if (x.Length != y.Length) return false;

        // 逐个比较数组中的元素是否相等
        for (int i = 0; i < x.Length; i++)
        {
            // 如果任何一个元素不相等，则返回 false
            if (!Equals(x[i], y[i]))
            {
                return false;
            }
        }

        // 如果所有元素都相等，则返回 true
        return true;
    }

    // 计算对象数组的哈希值
    public int GetHashCode(object[]? obj)
    {
        // 如果数组为空，则返回 0
        if (obj == null) return 0;

        // 初始化哈希值为 17
        int hash = 17;

        // 遍历数组中的每个元素，计算哈希值并与当前哈希值组合
        foreach (var item in obj)
        {
            // 如果元素不为空，则计算其哈希值并与当前哈希值组合
            hash = hash * 23 + (item?.GetHashCode() ?? 0);
        }

        // 返回最终计算得到的哈希值
        return hash;
    }
}
