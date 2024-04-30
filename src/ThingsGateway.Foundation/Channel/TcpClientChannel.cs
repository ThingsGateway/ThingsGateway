
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 简单Tcp客户端
    /// </summary>
    public class TcpClientChannel : TcpClient, IClientChannel,IDefaultSender
    {
        /// <inheritdoc/>
        public ConcurrentList<IProtocol> Collects { get; } = new();

        /// <inheritdoc/>
        public EasyLock WaitLock { get; } = new EasyLock();

        /// <summary>
        /// 接收到数据
        /// </summary>
        public ChannelReceivedEventHandler ChannelReceived { get; set; }

        /// <inheritdoc/>
        public ChannelTypeEnum ChannelType => ChannelTypeEnum.TcpClient;

        /// <inheritdoc/>
        public ChannelEventHandler Started { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Stoped { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Starting { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{IP}:{Port}";
        }
        /// <inheritdoc/>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (this.ChannelReceived != null)
            {
                await this.ChannelReceived.Invoke(this, e).ConfigureAwait(false);
                if (e.Handled)
                {
                    return;
                }
            }
             await base.OnTcpReceived(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnected(ConnectedEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Connected");
            if (Started != null)
                return Started.Invoke(this);
            return base.OnTcpConnected(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnecting(ConnectingEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Connecting{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            if (Starting != null)
                return Starting.Invoke(this);
            return base.OnTcpConnecting(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpClosing(ClosingEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Disconnecting{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
            return base.OnTcpClosing(e);
        }

        /// <inheritdoc/>
        protected override Task OnTcpClosed(ClosedEventArgs e)
        {
            Logger?.Debug($"{ToString()}   Disconnected{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            if (Stoped != null)
                return Stoped.Invoke(this);
            return base.OnTcpClosed(e);
        }

        public void DefaultSend(byte[] buffer, int offset, int length)
        {
            this.ProtectedDefaultSend(buffer, offset, length);
        }


        public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
                this.SetAdapter(singleStreamDataHandlingAdapter);
        }
    }
}
