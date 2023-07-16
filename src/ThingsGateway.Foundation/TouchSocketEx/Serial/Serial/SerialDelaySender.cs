#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
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
    private readonly Timer m_timer;
    private volatile bool m_sending;

    /// <summary>
    /// 延迟发送器
    /// </summary>
    public SerialDelaySender(SerialPort serialPort, int queueLength, Action<Exception> onError)
    {
        m_serial = serialPort;
        m_onError = onError;
        m_queueDatas = new IntelligentDataQueue<QueueDataBytes>(queueLength);
        m_lockSlim = new ReaderWriterLockSlim();
        m_timer = new Timer(TimerRun, null, 10, 10);
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
            using (new ReadLock(m_lockSlim))
            {
                return m_sending;
            }
        }

        private set
        {
            using (new WriteLock(m_lockSlim))
            {
                m_sending = value;
            }
        }
    }

    /// <summary>
    /// 发送
    /// </summary>
    public void Send(QueueDataBytes dataBytes)
    {
        m_queueDatas.Enqueue(dataBytes);
        if (SwitchToRun())
        {
            ThreadPool.QueueUserWorkItem(BeginSend);
        }
    }

    /// <summary>
    /// 释放
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        m_timer.SafeDispose();
        m_queueDatas.Clear();
        base.Dispose(disposing);
    }

    private void BeginSend(object o)
    {
        try
        {
            byte[] buffer = BytePool.Default.GetByteCore(DelayLength);
            while (!DisposedValue)
            {
                try
                {
                    if (TryGet(buffer, out QueueDataBytes asyncByte))
                    {
                        m_serial.Write(asyncByte.Buffer, asyncByte.Offset, asyncByte.Length);
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    m_onError?.Invoke(ex);
                    break;
                }
            }
            BytePool.Default.Recycle(buffer);
            Sending = false;
        }
        catch
        {
        }
    }

    private bool SwitchToRun()
    {
        using (new WriteLock(m_lockSlim))
        {
            if (m_sending)
            {
                return false;
            }
            else
            {
                m_sending = true;
                return true;
            }
        }
    }

    private void TimerRun(object state)
    {
        if (SwitchToRun())
        {
            BeginSend(null);
        }
    }

    private bool TryGet(byte[] buffer, out QueueDataBytes asyncByteDe)
    {
        int len = 0;
        int surLen = buffer.Length;
        while (true)
        {
            if (m_queueDatas.TryPeek(out QueueDataBytes asyncB))
            {
                if (surLen > asyncB.Length)
                {
                    if (m_queueDatas.TryDequeue(out QueueDataBytes asyncByte))
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