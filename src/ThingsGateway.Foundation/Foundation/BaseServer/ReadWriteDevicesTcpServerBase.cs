using System.Linq;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 服务设备
    /// </summary>
    public abstract class ReadWriteDevicesTcpServerBase : ReadWriteDevicesServerBase
    {
        /// <inheritdoc cref="ReadWriteDevicesTcpServerBase"/>
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
        /// <summary>
        /// 服务管理对象
        /// </summary>
        public TcpService TcpService { get; }
        /// <inheritdoc/>
        public override Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public override OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
        /// <inheritdoc/>
        public override void Start()
        {
            TcpService.Start();
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            TcpService.Stop();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return TcpService.Monitors.Select(a => a.IPHost.ToString() + Environment.NewLine).ToJson();
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// 接收解析
        /// </summary>
        protected abstract Task ReceivedAsync(SocketClient client, IRequestInfo requestInfo);

        private void Connected(SocketClient client, TouchSocketEventArgs e)
        {
            Logger?.Debug(client.IP + ":" + client.Port + "连接成功");
        }

        private void Connecting(SocketClient client, OperationEventArgs e)
        {
            Logger?.Debug(client.IP + ":" + client.Port + "正在连接");
            SetDataAdapter(client);
        }

        private void Disconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            Logger?.Debug(client.IP + ":" + client.Port + "断开连接-" + e.Message);
        }

        private void Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
        {
            Logger?.Debug(client.IP + ":" + client.Port + "正在主动断开连接-" + e.Message);
        }

        private async void Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            try
            {
                await ReceivedAsync(client, requestInfo);
            }
            catch (Exception ex)
            {
                Logger.Exception(this, ex);
            }
        }
    }
}