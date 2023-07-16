#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation;

/// <summary>
/// 简单锁，采用 Interlocked , AsyncAutoResetEvent
/// </summary>
public sealed class EasyLock : DisposableObject
{
    private Lazy<AsyncAutoResetEvent> m_waiterLock = new Lazy<AsyncAutoResetEvent>(() => new AsyncAutoResetEvent(false));

    private int m_waiters = 0;

    /// <summary>
    /// 当前锁是否在等待当中
    /// </summary>
    public bool IsWaitting => (uint)m_waiters > 0;
    /// <summary>
    /// 进入锁
    /// </summary>
    public void Lock()
    {
        if (Interlocked.Increment(ref m_waiters) == 1)
        {
            return;
        }
        m_waiterLock.Value.WaitOneAsync().GetAwaiter().GetResult();
    }
    /// <summary>
    /// 进入锁
    /// </summary>
    public async Task LockAsync()
    {
        if (Interlocked.Increment(ref m_waiters) == 1)
        {
            return;
        }
        await m_waiterLock.Value.WaitOneAsync();
    }
    /// <summary>
    /// 离开锁
    /// </summary>
    public void UnLock()
    {
        if (Interlocked.Decrement(ref m_waiters) == 0)
        {
            return;
        }
        m_waiterLock.Value.Set();
    }
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        m_waiterLock.Value.SafeDispose();
        base.Dispose(disposing);
    }
}