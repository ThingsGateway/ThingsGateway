//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.NewLife;

/// <summary>
/// WaitLock，使用轻量级SemaphoreSlim锁
/// </summary>
public sealed class WaitLock : IDisposable
{
    private readonly SemaphoreSlim _waiterLock;

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="maxCount">最大并发数</param>
    /// <param name="initialZeroState">初始无信号量</param>
    public WaitLock(int maxCount = 1, bool initialZeroState = false)
    {
        if (initialZeroState)
            _waiterLock = new SemaphoreSlim(0, maxCount);
        else
            _waiterLock = new SemaphoreSlim(maxCount, maxCount);

        MaxCount = maxCount;
    }

    /// <summary>
    /// 最大并发数
    /// </summary>
    public int MaxCount { get; }

    /// <inheritdoc/>
    ~WaitLock()
    {
        Dispose();
    }

    public bool Waited => _waiterLock.CurrentCount == 0;

    public int CurrentCount => _waiterLock.CurrentCount;
    public bool Waitting => _waiterLock.CurrentCount < MaxCount;

    /// <summary>
    /// 离开锁
    /// </summary>
    public void Release()
    {
        if (DisposedValue) return;
        _waiterLock.Release();
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public void Wait(CancellationToken cancellationToken = default)
    {
        _waiterLock.Wait(cancellationToken);
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken = default)
    {
        return _waiterLock.Wait(millisecondsTimeout, cancellationToken);
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public Task WaitAsync(CancellationToken cancellationToken = default)
    {
        return _waiterLock.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// 进入锁
    /// </summary>
    public Task<bool> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken = default)
    {
        return _waiterLock.WaitAsync(millisecondsTimeout, cancellationToken);
    }

    bool DisposedValue;
    public void Dispose()
    {
        DisposedValue = true;
        _waiterLock?.TryDispose();
        GC.SuppressFinalize(this);
    }
}
