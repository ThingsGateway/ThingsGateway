
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class ModbusRtuServerMessage : MessageBase, IResultMessage, IModbusServerMessage
{
    /// <inheritdoc/>
    public ModbusAddress ModbusAddress { get; set; }

    /// <inheritdoc/>
    public int Length { get; set; }

    /// <inheritdoc/>
    public override int HeadBytesLength => 7; //主站发送的报文最低也有8个字节

    /// <inheritdoc/>
    public override bool CheckHeadBytes(ByteBlock? headByteBlock)
    {
        if (headByteBlock == null || headByteBlock.Length <= 0) return false;
        //01 03 00 00 00 01 xx xx
        //01 04 00 00 00 01 xx xx
        //01 01 00 00 00 01 xx xx
        //01 02 00 00 00 01 xx xx
        //01 05 00 00 00 00 xx xx
        //01 06 00 00 00 00 xx xx
        //01 0f 00 00 00 01 01 00 xx xx
        //01 10 00 00 00 01 02 00 00 xx xx

        //modbusRtu 读取/单写
        if (headByteBlock[1] <= 0x06)
        {
            BodyLength = 1; //crc l
            return true;
        }
        else if (headByteBlock[1] <= 0x10)
        {
            //modbusRtu 多写
            BodyLength = headByteBlock[6] + 2; //数据区+crc
            return true;
        }

        return false;
    }
}
