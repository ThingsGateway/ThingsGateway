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

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// EasyLock，使用轻量级SemaphoreSlim锁，只允许一个并发量，并记录并发信息
/// </summary>
public sealed class EasyLock : DisposableObject
{
    private static long lockWaitCount;
    private readonly SemaphoreSlim m_waiterLock = new SemaphoreSlim(1, 1);

    /// <inheritdoc/>
    public EasyLock(bool initialState = true)
    {
        if (!initialState)
            m_waiterLock.Wait();
    }

    /// <inheritdoc/>
    ~EasyLock()
    {
        m_waiterLock.SafeDispose();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        m_waiterLock.SafeDispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 当前正在等待的数量
    /// </summary>
    public static long LockWaitCount => lockWaitCount;

    /// <summary>
    /// 当前锁是否在等待当中
    /// </summary>
    public bool IsWaitting => m_waiterLock.CurrentCount == 0;

    /// <summary>
    /// 离开锁
    /// </summary>
    public void Release()
    {
        m_waiterLock.Release();
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public void Wait(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref lockWaitCount);
        m_waiterLock.Wait(cancellationToken);
        Interlocked.Decrement(ref lockWaitCount);
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public bool Wait(TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref lockWaitCount);
        var data = m_waiterLock.Wait(timeSpan, cancellationToken);
        Interlocked.Decrement(ref lockWaitCount);
        return data;
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref lockWaitCount);
        await m_waiterLock.WaitAsync(cancellationToken);
        Interlocked.Decrement(ref lockWaitCount);
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public async Task<bool> WaitAsync(TimeSpan timeSpan, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref lockWaitCount);
        var data = await m_waiterLock.WaitAsync(timeSpan, cancellationToken);
        Interlocked.Decrement(ref lockWaitCount);
        return data;
    }
}