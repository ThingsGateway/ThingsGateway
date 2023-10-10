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
using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 触发器队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TriggerQueue<T> : DisposableObject
    {
        private readonly ReaderWriterLockSlim m_lockSlim;
        private readonly ConcurrentQueue<T> m_queue;
        private readonly Timer m_timer;
        private volatile bool m_sending;

        /// <summary>
        ///  触发器队列
        /// </summary>
        public TriggerQueue()
        {
            this.m_lockSlim = new ReaderWriterLockSlim();
            this.m_queue = new ConcurrentQueue<T>();
            this.m_timer = new Timer(this.TimerRun, null, 10, 10);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~TriggerQueue()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// 出队列处理。
        /// </summary>
        public Action<T> OnDequeue { get; set; }

        /// <summary>
        /// 发生错误
        /// </summary>
        public Action<Exception> OnError { get; set; }

        /// <summary>
        /// 是否处于发送状态
        /// </summary>
        public bool Sending
        {
            get
            {
                using (new ReadLock(this.m_lockSlim))
                {
                    return this.m_sending;
                }
            }

            private set
            {
                using (new WriteLock(this.m_lockSlim))
                {
                    this.m_sending = value;
                }
            }
        }

        /// <summary>
        /// 发送
        /// </summary>
        public void Enqueue(T data)
        {
            this.m_queue.Enqueue(data);
            if (this.SwitchToRun())
            {
                Task.Factory.StartNew(this.BeginTrigger);
            }
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_timer.SafeDispose();
            this.m_queue.Clear();
            base.Dispose(disposing);
        }

        private void BeginTrigger()
        {
            while (true)
            {
                try
                {
                    if (this.m_queue.TryDequeue(out var data))
                    {
                        this.OnDequeue?.Invoke(data);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    this.OnError?.Invoke(ex);
                }
            }
            this.Sending = false;
        }

        private bool SwitchToRun()
        {
            using (new ReadLock(this.m_lockSlim))
            {
                if (this.m_sending)
                {
                    return false;
                }
                else
                {
                    this.m_sending = true;
                    return true;
                }
            }
        }

        private void TimerRun(object state)
        {
            if (this.SwitchToRun())
            {
                this.BeginTrigger();
            }
        }
    }
}