
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using TouchSocket.SerialPorts;

namespace ThingsGateway.Foundation
{
    /// <inheritdoc cref="SerialPortClient"/>
    public class SerialPortChannel : SerialPortClient, IClientChannel, IDefaultSender
    {

        //~SerialPortChannel()
        //{
        //    Dispose(false);
        //}


        /// <inheritdoc/>
        public EasyLock WaitLock { get; } = new EasyLock();

        /// <inheritdoc/>
        public ConcurrentList<IProtocol> Collects { get; } = new();

        /// <summary>
        /// 接收到数据
        /// </summary>
        public ChannelReceivedEventHandler ChannelReceived { get; set; }

        /// <inheritdoc/>
        public ChannelTypeEnum ChannelType => ChannelTypeEnum.SerialPort;

        /// <inheritdoc/>
        public ChannelEventHandler Started { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Stoped { get; set; }

        /// <inheritdoc/>
        public ChannelEventHandler Starting { get; set; }


        /// <inheritdoc/>
        protected override Task OnSerialReceived(ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(this, e);
            }
            return base.OnSerialReceived(e);
        }

        /// <inheritdoc/>
        protected override Task OnSerialConnected(ConnectedEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Connected");
            if (Started != null)
                return Started.Invoke(this);
            return base.OnSerialConnected(e);
        }

        /// <inheritdoc/>
        protected override Task OnSerialConnecting(ConnectingEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Connecting{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
            if (Starting != null)
                return Starting.Invoke(this);
            return base.OnSerialConnecting(e);
        }

        /// <inheritdoc/>
        protected override Task OnSerialClosing(ClosingEventArgs e)
        {
            Logger?.Debug($"{ToString()} Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
            return base.OnSerialClosing(e);
        }

        /// <inheritdoc/>
        protected override Task OnSerialClosed(ClosedEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Closed{(e.Message.IsNullOrEmpty() ? string.Empty : $"-{e.Message}")}");
            if (Stoped != null)
                return Stoped.Invoke(this);
            return base.OnSerialClosed(e);
        }

        public override string ToString()
        {
            if (ProtectedMainSerialPort != null)
                return $"{ProtectedMainSerialPort.PortName}[{ProtectedMainSerialPort.BaudRate},{ProtectedMainSerialPort.DataBits},{ProtectedMainSerialPort.StopBits},{ProtectedMainSerialPort.Parity}]";
            return base.ToString();
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
