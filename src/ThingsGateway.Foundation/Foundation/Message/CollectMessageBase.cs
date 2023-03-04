namespace ThingsGateway.Foundation
{
    public abstract class MessageBase : OperResult<byte[]>, IMessage
    {
        private byte[] sendBytes = new byte[] { };

        public int BodyLength { get; set; }

        public byte[] HeadBytes { get; set; }

        public virtual int HeadBytesLength { get; }
        public byte[] ReceivedBytes { get; set; }

        public byte[] SendBytes
        {
            get
            {
                return sendBytes;
            }
            set
            {
                sendBytes = value;
                SendBytesThen();
            }
        }

        public abstract bool CheckHeadBytes(byte[] head);

        /// <summary>
        /// 写入<see cref="SendBytes"/>后触发此方法
        /// </summary>
        protected virtual void SendBytesThen()
        {
        }
    }
}