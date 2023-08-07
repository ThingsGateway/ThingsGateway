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

/// <summary>
/// 延迟发送器
/// </summary>
public sealed class SerialDelaySender : DisposableObject
{
    private readonly ReaderWriterLockSlim m_lockSlim;
    private readonly Action<Exception> m_onError;
    private readonly IntelligentDataQueue<QueueDataBytes> m_queueDatas;
    private readonly SerialPort m_serial;
    private volatile bool m_sending;

    /// <summary>
    /// 延迟发送器
    /// </summary>
    public SerialDelaySender(SerialPort serialPort, int queueLength, Action<Exception> onError)
    {
        this.m_serial = serialPort;
        this.m_onError = onError;
        this.m_queueDatas = new IntelligentDataQueue<QueueDataBytes>(queueLength);
        this.m_lockSlim = new ReaderWriterLockSlim();
    }

    /// <summary>
    /// 延迟包最大尺寸，默认1024*512字节。
    /// </summary>
    public int DelayLength { get; set; } = 1024 * 512;

    /// <summary>
    /// 是否处于发送状态
    /// </summary>
    public bool Sending
    {
        get
        {
            using (new ReadLock(this.m_lockSlim))
            {
                return this.m_sending;
            }
        }

        private set
        {
            using (new WriteLock(this.m_lockSlim))
            {
                this.m_sending = value;
            }
        }
    }

    /// <summary>
    /// 发送
    /// </summary>
    public void Send(QueueDataBytes dataBytes)
    {
        this.m_queueDatas.Enqueue(dataBytes);
        if (this.SwitchToRun())
        {
            Task.Factory.StartNew(this.BeginSend);
        }
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        this.m_queueDatas.Clear();
        base.Dispose(disposing);
    }

    private void BeginSend()
    {
        try
        {
            var buffer = BytePool.Default.Rent(this.DelayLength);
            while (!this.DisposedValue)
            {
                try
                {
                    if (this.TryGet(buffer, out var asyncByte))
                    {
                        this.m_serial.AbsoluteSend(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    this.m_onError?.Invoke(ex);
                    break;
                }
            }
            BytePool.Default.Return(buffer);
            this.Sending = false;
        }
        catch
        {
        }
    }

    private bool SwitchToRun()
    {
        using (new WriteLock(this.m_lockSlim))
        {
            if (this.m_sending)
            {
                return false;
            }
            else
            {
                this.m_sending = true;
                return true;
            }
        }
    }

    private bool TryGet(byte[] buffer, out QueueDataBytes asyncByteDe)
    {
        var len = 0;
        var surLen = buffer.Length;
        while (true)
        {
            if (this.m_queueDatas.TryPeek(out var asyncB))
            {
                if (surLen > asyncB.Length)
                {
                    if (this.m_queueDatas.TryDequeue(out var asyncByte))
                    {
                        Array.Copy(asyncByte.Buffer, asyncByte.Offset, buffer, len, asyncByte.Length);
                        len += asyncByte.Length;
                        surLen -= asyncByte.Length;
                    }
                }
                else if (asyncB.Length > buffer.Length)
                {
                    if (len > 0)
                    {
                        break;
                    }
                    else
                    {
                        asyncByteDe = asyncB;
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }
            else
            {
                if (len > 0)
                {
                    break;
                }
                else
                {
                    asyncByteDe = default;
                    return false;
                }
            }
        }
        asyncByteDe = new QueueDataBytes(buffer, 0, len);
        return true;
    }
}