using System.Linq;

namespace ThingsGateway.Foundation
{

    public abstract class ReadWriteDevicesTcpServerBase : ReadWriteDevicesServerBase
    {
        public ReadWriteDevicesTcpServerBase(TcpService tcpService)
        {
            TcpService = tcpService;
            TcpService.Connecting += Connecting;
            TcpService.Connected += Connected;
            TcpService.Received += Received;
            TcpService.Disconnecting += Disconnecting;
            TcpService.Disconnected += Disconnected;
            Logger = TcpService.Logger;
        }

        public TcpService TcpService { get; }

        public override Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
        public override OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
        public override void Start()
        {
            TcpService.Start();
        }

        public override void Stop()
        {
            TcpService.Stop();
        }

        public override string ToString()
        {
            return TcpService.Monitors.Select(a => a.IPHost.ToString() + Environment.NewLine).ToJson();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            TcpService.Stop();
            TcpService.Dispose();
            TcpService.Connecting -= Connecting;
            TcpService.Connected -= Connected;
            TcpService.Disconnecting -= Disconnecting;
            TcpService.Disconnected -= Disconnected;
            TcpService.Dispose();

        }

        protected abstract Task Received(SocketClient client, IRequestInfo requestInfo);

        private void Connected(SocketClient client, TouchSocketEventArgs e)
        {
            Logger?.Debug(client.GetIPPort() + "连接成功");
        }

        private void Connecting(SocketClient client, OperationEventArgs e)
        {
            Logger?.Debug(client.GetIPPort() + "正在连接");
            SetDataAdapter(client);
        }

        private void Disconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            Logger?.Debug(client.GetIPPort() + "断开连接-" + e.Message);
        }

        private void Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
        {
            Logger?.Debug(client.GetIPPort() + "正在主动断开连接-" + e.Message);
        }

        private async void Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            try
            {
                await Received(client, requestInfo);
            }
            catch (Exception ex)
            {
                Logger.Exception(this, ex);
            }
        }
    }
}