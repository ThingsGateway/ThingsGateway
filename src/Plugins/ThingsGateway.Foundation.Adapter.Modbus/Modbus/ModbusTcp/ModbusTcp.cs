using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusTcp : ReadWriteDevicesTcpClientBase
    {
        public ModbusTcpDataHandleAdapter DataHandleAdapter = new();

        public ModbusTcp(TGTcpClient tcpClient) : base(tcpClient)
        {
            ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
            RegisterByteLength = 2;
            waitingClient = TGTcpClient.GetTGWaitingClient(new());
        }
        private IWaitingClient<TGTcpClient> waitingClient;

        [Description("检测事务标识符")]
        public bool IsCheckMessageId { get; set; }

        [Description("站号")]
        public byte Station { get; set; } = 1;
        [Description("帧前时间")]
        public int FrameTime { get; set; }
        [Description("组包缓存时间")]
        public double CacheTimeout { get; set; } = 1;
        private async Task<ResponsedData> SendThenReturnAsync(OperResult<byte[]> commandResult, CancellationToken token)
        {
            try
            {
                var item = commandResult.Content;
                await TGTcpClient.EasyLock.LockAsync();
                await Task.Delay(FrameTime, token);

                var result = await waitingClient.SendThenResponseAsync(item, TimeOut, token);
                return result;

            }
            finally
            {
                TGTcpClient.EasyLock.UnLock();
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
            DataHandleAdapter.CacheTimeout = TimeSpan.FromSeconds(CacheTimeout);
            TGTcpClient.SetDataHandlingAdapter(DataHandleAdapter);
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
