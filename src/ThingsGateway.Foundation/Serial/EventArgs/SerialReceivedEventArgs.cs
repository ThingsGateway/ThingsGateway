namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 字节事件
    /// </summary>
    public class SerialReceivedEventArgs : TouchSocketEventArgs
    {
        /// <summary>
        /// 数据块
        /// </summary>
        public ByteBlock UserToken { get; set; }
    }
}