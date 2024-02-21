//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Cache;

/// <summary>
/// 缓存服务
/// </summary>
public partial interface ISimpleCacheService
{
    #region 基础操作

    /// <summary>是否包含缓存项</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool ContainsKey(string key);

    /// <summary>设置缓存项</summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间</param>
    /// <returns></returns>
    bool Set<T>(string key, T value, int expire = -1);

    /// <summary>设置缓存项</summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="expire">过期时间</param>
    /// <returns></returns>
    bool Set<T>(string key, T value, TimeSpan expire);

    /// <summary>获取缓存项</summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    T Get<T>(string key);

    /// <summary>批量移除缓存项</summary>
    /// <param name="keys">键集合</param>
    /// <returns></returns>
    int Remove(params string[] keys);

    /// <summary>清空所有缓存项</summary>
    void Clear();

    /// <summary>设置缓存项有效期</summary>
    /// <param name="key">键</param>
    /// <param name="expire">过期时间</param>
    bool SetExpire(string key, TimeSpan expire);

    /// <summary>获取缓存项有效期</summary>
    /// <param name="key">键</param>
    /// <returns></returns>
    TimeSpan GetExpire(string key);

    /// <summary>
    /// 模糊删除
    /// </summary>
    /// <param name="pattern">匹配关键字</param>
    void DelByPattern(string pattern);

    #endregion 基础操作

    #region 集合操作

    /// <summary>批量获取缓存项</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <returns></returns>
    IDictionary<string, T> GetAll<T>(IEnumerable<string> keys);

    /// <summary>批量设置缓存项</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间</param>
    void SetAll<T>(IDictionary<string, T> values, int expire = -1);

    /// <summary>获取列表</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    IList<T> GetList<T>(string key);

    /// <summary>获取哈希</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    IDictionary<string, T> GetDictionary<T>(string key);

    /// <summary>获取队列</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    IProducerConsumer<T> GetQueue<T>(string key);

    /// <summary>获取栈</summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="key">键</param>
    /// <returns></returns>
    IProducerConsumer<T> GetStack<T>(string key);

    /// <summary>获取Set</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    ICollection<T> GetSet<T>(string key);

    #endregion 集合操作

    #region 高级操作

    /// <summary>添加，已存在时不更新</summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <param name="expire">过期时间，秒。小于0时采用默认缓存时间</param>
    /// <returns></returns>
    bool Add<T>(string key, T value, int expire = -1);

    /// <summary>设置新值并获取旧值，原子操作</summary>
    /// <remarks>
    /// 常常配合Increment使用，用于累加到一定数后重置归零，又避免多线程冲突。
    /// </remarks>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns></returns>
    T Replace<T>(string key, T value);

    /// <summary>尝试获取指定键，返回是否包含值。有可能缓存项刚好是默认值，或者只是反序列化失败，解决缓存穿透问题</summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值。即使有值也不一定能够返回，可能缓存项刚好是默认值，或者只是反序列化失败</param>
    /// <returns>返回是否包含值，即使反序列化失败</returns>
    bool TryGetValue<T>(string key, out T value);

    /// <summary>累加，原子操作</summary>
    /// <param name="key">键</param>
    /// <param name="value">变化量</param>
    /// <returns></returns>
    long Increment(string key, long value);

    /// <summary>累加，原子操作</summary>
    /// <param name="key">键</param>
    /// <param name="value">变化量</param>
    /// <returns></returns>
    double Increment(string key, double value);

    /// <summary>递减，原子操作</summary>
    /// <param name="key">键</param>
    /// <param name="value">变化量</param>
    /// <returns></returns>
    long Decrement(string key, long value);

    /// <summary>递减，原子操作</summary>
    /// <param name="key">键</param>
    /// <param name="value">变化量</param>
    /// <returns></returns>
    double Decrement(string key, double value);

    #endregion 高级操作

    #region 事务

    /// <summary>提交变更。部分提供者需要刷盘</summary>
    /// <returns></returns>
    int Commit();

    /// <summary>申请分布式锁</summary>
    /// <param name="key">要锁定的key</param>
    /// <param name="msTimeout">锁等待时间，单位毫秒</param>
    /// <returns></returns>
    IDisposable AcquireLock(string key, int msTimeout);

    /// <summary>申请分布式锁</summary>
    /// <param name="key">要锁定的key</param>
    /// <param name="msTimeout">锁等待时间，申请加锁时如果遇到冲突则等待的最大时间，单位毫秒</param>
    /// <param name="msExpire">锁过期时间，超过该时间如果没有主动释放则自动释放锁，必须整数秒，单位毫秒</param>
    /// <param name="throwOnFailure">失败时是否抛出异常，如果不抛出异常，可通过返回null得知申请锁失败</param>
    /// <returns></returns>
    IDisposable AcquireLock(string key, int msTimeout, int msExpire, bool throwOnFailure);

    #endregion 事务
}