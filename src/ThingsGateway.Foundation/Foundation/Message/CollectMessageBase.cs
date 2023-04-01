namespace ThingsGateway.Foundation
{
    /// <inheritdoc cref="IMessage"/>
    public abstract class MessageBase : OperResult<byte[]>, IMessage
    {
        private byte[] sendBytes = new byte[] { };

        /// <inheritdoc/>
        public int BodyLength { get; set; }

        /// <inheritdoc/>
        public byte[] HeadBytes { get; set; }

        /// <inheritdoc/>
        public virtual int HeadBytesLength { get; }
        /// <inheritdoc/>
        public byte[] ReceivedBytes { get; set; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public abstract bool CheckHeadBytes(byte[] head);

        /// <summary>
        /// 写入<see cref="SendBytes"/>后触发此方法
        /// </summary>
        protected virtual void SendBytesThen()
        {
        }
    }
}