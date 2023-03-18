namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 具有预备连接的插件接口
    /// </summary>
    public interface IOpeningPlugin : IPlugin
    {
        /// <summary>
        ///在即将完成连接时触发。
        /// </summary>
        /// <param name="client">串口</param>
        /// <param name="e">参数</param>
        [AsyncRaiser]
        void OnOpening(object client, OperationEventArgs e);

        /// <summary>
        /// 在即将完成连接时触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnOpeningAsync(object client, OperationEventArgs e);
    }
}
