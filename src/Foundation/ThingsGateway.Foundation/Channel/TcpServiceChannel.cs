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
/// TCP服务器
/// </summary>
/// <typeparam name="TClient"></typeparam>
public abstract class TcpServiceChannelBase<TClient> : TcpService<TClient>, ITcpService<TClient> where TClient : TcpSessionClientChannel, new()
{
    private readonly WaitLock m_semaphoreForConnect = new WaitLock();

    /// <inheritdoc/>
    public ConcurrentList<IProtocol> Collects { get; } = new();

    /// <summary>
    /// 停止时是否发送ShutDown
    /// </summary>
    public bool ShutDownEnable { get; set; } = true;

    /// <inheritdoc/>
    public override async Task ClearAsync()
    {
        foreach (var client in Clients)
        {
            try
            {
                if (ShutDownEnable)
                    client.TryShutdown();
                await client.CloseAsync().ConfigureAwait(false);
                client.SafeDispose();
            }
            catch
            {
            }
        }
    }
    public async Task ClientDisposeAsync(string id)
    {
        if (this.TryGetClient(id, out var client))
        {
            if (ShutDownEnable)
                client.TryShutdown();
            await client.CloseAsync().ConfigureAwait(false);
            client.SafeDispose();
        }
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
                    Logger?.Info($"{Monitors.FirstOrDefault()?.Option.IpHost}{DefaultResource.Localizer["ServiceStarted"]}");
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
        if (Monitors.Any())
        {
            await ClearAsync().ConfigureAwait(false);
            var iPHost = Monitors.FirstOrDefault()?.Option.IpHost;
            await base.StopAsync().ConfigureAwait(false);
            if (!Monitors.Any())
                Logger?.Info($"{iPHost}{DefaultResource.Localizer["ServiceStoped"]}");
        }
        else
        {
            await base.StopAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        return Monitors.FirstOrDefault()?.Option?.IpHost.ToString();
    }

    /// <inheritdoc/>
    protected override Task OnTcpClosed(TClient socketClient, ClosedEventArgs e)
    {
        Logger?.Debug($"{socketClient}  Closed");
        return base.OnTcpClosed(socketClient, e);
    }

    /// <inheritdoc/>
    protected override Task OnTcpClosing(TClient socketClient, ClosingEventArgs e)
    {
        Logger?.Debug($"{socketClient} Closing");
        return base.OnTcpClosing(socketClient, e);
    }

    /// <inheritdoc/>
    protected override Task OnTcpConnected(TClient socketClient, ConnectedEventArgs e)
    {
        Logger?.Debug($"{socketClient}  Connected");
        return base.OnTcpConnected(socketClient, e);
    }

    /// <inheritdoc/>
    protected override Task OnTcpConnecting(TClient socketClient, ConnectingEventArgs e)
    {
        Logger?.Debug($"{socketClient}  Connecting");
        return base.OnTcpConnecting(socketClient, e);
    }
}

/// <summary>
/// Tcp服务器通道
/// </summary>
public class TcpServiceChannel : TcpServiceChannelBase<TcpSessionClientChannel>, IChannel
{
    /// <inheritdoc/>
    public ChannelReceivedEventHandler ChannelReceived { get; set; } = new();

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpService;

    /// <inheritdoc/>
    public bool Online => ServerState == ServerState.Running;

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; set; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; set; } = new();

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; set; } = new();
    /// <inheritdoc/>
    public ChannelEventHandler Stoping { get; set; } = new();

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
    public Task ConnectAsync(int timeout = 3000, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return EasyTask.CompletedTask;

        return StartAsync();
    }

    /// <inheritdoc/>
    protected override TcpSessionClientChannel NewClient()
    {
        return new TcpSessionClientChannel();
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosing(TcpSessionClientChannel socketClient, ClosingEventArgs e)
    {
        await socketClient.OnChannelEvent(Stoping).ConfigureAwait(false);
        await base.OnTcpClosing(socketClient, e);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(TcpSessionClientChannel socketClient, ClosedEventArgs e)
    {
        await socketClient.OnChannelEvent(Stoped).ConfigureAwait(false);
        await base.OnTcpClosed(socketClient, e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnected(TcpSessionClientChannel socketClient, ConnectedEventArgs e)
    {
        await socketClient.OnChannelEvent(Started).ConfigureAwait(false);
        await base.OnTcpConnected(socketClient, e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(TcpSessionClientChannel socketClient, ConnectingEventArgs e)
    {
        await socketClient.OnChannelEvent(Starting).ConfigureAwait(false);
        await base.OnTcpConnecting(socketClient, e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(TcpSessionClientChannel socketClient, ReceivedDataEventArgs e)
    {
        await base.OnTcpReceived(socketClient, e).ConfigureAwait(false);
        await socketClient.OnChannelReceivedEvent(e, ChannelReceived).ConfigureAwait(false);

    }
}
