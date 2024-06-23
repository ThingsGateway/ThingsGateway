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

/// <inheritdoc cref="TcpSessionClient"/>
public class TcpSessionClientChannel : TcpSessionClient, IClientChannel
{
    public TcpSessionClientChannel()
    {
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <inheritdoc/>
    public AsyncAutoResetEvent WaitLock { get; } = new AsyncAutoResetEvent(true);

    /// <summary>
    /// 等待池
    /// </summary>
    public WaitHandlePool<MessageBase> WaitHandlePool { get; private set; } = new();

    /// <inheritdoc/>
    public ConcurrentList<IProtocol> Collects { get; } = new();

    /// <summary>
    /// 接收到数据
    /// </summary>
    public ChannelReceivedEventHandler ChannelReceived { get; set; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpService;

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{IP}:{Port}:{Id}";
    }

    /// <inheritdoc/>
    public Task ConnectAsync(int timeout, CancellationToken token) => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task SetupAsync(TouchSocketConfig config)
    {
        return EasyTask.CompletedTask;
    }

    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
            this.SetAdapter(singleStreamDataHandlingAdapter);
    }

    public void Close(string msg)
    {
        this.CloseAsync(msg).ConfigureAwait(false);
    }

    public override Task CloseAsync(string msg)
    {
        WaitHandlePool.SafeDispose();
        return base.CloseAsync(msg);
    }

    public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (DisposedValue) return;
        WaitHandlePool.SafeDispose();
        base.Dispose(disposing);
    }

    protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
    {
        if (this.ChannelReceived != null)
        {
            await this.ChannelReceived.Invoke(this, e).ConfigureAwait(false);
        }
        await base.OnTcpReceived(e);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnected(ConnectedEventArgs e)
    {
        //Logger?.Debug($"{ToString()}{FoundationConst.Connected}");
        if (Started != null)
            await Started.Invoke(this).ConfigureAwait(false);
        await base.OnTcpConnected(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(ConnectingEventArgs e)
    {
        if (Starting != null)
            await Starting.Invoke(this).ConfigureAwait(false);
        await base.OnTcpConnecting(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosing(ClosingEventArgs e)
    {
        Logger?.Debug($"{ToString()} Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        await base.OnTcpClosing(e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(ClosedEventArgs e)
    {
        Logger?.Debug($"{ToString()} Closed{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
        await base.OnTcpClosed(e).ConfigureAwait(false);
    }
}
