namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 具有断开连接的插件接口
    /// </summary>
    public interface IClosedPlguin : IPlugin
    {
        /// <summary>
        /// 串口断开后触发
        /// </summary>
        /// <param name="client">串口</param>
        /// <param name="e">参数</param>
        [AsyncRaiser]
        void OnClosed(object client, CloseEventArgs e);

        /// <summary>
        /// 串口断开后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnClosedAsync(object client, CloseEventArgs e);
    }
}
