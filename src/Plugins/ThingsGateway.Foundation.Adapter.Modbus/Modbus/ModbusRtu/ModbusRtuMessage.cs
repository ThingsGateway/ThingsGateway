namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusRtuMessage : MessageBase, IMessage
    {
        public override int HeadBytesLength => -1;

        public override bool CheckHeadBytes(byte[] head)
        {
            return true;
        }

        protected override void SendBytesThen()
        {
            BodyLength = -1;
        }

    }
}