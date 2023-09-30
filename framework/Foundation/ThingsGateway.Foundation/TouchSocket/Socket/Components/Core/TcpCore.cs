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

using System.Net.Security;
using System.Net.Sockets;

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// Tcp核心
    /// </summary>
    public class TcpCore : SocketAsyncEventArgs, IDisposable
    {
        private const string m_msg1 = "远程终端主动关闭";

        #region 字段

        private long m_bufferRate;
        private bool m_disposedValue;
        private int m_receiveBufferSize = 64 * 1024;
        private ValueCounter m_receiveCounter;
        private int m_sendBufferSize = 1024 * 64;
        private ValueCounter m_sendCounter;
        private Stream m_workStream;

        #endregion 字段

        /// <summary>
        /// 创建一个Tcp核心
        /// </summary>
        /// <param name="socket"></param>
        public TcpCore(Socket socket)
        {
            this.Socket = socket;
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnReceivePeriod
            };

            this.m_sendCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnSendPeriod
            };
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~TcpCore()
        {
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// 当关闭的时候
        /// </summary>
        public Action<TcpCore, string> OnClose { get; set; }

        /// <summary>
        /// 当发生异常的时候
        /// </summary>
        public Action<TcpCore, Exception> OnException { get; set; }

        /// <summary>
        /// 当收到数据的时候
        /// </summary>
        public Action<TcpCore, ByteBlock> OnReceived { get; set; }

        /// <summary>
        /// 接收缓存池（可以设定初始值，运行时的值会根据流速自动调整）
        /// </summary>
        public int ReceiveBufferSize
        {
            get => m_receiveBufferSize;
            set
            {
                m_receiveBufferSize = value;
                this.Socket.ReceiveBufferSize = value;
            }
        }

        /// <summary>
        /// 发送缓存池（可以设定初始值，运行时的值会根据流速自动调整）
        /// </summary>
        public int SendBufferSize
        {
            get => this.m_sendBufferSize;
            set
            {
                this.m_sendBufferSize = value;
                this.Socket.SendBufferSize = value;
            }
        }

        /// <summary>
        /// Socket
        /// </summary>
        public Socket Socket { get; }

        /// <summary>
        /// 是否启用了Ssl
        /// </summary>
        public bool UseSsl { get; private set; }

        /// <summary>
        /// 以Ssl服务器模式授权
        /// </summary>
        /// <param name="sslOption"></param>
        public virtual void Authenticate(ServiceSslOption sslOption)
        {
            var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.Socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.Socket, false), false);
            sslStream.AuthenticateAsServer(sslOption.Certificate);

            this.m_workStream = sslStream;
            this.UseSsl = true;
        }

        /// <summary>
        /// 以Ssl客户端模式授权
        /// </summary>
        /// <param name="sslOption"></param>
        public virtual void Authenticate(ClientSslOption sslOption)
        {
            var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.Socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.Socket, false), false);
            if (sslOption.ClientCertificates == null)
            {
                sslStream.AuthenticateAsClient(sslOption.TargetHost);
            }
            else
            {
                sslStream.AuthenticateAsClient(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
            }
            this.m_workStream = sslStream;
            this.UseSsl = true;
        }

        /// <summary>
        /// 以Ssl服务器模式授权
        /// </summary>
        /// <param name="sslOption"></param>
        /// <returns></returns>
        public virtual async Task AuthenticateAsync(ServiceSslOption sslOption)
        {
            var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.Socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.Socket, false), false);
            await sslStream.AuthenticateAsServerAsync(sslOption.Certificate);

            this.m_workStream = sslStream;
            this.UseSsl = true;
        }

        /// <summary>
        /// 以Ssl客户端模式授权
        /// </summary>
        /// <param name="sslOption"></param>
        /// <returns></returns>
        public virtual async Task AuthenticateAsync(ClientSslOption sslOption)
        {
            var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.Socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.Socket, false), false);
            if (sslOption.ClientCertificates == null)
            {
                await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost);
            }
            else
            {
                await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
            }
            this.m_workStream = sslStream;
            this.UseSsl = true;
        }

        /// <summary>
        /// 开始以Iocp方式接收
        /// </summary>
        public virtual void BeginIocpReceive()
        {
            var byteBlock = BytePool.Default.GetByteBlock(this.ReceiveBufferSize);
            this.UserToken = byteBlock;
            this.SetBuffer(byteBlock.Buffer, 0, byteBlock.Capacity);
            if (!this.Socket.ReceiveAsync(this))
            {
                this.ProcessReceived(this);
            }
        }

        /// <summary>
        /// 开始以Ssl接收。
        /// <para>
        /// 注意，使用该方法时，应先完成授权。
        /// </para>
        /// </summary>
        /// <returns></returns>
        public async Task BeginSslReceive()
        {
            if (!this.UseSsl)
            {
                throw new Exception("请先完成Ssl验证授权");
            }
            while (true)
            {
                var byteBlock = new ByteBlock(this.ReceiveBufferSize);
                try
                {
                    var r = await Task<int>.Factory.FromAsync(this.m_workStream.BeginRead, this.m_workStream.EndRead, byteBlock.Buffer, 0, byteBlock.Capacity, default);
                    if (r == 0)
                    {
                        this.Close(m_msg1);
                        return;
                    }

                    byteBlock.SetLength(r);
                    this.HandleBuffer(byteBlock);
                }
                catch (Exception ex)
                {
                    byteBlock.Dispose();
                    this.Close(ex.Message);
                }
            }
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        public new void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 发送数据。
        /// <para>
        /// 内部会根据是否启用Ssl，进行直接发送，还是Ssl发送。
        /// </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            if (this.UseSsl)
            {
                this.m_workStream.Write(buffer, offset, length);
            }
            else
            {
                this.Socket.AbsoluteSend(buffer, offset, length);
            }
            this.m_sendCounter.Increment(length);
        }

        /// <summary>
        /// 当关闭的时候
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void Close(string msg)
        {
            this.OnClose?.Invoke(this, msg);
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposedValue)
            {
                if (disposing)
                {
                }

                this.m_disposedValue = true;
            }

            base.Dispose();
        }

        /// <summary>
        /// 当发生异常的时候
        /// </summary>
        /// <param name="ex"></param>
        protected virtual void Exception(Exception ex)
        {
            this.OnException?.Invoke(this, ex);
        }

        /// <inheritdoc/>
        protected override sealed void OnCompleted(SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                try
                {
                    this.m_bufferRate = 1;
                    this.ProcessReceived(e);
                }
                catch (Exception ex)
                {
                    e.SafeDispose();
                    this.Close(ex.Message);
                }
            }
        }

        /// <summary>
        /// 当收到数据的时候
        /// </summary>
        /// <param name="byteBlock"></param>
        protected virtual void Received(ByteBlock byteBlock)
        {
            this.OnReceived?.Invoke(this, byteBlock);
        }

        private void HandleBuffer(ByteBlock byteBlock)
        {
            try
            {
                this.m_receiveCounter.Increment(byteBlock.Length);
                this.Received(byteBlock);
            }
            catch (Exception ex)
            {
                this.Exception(ex);
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        private void OnReceivePeriod(long value)
        {
            this.ReceiveBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        private void OnSendPeriod(long value)
        {
            this.SendBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        private void ProcessReceived(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                e.SafeDispose();
                this.Close(e.SocketError.ToString());
                return;
            }
            else if (e.BytesTransferred > 0)
            {
                var byteBlock = (ByteBlock)e.UserToken;
                byteBlock.SetLength(e.BytesTransferred);
                this.HandleBuffer(byteBlock);
                try
                {
                    var newByteBlock = BytePool.Default.GetByteBlock((int)Math.Min(this.ReceiveBufferSize * this.m_bufferRate, TouchSocketUtility.MaxBufferLength));
                    e.UserToken = newByteBlock;
                    e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Capacity);

                    if (!this.Socket.ReceiveAsync(e))
                    {
                        this.m_bufferRate += 2;
                        this.ProcessReceived(e);
                    }
                }
                catch (Exception ex)
                {
                    e.SafeDispose();
                    this.Close(ex.Message);
                }
            }
            else
            {
                e.SafeDispose();
                this.Close(m_msg1);
            }
        }
    }
}