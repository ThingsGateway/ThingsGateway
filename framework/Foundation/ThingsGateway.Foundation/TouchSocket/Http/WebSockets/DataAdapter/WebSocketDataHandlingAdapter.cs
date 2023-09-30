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

namespace ThingsGateway.Foundation.Http.WebSockets
{
    /// <summary>
    /// WebSocket适配器
    /// </summary>
    public class WebSocketDataHandlingAdapter : SingleStreamDataHandlingAdapter
    {
        private WSDataFrame m_dataFrameTemp;

        /// <summary>
        /// 数据包剩余长度
        /// </summary>
        private int m_surPlusLength = 0;

        /// <summary>
        /// 临时包
        /// </summary>
        private ByteBlock m_tempByteBlock;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="dataFrame"></param>
        /// <returns></returns>
        public FilterResult DecodingFromBytes(byte[] dataBuffer, ref int offset, int length, out WSDataFrame dataFrame)
        {
            var index = offset;
            dataFrame = new WSDataFrame
            {
                RSV1 = dataBuffer[offset].GetBit(6) == 1,
                RSV2 = dataBuffer[offset].GetBit(5) == 1,
                RSV3 = dataBuffer[offset].GetBit(4) == 1,
                FIN = (dataBuffer[offset] >> 7) == 1,
                Opcode = (WSDataType)(dataBuffer[offset] & 0xf),
                Mask = (dataBuffer[++offset] >> 7) == 1
            };

            var payloadLength = dataBuffer[offset] & 0x7f;
            if (payloadLength < 126)
            {
                offset++;
            }
            else if (payloadLength == 126)
            {
                if (length < 4)
                {
                    offset = index;
                    return FilterResult.Cache;
                }
                payloadLength = TouchSocketBitConverter.BigEndian.ToUInt16(dataBuffer, ++offset);
                offset += 2;
            }
            else if (payloadLength == 127)
            {
                if (length < 12)
                {
                    this.m_tempByteBlock ??= new ByteBlock();
                    this.m_tempByteBlock.Write(dataBuffer, index, length);
                    offset = index;
                    return FilterResult.GoOn;
                }
                payloadLength = (int)TouchSocketBitConverter.BigEndian.ToUInt64(dataBuffer, ++offset);
                offset += 8;
            }

            dataFrame.PayloadLength = payloadLength;

            if (dataFrame.Mask)
            {
                if (length < (offset - index) + 4)
                {
                    this.m_tempByteBlock ??= new ByteBlock();
                    this.m_tempByteBlock.Write(dataBuffer, index, length);
                    offset = index;
                    return FilterResult.GoOn;
                }
                dataFrame.MaskingKey = new byte[4];
                dataFrame.MaskingKey[0] = dataBuffer[offset++];
                dataFrame.MaskingKey[1] = dataBuffer[offset++];
                dataFrame.MaskingKey[2] = dataBuffer[offset++];
                dataFrame.MaskingKey[3] = dataBuffer[offset++];
            }

            var byteBlock = new ByteBlock(payloadLength);
            dataFrame.PayloadData = byteBlock;

            var surlen = length - (offset - index);
            if (payloadLength <= surlen)
            {
                byteBlock.Write(dataBuffer, offset, payloadLength);
                offset += payloadLength;
            }
            else
            {
                byteBlock.Write(dataBuffer, offset, surlen);
                offset += surlen;
            }

            return FilterResult.Success;
        }

        /// <summary>
        /// 当接收到数据时处理数据
        /// </summary>
        /// <param name="byteBlock">数据流</param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            var buffer = byteBlock.Buffer;
            var r = byteBlock.Len;

            if (this.m_tempByteBlock != null)
            {
                this.m_tempByteBlock.Write(buffer, 0, r);
                buffer = this.m_tempByteBlock.ToArray();
                r = this.m_tempByteBlock.Pos;
                this.m_tempByteBlock.Dispose();
                this.m_tempByteBlock = null;
            }

            if (this.m_dataFrameTemp == null)
            {
                this.SplitPackage(buffer, 0, r);
            }
            else
            {
                if (this.m_surPlusLength == r)
                {
                    this.m_dataFrameTemp.PayloadData.Write(buffer, 0, this.m_surPlusLength);
                    this.PreviewHandle(this.m_dataFrameTemp);
                    this.m_dataFrameTemp = null;
                    this.m_surPlusLength = 0;
                }
                else if (this.m_surPlusLength < r)
                {
                    this.m_dataFrameTemp.PayloadData.Write(buffer, 0, this.m_surPlusLength);
                    this.PreviewHandle(this.m_dataFrameTemp);
                    this.m_dataFrameTemp = null;
                    this.SplitPackage(buffer, this.m_surPlusLength, r);
                }
                else
                {
                    this.m_dataFrameTemp.PayloadData.Write(buffer, 0, r);
                    this.m_surPlusLength -= r;
                }
            }
        }

        /// <summary>
        /// 当发送数据前处理数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected override void PreviewSend(byte[] buffer, int offset, int length)
        {
            this.GoSend(buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes)
        {
            throw new System.NotImplementedException();//因为设置了不支持拼接发送，所以该方法可以不实现。
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        protected override void PreviewSend(IRequestInfo requestInfo)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
            this.m_tempByteBlock.SafeDispose();
            this.m_tempByteBlock = null;
            this.m_dataFrameTemp = null;
            this.m_surPlusLength = 0;
            base.Reset();
        }

        private void PreviewHandle(WSDataFrame dataFrame)
        {
            try
            {
                if (dataFrame.Mask)
                {
                    WSTools.DoMask(dataFrame.PayloadData.Buffer, 0, dataFrame.PayloadData.Buffer, 0, dataFrame.PayloadData.Len, dataFrame.MaskingKey);
                }
                this.GoReceived(null, dataFrame);
            }
            finally
            {
                dataFrame.Dispose();
            }
        }

        /// <summary>
        /// 分解包
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        private void SplitPackage(byte[] dataBuffer, int offset, int length)
        {
            while (offset < length)
            {
                if (length - offset < 2)
                {
                    this.m_tempByteBlock ??= new ByteBlock();
                    this.m_tempByteBlock.Write(dataBuffer, offset, length - offset);
                    return;
                }

                switch (this.DecodingFromBytes(dataBuffer, ref offset, length - offset, out var dataFrame))
                {
                    case FilterResult.Cache:
                        {
                            if (this.m_tempByteBlock == null)
                            {
                                this.m_tempByteBlock = new ByteBlock();
                            }
                            this.m_tempByteBlock.Write(dataBuffer, offset, length - offset);
                            return;
                        }
                    case FilterResult.Success:
                        {
                            if (dataFrame.PayloadLength == dataFrame.PayloadData.Len)
                            {
                                this.PreviewHandle(dataFrame);
                            }
                            else
                            {
                                this.m_surPlusLength = dataFrame.PayloadLength - dataFrame.PayloadData.Len;
                                this.m_dataFrameTemp = dataFrame;
                            }
                        }
                        break;

                    case FilterResult.GoOn:
                    default:
                        return;
                }
            }
        }
    }
}