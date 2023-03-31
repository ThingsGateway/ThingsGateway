namespace ThingsGateway.Foundation
{
    /// <inheritdoc cref="ReadWriteDevicesBase"/>
    public abstract class ReadWriteDevicesClientBase : ReadWriteDevicesBase, IReadWriteDevice
    {
        /// <summary>
        /// 链接超时时间
        /// </summary>
        public ushort ConnectTimeOut { get; set; } = 3000;
        /// <summary>
        /// 连接操作
        /// </summary>
        public abstract void Connect();
        /// <inheritdoc cref="Connect"/>
        public abstract Task ConnectAsync();

        /// <summary>
        /// 断开操作
        /// </summary>
        public abstract void Disconnect();
        /// <summary>
        /// 设置适配器
        /// </summary>
        public abstract void SetDataAdapter();

    }
}