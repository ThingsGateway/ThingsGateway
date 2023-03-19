namespace ThingsGateway.Foundation
{
    public abstract class ReadWriteDevicesServerBase : ReadWriteDevicesBase, IReadWriteDevice
    {
        /// <summary>
        /// 链接超时时间
        /// </summary>
        public ushort ConnectTimeOut { get; set; } = 3000;

        public abstract void Start();

        public abstract void Stop();

        public abstract void SetDataAdapter(SocketClient client);


    }
}