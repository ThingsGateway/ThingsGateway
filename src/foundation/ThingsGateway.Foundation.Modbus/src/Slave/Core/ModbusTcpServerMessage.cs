#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

namespace ThingsGateway.Foundation.Modbus
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    internal class ModbusTcpServerMessage : MessageBase, IMessage, IModbusServerMessage
    {
        /// <summary>
        /// 当前关联的地址
        /// </summary>
        public ModbusAddress ModbusAddress { get; set; }

        /// <summary>
        /// 当前读写的数据长度
        /// </summary>
        public int Length { get; set; }

        /// <inheritdoc/>
        public override int HeadBytesLength => 6;

        /// <inheritdoc/>
        public override bool CheckHeadBytes(byte[] heads)
        {
            if (heads == null || heads.Length != 6) return false;
            HeadBytes = heads;

            int num = (HeadBytes[4] * 256) + HeadBytes[5];
            BodyLength = num;

            return true;
        }
    }
}