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
using System.Collections;
using System.Diagnostics;

namespace ThingsGateway.Foundation.Dmtp
{
    [DebuggerDisplay("Id={Id},Status={Status}")]
    internal partial class InternalChannel : DisposableObject, IDmtpChannel
    {
        private readonly DmtpActor m_actor;
        private readonly AsyncAutoResetEvent m_asyncAutoResetEvent = new AsyncAutoResetEvent(false);
        private readonly IntelligentDataQueue<ChannelPackage> m_dataQueue;
        private readonly FlowGate m_flowGate;
        private readonly AutoResetEvent m_resetEvent = new AutoResetEvent(false);
        private int m_cacheCapacity;
        private volatile bool m_canFree;
        private ByteBlock m_currentData;
        private DateTime m_lastOperationTime;
        private long m_maxSpeed;

        public InternalChannel(DmtpActor client, string targetId, Metadata metadata)
        {
            this.m_actor = client;
            this.m_lastOperationTime = DateTime.Now;
            this.TargetId = targetId;
            this.Status = ChannelStatus.Default;
            this.m_cacheCapacity = 1024 * 1024 * 5;
            this.m_dataQueue = new IntelligentDataQueue<ChannelPackage>(this.m_cacheCapacity)
            {
                OverflowWait = false,
                OnQueueChanged = OnQueueChanged
            };
            this.m_canFree = true;
            this.m_maxSpeed = int.MaxValue;
            this.m_flowGate = new FlowGate() { Maximum = m_maxSpeed };
            this.Metadata = metadata;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~InternalChannel()
        {
            this.Dispose(false);
        }

        public long Available => this.m_dataQueue.Count;

        /// <summary>
        /// 缓存容量
        /// </summary>
        public int CacheCapacity
        {
            get => this.m_cacheCapacity;
            set
            {
                if (value < 0)
                {
                    value = 1024;
                }
                this.m_cacheCapacity = value;
                this.m_dataQueue.MaxSize = value;
            }
        }

        /// <summary>
        /// 判断当前通道能否调用<see cref="MoveNext()"/>
        /// </summary>
        public bool CanMoveNext
        {
            get
            {
                return this.Available > 0 || (byte)this.Status < 4;
            }
        }

        /// <summary>
        /// 能否写入
        /// </summary>
        public bool CanWrite => (byte)this.Status <= 3;

        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 最后一次操作时显示消息
        /// </summary>
        public string LastOperationMes { get; private set; }

        public long MaxSpeed
        {
            get => this.m_maxSpeed;
            set
            {
                if (value < 1024)
                {
                    value = 1024;
                }
                this.m_maxSpeed = value;
                this.m_flowGate.Maximum = value;
            }
        }

        public Metadata Metadata { get; private set; }

        /// <summary>
        /// 状态
        /// </summary>
        public ChannelStatus Status { get; private set; }

        /// <summary>
        /// 目的Id地址。
        /// </summary>
        public string TargetId { get; }

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 是否被使用
        /// </summary>
        public bool Using { get; private set; }

        #region 操作

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel(string operationMes = null)
        {
            if ((byte)this.Status > 3)
            {
                return;
            }
            try
            {
                this.RequestCancel(true);
                var channelPackage = new ChannelPackage()
                {
                    ChannelId = this.Id,
                    RunNow = true,
                    DataType = ChannelDataType.CancelOrder,
                    Message = operationMes,
                    SourceId = this.m_actor.Id,
                    TargetId = this.TargetId,
                    Route = this.TargetId.HasValue()
                };
                this.m_actor.SendChannelPackage(channelPackage);
                this.m_lastOperationTime = DateTime.Now;
            }
            catch
            {
            }
        }

        public Task CancelAsync(string operationMes = null)
        {
            return Task.Run(() =>
            {
                this.Cancel(operationMes);
            });
        }

        /// <summary>
        /// 完成操作
        /// </summary>
        public void Complete(string operationMes = null)
        {
            if ((byte)this.Status > 3)
            {
                return;
            }

            this.RequestComplete(true);
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                RunNow = true,
                DataType = ChannelDataType.CompleteOrder,
                Message = operationMes,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId,
                Route = this.TargetId.HasValue()
            };
            this.m_actor.SendChannelPackage(channelPackage);
            this.m_lastOperationTime = DateTime.Now;
        }

        public Task CompleteAsync(string operationMes = null)
        {
            return Task.Run(() =>
            {
                this.Complete(operationMes);
            });
        }

