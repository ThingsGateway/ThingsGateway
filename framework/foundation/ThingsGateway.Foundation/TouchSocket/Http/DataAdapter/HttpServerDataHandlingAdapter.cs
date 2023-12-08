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

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// Http服务器数据处理适配器
    /// </summary>
    public class HttpServerDataHandlingAdapter : NormalDataHandlingAdapter
    {
        /// <summary>
        /// 缓存数据，如果需要手动释放，请先判断，然后到调用<see cref="IDisposable.Dispose"/>后，再置空；
        /// </summary>
        protected ByteBlock tempByteBlock;

        private ITcpClientBase m_client;

        private HttpRequest m_request;

        private long m_surLen;

        private Task m_task;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <inheritdoc/>
        public override void OnLoaded(object owner)
        {
            if (!(owner is ITcpClientBase clientBase))
            {
                throw new Exception($"此适配器必须适用于{nameof(ITcpClientBase)}");
            }
            this.m_client = clientBase;
            base.OnLoaded(owner);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            if (this.tempByteBlock == null)
            {
                byteBlock.Pos = 0;
                this.Single(byteBlock, false);
            }
            else
            {
                this.tempByteBlock.Write(byteBlock.Buffer, 0, byteBlock.Len);
                var block = this.tempByteBlock;
                this.tempByteBlock = null;
                block.Pos = 0;
                this.Single(block, true);
            }
        }

        private void Cache(ByteBlock byteBlock)
        {
            if (byteBlock.CanReadLen > 0)
            {
                this.tempByteBlock = new ByteBlock();
                this.tempByteBlock.Write(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);
                if (this.tempByteBlock.Len > this.MaxPackageSize)
                {
                    this.OnError("缓存的数据长度大于设定值的情况下未收到解析信号");
                }
            }
        }

        private Task RunGoReceived(HttpRequest request)
        {
            return Task.Run(() => { this.GoReceived(null, request); });
        }

        private void Single(ByteBlock byteBlock, bool dis)
        {
            try
            {
                while (byteBlock.CanReadLen > 0)
                {
                    if (this.m_request == null)
                    {
                        this.m_request = new HttpRequest(this.m_client, true);
                        if (this.m_request.ParsingHeader(byteBlock, byteBlock.CanReadLen))
                        {
                            byteBlock.Pos++;
                            if (this.m_request.ContentLength > byteBlock.CanReadLen)
                            {
                                this.m_surLen = this.m_request.ContentLength;

                                this.m_task = this.RunGoReceived(this.m_request);
                            }
                            else
                            {
                                byteBlock.Read(out var buffer, (int)this.m_request.ContentLength);
                                this.m_request.SetContent(buffer);
                                this.GoReceived(null, this.m_request);
                                this.m_request = null;
                            }
                        }
                        else
                        {
                            this.Cache(byteBlock);
                            this.m_request = null;
                            this.m_task?.Wait();
                            this.m_task = null;
                            return;
                        }
                    }

                    if (this.m_surLen > 0)
                    {
                        if (byteBlock.CanRead)
                        {
                            var len = (int)Math.Min(this.m_surLen, byteBlock.CanReadLen);
                            this.m_request.InternalInput(byteBlock.Buffer, byteBlock.Pos, len);
                            this.m_surLen -= len;
                            byteBlock.Pos += len;
                            if (this.m_surLen == 0)
                            {
                                this.m_request.InternalInput(null, 0, 0);
                                this.m_request = null;
                                this.m_task?.Wait();
                                this.m_task = null;
                            }
                        }
                    }
                    else
                    {
                        this.m_request = null;
                        this.m_task?.Wait();
                        this.m_task = null;
                    }
                }
            }
            finally
            {
                if (dis)
                {
                    byteBlock.Dispose();
                }
            }
        }
    }
}