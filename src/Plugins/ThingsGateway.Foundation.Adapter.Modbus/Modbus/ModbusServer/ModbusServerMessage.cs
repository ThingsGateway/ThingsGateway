namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusServerMessage : MessageBase, IMessage
    {
        public override int HeadBytesLength => 6;
        public override bool CheckHeadBytes(byte[] head)
        {
            if (head == null || head.Length != 6) return false;
            HeadBytes = head;

            int num = (HeadBytes[4] * 256) + HeadBytes[5];
            BodyLength = num;

            return true;
        }

        public ModbusAddress CurModbusAddress { get; set; }

    }
}