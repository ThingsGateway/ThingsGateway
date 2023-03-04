namespace ThingsGateway.Foundation.Adapter.Modbus
{
    public class ModbusTcpMessage : MessageBase, IMessage
    {
        public override int HeadBytesLength => 6;

        public bool IsCheckMessageId { get; set; } = false;
        public override bool CheckHeadBytes(byte[] head)
        {
            if (head == null || head.Length <= 0) return false;
            HeadBytes = head;

            int num = (HeadBytes[4] * 256) + HeadBytes[5];
            BodyLength = num;

            if (!IsCheckMessageId)
                return true;
            else
                return SendBytes[0] == HeadBytes[0] && SendBytes[1] == HeadBytes[1] && HeadBytes[2] == 0 && HeadBytes[3] == 0;
        }


    }
}