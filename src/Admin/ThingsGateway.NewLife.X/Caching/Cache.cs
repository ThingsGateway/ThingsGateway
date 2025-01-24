using System.Diagnostics.CodeAnalysis;

namespace ThingsGateway.NewLife.Caching;

/// <summary>缓存</summary>
public abstract class Cache : DisposeBase, ICache
{
    #region 静态默认实现
    /// <summary>默认缓存</summary>
    public static ICache Default { get; set; } = new MemoryCache();
    #endregion

    #region 属性
    /// <summary>名称</summary>
    public String Name { get; set; }

    /// <summary>默认过期时间。避免Set操作时没有设置过期时间，默认3600秒</summary>
    public Int32 Expire { get; set; } = 3600;

    /// <summary>获取和设置缓存，使用默认过期时间</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual Object? this[String key] { get => Get<Object>(key); set => Set(key, value); }

    /// <summary>缓存个数</summary>
    public abstract Int32 Count { get; }

    /// <summary>所有键</summary>
    public abstract ICollection<String> Keys { get; }
    #endregion

    #region 构造
    /// <summary>构造函数</summary>
    protected Cache() => Name = GetType().Name.TrimEnd("Cache");

    /// <summary>销毁。释放资源</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);
    }
    #endregion

    #region 基础操作
    /// <summary>使用连接字符串初始化配置</summary>
    /// <param name="config"></param>
    public virtual void Init(String config) { }

    /// <summary>是否包含缓存项</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public abstract Boolean ContainsKey(String key);

    /// <summary>设置缓存项</summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="expire">过期时间，秒</param>
    /// <returns></returns>
    public abstract Boolean Set<T>(String key, T value, Int32 expire = -1);

    /// <summary>设置缓存项</summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="expire">过期时间</param>
    /// <returns></returns>
    public virtual Boolean Set<T>(String key, T value, TimeSpan expire) => Set(key, value, (Int32)expire.TotalSeconds);

    /// <summary>获取缓存项</summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    [return: MaybeNull]
    public abstract T Get<T>(String key);

    /// <summary>移除缓存项</summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    public abstract Int32 Remove(String key);

    /// <summary>批量移除缓存项</summary>
    /// <param name="keys">键集合</param>
    /// <returns></returns>
    public abstract Int32 Remove(params String[] keys);

    /// <summary>清空所有缓存项</summary>
    public virtual void Clear() => throw new NotSupportedException();

    /// <summary>设置缓存项有效期</summary>
    /// <param name="key">键</param>
    /// <param name="expire">过期时间，秒</param>
    public abstract Boolean SetExpire(String key, TimeSpan expire);

    /// <summary>获取缓存项有效期</summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    public abstract TimeSpan GetExpire(String key);
    #endregion

    #region 集合操作
    /// <summary>批量获取缓存项</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <returns></returns>
    public virtual IDictionary<String, T?> GetAll<T>(IEnumerable<String> keys)
    {
        var dic = new Dictionary<String, T?>();
        foreach (var key in keys)
        {
            dic[key] = Get<T>(key);
        }

        return dic;
    }

    /// <summary>批量设置缓存项</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="expire">过期时间，秒</param>
    public virtual void SetAll<T>(IDictionary<String, T> values, Int32 expire = -1)
    {
        foreach (var item in values)
        {
            Set(item.Key, item.Value, expire);
        }
    }

    /// <summary>获取列表</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    public virtual IList<T> GetList<T>(String key) => throw new NotSupportedException();

    /// <summary>获取哈希</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    public virtual IDictionary<String, T> GetDictionary<T>(String key) => throw new NotSupportedException();

    /// <summary>获取队列</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    public virtual IProducerConsumer<T> GetQueue<T>(String key) => throw new NotSupportedException();

    /// <summary>获取栈</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    public virtual IProducerConsumer<T> GetStack<T>(String key) => throw new NotSupportedException();

    /// <summary>获取Set</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual ICollection<T> GetSet<T>(String key) => throw new NotSupportedException();
    #endregion

    #region 高级操作
    /// <summary>添加，已存在时不更新</summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="expire">过期时间，秒</param>
    /// <returns></returns>
    public virtual Boolean Add<T>(String key, T value, Int32 expire = -1)
    {
        if (ContainsKey(key)) return false;

        return Set(key, value, expire);
    }

    /// <summary>设置新值并获取旧值，原子操作</summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    [return: MaybeNull]
    public virtual T Replace<T>(String key, T value)
    {
        var rs = Get<T>(key);
        Set(key, value);
        return rs;
    }

    /// <summary>尝试获取指定键，返回是否包含值。有可能缓存项刚好是默认值，或者只是反序列化失败</summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值。即使有值也不一定能够返回，可能缓存项刚好是默认值，或者只是反序列化失败</param>
    /// <returns>返回是否包含值，即使反序列化失败</returns>
    public virtual Boolean TryGetValue<T>(String key, [MaybeNullWhen(false)] out T value)
    {
        value = Get<T>(key);
        if (!Equals(value, default)) return true;

        return ContainsKey(key);
    }

    /// <summary>获取 或 添加 缓存数据，在数据不存在时执行委托请求数据</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="callback"></param>
    /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间<seealso cref="Cache.Expire"/></param>
    /// <returns></returns>
    [return: MaybeNull]
    public virtual T GetOrAdd<T>(String key, Func<String, T> callback, Int32 expire = -1)
    {
        var value = Get<T>(key);
        if (!Equals(value, default)) return value;

        if (ContainsKey(key)) return value;

        value = callback(key);

        if (expire < 0) expire = Expire;
        if (Add(key, value, expire)) return value;

        return Get<T>(key);
    }

    /// <summary>累加，原子操作</summary>
    /// <param name="key">键</param>
    /// <param name="value">变化量</param>
    /// <returns></returns>
    public virtual Int64 Increment(String key, Int64 value)
    {
        lock (this)
        {
            var v = Get<Int64>(key);
            v += value;
            Set(key, v);

            return v;
        }
    }

    /// <summary>累加，原子操作</summary>
    /// <param name="key">键</param>
    /// <param name="value">变化量</param>
    /// <returns></returns>
    public virtual Double Increment(String key, Double value)
    {
        lock (this)
        {
            var v = Get<Double>(key);
            v += value;
            Set(key, v);

            return v;
        }
    }

    /// <summary>递减，原子操作</summary>
    /// <param name="key">键</param>
    /// <param name="value">变化量</param>
    /// <returns></returns>
    public virtual Int64 Decrement(String key, Int64 value)
    {
        lock (this)
        {
            var v = Get<Int64>(key);
            v -= value;
            Set(key, v);

            return v;
        }
    }

    /// <summary>递减，原子操作</summary>
    /// <param name="key">键</param>
    /// <param name="value">变化量</param>
    /// <returns></returns>
    public virtual Double Decrement(String key, Double value)
    {
        lock (this)
        {
            var v = Get<Double>(key);
            v -= value;
            Set(key, v);

            return v;
        }
    }
    #endregion

    #region 事务
    /// <summary>提交变更。部分提供者需要刷盘</summary>
    /// <returns></returns>
    public virtual Int32 Commit() => 0;

    /// <summary>申请分布式锁</summary>
    /// <param name="key">要锁定的key</param>
    /// <param name="msTimeout">锁等待时间，单位毫秒</param>
    /// <returns></returns>
    public virtual IDisposable? AcquireLock(String key, Int32 msTimeout)
    {
        var rlock = new CacheLock(this, key);
        if (!rlock.Acquire(msTimeout, msTimeout)) throw new InvalidOperationException($"Lock [{key}] failed! msTimeout={msTimeout}");

        return rlock;
    }

    /// <summary>申请分布式锁</summary>
    /// <param name="key">要锁定的key</param>
    /// <param name="msTimeout">锁等待时间，申请加锁时如果遇到冲突则等待的最大时间，单位毫秒</param>
    /// <param name="msExpire">锁过期时间，超过该时间如果没有主动释放则自动释放锁，必须整数秒，单位毫秒</param>
    /// <param name="throwOnFailure">失败时是否抛出异常，如果不抛出异常，可通过返回null得知申请锁失败</param>
    /// <returns></returns>
    public virtual IDisposable? AcquireLock(String key, Int32 msTimeout, Int32 msExpire, Boolean throwOnFailure)
    {
        var rlock = new CacheLock(this, key);
        if (!rlock.Acquire(msTimeout, msExpire))
        {
            if (throwOnFailure) throw new InvalidOperationException($"Lock [{key}] failed! msTimeout={msTimeout}");
            rlock.Dispose();
            return null;
        }

        return rlock;
    }
    #endregion

    #region 辅助
    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString() => Name;
    #endregion
