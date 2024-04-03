//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace NewLife.Caching;

/// <summary>分布式锁</summary>
public class CacheLock : DisposeBase
{
    private ICache Client { get; set; }

    /// <summary>
    /// 是否持有锁
    /// </summary>
    private Boolean _hasLock = false;

    /// <summary>键</summary>
    public String Key { get; set; }

    /// <summary>实例化</summary>
    /// <param name="client"></param>
    /// <param name="key"></param>
    public CacheLock(ICache client, String key)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));

        Client = client;
        Key = key;
    }

    /// <summary>申请锁</summary>
    /// <param name="msTimeout">锁等待时间，申请加锁时如果遇到冲突则等待的最大时间，单位毫秒</param>
    /// <param name="msExpire">锁过期时间，超过该时间如果没有主动释放则自动释放锁，必须整数秒，单位毫秒</param>
    /// <returns></returns>
    public Boolean Acquire(Int32 msTimeout, Int32 msExpire)
    {
        var ch = Client;
        var now = Runtime.TickCount64;

        // 循环等待
        var end = now + msTimeout;
        while (now < end)
        {
            // 申请加锁。没有冲突时可以直接返回
            var rs = ch.Add(Key, now + msExpire, msExpire / 1000);
            if (rs) return _hasLock = true;

            // 死锁超期检测
            var dt = ch.Get<Int64>(Key);
            if (dt <= now)
            {
                // 开抢死锁。所有竞争者都会修改该锁的时间戳，但是只有一个能拿到旧的超时的值
                var old = ch.Replace(Key, now + msExpire);
                // 如果拿到超时值，说明抢到了锁。其它线程会抢到一个为超时的值
                if (old <= dt)
                {
                    ch.SetExpire(Key, TimeSpan.FromMilliseconds(msExpire));
                    return _hasLock = true;
                }
            }

            // 没抢到，继续
            Thread.Sleep(200);

            now = Runtime.TickCount64;
        }

        return false;
    }

    /// <summary>销毁</summary>
    /// <param name="disposing"></param>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        // 如果客户端已释放，则不删除
        if (Client is DisposeBase db && db.Disposed)
        {
        }
        else
        {
            if (_hasLock)
            {
                Client.Remove(Key);
            }
        }
    }
}