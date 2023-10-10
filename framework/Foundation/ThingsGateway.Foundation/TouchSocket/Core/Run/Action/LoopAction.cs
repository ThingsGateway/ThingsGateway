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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 循环动作
    /// </summary>
    public class LoopAction : EasyTask, IDisposable
    {
        /// <summary>
        /// 析构函数
        /// </summary>
        ~LoopAction()
        {
            this.Dispose();
        }

        private readonly EventWaitHandle m_waitHandle;

        private LoopAction(int count, TimeSpan interval, Action<LoopAction> action)
        {
            this.LoopCount = count;
            this.ExecuteAction = action;
            this.Interval = interval;
            this.m_waitHandle = new AutoResetEvent(false);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="count">循环次数，设为-1时一直循环</param>
        /// <param name="interval">每次循环间隔</param>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(int count, TimeSpan interval, Action<LoopAction> action)
        {
            return new LoopAction(count, interval, action);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="count">循环次数，设为-1时一直循环</param>
        /// <param name="intervalMS">每次循环间隔，毫秒</param>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(int count, int intervalMS, Action<LoopAction> action)
        {
            return new LoopAction(count, TimeSpan.FromMilliseconds(intervalMS), action);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="count">循环次数，设为-1时一直循环</param>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(int count, Action<LoopAction> action)
        {
            return CreateLoopAction(count, TimeSpan.Zero, action);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="interval">每次循环间隔</param>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(TimeSpan interval, Action<LoopAction> action)
        {
            return CreateLoopAction(-1, interval, action);
        }

        /// <summary>
        /// 创建可循环操作体
        /// </summary>
        /// <param name="action">执行委托</param>
        /// <returns></returns>
        public static LoopAction CreateLoopAction(Action<LoopAction> action)
        {
            return CreateLoopAction(-1, TimeSpan.Zero, action);
        }

        /// <summary>
        /// 已执行次数
        /// </summary>
        public int ExecutedCount { get; private set; }

        /// <summary>
        /// 执行间隔
        /// </summary>
        public TimeSpan Interval { get; private set; }

        /// <summary>
        /// 循环次数
        /// </summary>
        public int LoopCount { get; }

        /// <summary>
        /// 执行委托
        /// </summary>
        public Action<LoopAction> ExecuteAction { get; private set; }

        /// <summary>
        /// 是否在运行
        /// </summary>
        public RunStatus RunStatus { get; private set; }

        /// <summary>
        /// 运行
        /// </summary>
        public void Run()
        {
            if (this.RunStatus == RunStatus.None)
            {
                this.RunStatus = RunStatus.Running;
                if (this.LoopCount >= 0)
                {
                    for (var i = 0; i < this.LoopCount; i++)
                    {
                        if (this.RunStatus == RunStatus.Disposed)
                        {
                            return;
                        }
                        this.ExecuteAction.Invoke(this);
                        this.ExecutedCount++;
                        if (this.RunStatus == RunStatus.Paused)
                        {
                            this.m_waitHandle.WaitOne();
                        }
                        this.m_waitHandle.WaitOne(this.Interval);
                    }
                }
                else
                {
                    while (true)
                    {
                        if (this.RunStatus == RunStatus.Disposed)
                        {
                            return;
                        }
                        this.ExecuteAction.Invoke(this);
                        this.ExecutedCount++;
                        if (this.RunStatus == RunStatus.Paused)
                        {
                            this.m_waitHandle.WaitOne();
                        }
                        this.m_waitHandle.WaitOne(this.Interval);
                    }
                }
                this.RunStatus = RunStatus.Completed;
            }
        }

        /// <summary>
        /// 重新运行
        /// </summary>
        public void Rerun()
        {
            if (this.RunStatus == RunStatus.Disposed)
            {
                throw new Exception("无法利用已释放的资源");
            }
            this.RunStatus = RunStatus.None;
            this.Run();
        }

        /// <summary>
        /// 以异步重新运行
        /// </summary>
        /// <returns></returns>
        public Task RerunAsync()
        {
            if (this.RunStatus == RunStatus.Disposed)
            {
                throw new Exception("无法利用已释放的资源");
            }
            this.RunStatus = RunStatus.None;
            return this.RunAsync();
        }

        /// <summary>
        /// 以异步运行
        /// </summary>
        /// <returns></returns>
        public Task RunAsync()
        {
            return Task.Run(() =>
            {
                this.Run();
            });
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            if (this.RunStatus == RunStatus.Running)
            {
                this.m_waitHandle.Reset();
                this.RunStatus = RunStatus.Paused;
            }
        }

        /// <summary>
        /// 回复
        /// </summary>
        public void Resume()
        {
            if (this.RunStatus == RunStatus.Paused)
            {
                this.RunStatus = RunStatus.Running;
                this.m_waitHandle.Set();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this.RunStatus == RunStatus.Disposed)
            {
                return;
            }
            if (this.RunStatus == RunStatus.Completed)
            {
                this.m_waitHandle.Dispose();
            }
            this.RunStatus = RunStatus.Disposed;
        }
    }
}