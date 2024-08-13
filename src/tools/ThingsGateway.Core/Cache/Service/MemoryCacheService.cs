//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.NewLife.X.Caching;

namespace ThingsGateway.Core;

/// <summary>
/// <inheritdoc cref="ICacheService"/>
/// 内存缓存
/// </summary>
public partial class MemoryCacheService : ICacheService, IDisposable
{
    private readonly MemoryCache _memoryCache;

    /// <summary>
    /// 内存缓存
    /// </summary>
    public MemoryCacheService()
    {
        _memoryCache = new MemoryCache();
    }

    ~MemoryCacheService()
    {
        Dispose();
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    #region 普通操作

    /// <inheritdoc/>
    public void Clear()
    {
        _memoryCache.Clear();
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
    {
        return _memoryCache.ContainsKey(key);
    }

    /// <inheritdoc/>
    public void DelByPattern(string pattern)
    {
        var keys = _memoryCache.Keys;//获取所有key
        foreach (var item in keys.ToList())
        {
            if (item.Contains(pattern))//如果匹配
                _memoryCache.Remove(item);
        }
    }

    /// <inheritdoc/>
    public T Get<T>(string key)
    {
        var data = _memoryCache.Get<T>(key);
        return data;
    }

    /// <inheritdoc/>
    public TimeSpan GetExpire(string key)
    {
        return _memoryCache.GetExpire(key);
    }

    /// <inheritdoc/>
    public T? GetOrCreate<T>(string key, Func<string, T> callback, int expire = -1)
    {
        if (_memoryCache.TryGetValue<T>(key, out var item))
        {
            return item;
        }
        else
        {
            lock (key)
            {
                if (_memoryCache.TryGetValue<T>(key, out var item1))
                {
                    return item1;
                }
                var data = callback(key);
                _memoryCache.Add(key, data, expire);
                return data;
            }
        }
    }

    /// <inheritdoc/>
    public int Remove(params string[] keys)
    {
        return _memoryCache.Remove(keys);
    }

    /// <inheritdoc/>
    public bool Set<T>(string key, T value, int expire = -1)
    {
        return _memoryCache.Set(key, value, expire);
    }

    /// <inheritdoc/>
    public bool Set<T>(string key, T value, TimeSpan expire)
    {
        return _memoryCache.Set(key, value, expire);
    }

    /// <inheritdoc/>
    public bool SetExpire(string key, TimeSpan expire)
    {
        return _memoryCache.SetExpire(key, expire);
    }

    #endregion 普通操作

    #region 集合操作

    /// <inheritdoc/>
    public IDictionary<string, T> GetAll<T>(IEnumerable<string> keys)
    {
        var data = _memoryCache.GetAll<T>(keys);//获取数据
        return data!;
    }

    /// <inheritdoc/>
    public IDictionary<string, T> GetDictionary<T>(string key)
    {
        var data = _memoryCache.GetDictionary<T>(key);
        return data;
    }

    /// <inheritdoc/>
    public IProducerConsumer<T> GetQueue<T>(string key)
    {
        return _memoryCache.GetQueue<T>(key);
    }

    /// <inheritdoc/>
    public ICollection<T> GetSet<T>(string key)
    {
        return _memoryCache.GetSet<T>(key);
    }

    /// <inheritdoc/>
    public IProducerConsumer<T> GetStack<T>(string key)
    {
        return _memoryCache.GetStack<T>(key);
    }

    /// <inheritdoc/>
    public void SetAll<T>(IDictionary<string, T> values, int expire = -1)
    {
        _memoryCache.SetAll(values, expire);
    }

    #endregion 集合操作

    #region 高级操作

    /// <inheritdoc/>
    public bool Add<T>(string key, T value, int expire = -1)
    {
        return _memoryCache.Add(key, value, expire);
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
    public IList<T> GetList<T>(string key)
    {
        var data = _memoryCache.GetList<T>(key);
        return data;
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

    /// <inheritdoc/>
    public T Replace<T>(string key, T value)
    {
        return _memoryCache.Replace(key, value);
    }

    /// <inheritdoc/>
    public bool TryGetValue<T>(string key, out T value)
    {
        return _memoryCache.TryGetValue<T>(key, out value!);
    }

    #endregion 高级操作

    #region 事务

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

    /// <inheritdoc/>
    public int Commit()
    {
        return _memoryCache.Commit();
    }

    #endregion 事务
}
