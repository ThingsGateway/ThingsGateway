namespace ThingsGateway.Foundation
{
    public abstract class ReadWriteDevicesClientBase : ReadWriteDevicesBase, IReadWriteDevice
    {
        /// <summary>
        /// 链接超时时间
        /// </summary>
        public ushort ConnectTimeOut { get; set; } = 3000;

        public abstract void Connect();
        public abstract Task ConnectAsync();

        public abstract void Disconnect();

        public abstract void SetDataAdapter();

    }
}