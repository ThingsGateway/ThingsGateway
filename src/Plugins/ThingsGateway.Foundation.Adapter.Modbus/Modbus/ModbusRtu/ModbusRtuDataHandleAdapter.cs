namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusRtuDataHandleAdapter : ReadWriteDevicesSerialDataHandleAdapter<ModbusRtuMessage>
    {
        public bool Crc16CheckEnable { get; set; } = true;

        public override byte[] PackCommand(byte[] command)
        {
            return ModbusHelper.AddCrc(command);
        }

        protected override ModbusRtuMessage GetInstance()
        {
            return new ModbusRtuMessage();
        }
        /// <summary>
        /// 解包获取实际数据包
        /// </summary>
        protected override FilterResult GetResponse(ByteBlock byteBlock, ModbusRtuMessage request, byte[] allBytes, byte[] bytes)
        {
            var unpackbytes = UnpackResponse(request.SendBytes, allBytes);
            request.Message = unpackbytes.Message;
            request.ResultCode = unpackbytes.ResultCode;
            if (unpackbytes.IsSuccess)
            {
                request.Content = unpackbytes.Content;
                request.ReceivedBytes = allBytes;
                return FilterResult.Success;
            }
            else
            {
                byteBlock.Pos = byteBlock.Len;
                request.ReceivedBytes = allBytes;
                request.Message = unpackbytes.Message;

                if (!(allBytes[1] <= 0x10))
                {
                    return FilterResult.Success;
                }
                else
                {
                    if ((allBytes.Length > allBytes[2] + 4))
                    {
                        return FilterResult.Success;
                    }
                    else
                    {
                        return FilterResult.Cache;
                    }
                }

            }
        }

        protected override OperResult<byte[]> UnpackResponse(byte[] send, byte[] response)
        {
            return ModbusHelper.GetModbusRtuData(send, response, Crc16CheckEnable);

        }

    }
}
