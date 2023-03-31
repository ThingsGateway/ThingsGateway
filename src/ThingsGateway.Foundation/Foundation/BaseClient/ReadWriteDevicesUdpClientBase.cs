namespace ThingsGateway.Foundation
{
    /// <summary>
    /// UDP读写设备
    /// </summary>
    public abstract class ReadWriteDevicesUdpClientBase : ReadWriteDevicesClientBase
    {
        /// <inheritdoc cref="ReadWriteDevicesUdpClientBase"/>
        public ReadWriteDevicesUdpClientBase(UdpSession udpSession)
        {
            UdpSession = udpSession;
            SetDataAdapter();
        }
        /// <summary>
        /// Socket管理对象
        /// </summary>
        public UdpSession UdpSession { get; }

        /// <inheritdoc/>
        public override Task ConnectAsync()
        {
            return Task.FromResult(UdpSession.Start());
        }
        /// <inheritdoc/>
        public override void Connect()
        {
            UdpSession.Start();
        }
        /// <inheritdoc/>
        public override void Disconnect()
        {
            UdpSession.Stop();
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            UdpSession.Stop();
            UdpSession.Dispose();
        }
        /// <inheritdoc/>
        public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            try
            {
                if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
                ResponsedData result = await UdpSession.GetTGWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, CancellationToken.None);
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
                ResponsedData result = UdpSession.GetTGWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, CancellationToken.None);
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
            return UdpSession.RemoteIPHost.ToString();
        }
    }
}