//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway;
#pragma warning disable CS8604 // 引用类型参数可能为 null。

/// <summary>
/// <inheritdoc cref="ICacheService"/>
/// 内存缓存
/// </summary>
public partial class MemoryCacheService : ICacheService
{
    /// <inheritdoc/>
    public void HashAdd<T>(string key, string hashKey, T value)
    {
        lock (_memoryCache)
        {
            //获取字典
            var exist = _memoryCache.GetDictionary<T>(key);
            if (exist.ContainsKey(hashKey))//如果包含Key
                exist[hashKey] = value;//重新赋值
            else exist.TryAdd(hashKey, value);//加上新的值
            _memoryCache.Set(key, exist);
        }
    }

    /// <inheritdoc/>
    public bool HashSet<T>(string key, Dictionary<string, T> dic)
    {
        lock (_memoryCache)
        {
            //获取字典
            var exist = _memoryCache.GetDictionary<T>(key);
            foreach (var it in dic)
            {
                if (exist.ContainsKey(it.Key))//如果包含Key
                    exist[it.Key] = it.Value;//重新赋值
                else exist.Add(it.Key, it.Value);//加上新的值
            }
            return true;
        }
    }

    /// <inheritdoc/>
    public int HashDel<T>(string key, params string[] fields)
    {
        var result = 0;
        //获取字典
        var exist = _memoryCache.GetDictionary<T>(key);
        foreach (var field in fields)
        {
            if (field != null && exist.ContainsKey(field))//如果包含Key
            {
                exist.Remove(field);//删除
                result++;
            }
        }
        return result;
    }

    /// <inheritdoc/>
    public List<T> HashGet<T>(string key, params string[] fields)
    {
        List<T> list = new List<T>();
        //获取字典
        var exist = _memoryCache.GetDictionary<T>(key);
        foreach (var field in fields)
        {
            if (exist.ContainsKey(field))//如果包含Key
            {
                list.Add(exist[field]);
            }
            else { list.Add(default); }
        }
        return list;
    }

    /// <inheritdoc/>
    public T HashGetOne<T>(string key, string field)
    {
        //获取字典
        var exist = _memoryCache.GetDictionary<T>(key);
        exist.TryGetValue(field, out var result);
        return result;
    }

    /// <inheritdoc/>
    public IDictionary<string, T> HashGetAll<T>(string key)
    {
        var data = _memoryCache.GetDictionary<T>(key);
        return data;
    }
}
