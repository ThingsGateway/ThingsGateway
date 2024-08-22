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
    private readonly EasyLock m_semaphoreForConnect = new EasyLock();

    public UdpSessionChannel()
    {
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <summary>
    /// 当收到数据时
    /// </summary>
    public ChannelReceivedEventHandler ChannelReceived { get; set; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.UdpSession;

    /// <inheritdoc/>
    public ConcurrentList<IProtocol> Collects { get; } = new();

    /// <inheritdoc/>
    public bool Online => ServerState == ServerState.Running;

    public DataHandlingAdapter ReadOnlyDataHandlingAdapter => DataHandlingAdapter;

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; set; }

    /// <summary>
    /// 等待池
    /// </summary>
    public WaitHandlePool<MessageBase> WaitHandlePool { get; } = new();

    /// <inheritdoc/>
    public EasyLock WaitLock { get; } = new EasyLock();

    public void Close(string msg)
    {
        CloseAsync(msg).GetFalseAwaitResult();
    }

    public Task CloseAsync(string msg)
    {
        return StopAsync();
    }

    public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default)
    {
        ConnectAsync(millisecondsTimeout, token).GetFalseAwaitResult();
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(int timeout = 3000, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return;
        if (Starting != null)
            await Starting.Invoke(this).ConfigureAwait(false);
        await StartAsync().ConfigureAwait(false);
        if (Started != null)
            await Started.Invoke(this).ConfigureAwait(false);
    }

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
                    Logger.Info($"{Monitor.IPHost}{DefaultResource.Localizer["ServiceStarted"]}");
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
    public override string? ToString()
    {
        return RemoteIPHost?.ToString().Replace("tcp", "udp");
    }

    /// <inheritdoc/>
    protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
    {
        if (ChannelReceived != null)
        {
            await ChannelReceived.Invoke(this, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }
        }
        await base.OnUdpReceived(e).ConfigureAwait(false);
    }
}
