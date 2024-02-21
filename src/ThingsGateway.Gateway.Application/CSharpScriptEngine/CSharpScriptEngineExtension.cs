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

using ThingsGateway.Core.Extension.Json;

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
    private static readonly CSharpScriptEngine _cSharpScriptEngine = new();

    /// <summary>
    /// GetDynamicModel
    /// </summary>
    public static dynamic GetDynamicModelData<T>(this T data, string script) where T : class
    {
        if (!string.IsNullOrEmpty(script))
        {
            //执行脚本，获取新实体
            var getDeviceModel = _cSharpScriptEngine.Do<IDynamicModelData>(script);
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
            var getDeviceModel = _cSharpScriptEngine.Do<IDynamicModel>(script);
            return getDeviceModel.GetList(datas);
        }
        else
        {
            return datas;
        }
    }

    public static IEnumerable<PropertyInfo> GetProperties(this IEnumerable<dynamic> value, params string[] names)
    {
        var type = value.GetType().GetGenericArguments().LastOrDefault() ?? throw new ArgumentNullException(nameof(value));
        var cacheKey = $"{names.ToJsonString()}-{type.FullName}-{type.TypeHandle.Value}";
        var result = NewLife.Caching.Cache.Default.GetOrAdd(cacheKey, a =>
         {
             var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                   .Where(pi => names.Contains(pi.Name))
                   .Where(pi => pi != null)
                   .AsEnumerable();
             if (names.Count() != properties.Count())
             {
                 throw new InvalidOperationException("Couldn't find all properties on type" + type.Name);
             }

             return properties;
         }, 3600);
        return result;
    }

    public static IEnumerable<IGrouping<object[], dynamic>> GroupByKeys(this IEnumerable<dynamic> values, IEnumerable<string> keys)
    {
        var properties = GetProperties(values, keys.ToArray());

        //objects array as key approch
        return values.GroupBy(v => properties.Select(property => property.GetValue(v)).ToArray(), new ArrayEqualityComparer());
    }
}

public class ArrayEqualityComparer : IEqualityComparer<object[]?>
{
    public bool Equals(object[]? x, object[]? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x == null || y == null) return false;

        if (x.Length != y.Length) return false;

        for (int i = 0; i < x.Length; i++)
        {
            if (!Equals(x[i], y[i]))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(object[]? obj)
    {
        if (obj == null) return 0;

        int hash = 17;

        foreach (var item in obj)
        {
            hash = hash * 23 + (item?.GetHashCode() ?? 0);
        }

        return hash;
    }
}