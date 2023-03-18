namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 具有完成连接动作的插件接口
    /// </summary>
    public interface IOpenedPlugin : IPlugin
    {
        /// <summary>
        /// 串口连接成功后触发
        /// </summary>
        /// <param name="client">串口</param>
        /// <param name="e">参数</param>
        [AsyncRaiser]
        void OnOpened(object client, TouchSocketEventArgs e);

        /// <summary>
        /// 串口连接成功后触发
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnOpenedAsync(object client, TouchSocketEventArgs e);
    }
}
