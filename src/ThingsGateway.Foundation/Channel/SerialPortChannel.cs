
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
    public class SerialPortChannel : SerialPortClient, IClientChannel
    {

        //~SerialPortChannel()
        //{
        //    Dispose(false);
        //}


        private readonly EasyLock m_semaphoreForConnect = new EasyLock();

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
        public override string ToString()
        {
            if (ProtectedMainSerialPort != null)
                return $"{ProtectedMainSerialPort.PortName}[{ProtectedMainSerialPort.BaudRate},{ProtectedMainSerialPort.DataBits},{ProtectedMainSerialPort.StopBits},{ProtectedMainSerialPort.Parity}]";
            return base.ToString();
        }

        public void SetDataHandlingAdapter(DataHandlingAdapter adapter)
        {
            if (adapter is SingleStreamDataHandlingAdapter singleStreamDataHandlingAdapter)
                this.SetAdapter(singleStreamDataHandlingAdapter);
        }

        public void Close(string msg)
        {
            this.CloseAsync(msg).ConfigureAwait(false);
        }

        public void Connect(int millisecondsTimeout = 3000, CancellationToken token = default)
        {
            this.ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false);
        }
        public new async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            try
            {
                await this.m_semaphoreForConnect.WaitAsync(token).ConfigureAwait(false);
                if (!this.Online)
                {
                    await base.ConnectAsync(millisecondsTimeout, token).ConfigureAwait(false);
                    Logger?.Debug($"{ToString()}  Connected");
                    if (Started != null)
                        await Started.Invoke(this).ConfigureAwait(false);
                }
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        public override async Task CloseAsync(string msg)
        {
            if (this.Online)
            {
                await base.CloseAsync(msg).ConfigureAwait(false);
                Logger?.Debug($"{ToString()}  Closed{msg}");
                if (Stoped != null)
                    await Stoped.Invoke(this).ConfigureAwait(false);
            }
        }
        /// <inheritdoc/>
        protected override async Task OnSerialReceived(ReceivedDataEventArgs e)
        {
            if (this.ChannelReceived != null)
            {
                await this.ChannelReceived.Invoke(this, e).ConfigureAwait(false);
                if (e.Handled)
                {
                    return;
                }
            }
            await base.OnSerialReceived(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnSerialConnecting(ConnectingEventArgs e)
        {
            Logger?.Debug($"{ToString()}  Connecting{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
            if (Starting != null)
                await Starting.Invoke(this).ConfigureAwait(false);
            await base.OnSerialConnecting(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnSerialClosing(ClosingEventArgs e)
        {
            Logger?.Debug($"{ToString()} Closing{(e.Message.IsNullOrEmpty() ? string.Empty : $" -{e.Message}")}");
            await base.OnSerialClosing(e).ConfigureAwait(false);
        }


    }
}
