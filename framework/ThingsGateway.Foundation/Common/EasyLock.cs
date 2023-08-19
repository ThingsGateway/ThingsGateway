#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation;

/// <summary>
/// EasyLock，使用轻量级SemaphoreSlim锁，只允许一个并发量，并记录并发信息
/// </summary>
public sealed class EasyLock : IDisposable
{
    private static long lockWaitCount;
    private readonly Lazy<SemaphoreSlim> m_waiterLock = new(() => new SemaphoreSlim(1));
    /// <inheritdoc/>
    ~EasyLock()
    {
        m_waiterLock.Value.SafeDispose();
    }
    /// <summary>
    /// 当前正在等待的数量
    /// </summary>
    public static long LockWaitCount => lockWaitCount;
    /// <summary>
    /// 当前锁是否在等待当中
    /// </summary>
    public bool IsWaitting => m_waiterLock.Value.CurrentCount == 0;

    /// <inheritdoc/>
    public void Dispose()
    {
        m_waiterLock.Value.SafeDispose();
    }

    /// <summary>
    /// 离开锁
    /// </summary>
    public void Release()
    {
        m_waiterLock.Value.Release();
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public void Wait()
    {
        Interlocked.Increment(ref lockWaitCount);
        m_waiterLock.Value.Wait();
        Interlocked.Decrement(ref lockWaitCount);
    }
    /// <summary>
    /// 进入锁
    /// </summary>
    public void Wait(TimeSpan timeSpan, CancellationToken token)
    {
        Interlocked.Increment(ref lockWaitCount);
        m_waiterLock.Value.Wait(timeSpan, token);
        Interlocked.Decrement(ref lockWaitCount);
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public async Task WaitAsync()
    {
        Interlocked.Increment(ref lockWaitCount);
        await m_waiterLock.Value.WaitAsync();
        Interlocked.Decrement(ref lockWaitCount);
    }
    /// <summary>
    /// 进入锁
    /// </summary>
    public async Task WaitAsync(TimeSpan timeSpan, CancellationToken token)
    {
        Interlocked.Increment(ref lockWaitCount);
        await m_waiterLock.Value.WaitAsync(timeSpan, token);
        Interlocked.Decrement(ref lockWaitCount);
    }

}