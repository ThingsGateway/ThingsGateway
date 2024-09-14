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

#if NETCOREAPP
using System.Text.Json;
#endif

namespace ThingsGateway.NewLife.X.Collections.Generic;

/// <summary>集合扩展</summary>
public static class CollectionHelper
{
    /// <summary>集合转为数组，加锁确保安全</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static T[] ToArray<T>(this ICollection<T> collection)
    {
        //if (collection == null) return null;
        lock (collection)
        {
            var count = collection.Count;
            if (count == 0) return Array.Empty<T>();

            var arr = new T[count];
            collection.CopyTo(arr, 0);

            return arr;
        }
    }

    /// <summary>集合转为数组</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static IList<TKey> ToKeyArray<TKey, TValue>(this IDictionary<TKey, TValue> collection, Int32 index = 0) where TKey : notnull
    {
        //if (collection == null) return null;

        if (collection is ConcurrentDictionary<TKey, TValue> cdiv && cdiv.Keys is IList<TKey> list) return list;

        if (collection.Count == 0) return new TKey[0];
        lock (collection)
        {
            var arr = new TKey[collection.Count - index];
            collection.Keys.CopyTo(arr, index);
            return arr;
        }
    }

    /// <summary>集合转为数组</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public static IList<TValue> ToValueArray<TKey, TValue>(this IDictionary<TKey, TValue> collection, Int32 index = 0) where TKey : notnull
    {
        //if (collection == null) return null;

        //if (collection is ConcurrentDictionary<TKey, TValue> cdiv) return cdiv.Values as IList<TValue>;
        if (collection is ConcurrentDictionary<TKey, TValue> cdiv && cdiv.Values is IList<TValue> list) return list;

        if (collection.Count == 0) return new TValue[0];
        lock (collection)
        {
            var arr = new TValue[collection.Count - index];
            collection.Values.CopyTo(arr, index);
            return arr;
        }
    }

    /// <summary>转为可空字典</summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="collection"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static IDictionary<TKey, TValue> ToNullable<TKey, TValue>(this IDictionary<TKey, TValue> collection, IEqualityComparer<TKey>? comparer = null) where TKey : notnull
    {
        //if (collection == null) return null;

        if (collection is NullableDictionary<TKey, TValue> dic && (comparer == null || dic.Comparer == comparer)) return dic;

        if (comparer == null)
            return new NullableDictionary<TKey, TValue>(collection);
        else
            return new NullableDictionary<TKey, TValue>(collection, comparer);
    }

    /// <summary>从队列里面获取指定个数元素</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection">消费集合</param>
    /// <param name="count">元素个数</param>
    /// <returns></returns>
    public static IEnumerable<T> Take<T>(this Queue<T> collection, Int32 count)
    {
        if (collection == null) yield break;

        while (count-- > 0 && collection.Count > 0)
        {
            yield return collection.Dequeue();
        }
    }

    /// <summary>从消费集合里面获取指定个数元素</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection">消费集合</param>
    /// <param name="count">元素个数</param>
    /// <returns></returns>
    public static IEnumerable<T> Take<T>(this IProducerConsumerCollection<T> collection, Int32 count)
    {
        if (collection == null) yield break;

        while (count-- > 0 && collection.TryTake(out var item))
        {
            yield return item;
        }
    }
}
