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

namespace ThingsGateway.Foundation.Sockets;

/// <summary>
/// Tcp核心
/// </summary>
public class TcpCore : SocketAsyncEventArgs, IDisposable, ISender
{
    private const string m_msg1 = "远程终端主动关闭";

    /// <summary>
    /// 最小缓存尺寸
    /// </summary>
    public int MinBufferSize { get; set; } = 1024 * 10;

    /// <summary>
    /// 最大缓存尺寸
    /// </summary>
    public int MaxBufferSize { get; set; } = 1024 * 1024 * 10;

    #region 字段

    /// <summary>
    /// 同步根
    /// </summary>
    public readonly object SyncRoot = new object();

    private long m_bufferRate;
    private volatile bool m_online;
    private int m_receiveBufferSize = 1024 * 10;
    private ValueCounter m_receiveCounter;
    private int m_sendBufferSize = 1024 * 10;
    private ValueCounter m_sendCounter;
    private Socket m_socket;
    private readonly EasyLock m_semaphoreForSend = new();

    #endregion 字段

    /// <summary>
    /// Tcp核心
    /// </summary>
    public TcpCore()
    {
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

    /// <inheritdoc/>
    public bool CanSend => this.m_online;

    /// <summary>
    /// 当中断Tcp的时候。当为<see langword="true"/>时，意味着是调用<see cref="Close(string)"/>。当为<see langword="false"/>时，则是其他中断。
    /// </summary>
    public Action<TcpCore, bool, string> OnBreakOut { get; set; }

    /// <summary>
    /// 当发生异常的时候
    /// </summary>
    public Action<TcpCore, Exception> OnException { get; set; }

    /// <summary>
    /// 在线状态
    /// </summary>
    public bool Online { get => this.m_online; }

    /// <summary>
    /// 当收到数据的时候
    /// </summary>
    public Action<TcpCore, ByteBlock> OnReceived { get; set; }

    /// <summary>
    /// 接收缓存池,运行时的值会根据流速自动调整
    /// </summary>
    public int ReceiveBufferSize
    {
        get => this.m_receiveBufferSize;
    }

    /// <summary>
    /// 接收计数器
    /// </summary>
    public ValueCounter ReceiveCounter { get => this.m_receiveCounter; }

    /// <summary>
    /// 发送缓存池,运行时的值会根据流速自动调整
    /// </summary>
    public int SendBufferSize
    {
        get => this.m_sendBufferSize;
    }

    /// <summary>
    /// 发送计数器
    /// </summary>
    public ValueCounter SendCounter { get => this.m_sendCounter; }

    /// <summary>
    /// Socket
    /// </summary>
    public Socket Socket { get => this.m_socket; }

    /// <summary>
    /// 提供一个用于客户端-服务器通信的流，该流使用安全套接字层 (SSL) 安全协议对服务器和（可选）客户端进行身份验证。
    /// </summary>
    public SslStream SslStream { get; private set; }

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
        var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_socket, false), false);
        sslStream.AuthenticateAsServer(sslOption.Certificate);

        this.SslStream = sslStream;
        this.UseSsl = true;
    }

    /// <summary>
    /// 以Ssl客户端模式授权
    /// </summary>
    /// <param name="sslOption"></param>
    public virtual void Authenticate(ClientSslOption sslOption)
    {
        var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_socket, false), false);
        if (sslOption.ClientCertificates == null)
        {
            sslStream.AuthenticateAsClient(sslOption.TargetHost);
        }
        else
        {
            sslStream.AuthenticateAsClient(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
        }
        this.SslStream = sslStream;
        this.UseSsl = true;
    }

    /// <summary>
    /// 以Ssl服务器模式授权
    /// </summary>
    /// <param name="sslOption"></param>
    /// <returns></returns>
    public virtual async Task AuthenticateAsync(ServiceSslOption sslOption)
    {
        var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_socket, false), false);
        await sslStream.AuthenticateAsServerAsync(sslOption.Certificate);

        this.SslStream = sslStream;
        this.UseSsl = true;
    }

    /// <summary>
    /// 以Ssl客户端模式授权
    /// </summary>
    /// <param name="sslOption"></param>
    /// <returns></returns>
    public virtual async Task AuthenticateAsync(ClientSslOption sslOption)
    {
        var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_socket, false), false);
        if (sslOption.ClientCertificates == null)
        {
            await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost);
        }
        else
        {
            await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
        }
        this.SslStream = sslStream;
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
        if (!this.m_socket.ReceiveAsync(this))
        {
            this.m_bufferRate += 2;
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
    public virtual async Task BeginSslReceive()
    {
        if (!this.UseSsl)
        {
            throw new Exception("请先完成Ssl验证授权");
        }
        while (this.m_online)
        {
            var byteBlock = new ByteBlock(this.ReceiveBufferSize);
            try
            {
                var r = await Task<int>.Factory.FromAsync(this.SslStream.BeginRead, this.SslStream.EndRead, byteBlock.Buffer, 0, byteBlock.Capacity, default);
                if (r == 0)
                {
                    this.PrivateBreakOut(false, m_msg1);
                    return;
                }

                byteBlock.SetLength(r);
                this.HandleBuffer(byteBlock);
            }
            catch (Exception ex)
            {
                byteBlock.Dispose();
                this.PrivateBreakOut(false, ex.ToString());
            }
        }
    }

    /// <summary>
    /// 请求关闭
    /// </summary>
    /// <param name="msg"></param>
    public virtual void Close(string msg)
    {
        this.PrivateBreakOut(true, msg);
    }

    /// <summary>
    /// 重置环境，并设置新的<see cref="Socket"/>。
    /// </summary>
    /// <param name="socket"></param>
    public virtual void Reset(Socket socket)
    {
        if (socket is null)
        {
            throw new ArgumentNullException(nameof(socket));
        }

        if (!socket.Connected)
        {
            throw new Exception("新的Socket必须在连接状态。");
        }
        this.Reset();
        this.m_online = true;
        this.m_socket = socket;
    }

    /// <summary>
    /// 重置环境。
    /// </summary>
    public virtual void Reset()
    {
        this.m_receiveCounter.Reset();
        this.m_sendCounter.Reset();
        this.SslStream?.Dispose();
        this.SslStream = null;
        this.m_socket = null;
        this.OnReceived = null;
        this.OnBreakOut = null;
        this.UserToken = null;
        this.m_bufferRate = 1;
        this.m_receiveBufferSize = this.MinBufferSize;
        this.m_sendBufferSize = this.MinBufferSize;
        this.m_online = false;
    }

    /// <summary>
    /// 判断，当不在连接状态时触发异常。
    /// </summary>
    /// <exception cref="NotConnectedException"></exception>
    protected void ThrowIfNotConnected()
    {
        if (!this.m_online)
        {
            throw new NotConnectedException();
        }
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
        this.ThrowIfNotConnected();
        try
        {
            this.m_semaphoreForSend.Wait();
            if (this.UseSsl)
            {
                this.SslStream.Write(buffer, offset, length);
            }
            else
            {
                while (length > 0)
                {
                    var r = this.m_socket.Send(buffer, offset, length, SocketFlags.None);
                    if (r == 0 && length > 0)
                    {
                        throw new Exception("发送数据不完全");
                    }
                    offset += r;
                    length -= r;
                }
            }
            this.m_sendCounter.Increment(length);
        }
        finally
        {
            this.m_semaphoreForSend.Release();
        }
    }

    /// <summary>
    /// 异步发送数据。
    /// <para>
    /// 内部会根据是否启用Ssl，进行直接发送，还是Ssl发送。
    /// </para>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual async Task SendAsync(byte[] buffer, int offset, int length)
    {
        this.ThrowIfNotConnected();
        try
        {
            await this.m_semaphoreForSend.WaitAsync();
#if NET6_0_OR_GREATER
            if (this.UseSsl)
            {
                await this.SslStream.WriteAsync(new ReadOnlyMemory<byte>(buffer, offset, length), CancellationToken.None);
            }
            else
            {
                while (length > 0)
                {
                    var r = await this.m_socket.SendAsync(new ArraySegment<byte>(buffer, offset, length), SocketFlags.None, CancellationToken.None);
                    if (r == 0 && length > 0)
                    {
                        throw new Exception("发送数据不完全");
                    }
                    offset += r;
                    length -= r;
                }
            }

#else
            if (this.UseSsl)
            {
                await this.SslStream.WriteAsync(buffer, offset, length, CancellationToken.None);
            }
            else
            {
                while (length > 0)
                {
                    var r = this.m_socket.Send(buffer, offset, length, SocketFlags.None);
                    if (r == 0 && length > 0)
                    {
                        throw new Exception("发送数据不完全");
                    }
                    offset += r;
                    length -= r;
                }
            }
#endif

            this.m_sendCounter.Increment(length);
        }
        finally
        {
            this.m_semaphoreForSend.Release();
        }
    }

    /// <summary>
    /// 当中断Tcp时。
    /// </summary>
    /// <param name="manual">当为<see langword="true"/>时，意味着是调用<see cref="Close(string)"/>。当为<see langword="false"/>时，则是其他中断。</param>
    /// <param name="msg"></param>
    protected virtual void BreakOut(bool manual, string msg)
    {
        this.OnBreakOut?.Invoke(this, manual, msg);
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
                this.PrivateBreakOut(false, ex.ToString());
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
        this.m_receiveBufferSize = Math.Max(TouchSocketUtility.HitBufferLength(value), this.MinBufferSize);
        if (this.m_socket != null)
        {
            this.m_socket.ReceiveBufferSize = this.m_receiveBufferSize;
        }
    }

    private void OnSendPeriod(long value)
    {
        this.m_sendBufferSize = Math.Max(TouchSocketUtility.HitBufferLength(value), this.MinBufferSize);
        if (this.m_socket != null)
        {
            this.m_socket.SendBufferSize = this.m_sendBufferSize;
        }
    }

    private void PrivateBreakOut(bool manual, string msg)
    {
        lock (this.SyncRoot)
        {
            if (this.m_online)
            {
                this.m_online = false;
                this.BreakOut(manual, msg);
            }
        }
    }

    private void ProcessReceived(SocketAsyncEventArgs e)
    {
        if (e.SocketError != SocketError.Success)
        {
            this.PrivateBreakOut(false, e.SocketError.ToString());
            return;
        }
        else if (e.BytesTransferred > 0)
        {
            var byteBlock = (ByteBlock)e.UserToken;
            byteBlock.SetLength(e.BytesTransferred);
            this.HandleBuffer(byteBlock);
            try
            {
                var newByteBlock = BytePool.Default.GetByteBlock((int)Math.Min(this.ReceiveBufferSize * this.m_bufferRate, this.MaxBufferSize));
                e.UserToken = newByteBlock;
                e.SetBuffer(newByteBlock.Buffer, 0, newByteBlock.Capacity);

                if (!this.m_socket.ReceiveAsync(e))
                {
                    this.m_bufferRate += 2;
                    this.ProcessReceived(e);
                }
            }
            catch (Exception ex)
            {
                this.PrivateBreakOut(false, ex.ToString());
            }
        }
        else
        {
            this.PrivateBreakOut(false, m_msg1);
        }
    }
}