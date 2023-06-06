#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation;

/// <summary>
/// 等待数据对象
/// </summary>
/// <typeparam name="T"></typeparam>
public class TGWaitData<T> : DisposableObject
{
    private readonly AsyncAutoResetEvent m_waitHandle;
    private WaitDataStatus m_status;
    private T m_waitResult;

    /// <summary>
    /// 构造函数
    /// </summary>
    public TGWaitData()
    {
        m_waitHandle = new AsyncAutoResetEvent(false);
    }

    /// <summary>
    /// 状态
    /// </summary>
    public WaitDataStatus Status => m_status;

    /// <summary>
    /// 等待数据结果
    /// </summary>
    public T WaitResult => m_waitResult;

    /// <summary>
    /// 取消任务
    /// </summary>
    public void Cancel()
    {
        m_status = WaitDataStatus.Canceled;
        m_waitHandle.Set();
    }

    /// <summary>
    /// 使等待的线程继续执行
    /// </summary>
    public bool Set()
    {
        m_status = WaitDataStatus.SetRunning;
        m_waitHandle.Set();
        return true;
    }

    /// <summary>
    /// 使等待的线程继续执行
    /// </summary>
    /// <param name="waitResult">等待结果</param>
    public bool Set(T waitResult)
    {
        m_waitResult = waitResult;
        m_status = WaitDataStatus.SetRunning;
        m_waitHandle.Set();
        return true;
    }

    /// <summary>
    /// 加载取消令箭
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void SetCancellationToken(CancellationToken cancellationToken)
    {
        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(Cancel);
        }
    }

    /// <summary>
    /// 载入结果
    /// </summary>
    public void SetResult(T result)
    {
        m_waitResult = result;
    }

    /// <summary>
    /// 等待指定时间
    /// </summary>
    /// <param name="timeSpan"></param>
    public WaitDataStatus Wait(TimeSpan timeSpan)
    {
        return this.Wait((int)timeSpan.TotalMilliseconds);
    }
    /// <summary>
    /// 等待指定毫秒
    /// </summary>
    /// <param name="millisecond"></param>
    public WaitDataStatus Wait(int millisecond)
    {

        var task = m_waitHandle.WaitOneAsync();
        if (Task.WhenAny(task, Task.Delay(millisecond)).GetAwaiter().GetResult() == task)
        {

        }
        else
        {
            m_status = WaitDataStatus.Overtime;
            m_waitHandle.Set();
        }

        return m_status;
    }

    /// <summary>
    /// 等待指定时间
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public Task<WaitDataStatus> WaitAsync(TimeSpan timeSpan)
    {
        return this.WaitAsync((int)timeSpan.TotalMilliseconds);
    }

    /// <summary>
    /// 等待指定毫秒
    /// </summary>
    /// <param name="millisecond"></param>
    public async Task<WaitDataStatus> WaitAsync(int millisecond)
    {
        var task = m_waitHandle.WaitOneAsync();
        if (await Task.WhenAny(task, Task.Delay(millisecond)) != task)
        {
            m_status = WaitDataStatus.Overtime;
            m_waitHandle.Set();
        }
        return m_status;
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        m_status = WaitDataStatus.Disposed;
        m_waitResult = default;
        m_waitHandle.SafeDispose();
        base.Dispose(disposing);
    }
}