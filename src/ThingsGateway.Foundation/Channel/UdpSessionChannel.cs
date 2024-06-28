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

/// <inheritdoc/>
public class UdpSessionChannel : UdpSession, IClientChannel
{
    public UdpSessionChannel()
    {
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <inheritdoc/>
    public EasyLock WaitLock { get; } = new EasyLock();

    public DataHandlingAdapter ReadOnlyDataHandlingAdapter => DataHandlingAdapter;

    /// <summary>
    /// 等待池
    /// </summary>
    public WaitHandlePool<MessageBase> WaitHandlePool { get; } = new();

    /// <summary>
    /// 当收到数据时
    /// </summary>
    public ChannelReceivedEventHandler ChannelReceived { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; set; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.UdpSession;

    /// <inheritdoc/>
    public bool Online => ServerState == ServerState.Running;

    /// <inheritdoc/>
    public ConcurrentList<IProtocol> Collects { get; } = new();

    /// <inheritdoc/>
    public override string? ToString()
    {
        return RemoteIPHost?.ToString().Replace("tcp", "udp");
    }

    /// <inheritdoc/>
    public override async Task StartAsync()
    {
        if (this.ServerState != ServerState.Running)
        {
            await base.StopAsync().ConfigureAwait(false);
            await base.StartAsync().ConfigureAwait(false);
            if (this.ServerState == ServerState.Running)
            {
                Logger.Info($"{Monitor.IPHost}{DefaultResource.Localizer["ServiceStarted"]}");
            }
        }
        else
        {
            await base.StartAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override async Task StopAsync()
    {
        if (Monitor != null)
        {
            await base.StopAsync().ConfigureAwait(false);
            if (Monitor == null)
                Logger.Info($"{DefaultResource.Localizer["ServiceStoped"]}");
        }
        else
        {
            await base.StopAsync().ConfigureAwait(false);
        }
        if (Stoped != null)
            await Stoped.Invoke(this).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(int timeout = 3000, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return;
        if (Starting != null)
            await Starting.Invoke(this);
        await StartAsync().ConfigureAwait(false);
        if (Started != null)
            await Started.Invoke(this).ConfigureAwait(false);
    }

    public Task CloseAsync(string msg)
    {
        return this.StopAsync();
    }

    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is UdpDataHandlingAdapter udpDataHandlingAdapter)
            this.SetAdapter(udpDataHandlingAdapter);
    }

    public void Close(string msg)
    {
        this.CloseAsync(msg).GetFalseAwaitResult();
    }

    public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default)
    {
        this.ConnectAsync(millisecondsTimeout, token).GetFalseAwaitResult();
    }

    /// <inheritdoc/>
    protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        if (this.ChannelReceived != null)
        {
            await this.ChannelReceived.Invoke(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }
        }
        await base.OnUdpReceived(e).ConfigureAwait(false);
    }
}
