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
using System.Net;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// UDP数据帧
    /// </summary>
    public struct UdpFrame
    {
        /// <summary>
        /// Crc校验
        /// </summary>
        public byte[] Crc { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 是否为终结帧
        /// </summary>
        public bool FIN { get; set; }

        /// <summary>
        /// 数据Id
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 帧序号
        /// </summary>
        public ushort SN { get; set; }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool Parse(byte[] buffer, int offset, int length)
        {
            if (length > 11)
            {
                this.Id = TouchSocketBitConverter.Default.ToInt64(buffer, offset);
                this.SN = TouchSocketBitConverter.Default.ToUInt16(buffer, 8 + offset);
                this.FIN = buffer[10 + offset].GetBit(7) == 1;
                if (this.FIN)
                {
                    this.Data = length > 13 ? (new byte[length - 13]) : (new byte[0]);
                    this.Crc = new byte[2] { buffer[length - 2], buffer[length - 1] };
                }
                else
                {
                    this.Data = new byte[length - 11];
                }

                Array.Copy(buffer, 11, this.Data, 0, this.Data.Length);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// UDP数据包
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Count={Count}")]
    public class UdpPackage
    {
        private readonly ConcurrentQueue<UdpFrame> m_frames;
        private readonly Timer m_timer;
        private int m_count;
        private int m_length;

        private int m_mtu;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id"></param>
        /// <param name="timeout"></param>
        /// <param name="revStore"></param>
        public UdpPackage(long id, int timeout, ConcurrentDictionary<long, UdpPackage> revStore)
        {
            this.Id = id;
            this.m_frames = new ConcurrentQueue<UdpFrame>();
            this.m_timer = new Timer((o) =>
            {
                if (revStore.TryRemove(this.Id, out var udpPackage))
                {
                    udpPackage.m_frames.Clear();
                }
            }, null, timeout, Timeout.Infinite);
        }

        /// <summary>
        /// 当前长度
        /// </summary>
        public int Count => this.m_count;

        /// <summary>
        /// Crc
        /// </summary>
        public byte[] Crc { get; private set; }

        /// <summary>
        /// 包唯一标识
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsComplated => this.TotalCount > 0 ? (this.TotalCount == this.m_count ? true : false) : false;

        /// <summary>
        /// 当前数据长度
        /// </summary>
        public int Length => this.m_length;

        /// <summary>
        /// MTU
        /// </summary>
        public int MTU => this.m_mtu + 11;

        /// <summary>
        /// 总长度，在收到最后一帧之前，为-1。
        /// </summary>
        public int TotalCount { get; private set; } = -1;

        /// <summary>
        /// 添加帧
        /// </summary>
        /// <param name="frame"></param>
        public void Add(UdpFrame frame)
        {
            Interlocked.Increment(ref this.m_count);

            if (frame.FIN)
            {
                this.TotalCount = frame.SN + 1;
                this.Crc = frame.Crc;
            }
            Interlocked.Add(ref this.m_length, frame.Data.Length);
            if (frame.SN == 0)
            {
                this.m_mtu = frame.Data.Length;
            }
            this.m_frames.Enqueue(frame);
        }

        /// <summary>
        /// 获得数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public bool TryGetData(ByteBlock byteBlock)
        {
            while (this.m_frames.TryDequeue(out var frame))
            {
                byteBlock.Pos = frame.SN * this.m_mtu;
                byteBlock.Write(frame.Data);
            }

            if (byteBlock.Len != this.Length)
            {
                return false;
            }
            var crc = ThingsGateway.Foundation.Core.Crc.Crc16(byteBlock.Buffer, 0, byteBlock.Len);
            return crc[0] == this.Crc[0] && crc[1] == this.Crc[1];
        }
    }

    /// <summary>
    /// UDP数据包的适配器
    /// </summary>
    public class UdpPackageAdapter : UdpDataHandlingAdapter
    {
        private readonly SnowflakeIdGenerator m_iDGenerator;
        private readonly ConcurrentDictionary<long, UdpPackage> m_revStore;
        private int m_mtu = 1472;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpPackageAdapter()
        {
            this.m_revStore = new ConcurrentDictionary<long, UdpPackage>();
            this.m_iDGenerator = new SnowflakeIdGenerator(4);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => true;

        /// <summary>
        /// 最大传输单元
        /// </summary>
        public int MTU
        {
            get => this.m_mtu + 11;
            set => this.m_mtu = value > 11 ? value : 1472;
        }

        /// <summary>
        /// 接收超时时间，默认5000ms
        /// </summary>
        public int Timeout { get; set; } = 5000;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            var udpFrame = new UdpFrame();
            if (udpFrame.Parse(byteBlock.Buffer, 0, byteBlock.Len))
            {
                var udpPackage = this.m_revStore.GetOrAdd(udpFrame.Id, (i) => new UdpPackage(i, this.Timeout, this.m_revStore));
                udpPackage.Add(udpFrame);
                if (udpPackage.Length > this.MaxPackageSize)
                {
                    this.m_revStore.TryRemove(udpPackage.Id, out _);
                    this.Logger?.Error("数据长度大于设定的最大值。");
                    return;
                }
                if (udpPackage.IsComplated)
                {
                    if (this.m_revStore.TryRemove(udpPackage.Id, out _))
                    {
                        using (var block = new ByteBlock(udpPackage.Length))
                        {
                            if (udpPackage.TryGetData(block))
                            {
                                this.GoReceived(remoteEndPoint, block, null);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected override void PreviewSend(EndPoint endPoint, byte[] buffer, int offset, int length)
        {
            if (length > this.MaxPackageSize)
            {
                throw new OverlengthException("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }
            var id = this.m_iDGenerator.NextId();
            var off = 0;
            var surLen = length;
            var freeRoom = this.m_mtu - 11;
            ushort sn = 0;
            /*|********|**|*|n|*/
            /*|********|**|*|**|*/
            while (surLen > 0)
            {
                var data = new byte[this.m_mtu];
                Buffer.BlockCopy(TouchSocketBitConverter.Default.GetBytes(id), 0, data, 0, 8);
                Buffer.BlockCopy(TouchSocketBitConverter.Default.GetBytes(sn++), 0, data, 8, 2);
                if (surLen > freeRoom)//有余
                {
                    Buffer.BlockCopy(buffer, off, data, 11, freeRoom);
                    off += freeRoom;
                    surLen -= freeRoom;
                    this.GoSend(endPoint, data, 0, this.m_mtu);
                }
                else if (surLen + 2 <= freeRoom)//结束且能容纳Crc
                {
                    byte flag = 0;
                    data[10] = flag.SetBit(7, 1);//设置终结帧

                    Buffer.BlockCopy(buffer, off, data, 11, surLen);
                    Buffer.BlockCopy(Crc.Crc16(buffer, offset, length), 0, data, 11 + surLen, 2);

                    this.GoSend(endPoint, data, 0, surLen + 11 + 2);

                    off += surLen;
                    surLen -= surLen;
                }
                else//结束但不能容纳Crc
                {
                    Buffer.BlockCopy(buffer, off, data, 11, surLen);
                    this.GoSend(endPoint, data, 0, surLen + 11);
                    off += surLen;
                    surLen -= surLen;

                    var finData = new byte[13];
                    Buffer.BlockCopy(TouchSocketBitConverter.Default.GetBytes(id), 0, finData, 0, 8);
                    Buffer.BlockCopy(TouchSocketBitConverter.Default.GetBytes(sn++), 0, finData, 8, 2);
                    byte flag = 0;
                    finData[10] = flag.SetBit(7, 1);
                    Buffer.BlockCopy(Crc.Crc16(buffer, offset, length), 0, finData, 11, 2);
                    this.GoSend(endPoint, finData, 0, finData.Length);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        protected override void PreviewSend(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }

            if (length > this.MaxPackageSize)
            {
                throw new OverlengthException("发送数据大于设定值，相同解析器可能无法收到有效数据，已终止发送");
            }

            using (var byteBlock = new ByteBlock(length))
            {
                foreach (var item in transferBytes)
                {
                    byteBlock.Write(item.Array, item.Offset, item.Count);
                }
                this.PreviewSend(endPoint, byteBlock.Buffer, 0, byteBlock.Len);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
        }
    }
}