        /// <summary>
        /// 继续。
        /// <para>调用该指令时，接收方会跳出接收，但是通道依然可用，所以接收方需要重新调用<see cref="MoveNext()"/></para>
        /// </summary>
        /// <param name="operationMes"></param>
        public void HoldOn(string operationMes = null)
        {
            if ((byte)this.Status > 3)
            {
                return;
            }
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                RunNow = true,
                DataType = ChannelDataType.HoldOnOrder,
                Message = operationMes,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId,
                Route = this.TargetId.HasValue()
            };
            this.m_actor.SendChannelPackage(channelPackage);
            this.m_lastOperationTime = DateTime.Now;
        }

        public Task HoldOnAsync(string operationMes = null)
        {
            return Task.Run(() =>
            {
                this.HoldOn(operationMes);
            });
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                this.RequestDispose(true);

                if ((byte)this.Status > 3)
                {
                    return;
                }

                var channelPackage = new ChannelPackage()
                {
                    ChannelId = this.Id,
                    RunNow = true,
                    DataType = ChannelDataType.HoldOnOrder,
                    SourceId = this.m_actor.Id,
                    TargetId = this.TargetId,
                    Route = this.TargetId.HasValue()
                };
                this.m_actor.SendChannelPackage(channelPackage);
                this.m_lastOperationTime = DateTime.Now;
            }
            catch
            {
            }
            base.Dispose(disposing);
        }

        #endregion 操作

        public ByteBlock GetCurrent()
        {
            return this.m_currentData;
        }

        /// <summary>
        /// 转向下个元素
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (!this.CanMoveNext)
            {
                return false;
            }
            if (this.m_dataQueue.TryDequeue(out var channelPackage))
            {
                switch (channelPackage.DataType)
                {
                    case ChannelDataType.DataOrder:
                        {
                            this.m_currentData = channelPackage.Data;
                            this.m_flowGate.AddCheckWait(channelPackage.Data.Len);
                            return true;
                        }
                    case ChannelDataType.CompleteOrder:
                        this.RequestComplete(true);
                        return false;

                    case ChannelDataType.CancelOrder:
                        this.RequestCancel(true);
                        return false;

                    case ChannelDataType.DisposeOrder:
                        this.RequestDispose(true);
                        return false;

                    case ChannelDataType.HoldOnOrder:
                        this.Status = ChannelStatus.HoldOn;
                        return false;

                    case ChannelDataType.QueueRun:
                        this.m_canFree = true;
                        return false;

                    case ChannelDataType.QueuePause:
                        this.m_canFree = false;
                        return false;

                    default:
                        return false;
                }
            }

            this.Reset();
            if (this.Wait())
            {
                return this.MoveNext();
            }
            else
            {
                this.Status = ChannelStatus.Overtime;
                return false;
            }
        }

        public async Task<bool> MoveNextAsync()
        {
            if (!this.CanMoveNext)
            {
                return false;
            }
            if (this.m_dataQueue.TryDequeue(out var channelPackage))
            {
                switch (channelPackage.DataType)
                {
                    case ChannelDataType.DataOrder:
                        {
                            this.m_currentData = channelPackage.Data;
                            await this.m_flowGate.AddCheckWaitAsync(channelPackage.Data.Len);
                            return true;
                        }
                    case ChannelDataType.CompleteOrder:
                        this.RequestComplete(true);
                        return false;

                    case ChannelDataType.CancelOrder:
                        this.RequestCancel(true);
                        return false;

                    case ChannelDataType.DisposeOrder:
                        this.RequestDispose(true);
                        return false;

                    case ChannelDataType.HoldOnOrder:
                        this.Status = ChannelStatus.HoldOn;
                        return false;

                    case ChannelDataType.QueueRun:
                        this.m_canFree = true;
                        return false;

                    case ChannelDataType.QueuePause:
                        this.m_canFree = false;
                        return false;

                    default:
                        return false;
                }
            }

            this.Reset();
            if (await this.WaitAsync())
            {
                return await this.MoveNextAsync();
            }
            else
            {
                this.Status = ChannelStatus.Overtime;
                return false;
            }
        }

        /// <summary>
        /// 写入通道
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Write(byte[] data, int offset, int length)
        {
            if ((byte)this.Status > 3)
            {
                throw new Exception($"通道已{this.Status}");
            }

            if (!SpinWait.SpinUntil(() => { return this.m_canFree; }, this.Timeout))
            {
                throw new TimeoutException();
            }
            this.m_flowGate.AddCheckWait(length);
            var channelPackage = new ChannelPackage()
            {
                ChannelId = this.Id,
                DataType = ChannelDataType.DataOrder,
                SourceId = this.m_actor.Id,
                TargetId = this.TargetId,
                Route = this.TargetId.HasValue()
            };

            if (data.Length == length)
            {
                channelPackage.Data = new ByteBlock(data);
            }
            else
            {
                var byteBlock = new ByteBlock(length);
                byteBlock.Write(data, offset, length);
                channelPackage.Data = byteBlock;
            }
            using (channelPackage.Data)
            {
                this.m_actor.SendChannelPackage(channelPackage);
                this.m_lastOperationTime = DateTime.Now;
            }
        }

        public Task WriteAsync(byte[] data, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.Write(data, offset, length);
            });
        }

        internal void ReceivedData(ChannelPackage channelPackage)
        {
            this.m_lastOperationTime = DateTime.Now;
            if (channelPackage.RunNow)
            {
                switch (channelPackage.DataType)
                {
                    case ChannelDataType.CompleteOrder:
                        this.LastOperationMes = channelPackage.Message;
                        this.RequestComplete(false);
                        break;

                    case ChannelDataType.CancelOrder:
                        this.LastOperationMes = channelPackage.Message;
                        this.RequestCancel(false);
                        break;

                    case ChannelDataType.DisposeOrder:
                        this.RequestDispose(false);
                        break;

                    case ChannelDataType.HoldOnOrder:
                        this.LastOperationMes = channelPackage.Message;
                        this.Status = ChannelStatus.HoldOn;
                        break;

                    case ChannelDataType.QueueRun:
                        this.m_canFree = true;
                        return;

                    case ChannelDataType.QueuePause:
                        this.m_canFree = false;
                        return;

                    default:
                        return;
                }
            }
            this.m_dataQueue.Enqueue(channelPackage);
            this.Set();
        }

        internal void RequestDispose(bool clear)
        {
            if (clear)
            {
                this.Clear();
            }
            if ((byte)this.Status > 3)
            {
                return;
            }
            this.Status = ChannelStatus.Disposed;
        }

        internal void SetId(int id)
        {
            this.Id = id;
        }

        internal void SetUsing()
        {
            this.Using = true;
        }

        private void Clear()
        {
            try
            {
                lock (this)
                {
                    this.m_dataQueue.Clear(package =>
                    {
                        package.Data.SafeDispose();
                    });
                    if (this.m_actor.RemoveChannel(this.Id))
                    {
                        this.Set();
                        this.m_resetEvent.SafeDispose();
                        this.m_asyncAutoResetEvent.SafeDispose();
                    }
                }
            }
            catch
            {
            }
        }

        private void OnQueueChanged(bool free)
        {
            if ((byte)this.Status > 3)
            {
                return;
            }

            try
            {
                var channelPackage = new ChannelPackage()
                {
                    ChannelId = this.Id,
                    RunNow = true,
                    DataType = free ? ChannelDataType.QueueRun : ChannelDataType.QueuePause,
                    SourceId = this.m_actor.Id,
                    TargetId = this.TargetId,
                    Route = this.TargetId.HasValue()
                };
                this.m_actor.SendChannelPackage(channelPackage);
                this.m_lastOperationTime = DateTime.Now;
            }
            catch
            {
            }
        }

        private void RequestCancel(bool clear)
        {
            this.Status = ChannelStatus.Cancel;
            if (clear)
            {
                this.Clear();
            }
        }

        private void RequestComplete(bool clear)
        {
            this.Status = ChannelStatus.Completed;
            if (clear)
            {
                this.Clear();
            }
        }

        private void Reset()
        {
            this.m_resetEvent.Reset();
            this.m_asyncAutoResetEvent.Reset();
        }

        private void Set()
        {
            this.m_resetEvent.Set();
            this.m_asyncAutoResetEvent.Set();
        }

        private bool Wait()
        {
            return this.m_resetEvent.WaitOne(this.Timeout);
        }

        private Task<bool> WaitAsync()
        {
            return this.m_asyncAutoResetEvent.WaitOneAsync(this.Timeout);
        }

        #region 迭代器

        public IEnumerator<ByteBlock> GetEnumerator()
        {
            ByteBlock byteBlock = null;
            while (this.MoveNext())
            {
                byteBlock.SafeDispose();
                byteBlock = this.GetCurrent();
                yield return byteBlock;
            }
            byteBlock.SafeDispose();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion 迭代器
    }
}