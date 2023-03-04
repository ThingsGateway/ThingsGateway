namespace ThingsGateway.Foundation
{
    public interface IReadWriteDevice : IReadWrite, IDisposable
    {
        /// <summary>
        /// 链接超时时间
        /// </summary>
        ushort ConnectTimeOut { get; set; }

        /// <summary>
        /// 多字节数据解析规则
        /// </summary>
        DataFormat DataFormat { get; set; }

        /// <summary>
        /// 数据解析规则
        /// </summary>
        IThingsGatewayBitConverter ThingsGatewayBitConverter { get; }

        /// <summary>
        /// 读写超时时间
        /// </summary>
        ushort TimeOut { get; set; }

        /// <summary>
        /// 一个寄存器所占的字节长度
        /// </summary>
        ushort RegisterByteLength { get; set; }
    }
}