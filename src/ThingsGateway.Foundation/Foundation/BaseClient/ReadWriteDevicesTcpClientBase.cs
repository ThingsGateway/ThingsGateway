namespace ThingsGateway.Foundation
{
    /// <summary>
    /// TCP读写设备
    /// </summary>
    public abstract class ReadWriteDevicesTcpClientBase : ReadWriteDevicesClientBase
    {
        /// <inheritdoc cref="ReadWriteDevicesTcpClientBase"/>
        public ReadWriteDevicesTcpClientBase(TGTcpClient tcpClient)
        {
            TGTcpClient = tcpClient;
            TGTcpClient.Connecting += Connecting;
            TGTcpClient.Connected += Connected;
            TGTcpClient.Disconnecting += Disconnecting;
            TGTcpClient.Disconnected += Disconnected;
            Logger = TGTcpClient.Logger;
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            TGTcpClient.Close();
            TGTcpClient.Connecting -= Connecting;
            TGTcpClient.Connected -= Connected;
            TGTcpClient.Disconnecting -= Disconnecting;
            TGTcpClient.Disconnected -= Disconnected;
            TGTcpClient.Dispose();
        }

        /// <summary>
        /// Socket管理对象
        /// </summary>
        public TGTcpClient TGTcpClient { get; }

        /// <inheritdoc/>
        public override Task ConnectAsync()
        {
            return TGTcpClient.ConnectAsync(ConnectTimeOut);
        }
        /// <inheritdoc/>
        public override void Connect()
        {
            TGTcpClient.Connect(ConnectTimeOut);
        }
        /// <inheritdoc/>
        public override void Disconnect()
        {
            TGTcpClient.Close();
        }

        /// <inheritdoc/>
        public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            try
            {
                if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
                await ConnectAsync();
                ResponsedData result = await TGTcpClient.GetTGWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, token);
                return OperResult.CreateSuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex);
            }
        }

        /// <inheritdoc/>
        public override OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            try
            {
                if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
                Connect();
                ResponsedData result = TGTcpClient.GetTGWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, token);
                return OperResult.CreateSuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return TGTcpClient.RemoteIPHost.ToString();
        }

        private void Connected(ITcpClient client, MsgEventArgs e)
        {
            Logger?.Debug(client.RemoteIPHost.ToString() + "连接成功");
        }

        private void Connecting(ITcpClient client, ConnectingEventArgs e)
        {
            Logger?.Debug(client.RemoteIPHost.ToString() + "正在连接");
            SetDataAdapter();
        }

        private void Disconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            Logger?.Debug(client.IP + ":" + client.Port + "断开连接-" + e.Message);
        }

        private void Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
        {
            Logger?.Debug(client.IP + ":" + client.Port + "正在主动断开连接-" + e.Message);
        }
    }
}