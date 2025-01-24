//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Collections.Concurrent;

namespace ThingsGateway.Foundation;

/// <summary>
/// Tcp客户端通道
/// </summary>
public class TcpClientChannel : TcpClient, IClientChannel
{
    /// <inheritdoc/>
    public TcpClientChannel(IChannelOptions channelOptions)
    {
        ChannelOptions = channelOptions;

        WaitHandlePool.MaxSign = ushort.MaxValue;
    }
    public override TouchSocketConfig Config => base.Config ?? ChannelOptions.Config;
    public int MaxSign { get => WaitHandlePool.MaxSign; set => WaitHandlePool.MaxSign = value; }

    /// <inheritdoc/>
    public ChannelReceivedEventHandler ChannelReceived { get; } = new();

    /// <inheritdoc/>
    public IChannelOptions ChannelOptions { get; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelOptions.ChannelType;

    /// <inheritdoc/>
    public ConcurrentList<IDevice> Collects { get; } = new();

    /// <inheritdoc/>
    public DataHandlingAdapter ReadOnlyDataHandlingAdapter => DataHandlingAdapter;

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; } = new();
    /// <inheritdoc/>
    public ChannelEventHandler Stoping { get; } = new();
    /// <summary>
    /// 等待池
    /// </summary>
    public WaitHandlePool<MessageBase> WaitHandlePool { get; } = new();


    /// <inheritdoc/>
    public WaitLock WaitLock => ChannelOptions.WaitLock;

    /// <inheritdoc/>
    public ConcurrentDictionary<long, Func<IClientChannel, ReceivedDataEventArgs, bool, Task>> ChannelReceivedWaitDict { get; } = new();

    private readonly WaitLock _connectLock = new WaitLock();
    /// <inheritdoc/>
    public override async Task CloseAsync(string msg)
    {
        if (Online)
        {
            try
            {
                await _connectLock.WaitAsync().ConfigureAwait(false);
                if (Online)
                {
                    await base.CloseAsync(msg).ConfigureAwait(false);
                    Logger?.Debug($"{ToString()}  Closed{msg}");
                    await this.OnChannelEvent(Stoped).ConfigureAwait(false);
                }
            }
            finally
            {
                _connectLock.Release();
            }
        }
    }

    /// <inheritdoc/>
    public override async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
    {
        if (!Online)
        {
            try
            {
                await _connectLock.WaitAsync(token).ConfigureAwait(false);
                if (!Online)
                {
                    //await SetupAsync(Config.Clone()).ConfigureAwait(false);
                    await base.ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false);
                    Logger?.Debug($"{ToString()}  Connected");
                    await this.OnChannelEvent(Started).ConfigureAwait(false);

                }
            }
            finally
            {
                _connectLock.Release();
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
        if (e.RequestInfo is MessageBase response)
        {
            if (ChannelReceivedWaitDict.TryRemove(response.Sign, out var func))
            {
                await func.Invoke(this, e, ChannelReceived.Count == 1).ConfigureAwait(false);
                e.Handled = true;
            }
        }
        if (e.Handled)
            return;


        await this.OnChannelReceivedEvent(e, ChannelReceived).ConfigureAwait(false);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        WaitHandlePool.SafeDispose();
        base.Dispose(disposing);
    }
}
