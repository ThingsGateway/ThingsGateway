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
/// Udp通道
/// </summary>
public class UdpSessionChannel : UdpSession, IClientChannel
{
    private readonly WaitLock m_semaphoreForConnect = new WaitLock();

    /// <inheritdoc/>
    public UdpSessionChannel()
    {
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <inheritdoc/>
    public ChannelReceivedEventHandler ChannelReceived { get; set; } = new();

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.UdpSession;

    /// <inheritdoc/>
    public ConcurrentList<IProtocol> Collects { get; } = new();

    /// <inheritdoc/>
    public bool Online => ServerState == ServerState.Running;

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
    public Task CloseAsync(string msg)
    {
        return StopAsync();
    }

    /// <inheritdoc/>
    public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default)
    {
        ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(int timeout = 3000, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return;
        await this.OnChannelEvent(Starting).ConfigureAwait(false);
        await StartAsync().ConfigureAwait(false);
        await this.OnChannelEvent(Started).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
    {
        if (adapter is UdpDataHandlingAdapter udpDataHandlingAdapter)
            SetAdapter(udpDataHandlingAdapter);
    }

    /// <inheritdoc/>
    public override async Task StartAsync()
    {
        try
        {
            await m_semaphoreForConnect.WaitAsync().ConfigureAwait(false);

            if (ServerState != ServerState.Running)
            {
                await base.StopAsync().ConfigureAwait(false);
                await SetupAsync(Config.Clone()).ConfigureAwait(false);
                await base.StartAsync().ConfigureAwait(false);
                if (ServerState == ServerState.Running)
                {
                    Logger?.Info($"{Monitor.IPHost}{DefaultResource.Localizer["ServiceStarted"]}");
                }
            }
            else
            {
                await base.StartAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            m_semaphoreForConnect.Release();
        }
    }

    /// <inheritdoc/>
    public override async Task StopAsync()
    {
        if (Monitor != null)
        {
            await this.OnChannelEvent(Stoping).ConfigureAwait(false);
            await base.StopAsync().ConfigureAwait(false);
            if (Monitor == null)
            {
                await this.OnChannelEvent(Stoped).ConfigureAwait(false);
                Logger?.Info($"{DefaultResource.Localizer["ServiceStoped"]}");
            }
        }
        else
        {
            await base.StopAsync().ConfigureAwait(false);
        }

    }


    /// <inheritdoc/>
    public override string? ToString()
    {
        return RemoteIPHost?.ToString().Replace("tcp", "udp");
    }

    /// <inheritdoc/>
    protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        await base.OnUdpReceived(e).ConfigureAwait(false);
        await this.OnChannelReceivedEvent(e, ChannelReceived).ConfigureAwait(false);

    }
}
