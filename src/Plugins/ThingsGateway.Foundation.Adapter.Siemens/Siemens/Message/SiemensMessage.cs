namespace ThingsGateway.Foundation.Adapter.Siemens
{
    public class SiemensMessage : MessageBase, IMessage
    {
        public override int HeadBytesLength => 4;

        public override bool CheckHeadBytes(byte[] token)
        {
            HeadBytes = token;
            byte[] headBytes = HeadBytes;
            if (headBytes == null || headBytes.Length < 4)
                BodyLength = 0;
            int length = (HeadBytes[2] * 256) + HeadBytes[3] - 4;
            if (length < 0)
                length = 0;
            BodyLength = length;
            return HeadBytes != null && HeadBytes[0] == 3 && HeadBytes[1] == 0;
        }

        protected override void SendBytesThen()
        {

        }

    }
}