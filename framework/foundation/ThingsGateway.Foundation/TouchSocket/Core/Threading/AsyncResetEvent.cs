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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 异步AsyncResetEvent
    /// 能够创建一个手动Reset或者自动Reset.
    /// </summary>
    public class AsyncResetEvent : DisposableObject
    {
        private readonly bool m_autoReset;

        private readonly object m_locker = new object();

        private readonly Queue<TaskCompletionSource<bool>> m_waitQueue = new Queue<TaskCompletionSource<bool>>();

        private volatile bool m_eventSet;

        private static readonly Task m_completeTask = Task.FromResult(true);

        /// <summary>
        /// 创建一个异步AsyncResetEvent
        /// </summary>
        /// <param name="initialState">是否包含初始信号</param>
        /// <param name="autoReset">是否自动重置</param>
        public AsyncResetEvent(bool initialState, bool autoReset)
        {
            this.m_eventSet = initialState;
            this.m_autoReset = autoReset;
        }

        /// <summary>
        /// 异步等待设置此事件
        /// </summary>
        public Task WaitOneAsync()
        {
            return this.WaitOneAsync(CancellationToken.None);
        }

        /// <summary>
        ///异步等待指定时间
        /// </summary>
        /// <param name="timeout">超时时间</param>
        public async Task<bool> WaitOneAsync(TimeSpan timeout)
        {
            try
            {
                using (var timeoutSource = new CancellationTokenSource(timeout))
                {
                    await this.WaitOneAsync(timeoutSource.Token);
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// 异步等待可取消
        /// </summary>
        /// <param name="cancellationToken">可取消令箭</param>
        public Task WaitOneAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
#if NET45
                var tcs = new TaskCompletionSource<bool>();
                tcs.TrySetCanceled();
                return tcs.Task;
#else
                return Task.FromCanceled(cancellationToken);
#endif
            }

            lock (this.m_locker)
            {
                if (this.m_eventSet)
                {
                    if (this.m_autoReset)
                    {
                        this.m_eventSet = false;
                    }

                    return m_completeTask;
                }
                else
                {
#if NET45
                    var completionSource = new TaskCompletionSource<bool>();
#else
                    var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
#endif

                    var registration = cancellationToken.Register(() =>
                    {
                        lock (this.m_locker)
                        {
#if NET45
                            completionSource.TrySetCanceled();
#else
                            completionSource.TrySetCanceled(cancellationToken);
#endif
                        }
                    }, useSynchronizationContext: false);

                    this.m_waitQueue.Enqueue(completionSource);

                    completionSource.Task.ContinueWith(
                            (_) => registration.Dispose(),
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default
                            );

                    return completionSource.Task;
                }
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        public bool Reset()
        {
            lock (this.m_locker)
            {
                this.m_eventSet = false;
                return true;
            }
        }

        /// <summary>
        /// 设置信号
        /// </summary>
        public bool Set()
        {
            lock (this.m_locker)
            {
                while (this.m_waitQueue.Count > 0)
                {
                    var toRelease = this.m_waitQueue.Dequeue();

                    if (toRelease.Task.IsCompleted)
                    {
                        continue;
                    }

                    var b = toRelease.TrySetResult(true);

                    if (this.m_autoReset)
                    {
                        return b;
                    }
                }

                this.m_eventSet = true;
                return false;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            while (true)
            {
                lock (this.m_locker)
                {
                    if (this.m_waitQueue.Count == 0)
                    {
                        break;
                    }
                }

                this.Set();
            }
            base.Dispose(disposing);
        }
    }
}