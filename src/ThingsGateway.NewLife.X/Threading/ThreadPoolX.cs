
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Diagnostics;

namespace NewLife.Threading;

/// <summary>线程池助手</summary>
public class ThreadPoolX : DisposeBase
{
    #region 全局线程池助手

    static ThreadPoolX()
    {
        // 在这个同步异步大量混合使用的时代，需要更多的初始线程来屏蔽各种对TPL的不合理使用
        ThreadPool.GetMinThreads(out var wt, out var io);
        if (wt < 32 || io < 32)
        {
            if (wt < 32) wt = 32;
            if (io < 32) io = 32;
            ThreadPool.SetMinThreads(wt, io);
        }
    }

    /// <summary>初始化线程池
    /// </summary>
    public static void Init()
    { }

    /// <summary>带异常处理的线程池任务调度，不允许异常抛出，以免造成应用程序退出，同时不会捕获上下文</summary>
    /// <param name="callback"></param>
    [DebuggerHidden]
    public static void QueueUserWorkItem(Action callback)
    {
        if (callback == null) return;

        ThreadPool.UnsafeQueueUserWorkItem(s =>
        {
            try
            {
                callback();
            }
            catch
            {
            }
        }, null);

        //Instance.QueueWorkItem(callback);
    }

    /// <summary>带异常处理的线程池任务调度，不允许异常抛出，以免造成应用程序退出，同时不会捕获上下文</summary>
    /// <param name="callback"></param>
    /// <param name="state"></param>
    [DebuggerHidden]
    public static void QueueUserWorkItem<T>(Action<T> callback, T state)
    {
        if (callback == null) return;

        ThreadPool.UnsafeQueueUserWorkItem(s =>
        {
            try
            {
                callback(state);
            }
            catch
            {
            }
        }, null);

        //Instance.QueueWorkItem(() => callback(state));
    }

    #endregion 全局线程池助手
}