//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// WaitLock，使用轻量级SemaphoreSlim锁，只允许一个并发量，并记录并发信息
/// </summary>
public sealed class WaitLock : DisposableObject
{
    private readonly SemaphoreSlim m_waiterLock = new SemaphoreSlim(1, 1);

    /// <inheritdoc/>
    public WaitLock(bool initialState = true)
    {
        if (!initialState)
            m_waiterLock.Wait();
    }

    /// <inheritdoc/>
    ~WaitLock()
    {
        this.SafeDispose();
    }

    /// <summary>
    /// 当前锁是否在等待当中
    /// </summary>
    public bool IsWaitting => m_waiterLock.CurrentCount == 0;

    /// <summary>
    /// 离开锁
    /// </summary>
    public void Release()
    {
        lock (this)
        {
            if (IsWaitting)
                m_waiterLock.Release();
        }
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public void Wait(CancellationToken cancellationToken = default)
    {
        m_waiterLock.Wait(cancellationToken);
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public bool Wait(TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        var data = m_waiterLock.Wait(timeSpan, cancellationToken);
        return data;
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        return m_waiterLock.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public Task<bool> WaitAsync(TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        return m_waiterLock.WaitAsync(timeSpan, cancellationToken);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        m_waiterLock.SafeDispose();
        base.Dispose(disposing);
    }
}
