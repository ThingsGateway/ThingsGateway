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

using System.Text;

namespace ThingsGateway.Foundation.Dmtp
{
    /// <summary>
    /// Dmtp协议的消息。
    /// <para>|*2*|**4**|***************n***********|</para>
    /// <para>|ProtocolFlags|Length|Data|</para>
    /// <para>|ushort|int32|bytes|</para>
    /// </summary>
    public class DmtpMessage : DisposableObject, IFixedHeaderByteBlockRequestInfo
    {
        private int m_bodyLength;

        /// <summary>
        /// Dmtp协议的消息。
        /// <para>|*2*|**4**|***************n***********|</para>
        /// <para>|ProtocolFlags|Length|Data|</para>
        /// <para>|ushort|int32|bytes|</para>
        /// </summary>
        public DmtpMessage()
        {
        }

        /// <summary>
        /// Dmtp协议的消息。
        /// <para>|*2*|**4**|***************n***********|</para>
        /// <para>|ProtocolFlags|Length|Data|</para>
        /// <para>|ushort|int32|bytes|</para>
        /// <param name="protocolFlags"></param>
        /// </summary>
        public DmtpMessage(ushort protocolFlags)
        {
            this.ProtocolFlags = protocolFlags;
        }

        /// <summary>
        /// 实际使用的Body数据。
        /// </summary>
        public ByteBlock BodyByteBlock { get; set; }

        int IFixedHeaderByteBlockRequestInfo.BodyLength => this.m_bodyLength;

        /// <summary>
        /// 协议标识
        /// </summary>
        public ushort ProtocolFlags { get; set; }

        /// <summary>
        /// 构建数据到<see cref="ByteBlock"/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Build(ByteBlock byteBlock)
        {
            byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(this.ProtocolFlags));
            if (this.BodyByteBlock == null)
            {
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(0));
            }
            else
            {
                byteBlock.Write(TouchSocketBitConverter.BigEndian.GetBytes(this.BodyByteBlock.Len));
                byteBlock.Write(this.BodyByteBlock);
            }
        }

        /// <summary>
        /// 构建数据到<see langword="byte[]" />
        /// </summary>
        /// <returns></returns>
        public byte[] BuildAsBytes()
        {
            using (var byteBlock = new ByteBlock(this.GetLength()))
            {
                this.Build(byteBlock);
                return byteBlock.ToArray();
            }
        }

        /// <summary>
        /// 从当前内存中解析出一个<see cref="DmtpMessage"/>
        /// <para>注意：
        /// <list type="number">
        /// <item>本解析只能解析一个完整消息。所以使用该方法时，请确认是否已经接收完成一个完整的<see cref="DmtpMessage"/>包。</item>
        /// <item>本解析所得的<see cref="DmtpMessage"/>消息会脱离生命周期管理，所以需要手动释放。</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static DmtpMessage CreateFrom(ArraySegment<byte> bytes)
        {
            var buffer = bytes.Array;
            var offset = bytes.Offset;

            var protocolFlags = TouchSocketBitConverter.BigEndian.ToUInt16(buffer, offset);
            var bodyLength = TouchSocketBitConverter.BigEndian.ToInt32(buffer, 2 + offset);
            var byteBlock = new ByteBlock(bodyLength);
            byteBlock.Write(buffer, 6 + offset, bodyLength);

            return new DmtpMessage()
            {
                m_bodyLength = bodyLength,
                BodyByteBlock = byteBlock,
                ProtocolFlags = protocolFlags
            };
        }

        /// <summary>
        /// 从当前内存中解析出一个<see cref="DmtpMessage"/>
        /// <para>注意：
        /// <list type="number">
        /// <item>本解析只能解析一个完整消息。所以使用该方法时，请确认是否已经接收完成一个完整的<see cref="DmtpMessage"/>包。</item>
        /// <item>本解析所得的<see cref="DmtpMessage"/>消息会脱离生命周期管理，所以需要手动释放。</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public static DmtpMessage CreateFrom(ByteBlock block)
        {
            var buffer = block.Buffer;
            var protocolFlags = TouchSocketBitConverter.BigEndian.ToUInt16(buffer, 0);
            var bodyLength = TouchSocketBitConverter.BigEndian.ToInt32(buffer, 2);
            var byteBlock = new ByteBlock(bodyLength);
            byteBlock.Write(buffer, 6, bodyLength);
            byteBlock.SeekToStart();
            return new DmtpMessage()
            {
                m_bodyLength = bodyLength,
                BodyByteBlock = byteBlock,
                ProtocolFlags = protocolFlags
            };
        }

        /// <summary>
        /// 将<see cref="BodyByteBlock"/>的有效数据转换为utf-8的字符串。
        /// </summary>
        /// <returns></returns>
        public string GetBodyString()
        {
            if (this.BodyByteBlock == null || this.BodyByteBlock.Len == 0)
            {
                return default;
            }
            else
            {
                return Encoding.UTF8.GetString(this.BodyByteBlock.Buffer, 0, this.BodyByteBlock.Len);
            }
        }

        /// <summary>
        /// 获取整个<see cref="DmtpMessage"/>的数据长度。
        /// </summary>
        /// <returns></returns>
        public int GetLength()
        {
            return this.BodyByteBlock == null ? 6 : this.BodyByteBlock.Len + 6;
        }

        bool IFixedHeaderByteBlockRequestInfo.OnParsingBody(ByteBlock byteBlock)
        {
            if (byteBlock.Len == this.m_bodyLength)
            {
                this.BodyByteBlock = byteBlock;
                return true;
            }

            return false;
        }

        bool IFixedHeaderByteBlockRequestInfo.OnParsingHeader(byte[] header)
        {
            if (header.Length == 6)
            {
                this.ProtocolFlags = TouchSocketBitConverter.BigEndian.ToUInt16(header, 0);
                this.m_bodyLength = TouchSocketBitConverter.BigEndian.ToInt32(header, 2);
                return true;
            }
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (disposing)
            {
                this.BodyByteBlock.SafeDispose();
            }

            base.Dispose(disposing);
        }
    }
}