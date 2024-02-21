//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Core.Extension.Json;

namespace ThingsGateway.Cache;

/// <summary>
/// <inheritdoc cref="ISimpleCacheService"/>
/// 内存缓存
/// </summary>
public partial class MemoryCacheService : ISimpleCacheService
{
    private readonly MemoryCache _memoryCache;

    /// <summary>
    /// 内存缓存
    /// </summary>
    public MemoryCacheService()
    {
        _memoryCache = new MemoryCache();
    }

    #region 普通操作

    /// <inheritdoc/>
    public T Get<T>(string key)
    {
        var data = _memoryCache.Get<string>(key);
        return data != null ? data.FromJsonString<T>() : default;
    }

    /// <inheritdoc/>
    public int Remove(params string[] keys)
    {
        return _memoryCache.Remove(keys);
    }

    /// <inheritdoc/>
    public bool Set<T>(string key, T value, int expire = -1)
    {
        return _memoryCache.Set(key, value.ToJsonString(), expire);
    }

    /// <inheritdoc/>
    public bool Set<T>(string key, T value, TimeSpan expire)
    {
        return _memoryCache.Set(key, value.ToJsonString(), expire);
    }

    /// <inheritdoc/>
    public bool SetExpire(string key, TimeSpan expire)
    {
        return _memoryCache.SetExpire(key, expire);
    }

    /// <inheritdoc/>
    public TimeSpan GetExpire(string key)
    {
        return _memoryCache.GetExpire(key);
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return _memoryCache.ContainsKey(key);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _memoryCache.Clear();
    }

    /// <inheritdoc/>
    public void DelByPattern(string pattern)
    {
        var keys = _memoryCache.Keys.ToList();//获取所有key
        keys.ForEach(it =>
        {
            if (it.Contains(pattern))//如果匹配
                _memoryCache.Remove(pattern);
        });
    }

    #endregion 普通操作

    #region 集合操作

    /// <inheritdoc/>
    public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
    {
        IDictionary<string, T>? result = default;//定义集合
        IDictionary<string, string?>? data = _memoryCache.GetAll<string>(keys);//获取数据
        foreach (var it in data)
        {
            result.Add(it.Key, it.Value.FromJsonString<T>());//遍历数据,格式化并加到新的数据集合
        }
        return result;
    }

    /// <inheritdoc/>
    public void SetAll<T>(IDictionary<string, T> values, int expire = -1)
    {
        IDictionary<string, string>? result = default;//定义集合
        foreach (var it in values)
        {
            result.Add(it.Key, it.Value.ToJsonString());//遍历数据,格式化并加到新的数据集合
        };
        _memoryCache.SetAll(values, expire);
    }

    /// <inheritdoc/>
    public IDictionary<string, T> GetDictionary<T>(string key)
    {
        IDictionary<string, T>? result = default;//定义集合
        var data = _memoryCache.GetDictionary<string>(key);
        foreach (var it in data)
        {
            result.Add(it.Key, it.Value.FromJsonString<T>());//遍历数据,格式化并加到新的数据集合
        };
        return result;
    }

    /// <inheritdoc/>
    public IProducerConsumer<T> GetQueue<T>(string key)
    {
        return _memoryCache.GetQueue<T>(key);
    }

    /// <inheritdoc/>
    public IProducerConsumer<T> GetStack<T>(string key)
    {
        return _memoryCache.GetStack<T>(key);
    }

    /// <inheritdoc/>
    public ICollection<T> GetSet<T>(string key)
    {
        return _memoryCache.GetSet<T>(key);
    }

    #endregion 集合操作

    #region 高级操作

    /// <inheritdoc/>
    public bool Add<T>(string key, T value, int expire = -1)
    {
        return _memoryCache.Add(key, value.ToJsonString(), expire);
    }

    /// <inheritdoc/>
    public IList<T> GetList<T>(string key)
    {
        IList<T>? result = default;//定义集合
        var data = _memoryCache.GetList<string>(key);
        foreach (var it in data)
        {
            result.Add(it.FromJsonString<T>());//遍历数据,格式化并加到新的数据集合
        };
        return result;
    }

    /// <inheritdoc/>
    public T Replace<T>(string key, T value)
    {
        return _memoryCache.Replace(key, value);
    }

    /// <inheritdoc/>
    public bool TryGetValue<T>(string key, out T value)
    {
        var result = string.Empty;
        _ = _memoryCache.TryGetValue<string>(key, out result);
        value = result.FromJsonString<T>();
        return value == null;
    }

    /// <inheritdoc/>
    public long Decrement(string key, long value)
    {
        return _memoryCache.Decrement(key, value);
    }

    /// <inheritdoc/>
    public double Decrement(string key, double value)
    {
        return _memoryCache.Decrement(key, value);
    }

    /// <inheritdoc/>
    public long Increment(string key, long value)
    {
        return _memoryCache.Increment(key, value);
    }

    /// <inheritdoc/>
    public double Increment(string key, double value)
    {
        return _memoryCache.Increment(key, value);
    }

    #endregion 高级操作

    #region 事务

    /// <inheritdoc/>
    public int Commit()
    {
        return _memoryCache.Commit();
    }

    /// <inheritdoc/>
    public IDisposable AcquireLock(string key, int msTimeout)
    {
        return _memoryCache.AcquireLock(key, msTimeout);
    }

    /// <inheritdoc/>
    public IDisposable AcquireLock(string key, int msTimeout, int msExpire, bool throwOnFailure)
    {
        return _memoryCache.AcquireLock(key, msTimeout, msExpire, throwOnFailure);
    }

    #endregion 事务
}