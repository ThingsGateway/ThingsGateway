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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 等待处理数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TgWaitHandlePool<T> : DisposableObject where T : IWaitHandle
    {
        private readonly ConcurrentDictionary<long, WaitData<T>> m_waitDic;
        private readonly ConcurrentDictionary<long, WaitDataAsync<T>> m_waitDicAsync;
        private readonly ConcurrentQueue<WaitData<T>> m_waitQueue;
        private readonly ConcurrentQueue<WaitDataAsync<T>> m_waitQueueAsync;
        private long m_maxSign = long.MaxValue;
        private long m_minSign = long.MinValue;
        private long m_waitCount;
        private long m_waitReverseCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TgWaitHandlePool()
        {
            this.m_waitDic = new ConcurrentDictionary<long, WaitData<T>>();
            this.m_waitDicAsync = new ConcurrentDictionary<long, WaitDataAsync<T>>();
            this.m_waitQueue = new ConcurrentQueue<WaitData<T>>();
            this.m_waitQueueAsync = new ConcurrentQueue<WaitDataAsync<T>>();
        }

        /// <summary>
        /// 最大Sign
        /// </summary>
        public long MaxSign { get => m_maxSign; set => m_maxSign = value; }

        /// <summary>
        /// 最小Sign
        /// </summary>
        public long MinSign { get => m_minSign; set => m_minSign = value; }

        /// <summary>
        /// 取消全部
        /// </summary>
        public void CancelAll()
        {
            foreach (var item in this.m_waitDic.Values)
            {
                item.Cancel();
            }
            foreach (var item in this.m_waitDicAsync.Values)
            {
                item.Cancel();
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="waitData"></param>
        public void Destroy(WaitData<T> waitData)
        {
            if (waitData.WaitResult == null)
            {
                return;
            }
            if (this.m_waitDic.TryRemove(waitData.WaitResult.Sign, out var wait))
            {
                if (wait.DisposedValue)
                {
                    return;
                }

                //wait.Reset();
                this.m_waitQueue.Enqueue(wait);
            }
        }

        /// <summary>
        /// 销毁
        /// </summary>
        /// <param name="waitData"></param>
        public void Destroy(WaitDataAsync<T> waitData)
        {
            if (waitData.WaitResult == null)
            {
                return;
            }
            if (this.m_waitDicAsync.TryRemove(waitData.WaitResult.Sign, out var wait))
            {
                if (wait.DisposedValue)
                {
                    return;
                }

                //wait.Reset();
                this.m_waitQueueAsync.Enqueue(wait);
            }
        }

        /// <summary>
        ///  获取一个Sign为负数的可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitData<T> GetReverseWaitData(T result, bool autoSign = true)
        {
            if (this.m_waitQueue.TryDequeue(out var waitData))
            {
                if (autoSign)
                {
                    result.Sign = this.GetSign(true);
                }
                waitData.SetResult(result);
                this.m_waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            if (autoSign)
            {
                result.Sign = this.GetSign(true);
            }
            waitData.SetResult(result);
            this.m_waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 获取一个Sign为负数的可等待对象
        /// </summary>
        /// <param name="sign"></param>
        /// <returns></returns>
        public WaitData<T> GetReverseWaitData(out long sign)
        {
            if (this.m_waitQueue.TryDequeue(out var waitData))
            {
                sign = this.GetSign(true);
                waitData.SetResult(default);
                this.m_waitDic.TryAdd(sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            sign = this.GetSign(true);
            waitData.SetResult(default);
            this.m_waitDic.TryAdd(sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个Sign为负数的可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitDataAsync<T> GetReverseWaitDataAsync(T result, bool autoSign = true)
        {
            if (this.m_waitQueueAsync.TryDequeue(out var waitData))
            {
                if (autoSign)
                {
                    result.Sign = this.GetSign(true);
                }
                waitData.SetResult(result);
                this.m_waitDicAsync.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitDataAsync<T>();
            if (autoSign)
            {
                result.Sign = this.GetSign(true);
            }
            waitData.SetResult(result);
            this.m_waitDicAsync.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个Sign为负数的可等待对象
        /// </summary>
        /// <returns></returns>
        public WaitDataAsync<T> GetReverseWaitDataAsync(out long sign)
        {
            if (this.m_waitQueueAsync.TryDequeue(out var waitData))
            {
                sign = GetSign(true);
                waitData.SetResult(default);
                this.m_waitDicAsync.TryAdd(sign, waitData);
                return waitData;
            }

            waitData = new WaitDataAsync<T>();
            sign = GetSign(true);
            waitData.SetResult(default);
            this.m_waitDicAsync.TryAdd(sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitData<T> GetWaitData(T result, bool autoSign = true)
        {
            if (this.m_waitQueue.TryDequeue(out var waitData))
            {
                if (autoSign)
                {
                    result.Sign = GetSign(false);
                }
                waitData.SetResult(result);
                this.m_waitDic.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            if (autoSign)
            {
                result.Sign = GetSign(false);
            }
            waitData.SetResult(result);
            this.m_waitDic.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 获取一个可等待对象。并out返回标识。
        /// </summary>
        /// <param name="sign"></param>
        /// <returns></returns>
        public WaitData<T> GetWaitData(out long sign)
        {
            if (this.m_waitQueue.TryDequeue(out var waitData))
            {
                sign = GetSign(false);
                waitData.SetResult(default);
                this.m_waitDic.TryAdd(sign, waitData);
                return waitData;
            }

            waitData = new WaitData<T>();
            sign = GetSign(false);
            waitData.SetResult(default);
            this.m_waitDic.TryAdd(sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个可等待对象
        /// </summary>
        /// <param name="result"></param>
        /// <param name="autoSign">设置为false时，不会生成sign</param>
        /// <returns></returns>
        public WaitDataAsync<T> GetWaitDataAsync(T result, bool autoSign = true)
        {
            if (this.m_waitQueueAsync.TryDequeue(out var waitData))
            {
                if (autoSign)
                {
                    result.Sign = GetSign(false);
                }
                waitData.SetResult(result);
                this.m_waitDicAsync.TryAdd(result.Sign, waitData);
                return waitData;
            }

            waitData = new WaitDataAsync<T>();
            if (autoSign)
            {
                result.Sign = GetSign(false);
            }
            waitData.SetResult(result);
            this.m_waitDicAsync.TryAdd(result.Sign, waitData);
            return waitData;
        }

        /// <summary>
        ///  获取一个可等待对象
        /// </summary>
        /// <param name="sign"></param>
        /// <returns></returns>
        public WaitDataAsync<T> GetWaitDataAsync(out long sign)
        {
            if (this.m_waitQueueAsync.TryDequeue(out var waitData))
            {
                sign = GetSign(false);
                waitData.SetResult(default);
                this.m_waitDicAsync.TryAdd(sign, waitData);
                return waitData;
            }

            waitData = new WaitDataAsync<T>();
            sign = GetSign(false);
            waitData.SetResult(default);
            this.m_waitDicAsync.TryAdd(sign, waitData);
            return waitData;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        public bool SetRun(long sign)
        {
            var result = false;
            if (this.m_waitDic.TryGetValue(sign, out var waitData))
            {
                waitData.Set();
                result = true;
            }

            if (this.m_waitDicAsync.TryGetValue(sign, out var waitDataAsync))
            {
                waitDataAsync.Set();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="sign"></param>
        /// <param name="waitResult"></param>
        public bool SetRun(long sign, T waitResult)
        {
            var result = false;
            if (this.m_waitDic.TryGetValue(sign, out var waitData))
            {
                waitData.Set(waitResult);
                result = true;
            }

            if (this.m_waitDicAsync.TryGetValue(sign, out var waitDataAsync))
            {
                waitDataAsync.Set(waitResult);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 让等待对象恢复运行
        /// </summary>
        /// <param name="waitResult"></param>
        public bool SetRun(T waitResult)
        {
            var result = false;
            if (this.m_waitDic.TryGetValue(waitResult.Sign, out var waitData))
            {
                waitData.Set(waitResult);
                result = true;
            }

            if (this.m_waitDicAsync.TryGetValue(waitResult.Sign, out var waitDataAsync))
            {
                waitDataAsync.Set(waitResult);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            foreach (var item in this.m_waitDic.Values)
            {
                item.SafeDispose();
            }
            foreach (var item in this.m_waitQueue)
            {
                item.SafeDispose();
            }
            this.m_waitDic.Clear();

            this.m_waitQueue.Clear();
            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long GetSign(bool reverse)
        {
            if (reverse)
            {
                Interlocked.CompareExchange(ref this.m_waitReverseCount, 0, this.m_minSign);
                return Interlocked.Decrement(ref this.m_waitReverseCount);
            }
            else
            {
                Interlocked.CompareExchange(ref this.m_waitCount, 0, this.m_maxSign);
                return Interlocked.Increment(ref this.m_waitCount);
            }
        }
    }
}