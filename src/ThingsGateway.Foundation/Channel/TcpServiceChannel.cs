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
public abstract class TcpServiceChannelBase<TClient> : TcpService<TClient>, ITcpService<TClient> where TClient : TcpSessionClientChannel, new()
{
    /// <inheritdoc/>
    public ConcurrentList<IProtocol> Collects { get; } = new();

    private readonly EasyLock m_semaphoreForConnect = new EasyLock();

    /// <summary>
    /// 停止时是否发送ShutDown
    /// </summary>
    public bool ShutDownEnable { get; set; }

    /// <inheritdoc/>
    public override async Task ClearAsync()
    {
        foreach (var id in this.GetIds())
        {
            if (this.TryGetClient(id, out var client))
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
    }

    public async Task ClientDispose(string id)
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
            await this.m_semaphoreForConnect.WaitAsync().ConfigureAwait(false);

            if (this.ServerState != ServerState.Running)
            {
                await base.StopAsync().ConfigureAwait(false);
                await base.StartAsync().ConfigureAwait(false);
                if (this.ServerState == ServerState.Running)
                {
                    Logger.Info($"{Monitors.FirstOrDefault()?.Option.IpHost}{DefaultResource.Localizer["ServiceStarted"]}");
                }
            }
            else
            {
                await base.StartAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            this.m_semaphoreForConnect.Release();
        }
    }

    public override string ToString()
    {
        return Monitors.FirstOrDefault()?.Option?.IpHost.ToString();
    }

    /// <inheritdoc/>
    public override async Task StopAsync()
    {
        if (Monitors.Any())
        {
            await ClearAsync();
            var iPHost = Monitors.FirstOrDefault()?.Option.IpHost;
            await base.StopAsync().ConfigureAwait(false);
            if (!Monitors.Any())
                Logger.Info($"{iPHost}{DefaultResource.Localizer["ServiceStoped"]}");
        }
        else
        {
            await base.StopAsync().ConfigureAwait(false);
        }
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

    protected override Task OnTcpClosed(TClient socketClient, ClosedEventArgs e)
    {
        Logger?.Debug($"{socketClient}  Closed");
        return base.OnTcpClosed(socketClient, e);
    }

    protected override Task OnTcpClosing(TClient socketClient, ClosingEventArgs e)
    {
        Logger?.Debug($"{socketClient} Closing");
        return base.OnTcpClosing(socketClient, e);
    }
}

/// <summary>
/// Tcp服务器
/// </summary>
public class TcpServiceChannel : TcpServiceChannelBase<TcpSessionClientChannel>, IChannel
{
    /// <summary>
    /// 处理数据
    /// </summary>
    public ChannelReceivedEventHandler ChannelReceived { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Started { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Stoped { get; set; }

    /// <inheritdoc/>
    public ChannelEventHandler Starting { get; set; }

    /// <inheritdoc/>
    public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpService;

    /// <inheritdoc/>
    public bool Online => ServerState == ServerState.Running;

    /// <inheritdoc/>
    public Task ConnectAsync(int timeout = 3000, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return EasyTask.CompletedTask;

        return this.StartAsync();
    }

    public Task CloseAsync(string msg)
    {
        return this.StopAsync();
    }

    public void Close(string msg)
    {
        this.CloseAsync(msg).ConfigureAwait(false);
    }

    public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default)
    {
        this.ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnected(TcpSessionClientChannel socketClient, ConnectedEventArgs e)
    {
        if (Started != null)
            await Started.Invoke(socketClient).ConfigureAwait(false);
        await base.OnTcpConnected(socketClient, e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpConnecting(TcpSessionClientChannel socketClient, ConnectingEventArgs e)
    {
        if (Starting != null)
            await Starting.Invoke(socketClient).ConfigureAwait(false);
        await base.OnTcpConnecting(socketClient, e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpClosed(TcpSessionClientChannel socketClient, ClosedEventArgs e)
    {
        if (Stoped != null)
            await Stoped.Invoke(socketClient).ConfigureAwait(false);
        await base.OnTcpClosed(socketClient, e).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override async Task OnTcpReceived(TcpSessionClientChannel socketClient, ReceivedDataEventArgs e)
    {
        if (this.ChannelReceived != null)
        {
            await this.ChannelReceived.Invoke(socketClient, e).ConfigureAwait(false);
            if (e.Handled)
            {
                return;
            }
        }
        await base.OnTcpReceived(socketClient, e).ConfigureAwait(false);
    }

    protected override TcpSessionClientChannel NewClient()
    {
        return new TcpSessionClientChannel();
    }
}
