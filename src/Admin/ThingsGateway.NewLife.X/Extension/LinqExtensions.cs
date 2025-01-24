//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace ThingsGateway.Extension.Generic;

/// <inheritdoc/>
public static class LinqExtensions
{
    /// <inheritdoc/>
    public static ICollection<T> AddIF<T>(this ICollection<T> thisValue, bool isOk, Func<T> predicate)
    {
        if (isOk)
        {
            thisValue.Add(predicate());
        }

        return thisValue;
    }

    /// <inheritdoc/>
    public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> @where)
    {
        foreach (var obj in @this.Where(where).ToList())
        {
            @this.Remove(obj);
        }
    }

    /// <inheritdoc/>
    public static IEnumerable<T> WhereIf<T>(this IEnumerable<T> thisValue, bool isOk, Func<T, bool> predicate)
    {
        if (isOk)
        {
            thisValue = thisValue.Where(predicate);
        }
        return thisValue;
    }

    /// <inheritdoc/>
    public static void AddRange<T>(this ICollection<T> @this, IEnumerable<T> values)
    {
        foreach (T value in values)
        {
            @this.Add(value);
        }
    }
    /// <inheritdoc/>
    public static void AddRange<T>(this ICollection<T> @this, params T[] values)
    {
        foreach (T item in values)
        {
            @this.Add(item);
        }
    }

    /// <summary>
    /// 从并发字典中删除
    /// </summary>
    public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key) where TKey : notnull
    {
        return dict.TryRemove(key, out TValue? _);
    }


}
