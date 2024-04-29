
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
    public abstract class TgTcpServiceBase<TClient> : TcpService<TClient>, ITcpService<TClient> where TClient : TgSocketClient, new()
    {
        /// <inheritdoc/>
        ~TgTcpServiceBase()
        {
            Dispose(true);
        }

        /// <inheritdoc/>
        public ConcurrentList<IProtocol> Collects { get; } = new();

        #region 事件

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
            Logger?.Debug($"{socketClient}  Disconnected");
            return base.OnTcpClosed(socketClient, e);
        }

        protected override Task OnTcpClosing(TClient socketClient, ClosingEventArgs e)
        {
            Logger?.Debug($"{socketClient} Disconnecting");
            return base.OnTcpClosing(socketClient, e);
        }

        #endregion 事件

        /// <summary>
        /// 停止时是否发送ShutDown
        /// </summary>
        public bool ShutDownEnable { get; set; }

        private void ShutDown()
        {
            foreach (var item in Clients)
            {
                try
                {
                    if (ShutDownEnable)
                        item.MainSocket?.Shutdown(SocketShutdown.Both);
                    item.SafeDispose();
                }
                catch
                {
                }
            }
        }

        public override async Task ClearAsync()
        {
            ShutDown();
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

        /// <inheritdoc/>
        public override async Task StopAsync()
        {
            if (Monitors.Count() > 0)
            {
                await base.StopAsync().ConfigureAwait(false);
                if (Monitors.Count() == 0)
                    Logger.Info($"{Monitors.FirstOrDefault()?.Option.IpHost}{DefaultResource.Localizer["ServiceStoped"]}");
            }
            else
            {
                await base.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            var count = Monitors.Count();
            base.Dispose(disposing);
            if (count > 0)
            {
                if (Monitors.Count() == 0)
                    Logger.Info($"{this}{DefaultResource.Localizer["ServiceStoped"]}");
            }
        }
    }

    /// <summary>
    /// Tcp服务器
    /// </summary>
    public class TgTcpService : TgTcpServiceBase<TgSocketClient>, IChannel
    {
        /// <summary>
        /// 处理数据
        /// </summary>
        public TgReceivedEventHandler Received { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Started { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Stoped { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Starting { get; set; }

        /// <inheritdoc/>
        public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpService;

        /// <inheritdoc/>
        public bool CanSend => ServerState == ServerState.Running;

        /// <inheritdoc/>
        public void Close(string msg)
        {
            this.Stop();
        }

        /// <inheritdoc/>
        public void Connect(int timeout, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            this.Start();
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnected(TgSocketClient socketClient, ConnectedEventArgs e)
        {
            if (Started != null)
                return Started.Invoke(socketClient);
            return base.OnTcpConnected(socketClient, e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnecting(TgSocketClient socketClient, ConnectingEventArgs e)
        {
            if (Starting != null)
                return Starting.Invoke(socketClient);
            return base.OnTcpConnecting(socketClient, e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpClosed(TgSocketClient socketClient, ClosedEventArgs e)
        {
            if (Stoped != null)
                return Stoped.Invoke(socketClient);
            return base.OnTcpClosed(socketClient, e);
        }

        /// <inheritdoc/>
        public Task ConnectAsync(int timeout, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return EasyTask.CompletedTask;
            return base.StartAsync();
        }

        /// <inheritdoc/>
        protected override async Task OnTcpReceived(TgSocketClient socketClient, ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(socketClient, e).ConfigureAwait(false);
                if (e.Handled)
                {
                    return;
                }
            }
            await base.OnTcpReceived(socketClient, e).ConfigureAwait(false);
        }

        protected override TgSocketClient NewClient()
        {
            return new TgSocketClient();
        }
    }
}
