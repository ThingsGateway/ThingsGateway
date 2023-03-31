namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 串口插件接口
    /// </summary>
    public interface ISerialPlugin : IPlugin, IOpeningPlugin, IOpenedPlugin, IClosingPlugin, IClosedPlguin
    {
        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        [AsyncRaiser]
        void OnReceivedData(ISerialClientBase client, ReceivedDataEventArgs e);

        /// <summary>
        /// 在收到数据时触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnReceivedDataAsync(ISerialClientBase client, ReceivedDataEventArgs e);

        /// <summary>
        /// 在刚收到数据时触发，即在适配器之前。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        [AsyncRaiser]
        void OnReceivingData(ISerialClientBase client, ByteBlockEventArgs e);

        /// <summary>
        /// 在刚收到数据时触发，即在适配器之前。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnReceivingDataAsync(ISerialClientBase client, ByteBlockEventArgs e);

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client">客户端</param>
        /// <param name="e">参数</param>
        [AsyncRaiser]
        void OnSendingData(ISerialClientBase client, SendingEventArgs e);

        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnSendingDataAsync(ISerialClientBase client, SendingEventArgs e);
    }
}