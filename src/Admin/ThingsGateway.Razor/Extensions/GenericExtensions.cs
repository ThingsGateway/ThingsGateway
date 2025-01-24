//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Reflection;

using ThingsGateway.NewLife.Caching;

namespace ThingsGateway.Extension.Generic;

/// <inheritdoc/>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public static class GenericExtensions
{
    private static MemoryCache Instance { get; set; } = new MemoryCache();

    /// <summary>
    /// 把已修改的属性赋值到列表中，并返回字典
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="models"></param>
    /// <param name="oldModel"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static Dictionary<string, object?> GetDiffProperty<T>(this IEnumerable<T> models, T oldModel, T model)
    {
        // 获取Channel类型的所有公共属性
        var properties = typeof(T).GetRuntimeProperties();

        // 比较oldModel和model的属性，找出差异
        var differences = properties
            .Where(prop => prop.CanRead && prop.CanWrite) // 确保属性可读可写
            .Where(prop => !Equals(prop.GetValue(oldModel), prop.GetValue(model))) // 找出值不同的属性
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(model)); // 将属性名和新值存储到字典中

        // 应用差异到channels列表中的每个Channel对象
        foreach (var channel in models)
        {
            foreach (var difference in differences)
            {
                BootstrapBlazor.Components.Utility.SetPropertyValue(channel, difference.Key, difference.Value);
            }
        }

        return differences;
    }


    /// <inheritdoc/>
    public static IEnumerable<PropertyInfo> GetProperties(this IEnumerable<dynamic> value, params string[] names)
    {
        // 获取动态对象集合的类型
        var type = value.GetType().GetGenericArguments().LastOrDefault() ?? throw new ArgumentNullException(nameof(value));

        var namesStr = System.Text.Json.JsonSerializer.Serialize(names);
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
            if (names.Length != properties.Count())
            {
                throw new InvalidOperationException($"Couldn't find properties on type：{type.Name}，{Environment.NewLine}names：{namesStr}");
            }

            return properties; // 返回属性信息集合
        }, 3600); // 缓存有效期为3600秒

        return result!; // 返回属性信息集合
    }

    /// <inheritdoc/>
    public static IEnumerable<IGrouping<object[], dynamic>> GroupByKeys(this IEnumerable<dynamic> values, IEnumerable<string> keys)
    {
        // 获取动态对象集合中指定键的属性信息
        var properties = GetProperties(values, keys.ToArray());

        // 使用对象数组作为键进行分组
        return values.GroupBy(v => properties.Select(property => property.GetValue(v)).ToArray(), new ArrayEqualityComparer());
    }

    /// <summary>
    /// 是否都包含
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="first">第一个列表</param>
    /// <param name="second">第二个列表</param>
    /// <returns></returns>
    public static bool ContainsAll<T>(this IEnumerable<T> first, IEnumerable<T>? second)
    {
        return second.All(s => first.Any(f => f.Equals(s)));
    }
}

/// <inheritdoc/>
public class ArrayEqualityComparer : IEqualityComparer<object[]>
{
    /// <inheritdoc/>
    public bool Equals(object[]? x, object[]? y)
    {
        // 如果引用相同，则返回 true
        if (ReferenceEquals(x, y)) return true;

        // 如果其中一个数组为空，则返回 false
        if (x == null || y == null) return false;

        // 如果两个数组的长度不相等，则返回 false
        if (x.Length != y.Length) return false;

        // 逐个比较数组中的元素是否相等
        for (var i = 0; i < x.Length; i++)
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

    /// <summary>
    /// 计算对象数组的哈希值
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode(object[]? obj)
    {
        // 如果数组为空，则返回 0
        if (obj == null) return 0;

        // 初始化哈希值为 17
        var hash = 17;

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
