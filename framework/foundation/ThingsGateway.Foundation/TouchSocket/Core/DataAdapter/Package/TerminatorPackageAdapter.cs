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
using System.Text;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 终止字符数据包处理适配器，支持以任意字符、字节数组结尾的数据包。
    /// </summary>
    public class TerminatorPackageAdapter : SingleStreamDataHandlingAdapter
    {
        private ByteBlock m_tempByteBlock;

        private readonly byte[] m_terminatorCode;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="terminator"></param>
        public TerminatorPackageAdapter(string terminator) : this(0, Encoding.UTF8.GetBytes(terminator))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="terminator"></param>
        /// <param name="encoding"></param>
        public TerminatorPackageAdapter(string terminator, Encoding encoding)
            : this(0, encoding.GetBytes(terminator))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="minSize"></param>
        /// <param name="terminatorCode"></param>
        public TerminatorPackageAdapter(int minSize, byte[] terminatorCode)
        {
            this.MinSize = minSize;
            this.m_terminatorCode = terminatorCode;
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
        /// 即使找到了终止因子，也不会结束，默认0
        /// </summary>
        public int MinSize { get; set; } = 0;

        /// <summary>
        /// 保留终止因子
        /// </summary>
        public bool ReserveTerminatorCode { get; set; }

        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            if (this.CacheTimeoutEnable && DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.Reset();
            }
            var buffer = byteBlock.Buffer;
            var r = byteBlock.Len;
            if (this.m_tempByteBlock != null)
            {
                this.m_tempByteBlock.Write(buffer, 0, r);
                buffer = this.m_tempByteBlock.Buffer;
                r = (int)this.m_tempByteBlock.Pos;
            }

            var indexes = buffer.IndexOfInclude(0, r, this.m_terminatorCode);
            if (indexes.Count == 0)
            {
                if (r > this.MaxPackageSize)
                {
                    this.Reset();
                    this.Logger?.Error("在已接收数据大于设定值的情况下未找到终止因子，已放弃接收");
                }
                else if (this.m_tempByteBlock == null)
                {
                    this.m_tempByteBlock = new ByteBlock(r * 2);
                    this.m_tempByteBlock.Write(buffer, 0, r);
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                }
            }
            else
            {
                var startIndex = 0;
                foreach (var lastIndex in indexes)
                {
                    var length = this.ReserveTerminatorCode ? lastIndex - startIndex + 1 : lastIndex - startIndex - this.m_terminatorCode.Length + 1;
                    var packageByteBlock = new ByteBlock(length);
                    packageByteBlock.Write(buffer, startIndex, length);

                    var mes = Encoding.UTF8.GetString(packageByteBlock.Buffer, 0, (int)packageByteBlock.Pos);

                    this.PreviewHandle(packageByteBlock);
                    startIndex = lastIndex + 1;
                }
                this.Reset();
                if (startIndex < r)
                {
                    this.m_tempByteBlock = new ByteBlock((r - startIndex) * 2);
                    this.m_tempByteBlock.Write(buffer, startIndex, r - startIndex);
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                }
            }
        }

        /// <summary>
        /// 预处理
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected override void PreviewSend(byte[] buffer, int offset, int length)
        {
            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送的数据长度大于适配器设定的最大值，接收方可能会抛弃。");
            }
            var dataLen = length - offset + this.m_terminatorCode.Length;
            var byteBlock = new ByteBlock(dataLen);
            byteBlock.Write(buffer, offset, length);
            byteBlock.Write(this.m_terminatorCode);

            try
            {
                this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        protected override void PreviewSend(IList<ArraySegment<byte>> transferBytes)
        {
            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }
            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送的数据长度大于适配器设定的最大值，接收方可能会抛弃。");
            }
            var dataLen = length + this.m_terminatorCode.Length;
            var byteBlock = new ByteBlock(dataLen);
            foreach (var item in transferBytes)
            {
                byteBlock.Write(item.Array, item.Offset, item.Count);
            }

            byteBlock.Write(this.m_terminatorCode);

            try
            {
                this.GoSend(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
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
            base.Reset();
        }

        private void PreviewHandle(ByteBlock byteBlock)
        {
            try
            {
                byteBlock.Position = 0;
                this.GoReceived(byteBlock, null);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override Task PreviewSendAsync(IRequestInfo requestInfo)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        protected override async Task PreviewSendAsync(byte[] buffer, int offset, int length)
        {
            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送的数据长度大于适配器设定的最大值，接收方可能会抛弃。");
            }
            var dataLen = length - offset + this.m_terminatorCode.Length;
            var byteBlock = new ByteBlock(dataLen);
            byteBlock.Write(buffer, offset, length);
            byteBlock.Write(this.m_terminatorCode);

            try
            {
                await this.GoSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <inheritdoc/>
        protected override async Task PreviewSendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            var length = 0;
            foreach (var item in transferBytes)
            {
                length += item.Count;
            }
            if (length > this.MaxPackageSize)
            {
                throw new Exception("发送的数据长度大于适配器设定的最大值，接收方可能会抛弃。");
            }
            var dataLen = length + this.m_terminatorCode.Length;
            var byteBlock = new ByteBlock(dataLen);
            foreach (var item in transferBytes)
            {
                byteBlock.Write(item.Array, item.Offset, item.Count);
            }

            byteBlock.Write(this.m_terminatorCode);

            try
            {
                await this.GoSendAsync(byteBlock.Buffer, 0, byteBlock.Len);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }
    }
}