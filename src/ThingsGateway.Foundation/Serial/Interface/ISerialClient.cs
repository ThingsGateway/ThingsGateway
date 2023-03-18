namespace ThingsGateway.Foundation.Serial
{
    public interface ISerialClient : ISerialClientBase, IClientSender, IPluginObject
    {
        /// <summary>
        /// 成功连接
        /// </summary>
        MessageEventHandler<ISerialClient> Opened { get; set; }

        /// <summary>
        /// 准备连接的时候
        /// </summary>
        OpeningEventHandler<ISerialClient> Opening { get; set; }

        /// <summary>
        /// 串口描述
        /// </summary>
        SerialProperty SerialProperty { get; }

        /// <summary>
        /// 连接串口
        /// </summary>
        /// <exception cref="Exception"></exception>
        ISerialClient Open();

        /// <summary>
        /// 异步连接串口
        /// </summary>
        /// <exception cref="Exception"></exception>
        Task<ISerialClient> OpenAsync();

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="Exception"></exception>
        ISerialClient Setup(TouchSocketConfig config);

    }
}