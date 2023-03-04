namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 应用于多字节数据的解析或是生成格式<br />
    /// </summary>
    public enum DataFormat
    {

        /// <summary>Big-Endian</summary>
        ABCD,

        /// <summary>Big-Endian Byte Swap</summary>
        BADC,

        /// <summary>Little-Endian Byte Swap</summary>
        CDAB,

        /// <summary>Little-Endian</summary>
        DCBA,
    }
}