namespace ThingsGateway.Foundation
{
    public abstract class ReadWriteDevicesUdpClientBase : ReadWriteDevicesClientBase
    {
        public ReadWriteDevicesUdpClientBase(UdpSession udpSession)
        {
            UdpSession = udpSession;
            SetDataAdapter();
        }

        public UdpSession UdpSession { get; }

        public override Task ConnectAsync()
        {
            return Task.FromResult(UdpSession.Start());
        }
        public override void Connect()
        {
            UdpSession.Start();
        }
        public override void Disconnect()
        {
            UdpSession.Stop();
        }

        public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            try
            {
                if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
                ResponsedData result = await UdpSession.GetWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, CancellationToken.None);
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
                ResponsedData result = UdpSession.GetWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, CancellationToken.None);
                return OperResult.CreateSuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex);
            }
        }

        public override string ToString()
        {
            return UdpSession.RemoteIPHost.ToString();
        }
    }
}