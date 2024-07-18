//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using CSScripting;

using CSScriptLib;

using NewLife.Caching;

using System.Reflection;
using System.Text;

namespace ThingsGateway.Gateway.Application;

public interface IDynamicModel
{
    IEnumerable<dynamic> GetList(IEnumerable<object> datas);
}

public interface IDynamicModelData
{
    dynamic GeData(object datas);
}

/// <summary>
/// 脚本扩展方法
/// </summary>
public static class CSharpScriptEngineExtension
{
    private static string CacheKey = $"{nameof(CSharpScriptEngineExtension)}-{nameof(Do)}";

    private static SemaphoreSlim m_waiterLock = new SemaphoreSlim(1, 1);

    static CSharpScriptEngineExtension()
    {
        Task.Factory.StartNew(async () =>
        {
            while (true)
            {
                await Task.Delay(30000);
                //检测缓存
                try
                {
                    var data = Instance.GetAll();
                    m_waiterLock.Wait();

                    foreach (var item in data)
                    {
                        if (item.Value.ExpiredTime < item.Value.VisitTime + 1800_000)
                        {
                            Instance.Remove(item.Key);
                            item.Value?.Value?.GetType().Assembly.Unload();
                            GC.Collect();
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    m_waiterLock.Release();
                }

                await Task.Delay(30000);
            }
        });
    }

    private static MemoryCache Instance { get; set; } = new MemoryCache();

    /// <summary>
    /// 执行脚本获取返回值ReadWriteExpressions
    /// </summary>
    public static T Do<T>(string source) where T : class
    {
        var field = $"{CacheKey}-{source}";
        var runScript = Instance.Get<T>(field);
        if (runScript == null)
        {
            try
            {
                m_waiterLock.Wait();
                runScript = Instance.Get<T>(field);
                if (runScript == null)
                {
                    if (!source.Contains("return"))
                    {
                        source = $"return {source}";//只判断简单脚本中可省略return字符串
                    }

                    var src = source.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    StringBuilder _using = new StringBuilder();
                    StringBuilder _body = new StringBuilder();
                    src.ToList().ForEach(l =>
                    {
                        if (l.StartsWith("using "))
                        {
                            _using.AppendLine(l);
                        }
                        else
                        {
                            _body.AppendLine(l);
                        }

                    });

                    // 动态加载并执行代码
                    runScript = CSScript.Evaluator.With(eval => eval.IsAssemblyUnloadingEnabled = true).LoadCode<T>(
                       $@"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using ThingsGateway.Gateway.Application;
        using ThingsGateway.Gateway.Application.Extensions;
        {_using}    
        {_body}    
    ");
                    GC.Collect();
                    Instance.Set(field, runScript);
                }
            }
            finally
            {
                m_waiterLock.Release();
            }
        }
        Instance.SetExpire(field, TimeSpan.FromHours(1));

        return runScript;
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

    public static IEnumerable<PropertyInfo> GetProperties(this IEnumerable<dynamic> value, params string[] names)
    {
        // 获取动态对象集合的类型
        var type = value.GetType().GetGenericArguments().LastOrDefault() ?? throw new ArgumentNullException(nameof(value));

        var namesStr = Newtonsoft.Json.JsonConvert.SerializeObject(names);
        // 构建缓存键，包括属性名和类型信息
        var cacheKey = $"{namesStr}-{type.FullName}-{type.TypeHandle.Value}";

        // 从缓存中获取属性信息，如果缓存不存在，则创建并缓存
        var result = Instance.GetOrAdd(cacheKey, a =>
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
