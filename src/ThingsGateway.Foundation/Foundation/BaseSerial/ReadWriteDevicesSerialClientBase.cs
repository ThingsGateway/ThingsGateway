using ThingsGateway.Foundation.Serial;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// TCP读写设备
    /// </summary>
    public abstract class ReadWriteDevicesSerialClientBase : ReadWriteDevicesSerialBase
    {
        /// <summary>
        /// <inheritdoc cref="ReadWriteDevicesSerialClientBase"/>
        /// </summary>
        /// <param name="serialClient"></param>
        public ReadWriteDevicesSerialClientBase(SerialClient serialClient)
        {
            SerialClient = serialClient;
            SerialClient.Opening += Opening;
            SerialClient.Opened += Opened;
            SerialClient.Closing += Closing;
            SerialClient.Closed += Closed;
            Logger = SerialClient.Logger;
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            SerialClient.Close();
            SerialClient.Opening -= Opening;
            SerialClient.Opened -= Opened;
            SerialClient.Closing -= Closing;
            SerialClient.Closed -= Closed;
            SerialClient.Dispose();
        }
        /// <summary>
        /// 串口管理对象
        /// </summary>
        public SerialClient SerialClient { get; }

        /// <inheritdoc/>
        public override Task OpenAsync()
        {
            return SerialClient.OpenAsync();
        }
        /// <inheritdoc/>
        public override void Open()
        {
            SerialClient.Open();
        }
        /// <inheritdoc/>
        public override void Close()
        {
            SerialClient.Close();
        }

        /// <inheritdoc/>
        public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
        {
            try
            {
                if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
                await OpenAsync();
                ResponsedData result = await SerialClient.GetTGWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, token);
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
                Open();
                ResponsedData result = SerialClient.GetTGWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, token);
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