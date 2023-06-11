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

//https://github.com/dotnet-campus/AsyncWorkerCollection
namespace ThingsGateway.Foundation
{
    /// <summary>
    /// WaitOneAsync方法会返回一个task，通过await方式等待
    /// </summary>
    public class AsyncAutoResetEvent : IDisposable
    {
        private static readonly Task<bool> CompletedSourceTask = Task.FromResult(true);

        private readonly object _locker = new();

        private readonly Queue<TaskCompletionSource<bool>> _waitQueue = new Queue<TaskCompletionSource<bool>>();

        private bool _isDisposed;

        /// <summary>
        /// 用于在没有任何等待时让下一次等待通过
        /// </summary>
        private bool _isSignaled;

        /// <summary>
        /// 提供一个信号初始值，确定是否有信号
        /// </summary>
        /// <param name="initialState">true为有信号，第一个等待可以直接通过</param>
        public AsyncAutoResetEvent(bool initialState)
        {
            _isSignaled = initialState;
        }

        /// <summary>
        /// 析构方法
        /// </summary>
        ~AsyncAutoResetEvent()
        {
            this.SafeDispose();
        }
        /// <summary>
        /// 非线程安全 调用时将会释放所有等待 <see cref="WaitOneAsync"/> 方法
        /// </summary>
        public void Dispose()
        {
            lock (_locker)
            {
                _isDisposed = true;

            }

            GC.SuppressFinalize(this);

            while (true)
            {
                lock (_locker)
                {
                    if (_waitQueue.Count == 0)
                    {
                        return;
                    }
                }

                Set();
            }
        }

        /// <summary>
        /// 设置一个信号量，让一个waitone获得信号，每次调用 <see cref="Set"/> 方法最多只有一个等待通过
        /// </summary>
        public void Set()
        {
            TaskCompletionSource<bool> releaseSource = null;
            bool result;
            lock (_locker)
            {
                if (_waitQueue.Count > 0)
                {
                    releaseSource = _waitQueue.Dequeue();
                }

                if (releaseSource is null)
                {
                    if (!_isSignaled)
                    {
                        _isSignaled = true;
                    }
                }

                // 如果这个类被释放了，那么返回 false 值
                result = !_isDisposed;
            }

            releaseSource?.SetResult(result);
        }

        /// <summary>
        /// 异步等待一个信号，需要 await 等待
        /// <para></para>
        /// 可以通过返回值是 true 或 false 判断当前是收到信号还是此类被释放
        /// </summary>
        /// <returns>
        /// 如果是正常解锁，那么返回 true 值。如果是对象调用 <see cref="Dispose"/> 释放，那么返回 false 值
        /// </returns>
        public Task<bool> WaitOneAsync()
        {
            lock (_locker)
            {
                {
                    if (_isDisposed)
                    {
                        throw new ObjectDisposedException(nameof(AsyncAutoResetEvent));
                    }

                    if (_isSignaled)
                    {
                        // 按照 AutoResetEvent 的设计，在没有任何等待进入时，如果有设置 Set 方法，那么下一次第一个进入的等待将会通过
                        // 也就是在没有任何等待时，无论调用多少次 Set 方法，在调用之后只有一个等待通过
                        _isSignaled = false;
                        return CompletedSourceTask;
                    }

                    var source = new TaskCompletionSource<bool>();
                    _waitQueue.Enqueue(source);
                    return source.Task;
                }

            }


        }
    }
}
