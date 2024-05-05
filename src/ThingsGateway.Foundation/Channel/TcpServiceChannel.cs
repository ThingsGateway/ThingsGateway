
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Net.Sockets;

namespace ThingsGateway.Foundation
{
    /// <inheritdoc/>
    public abstract class TcpServiceChannelBase<TClient> : TcpService<TClient>, ITcpService<TClient> where TClient : SocketClientChannel, new()
    {
        ///// <inheritdoc/>
        //~TcpServiceChannelBase()
        //{
        //    Dispose(true);
        //}

        /// <inheritdoc/>
        public ConcurrentList<IProtocol> Collects { get; } = new();


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
                    if (ShutDownEnable)
                        client.MainSocket?.Shutdown(SocketShutdown.Both);

                    await client.CloseAsync();
                    client.SafeDispose();
                }
            }
        }

        /// <inheritdoc/>
        public override async Task StartAsync()
        {
            if (this.ServerState != ServerState.Running)
            {
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
        public override string ToString()
        {
            return Monitors.FirstOrDefault()?.Option?.IpHost.ToString();
        }

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            if (Monitors.Any())
            {
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
    public class TcpServiceChannel : TcpServiceChannelBase<SocketClientChannel>, IChannel
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
        protected override async Task OnTcpConnected(SocketClientChannel socketClient, ConnectedEventArgs e)
        {
            if (Started != null)
                await Started.Invoke(socketClient).ConfigureAwait(false);
            await base.OnTcpConnected(socketClient, e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpConnecting(SocketClientChannel socketClient, ConnectingEventArgs e)
        {
            if (Starting != null)
                await Starting.Invoke(socketClient).ConfigureAwait(false);
            await base.OnTcpConnecting(socketClient, e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpClosed(SocketClientChannel socketClient, ClosedEventArgs e)
        {
            if (Stoped != null)
                await Stoped.Invoke(socketClient).ConfigureAwait(false);
            await base.OnTcpClosed(socketClient, e).ConfigureAwait(false);
        }
        /// <inheritdoc/>
        protected override async Task OnTcpReceived(SocketClientChannel socketClient, ReceivedDataEventArgs e)
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

        protected override SocketClientChannel NewClient()
        {
            return new SocketClientChannel();
        }
    }
}
