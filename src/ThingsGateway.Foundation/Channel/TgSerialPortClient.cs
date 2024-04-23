
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation
{
    /// <inheritdoc cref="SerialPortClientBase"/>
    public class TgSerialPortClient : SerialPortClientBase, IClientChannel
    {
        /// <summary>
        /// Tcp客户端
        /// </summary>
        ~TgSerialPortClient()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public EasyLock WaitLock { get; } = new EasyLock();

        /// <inheritdoc/>
        public ConcurrentList<IProtocol> Collects { get; } = new();

        /// <summary>
        /// 接收到数据
        /// </summary>
        public TgReceivedEventHandler Received { get; set; }

        /// <inheritdoc/>
        public ChannelTypeEnum ChannelType => ChannelTypeEnum.SerialPortClient;

        /// <inheritdoc/>
        DataHandlingAdapter IClientChannel.DataHandlingAdapter => DataHandlingAdapter;

        /// <inheritdoc/>
        public ChannelEventHandler Started { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Stoped { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Starting { get; set; }

        /// <inheritdoc/>
        public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is SingleStreamDataHandlingAdapter single)
                base.SetDataHandlingAdapter(single);
            else
                throw new NotSupportedException(DefaultResource.Localizer["AdapterTypeError", nameof(SingleStreamDataHandlingAdapter)]);
        }

        /// <inheritdoc/>
        protected override  Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(this, e);
            }
            return base.ReceivedData(e);
        }

        /// <inheritdoc/>
        protected override Task OnConnected(ConnectedEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Connected");
            if (Started != null)
                return Started.Invoke(this);
            return base.OnConnected(e);
        }

        /// <inheritdoc/>
        protected override  Task OnConnecting(SerialConnectingEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Connecting{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
            if (Starting != null)
                return Starting.Invoke(this);
            return base.OnConnecting(e);
        }

        /// <inheritdoc/>
        protected override Task OnDisconnecting(DisconnectEventArgs e)
        {
            Logger?.Debug($"{ToString()} Disconnecting{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
            return base.OnDisconnecting(e);
        }

        /// <inheritdoc/>
        protected override Task OnDisconnected(DisconnectEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Disconnected{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            if (Stoped != null)
                return Stoped.Invoke(this);
            return base.OnDisconnected(e);
        }
    }
}
