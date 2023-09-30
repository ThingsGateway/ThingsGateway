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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 等待数据对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WaitData<T> : DisposableObject, IWaitData<T>
    {
        private readonly AutoResetEvent m_waitHandle;
        private volatile WaitDataStatus m_status;
        private CancellationTokenRegistration m_tokenRegistration;

        /// <summary>
        /// WaitData
        /// </summary>
        public WaitData()
        {
            this.m_waitHandle = new AutoResetEvent(false);
        }

        /// <inheritdoc/>
        public WaitDataStatus Status { get => this.m_status; }

        /// <inheritdoc/>
        public T WaitResult { get; private set; }

        /// <inheritdoc/>
        public void Cancel()
        {
            this.m_status = WaitDataStatus.Canceled;
            this.m_waitHandle.Set();
        }

        /// <inheritdoc/>
        public void Reset()
        {
            this.m_status = WaitDataStatus.Default;
            this.WaitResult = default;
            this.m_waitHandle.Reset();
        }

        /// <inheritdoc/>
        public bool Set()
        {
            this.m_status = WaitDataStatus.SetRunning;
            return this.m_waitHandle.Set();
        }

        /// <inheritdoc/>
        public bool Set(T waitResult)
        {
            this.WaitResult = waitResult;
            this.m_status = WaitDataStatus.SetRunning;
            return this.m_waitHandle.Set();
        }
        /// <inheritdoc/>
        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                if (this.m_tokenRegistration == default)
                {
                    this.m_tokenRegistration = cancellationToken.Register(this.Cancel);
                }
                else
                {
                    this.m_tokenRegistration.Dispose();
                    this.m_tokenRegistration = cancellationToken.Register(this.Cancel);
                }
            }
        }

        /// <inheritdoc/>
        public void SetResult(T result)
        {
            this.WaitResult = result;
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
            if (!this.m_waitHandle.WaitOne(millisecond))
            {
                this.m_status = WaitDataStatus.Overtime;
            }
            return this.m_status;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_status = WaitDataStatus.Disposed;
            this.WaitResult = default;
            this.m_waitHandle.SafeDispose();
            this.m_tokenRegistration.Dispose();
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 等待数据对象
    /// </summary>
    public class WaitDataEx : WaitData<object>
    {
    }
}