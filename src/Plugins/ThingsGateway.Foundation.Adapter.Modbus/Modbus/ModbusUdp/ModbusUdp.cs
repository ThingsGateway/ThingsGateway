using System.Threading;
using System.Threading.Tasks;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusUdp : ReadWriteDevicesUdpClientBase
    {
        public ModbusUdpDataHandleAdapter DataHandleAdapter = new();

        public ModbusUdp(TGUdpSession udpSession) : base(udpSession)
        {
            ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
            RegisterByteLength = 2;
            waitingClient = TGUdpSession.GetTGWaitingClient(new());
        }
        private IWaitingClient<TGUdpSession> waitingClient;
        public bool IsCheckMessageId { get => DataHandleAdapter.IsCheckMessageId; set => DataHandleAdapter.IsCheckMessageId = value; }

        public byte Station { get; set; } = 1;
        public int FrameTime { get; set; }
        private async Task<ResponsedData> SendThenReturnAsync(OperResult<byte[]> commandResult, CancellationToken token)
        {
            try
            {
                var item = commandResult.Content;
                await TGUdpSession.EasyLock.LockAsync();
                await Task.Delay(FrameTime, token);

                var result = await waitingClient.SendThenResponseAsync(item, TimeOut, token);
                return result;

            }
            finally
            {
                TGUdpSession.EasyLock.UnLock();
            }
        }

        public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token = default)
        {
            try
            {
                await ConnectAsync();
                var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
                if (commandResult.IsSuccess)
                {
                    ResponsedData result = await SendThenReturnAsync(commandResult, token);
                    if (result.RequestInfo is MessageBase collectMessage)
                    {
                        return collectMessage;
                    }
                }
                else
                {
                    return OperResult.CreateFailedResult<byte[]>(commandResult);
                }
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex);
            }
            return new OperResult<byte[]>(TouchSocketStatus.UnknownError.GetDescription());

        }

        public override void SetDataAdapter()
        {
            DataHandleAdapter = new();
            DataHandleAdapter.IsCheckMessageId = IsCheckMessageId;
            TGUdpSession.SetDataHandlingAdapter(DataHandleAdapter);
        }
        public override async Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default)
        {
            try
            {
                await ConnectAsync();
                var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
                if (commandResult.IsSuccess)
                {
                    ResponsedData result = await SendThenReturnAsync(commandResult, token);
                    if (result.RequestInfo is MessageBase collectMessage)
                    {
                        return collectMessage;
                    }
                }
                else
                {
                    return OperResult.CreateFailedResult<bool[]>(commandResult);
                }
            }
            catch (Exception ex)
            {
                return new OperResult<bool[]>(ex);
            }
            return new OperResult<byte[]>(TouchSocketStatus.UnknownError.GetDescription());

        }

        public override async Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken token = default)
        {
            try
            {
                await ConnectAsync();
                var commandResult = ModbusHelper.GetWriteBoolModbusCommand(address, value, Station);
                if (commandResult.IsSuccess)
                {
                    ResponsedData result = await SendThenReturnAsync(commandResult, token);
                    if (result.RequestInfo is MessageBase collectMessage)
                    {
                        return collectMessage;
                    }
                }
                else
                {
                    return OperResult.CreateFailedResult<bool[]>(commandResult);
                }
            }
            catch (Exception ex)
            {
                return new OperResult<bool[]>(ex);
            }
            return new OperResult<byte[]>(TouchSocketStatus.UnknownError.GetDescription());

        }

    }
}
