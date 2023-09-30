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

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 队列数据
    /// </summary>
    public interface IQueueData
    {
        /// <summary>
        /// 数据长度
        /// </summary>
        int Size { get; }
    }

    /// <summary>
    /// 传输字节
    /// </summary>
    public class QueueDataBytes : IQueueData
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public QueueDataBytes(byte[] buffer, int offset, int length)
        {
            this.Offset = offset;
            this.Length = length;
            this.Buffer = buffer;
            this.Size = length;
        }

        /// <summary>
        /// 从指定内存创建一个新对象，且内存也为新创建。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static QueueDataBytes CreateNew(byte[] buffer, int offset, int length)
        {
            var buf = new byte[length];
            Array.Copy(buffer, offset, buf, 0, length);

            return new QueueDataBytes(buf);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer"></param>
        public QueueDataBytes(byte[] buffer) : this(buffer, 0, buffer.Length)
        {
        }

        /// <summary>
        /// 数据内存
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// 偏移
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// 尺寸
        /// </summary>
        public int Size { get; }
    }

    /// <summary>
    /// 智能数据安全队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntelligentDataQueue<T> : ConcurrentQueue<T> where T : IQueueData
    {
        private long m_actualSize;
        private long m_maxSize;

        /// <summary>
        /// 智能数据安全队列
        /// </summary>
        /// <param name="maxSize"></param>
        public IntelligentDataQueue(long maxSize)
        {
            this.Free = true;
            this.OverflowWait = true;
            this.MaxSize = maxSize;
        }

        /// <summary>
        /// 智能数据安全队列
        /// </summary>
        public IntelligentDataQueue() : this(1024 * 1024 * 10)
        {
        }

        /// <summary>
        /// 实际尺寸
        /// </summary>
        public long ActualSize => this.m_actualSize;

        /// <summary>
        /// 是否有空位允许入队
        /// </summary>
        public bool Free { get; private set; }

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public long MaxSize
        {
            get => this.m_maxSize;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                this.m_maxSize = value;
            }
        }

        /// <summary>
        /// 在队列修改时
        /// </summary>
        public Action<bool> OnQueueChanged { get; set; }

        /// <summary>
        /// 溢出等待
        /// </summary>
        public bool OverflowWait { get; set; }

        /// <summary>
        /// 超时时间。默认1000*30ms；
        /// </summary>
        public int Timeout { get; set; } = 1000 * 30;

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear(Action<T> onClear)
        {
            while (base.TryDequeue(out var t))
            {
                onClear?.Invoke(t);
            }
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(in T item)
        {
            lock (this)
            {
                var free = this.m_actualSize < this.m_maxSize;
                if (this.Free != free)
                {
                    this.Free = free;
                    this.OnQueueChanged?.Invoke(this.Free);
                }

                if (this.OverflowWait)
                {
                    SpinWait.SpinUntil(this.Check, this.Timeout);
                }

                Interlocked.Add(ref this.m_actualSize, item.Size);
                base.Enqueue(item);
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public new bool TryDequeue(out T result)
        {
            if (base.TryDequeue(out result))
            {
                Interlocked.Add(ref this.m_actualSize, -result.Size);
                var free = this.m_actualSize < this.m_maxSize;
                if (this.Free != free)
                {
                    this.Free = free;
                    this.OnQueueChanged?.Invoke(this.Free);
                }
                return true;
            }
            return false;
        }

        private bool Check()
        {
            return this.m_actualSize < this.m_maxSize;
        }
    }
}