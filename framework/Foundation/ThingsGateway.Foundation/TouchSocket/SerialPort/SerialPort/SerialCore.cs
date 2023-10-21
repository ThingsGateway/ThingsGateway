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

using System.IO.Ports;

namespace ThingsGateway.Foundation.Serial;
internal sealed class InternalSerialCore : SerialCore
{

}
/// <summary>
/// Serial核心
/// </summary>
public class SerialCore : IDisposable, ISender
{
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
    private bool m_online => m_serialPort?.IsOpen == true;
    private int m_receiveBufferSize = 1024 * 10;
    private ValueCounter m_receiveCounter;
    private int m_sendBufferSize = 1024 * 10;
    private ValueCounter m_sendCounter;
    private readonly EasyLock m_semaphoreForSend = new EasyLock();
    private SerialPort m_serialPort;

    #endregion 字段

    /// <summary>
    /// Tcp核心
    /// </summary>
    public SerialCore()
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

    /// <summary>
    /// 析构函数
    /// </summary>
    ~SerialCore()
    {
        this.SafeDispose();
    }

    /// <inheritdoc/>
    public bool CanSend => this.m_online;

    /// <summary>
    /// 当中断Tcp的时候。当为<see langword="true"/>时，意味着是调用<see cref="Close(string)"/>。当为<see langword="false"/>时，则是其他中断。
    /// </summary>
    public Action<SerialCore, bool, string> OnBreakOut { get; set; }

    /// <summary>
    /// 当发生异常的时候
    /// </summary>
    public Action<SerialCore, Exception> OnException { get; set; }

    /// <summary>
    /// 在线状态
    /// </summary>
    public bool Online { get => this.m_online; }
    /// <summary>
    /// UserToken
    /// </summary>
    public ByteBlock UserToken { get; set; }

    /// <summary>
    /// 当收到数据的时候
    /// </summary>
    public Action<SerialCore, ByteBlock> OnReceived { get; set; }

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
    /// SerialPort
    /// </summary>
    public SerialPort MainSerialPort { get => this.m_serialPort; }


    /// <summary>
    /// 开始以Iocp方式接收
    /// </summary>
    public virtual void BeginIocpReceive()
    {
        var byteBlock = BytePool.Default.GetByteBlock(this.ReceiveBufferSize);
        this.UserToken = byteBlock;
        byteBlock.SetLength(0);
        if (this.m_serialPort.BytesToRead > 0)
        {
            this.m_bufferRate += 2;
            this.ProcessReceived();
        }
        m_serialPort.DataReceived += MainSerialPort_DataReceived;
    }

    private void MainSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            this.m_bufferRate = 1;
            this.ProcessReceived();
        }
        catch (Exception ex)
        {
            this.PrivateBreakOut(false, ex.ToString());
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
    /// 释放对象
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        UserToken.SafeDispose();
    }

    /// <summary>
    /// 重置环境，并设置新的<see cref="m_serialPort"/>。
    /// </summary>
    /// <param name="socket"></param>
    public virtual void Reset(SerialPort socket)
    {
        if (socket is null)
        {
            throw new ArgumentNullException(nameof(socket));
        }

        if (!socket.IsOpen)
        {
            throw new Exception("新的SerialPort必须在连接状态。");
        }
        this.Reset();
        this.m_serialPort = socket;
    }

    /// <summary>
    /// 重置环境。
    /// </summary>
    public virtual void Reset()
    {
        this.m_receiveCounter.Reset();
        this.m_sendCounter.Reset();
        this.m_serialPort = null;
        this.OnReceived = null;
        this.OnBreakOut = null;
        this.UserToken = null;
        this.m_bufferRate = 1;
        this.m_receiveBufferSize = this.MinBufferSize;
        this.m_sendBufferSize = this.MinBufferSize;
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
            this.m_serialPort.Write(buffer, offset, length);
        }
        finally
        {
            this.m_semaphoreForSend.Release();
        }
        this.m_sendCounter.Increment(length);
    }

    /// <summary>
    /// 异步发送数据。
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

            this.m_serialPort.Write(buffer, offset, length);
        }
        finally
        {
            this.m_semaphoreForSend.Release();
        }

        this.m_sendCounter.Increment(length);
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
        if (this.MainSerialPort != null && !MainSerialPort.IsOpen)
        {
            this.MainSerialPort.ReadBufferSize = this.m_receiveBufferSize;
        }
    }

    private void OnSendPeriod(long value)
    {
        this.m_sendBufferSize = Math.Max(TouchSocketUtility.HitBufferLength(value), this.MinBufferSize);
        if (this.MainSerialPort != null && !MainSerialPort.IsOpen)
        {
            this.MainSerialPort.WriteBufferSize = this.m_sendBufferSize;
        }
    }

    private void PrivateBreakOut(bool manual, string msg)
    {
        lock (this.SyncRoot)
        {
            if (this.m_online)
            {
                this.BreakOut(manual, msg);
            }
        }
    }

    private void ProcessReceived()
    {
        if (!this.m_online)
        {
            UserToken?.SafeDispose();
            return;
        }
        if (m_serialPort.BytesToRead > 0)
        {
            var byteBlock = UserToken;
            byte[] buffer = BytePool.Default.Rent(m_serialPort.BytesToRead);
            int num = m_serialPort.Read(buffer, 0, m_serialPort.BytesToRead);
            byteBlock.Write(buffer, 0, num);
            byteBlock.SetLength(num);
            this.HandleBuffer(byteBlock);
            try
            {
                var newByteBlock = BytePool.Default.GetByteBlock((int)Math.Min(this.ReceiveBufferSize * this.m_bufferRate, this.MaxBufferSize));
                newByteBlock.SetLength(0);
                UserToken = newByteBlock;

                if (m_serialPort.BytesToRead > 0)
                {
                    this.m_bufferRate += 2;
                    this.ProcessReceived();
                }
            }
            catch (Exception ex)
            {
                this.PrivateBreakOut(false, ex.ToString());
            }
        }
    }
}