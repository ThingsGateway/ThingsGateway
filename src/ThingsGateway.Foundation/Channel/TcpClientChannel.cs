//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// 简单Tcp客户端
/// </summary>
public class TcpClientChannel : TcpClient, IClientChannel
{
    public TcpClientChannel()
    {
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    private readonly EasyLock m_semaphoreForConnect = new EasyLock();

    /// <inheritdoc/>
    public ConcurrentList<IProtocol> Collects { get; } = new();

    /// <inheritdoc/>
    public AsyncAutoResetEvent WaitLock { get; } = new AsyncAutoResetEvent();

    /// <summary>
    /// 等待池
    /// </summary>
    public WaitHandlePool<MessageBase> WaitHandlePool { get; } = new();

    /// <summary>
    /// 接收到数据
    /// </summary>
    public ChannelReceivedEventHandler ChannelReceived { get; set; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpClient;

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{IP}:{Port}";
    }

    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
            this.SetAdapter(singleStreamDataHandlingAdapter);
    }

    public void Close(string msg)
    {
        this.CloseAsync(msg).GetFalseAwaitResult();
    }

    public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default)
    {
        this.ConnectAsync(millisecondsTimeout, token).GetFalseAwaitResult();
    }

    public override async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        if (!this.Online)
        {
            try
            {
                await this.m_semaphoreForConnect.WaitAsync(token).ConfigureAwait(false);
                if (!this.Online)
                {
                    await base.ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false);
                    Logger?.Debug($"{ToString()}  Connected");
                    if (Started != null)
                        await Started.Invoke(this).ConfigureAwait(false);
                }
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }
    }

    public override async Task CloseAsync(string msg)
    {
        if (this.Online)
        {
            await base.CloseAsync(msg).ConfigureAwait(false);
            Logger?.Debug($"{ToString()}  Closed{msg}");
            if (Stoped != null)
                await Stoped.Invoke(this).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (this.ChannelReceived != null)
        {
            await this.ChannelReceived.Invoke(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }
        }
        await base.OnTcpReceived(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        Logger?.Debug($"{ToString()}  Connecting{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        if (Starting != null)
            await Starting.Invoke(this).ConfigureAwait(false);
        await base.OnTcpConnecting(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosing(ClosingEventArgs e)
    {
        Logger?.Debug($"{ToString()}  Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");

        await base.OnTcpClosing(e).ConfigureAwait(false);
    }
}
