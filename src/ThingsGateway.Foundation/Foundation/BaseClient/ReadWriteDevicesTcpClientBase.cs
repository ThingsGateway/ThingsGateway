namespace ThingsGateway.Foundation
{
    /// <summary>
    /// TCP读写设备
    /// </summary>
    public abstract class ReadWriteDevicesTcpClientBase : ReadWriteDevicesClientBase
    {
        public ReadWriteDevicesTcpClientBase(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            TcpClient.Connecting += Connecting;
            TcpClient.Connected += Connected;
            TcpClient.Disconnecting += Disconnecting;
            TcpClient.Disconnected += Disconnected;
            Logger = TcpClient.Logger;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            TcpClient.Close();
            TcpClient.Connecting -= Connecting;
            TcpClient.Connected -= Connected;
            TcpClient.Disconnecting -= Disconnecting;
            TcpClient.Disconnected -= Disconnected;
            TcpClient.Dispose();
        }
        public TcpClient TcpClient { get; }

        public override Task ConnectAsync()
        {
            return TcpClient.ConnectAsync(ConnectTimeOut);
        }
        public override void Connect()
        {
            TcpClient.Connect(ConnectTimeOut);
        }
        public override void Disconnect()
        {
            TcpClient.Close();
        }

        public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            try
            {
                if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
                await ConnectAsync();
                ResponsedData result = await TcpClient.GetWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, token);
                return OperResult.CreateSuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex);
            }
        }

        public override OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            try
            {
                if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
                Connect();
                ResponsedData result = TcpClient.GetWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, token);
                return OperResult.CreateSuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex);
            }
        }

        public override string ToString()
        {
            return TcpClient.RemoteIPHost.ToString();
        }

        private void Connected(ITcpClient client, MsgEventArgs e)
        {
            Logger?.Debug(client.GetIPPort() + "连接成功");
        }

        private void Connecting(ITcpClient client, ConnectingEventArgs e)
        {
            Logger?.Debug(client.GetIPPort() + "正在连接");
            SetDataAdapter();
        }

        private void Disconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            Logger?.Debug(client.GetIPPort() + "断开连接-" + e.Message);
        }

        private void Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
        {
            Logger?.Debug(client.GetIPPort() + "正在主动断开连接-" + e.Message);
        }
    }
}