namespace ThingsGateway.Foundation.Serial
{
    /// <summary>
    /// 串口基接口
    /// </summary>
    public interface ISerial : IDisposable
    {
        /// <summary>
        /// 数据交互缓存池限制
        /// </summary>
        int BufferLength { get; }

        /// <summary>
        /// 日志记录器
        /// </summary>
        ILog Logger { get; }
    }
}