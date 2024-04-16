//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.OpcUa;

/// <summary>
/// DictionaryExtension
/// </summary>
internal static class DictionaryExtension
{
    #region 字典扩展

    /// <summary>
    /// 移除满足条件的项目。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="pairs"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    internal static int RemoveWhen<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> pairs, Func<KeyValuePair<TKey, TValue>, bool> func)
    {
        var list = new List<TKey>();
        foreach (var item in pairs)
        {
            if (func?.Invoke(item) == true)
            {
                list.Add(item.Key);
            }
        }

        var count = 0;
        foreach (var item in list)
        {
            if (pairs.TryRemove(item, out _))
            {
                count++;
            }
        }
        return count;
    }

#if NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER

    /// <summary>
    /// 尝试添加
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="tkey"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool TryAdd<Tkey, TValue>(this Dictionary<Tkey, TValue> dictionary, Tkey tkey, TValue value)
    {
        if (dictionary.ContainsKey(tkey))
        {
            return false;
        }
        dictionary.Add(tkey, value);
        return true;
    }

#endif

    /// <summary>
    /// 尝试添加
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="tkey"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static void AddOrUpdate<Tkey, TValue>(this Dictionary<Tkey, TValue> dictionary, Tkey tkey, TValue value)
    {
        if (dictionary.ContainsKey(tkey))
        {
            dictionary[tkey] = value;
        }
        else
        {
            dictionary.Add(tkey, value);
        }
    }

    /// <summary>
    /// 获取值。如果键不存在，则返回默认值。
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="tkey"></param>
    /// <returns></returns>
    internal static TValue GetValue<Tkey, TValue>(this Dictionary<Tkey, TValue> dictionary, Tkey tkey)
    {
        return dictionary.TryGetValue(tkey, out var value) ? value : default;
    }

    #endregion 字典扩展
}