#if NET6_0_OR_GREATER
    #region 集合
    /// <inheritdoc/>
    public virtual void HashAdd<T>(string key, string hashKey, T value)
    {
        lock (this)
        {
            //获取字典
            var exist = GetDictionary<T>(key);
            if (exist.ContainsKey(hashKey))//如果包含Key
                exist[hashKey] = value;//重新赋值
            else exist.TryAdd(hashKey, value);//加上新的值
            Set(key, exist);
        }
    }

    /// <inheritdoc/>
    public virtual bool HashSet<T>(string key, Dictionary<string, T> dic)
    {
        lock (this)
        {
            //获取字典
            var exist = GetDictionary<T>(key);
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
    public virtual int HashDel<T>(string key, params string[] fields)
    {
        var result = 0;
        //获取字典
        var exist = GetDictionary<T>(key);
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
    public virtual List<T> HashGet<T>(string key, params string[] fields)
    {
        var list = new List<T>();
        //获取字典
        var exist = GetDictionary<T>(key);
        foreach (var field in fields)
        {
            if (exist.TryGetValue(field, out var data))//如果包含Key
            {
                list.Add(data);
            }
            else { list.Add(default); }
        }
        return list;
    }

    /// <inheritdoc/>
    public virtual T HashGetOne<T>(string key, string field)
    {
        //获取字典
        var exist = GetDictionary<T>(key);
        exist.TryGetValue(field, out var result);
        return result;
    }

    /// <inheritdoc/>
    public virtual IDictionary<string, T> HashGetAll<T>(string key)
    {
        var data = GetDictionary<T>(key);
        return data;
    }
    /// <inheritdoc/>
    public void DelByPattern(string pattern)
    {
        var keys = Keys;//获取所有key
        foreach (var item in keys.ToList())
        {
            if (item.StartsWith(pattern))//如果匹配
                Remove(item);
        }
    }
    #endregion

#endif

}
