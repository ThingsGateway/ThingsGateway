namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 设备地址数据的信息，参考HSL代码思路，对每个协议都建立其变量地址的表示类
    /// </summary>
    public class DeviceAddressBase
    {
        /// <summary>
        /// 起始地址
        /// </summary>
        public int AddressStart { get; set; }

        /// <summary>
        /// 读取的数据长度，有可能不参与运算
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 字符串地址转换为实体类
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        public virtual void Parse(string address, int length)
        {
            AddressStart = int.Parse(address);
            Length = length;
        }
        /// <summary>
        /// 实体类转换为字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return AddressStart.ToString();
        }
    }
}