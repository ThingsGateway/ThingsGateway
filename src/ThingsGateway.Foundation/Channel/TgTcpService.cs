
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
    public class TgTcpServiceBase<TClient> : TcpService<TClient>, ITcpService<TClient> where TClient : TgSocketClient, new()
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
        protected override Task OnConnected(TClient socketClient, ConnectedEventArgs e)
        {
            Logger?.Debug($"{socketClient}  Connected");
            return base.OnConnected(socketClient, e);
        }

        /// <inheritdoc/>
        protected override Task OnConnecting(TClient socketClient, ConnectingEventArgs e)
        {
            Logger?.Debug($"{socketClient}  Connecting");
            return base.OnConnecting(socketClient, e);
        }

        /// <summary>
        /// 客户端断开连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override Task OnDisconnected(TClient socketClient, DisconnectEventArgs e)
        {
            Logger?.Debug($"{socketClient}  Disconnected");
            return base.OnDisconnected(socketClient, e);
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override Task OnDisconnecting(TClient socketClient, DisconnectEventArgs e)
        {
            Logger?.Debug($"{socketClient} Disconnecting");
            return base.OnDisconnecting(socketClient, e);
        }

        #endregion 事件

        /// <summary>
        /// 停止时是否发送ShutDown
        /// </summary>
        public bool ShutDownEnable { get; set; }

        private void ShutDown()
        {
            foreach (var item in GetClients())
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

        /// <inheritdoc/>
        public override void Clear()
        {
            ShutDown();
        }

        /// <inheritdoc/>
        public override void Start()
        {
            if (this.ServerState != ServerState.Running)
            {
                base.Start();
                if (this.ServerState == ServerState.Running)
                {
                    Logger.Info($"{Monitors.FirstOrDefault()?.Option.IpHost}{DefaultResource.Localizer["ServiceStarted"]}");
                }
            }
            else
            {
                base.Start();
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

        /// <inheritdoc/>
        public override void Stop()
        {
            if (Monitors.Count() > 0)
            {
                base.Stop();
                if (Monitors.Count() == 0)
                    Logger.Info($"{Monitors.FirstOrDefault()?.Option.IpHost}{DefaultResource.Localizer["ServiceStoped"]}");
            }
            else
            {
                base.Stop();
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
            base.Stop();
        }

        /// <inheritdoc/>
        public void Connect(int timeout, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            base.Start();
        }

        /// <inheritdoc/>
        protected override Task OnConnected(TgSocketClient socketClient, ConnectedEventArgs e)
        {
            if (Started != null)
                return Started.Invoke(socketClient);
            return base.OnConnected(socketClient, e);
        }

        /// <inheritdoc/>
        protected override Task OnConnecting(TgSocketClient socketClient, ConnectingEventArgs e)
        {
            if (Starting != null)
                return Starting.Invoke(socketClient);
            return base.OnConnecting(socketClient, e);
        }

        /// <inheritdoc/>
        protected override Task OnDisconnected(TgSocketClient socketClient, DisconnectEventArgs e)
        {
            if (Stoped != null)
                return Stoped.Invoke(socketClient);
            return base.OnDisconnected(socketClient, e);
        }

        /// <inheritdoc/>
        public Task ConnectAsync(int timeout, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return EasyTask.CompletedTask;
            return base.StartAsync();
        }

        /// <inheritdoc/>
        protected override async Task OnReceived(TgSocketClient socketClient, ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(socketClient, e).ConfigureAwait(false);
                if (e.Handled)
                {
                    return;
                }
            }
            await base.OnReceived(socketClient, e).ConfigureAwait(false);
        }
    }
}
