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

        protected override OperResult<byte[]> UnpackResponse(
                          byte[] send, byte[] response)
        {
            return ModbusHelper.GetModbusRtuData(send, response, Crc16CheckEnable);
        }
    }
}
