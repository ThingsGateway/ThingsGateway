using ThingsGateway.Foundation.Serial;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// TCP读写设备
    /// </summary>
    public abstract class ReadWriteDevicesSerialClientBase : ReadWriteDevicesSerialBase
    {
        public ReadWriteDevicesSerialClientBase(SerialClient serialClient)
        {
            SerialClient = serialClient;
            SerialClient.Opening += Opening;
            SerialClient.Opened += Opened;
            SerialClient.Closing += Closing;
            SerialClient.Closed += Closed;
            Logger = SerialClient.Logger;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            SerialClient.Close();
            SerialClient.Opening -= Opening;
            SerialClient.Opened -= Opened;
            SerialClient.Closing -= Closing;
            SerialClient.Closed -= Closed;
        }
        public SerialClient SerialClient { get; }

        public override Task OpenAsync()
        {
            return SerialClient.OpenAsync();
        }
        public override void Open()
        {
            SerialClient.Open();
        }
        public override void Close()
        {
            SerialClient.Close();
        }

        public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            try
            {
                if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
                await OpenAsync();
                ResponsedData result = await SerialClient.GetWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, token);
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
                Open();
                ResponsedData result = SerialClient.GetWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, token);
                return OperResult.CreateSuccessResult(result.Data);
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex);
            }
        }

        public override string ToString()
        {
            return SerialClient.SerialProperty.ToString();
        }

        private void Opened(ISerialClient client, MsgEventArgs e)
        {
            Logger?.Debug(client.SerialProperty.ToString() + "连接成功");
        }

        private void Opening(ISerialClient client, OpeningEventArgs e)
        {
            Logger?.Debug(client.SerialProperty.ToString() + "正在连接");
            SetDataAdapter();
        }

        private void Closed(ISerialClientBase client, CloseEventArgs e)
        {
            Logger?.Debug(client.SerialProperty.ToString() + "断开连接-" + e.Message);
        }

        private void Closing(ISerialClientBase client, CloseEventArgs e)
        {
            Logger?.Debug(client.SerialProperty.ToString() + "正在主动断开连接-" + e.Message);
        }
    }
}