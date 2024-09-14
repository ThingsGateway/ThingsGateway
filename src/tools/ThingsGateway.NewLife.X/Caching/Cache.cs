//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;


namespace ThingsGateway.NewLife.X.Caching;

/// <summary>缓存</summary>
public abstract class Cache : DisposeBase, ICache
{
    #region 属性

    /// <summary>缓存个数</summary>
    public abstract Int32 Count { get; }

    /// <summary>默认过期时间。避免Set操作时没有设置过期时间，默认0秒表示不过期</summary>
    public Int32 Expire { get; set; }

    /// <summary>所有键</summary>
    public abstract ICollection<String> Keys { get; }

    /// <summary>名称</summary>
    public String Name { get; set; }

    /// <summary>获取和设置缓存，使用默认过期时间</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual Object? this[String key] { get => Get<Object>(key); set => Set(key, value); }

    #endregion 属性

    #region 构造

    /// <summary>构造函数</summary>
    protected Cache() => Name = GetType().Name.TrimEnd("Cache");

    #endregion 构造

    #region 基础操作

    /// <summary>清空所有缓存项</summary>
    public virtual void Clear() => throw new NotSupportedException();

    /// <summary>是否包含缓存项</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public abstract Boolean ContainsKey(String key);

    /// <summary>获取缓存项</summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    [return: MaybeNull]
    public abstract T Get<T>(String key);

    /// <summary>获取缓存项有效期</summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    public abstract TimeSpan GetExpire(String key);

    /// <summary>使用连接字符串初始化配置</summary>
    /// <param name="config"></param>
    public virtual void Init(String config)
    { }

    /// <summary>批量移除缓存项</summary>
    /// <param name="keys">键集合</param>
    /// <returns></returns>
    public abstract Int32 Remove(params String[] keys);

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

    /// <summary>设置缓存项有效期</summary>
    /// <param name="key">键</param>
    /// <param name="expire">过期时间，秒</param>
    public abstract Boolean SetExpire(String key, TimeSpan expire);

    #endregion 基础操作

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

    /// <summary>获取哈希</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    public virtual IDictionary<String, T> GetDictionary<T>(String key) => throw new NotSupportedException();

    /// <summary>获取列表</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    public virtual IList<T> GetList<T>(String key) => throw new NotSupportedException();

    /// <summary>获取队列</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    public virtual IProducerConsumer<T> GetQueue<T>(String key) => throw new NotSupportedException();

    /// <summary>获取Set</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public virtual ICollection<T> GetSet<T>(String key) => throw new NotSupportedException();

    /// <summary>获取栈</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    public virtual IProducerConsumer<T> GetStack<T>(String key) => throw new NotSupportedException();

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

    #endregion 集合操作

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

    #endregion 高级操作

    #region 事务

    /// <summary>申请分布式锁</summary>
    /// <param name="key">要锁定的key</param>
    /// <param name="msTimeout">锁等待时间，单位毫秒</param>
    /// <returns></returns>
    public IDisposable? AcquireLock(String key, Int32 msTimeout)
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
    public IDisposable? AcquireLock(String key, Int32 msTimeout, Int32 msExpire, Boolean throwOnFailure)
    {
        var rlock = new CacheLock(this, key);
        if (!rlock.Acquire(msTimeout, msExpire))
        {
            if (throwOnFailure) throw new InvalidOperationException($"Lock [{key}] failed! msTimeout={msTimeout}");

            return null;
        }

        return rlock;
    }

    /// <summary>提交变更。部分提供者需要刷盘</summary>
    /// <returns></returns>
    public virtual Int32 Commit() => 0;

    #endregion 事务

    #region 辅助

    /// <summary>已重载。</summary>
    /// <returns></returns>
    public override String ToString() => Name;

    #endregion 辅助
}
