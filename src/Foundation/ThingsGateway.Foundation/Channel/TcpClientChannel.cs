//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// Tcp客户端通道
/// </summary>
public class TcpClientChannel : TcpClient, IClientChannel
{
    private readonly WaitLock m_semaphoreForConnect = new WaitLock();

    /// <inheritdoc/>
    public TcpClientChannel()
    {
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <inheritdoc/>
    public ChannelReceivedEventHandler ChannelReceived { get; set; } = new();

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpClient;

    /// <inheritdoc/>
    public ConcurrentList<IProtocol> Collects { get; } = new();

    /// <inheritdoc/>
    public DataHandlingAdapter ReadOnlyDataHandlingAdapter => DataHandlingAdapter;

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; set; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; set; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; set; } = new();
    /// <inheritdoc/>
    public ChannelEventHandler Stoping { get; set; } = new();
    /// <summary>
    /// 等待池
    /// </summary>
    public WaitHandlePool<MessageBase> WaitHandlePool { get; } = new();

    /// <inheritdoc/>
    public WaitLock WaitLock { get; } = new WaitLock();

    /// <inheritdoc/>
    public void Close(string msg)
    {
        CloseAsync(msg).ConfigureAwait(false).GetAwaiter().GetResult();
    }


    /// <inheritdoc/>
    public override async Task CloseAsync(string msg)
    {
        if (Online)
        {
            await base.CloseAsync(msg).ConfigureAwait(false);
            Logger?.Debug($"{ToString()}  Closed{msg}");
            await this.OnChannelEvent(Stoped).ConfigureAwait(false);

        }
    }

    /// <inheritdoc/>
    public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default)
    {
        ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public override async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        if (!Online)
        {
            try
            {
                await m_semaphoreForConnect.WaitAsync(token).ConfigureAwait(false);
                if (!Online)
                {
                    await SetupAsync(Config.Clone()).ConfigureAwait(false);
                    await base.ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false);
                    Logger?.Debug($"{ToString()}  Connected");
                    await this.OnChannelEvent(Started).ConfigureAwait(false);

                }
            }
            finally
            {
                m_semaphoreForConnect.Release();
            }
        }
    }

    /// <inheritdoc/>
    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
            SetAdapter(singleStreamDataHandlingAdapter);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{IP}:{Port}";
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosing(ClosingEventArgs e)
    {
        await this.OnChannelEvent(Stoping).ConfigureAwait(false);
        Logger?.Debug($"{ToString()}  Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");

        await base.OnTcpClosing(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        Logger?.Debug($"{ToString()}  Connecting{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        await this.OnChannelEvent(Starting).ConfigureAwait(false);
        await base.OnTcpConnecting(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        await base.OnTcpReceived(e).ConfigureAwait(false);
        await this.OnChannelReceivedEvent(e, ChannelReceived).ConfigureAwait(false);
    }

}
