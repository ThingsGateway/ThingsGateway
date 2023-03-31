namespace ThingsGateway.Foundation
{
    /// <inheritdoc cref="ReadWriteDevicesBase"/>
    public abstract class ReadWriteDevicesSerialBase : ReadWriteDevicesBase, IReadWriteDevice
    {
        /// <summary>
        /// 连接操作
        /// </summary>
        public abstract void Open();
        /// <inheritdoc cref="Open"/>
        public abstract Task OpenAsync();
        /// <summary>
        /// 断开操作
        /// </summary>
        public abstract void Close();
        /// <summary>
        /// 设置适配器
        /// </summary>
        public abstract void SetDataAdapter();

    }
}