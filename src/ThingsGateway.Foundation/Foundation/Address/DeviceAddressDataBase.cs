#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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