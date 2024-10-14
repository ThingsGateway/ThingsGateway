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
/// Tcp终端通道
/// </summary>
public class TcpSessionClientChannel : TcpSessionClient, IClientChannel
{
    /// <inheritdoc/>
    public TcpSessionClientChannel()
    {
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <inheritdoc/>
    public ChannelReceivedEventHandler ChannelReceived { get; set; } = new();

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpService;

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
    public WaitHandlePool<MessageBase> WaitHandlePool { get; private set; } = new();

    /// <inheritdoc/>
    public WaitLock WaitLock { get; } = new WaitLock();

    /// <inheritdoc/>
    public void Close(string msg)
    {
        CloseAsync(msg).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override Task CloseAsync(string msg)
    {
        WaitHandlePool.SafeDispose();
        return base.CloseAsync(msg);
    }

    /// <inheritdoc/>
    public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default) => throw new NotSupportedException();

    /// <inheritdoc/>
    public Task ConnectAsync(int timeout, CancellationToken token) => throw new NotImplementedException();

    /// <inheritdoc/>
    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
            SetAdapter(singleStreamDataHandlingAdapter);
    }

    /// <inheritdoc/>
    public Task SetupAsync(TouchSocketConfig config)
    {
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{IP}:{Port}:{Id}";
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (DisposedValue) return;
        WaitHandlePool.SafeDispose();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        Logger?.Debug($"{ToString()} Closed{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        await this.OnChannelEvent(Stoped).ConfigureAwait(false);
        await base.OnTcpClosed(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosing(ClosingEventArgs e)
    {
        Logger?.Debug($"{ToString()} Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        await this.OnChannelEvent(Stoping).ConfigureAwait(false);
        await base.OnTcpClosing(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnected(ConnectedEventArgs e)
    {
        //Logger?.Debug($"{ToString()}{FoundationConst.Connected}");
        await this.OnChannelEvent(Started).ConfigureAwait(false);

        await base.OnTcpConnected(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
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
