namespace ThingsGateway.Foundation
{
    /// <inheritdoc cref="ReadWriteDevicesBase"/>
    public abstract class ReadWriteDevicesServerBase : ReadWriteDevicesBase, IReadWriteDevice
    {
        /// <summary>
        /// 链接超时时间
        /// </summary>
        public ushort ConnectTimeOut { get; set; } = 3000;

        /// <summary>
        /// 启动服务
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// 停止服务
        /// </summary>
        public abstract void Stop();
        /// <summary>
        /// 设置适配器
        /// </summary>
        /// <param name="client"></param>
        public abstract void SetDataAdapter(SocketClient client);


    }
}