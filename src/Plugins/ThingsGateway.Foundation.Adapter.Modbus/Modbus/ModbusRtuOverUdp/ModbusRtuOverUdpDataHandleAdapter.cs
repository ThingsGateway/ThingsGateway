namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusRtuOverUdpDataHandleAdapter : ReadWriteDevicesUdpDataHandleAdapter<ModbusRtuMessage>
    {
        public bool Crc16CheckEnable { get; set; } = true;

        public override byte[] PackCommand(byte[] command)
        {
            return ModbusHelper.AddCrc(command);
        }

        protected override OperResult<byte[]> UnpackResponse(
                  byte[] send, byte[] response)
        {
            return ModbusHelper.GetModbusRtuData(send, response, Crc16CheckEnable);
        }

        protected override ModbusRtuMessage GetInstance()
        {
            return new ModbusRtuMessage();
        }


    }
}
