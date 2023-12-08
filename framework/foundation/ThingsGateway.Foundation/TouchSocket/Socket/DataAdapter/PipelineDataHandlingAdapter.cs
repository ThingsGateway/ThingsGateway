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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// Pipeline读取管道
    /// </summary>
    public abstract class Pipeline : BlockReadStream, IRequestInfo
    {
        /// <summary>
        /// Pipeline读取管道
        /// </summary>
        /// <param name="client"></param>
        protected Pipeline(ITcpClientBase client)
        {
            this.Client = client;
        }

        /// <summary>
        /// 当前支持此管道的客户端。
        /// </summary>
        public ITcpClientBase Client { get; set; }
    }

    /// <summary>
    /// 管道数据处理适配器。
    /// 使用该适配器后，<see cref="IRequestInfo"/>将为<see cref="Pipeline"/>.
    /// </summary>
    public class PipelineDataHandlingAdapter : NormalDataHandlingAdapter
    {
        private ITcpClientBase m_client;
        private byte[] m_buffer;
        private InternalPipeline m_pipeline;

        private Task m_task;

        /// <summary>
        /// 管道数据处理适配器。
        /// 使用该适配器后，<see cref="IRequestInfo"/>将为<see cref="Pipeline"/>.
        /// </summary>
        public PipelineDataHandlingAdapter()
        {
        }

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
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_pipeline.SafeDispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            if (this.m_pipeline == null || !this.m_pipeline.Enable)
            {
                this.m_task?.Wait();
                this.m_pipeline = new InternalPipeline(this.m_client);
                this.m_task = Task.Run(() =>
                  {
                      try
                      {
                          this.GoReceived(default, this.m_pipeline);
                          if (this.m_pipeline.CanReadLen > 0)
                          {
                              this.m_buffer = new byte[this.m_pipeline.CanReadLen];
                              this.m_pipeline.Read(this.m_buffer, 0, this.m_buffer.Length);
                          }
                      }
                      catch
                      {
                      }
                      finally
                      {
                          this.m_pipeline.SafeDispose();
                      }
                  });
            }
            if (this.m_buffer != null)
            {
                this.m_pipeline.InternalInput(this.m_buffer, 0, this.m_buffer.Length);
                this.m_buffer = null;
            }
            this.m_pipeline.InternalInput(byteBlock.Buffer, 0, byteBlock.Len);
        }
    }

    internal class InternalPipeline : Pipeline
    {
        private readonly object m_locker = new object();

        private bool m_disposedValue;

        /// <summary>
        /// Pipeline读取管道
        /// </summary>
        /// <param name="client"></param>
        public InternalPipeline(ITcpClientBase client) : base(client)
        {
            this.ReadTimeout = 60 * 1000;
        }

        public override bool CanRead => this.Enable;

        public override bool CanWrite => this.Client.CanSend;

        public bool Enable
        {
            get
            {
                lock (this.m_locker)
                {
                    return !this.m_disposedValue;
                }
            }
        }

        public override void Flush()
        {
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.Client.DefaultSend(buffer, offset, count);
        }

        internal void InternalInput(byte[] buffer, int offset, int length)
        {
            this.Input(buffer, offset, length);
        }

        protected override void Dispose(bool disposing)
        {
            lock (this.m_locker)
            {
                this.m_disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}