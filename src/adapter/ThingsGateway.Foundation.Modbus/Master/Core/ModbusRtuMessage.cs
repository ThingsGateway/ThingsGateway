
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class ModbusRtuMessage : MessageBase, IMessage
{
    /// <inheritdoc/>
    public override int HeadBytesLength => 3;

    /// <inheritdoc/>
    public override bool CheckHeadBytes(byte[] heads)
    {
        if (heads == null || heads.Length <= 0) return false;
        HeadBytes = heads;
        //01 03 02 00 01 xx xx
        //01 04 02 00 01 xx xx
        //01 01 02 00 01 xx xx
        //01 02 02 00 01 xx xx
        //01 05 00 00 00 00 xx xx
        //01 06 00 00 00 00 xx xx
        //01 0f 00 00 00 01 xx xx
        //01 10 00 00 00 01 xx xx

        //modbusRtu 读取
        if (heads[1] <= 0x04)
        {
            int num = (heads[2]);
            if (num > 0xff - 4) return false;
            BodyLength = num + 2; //数据区+crc
        }
        else
        {
            if (heads[1] <= 0x10)
            {
                //modbusRtu 写入
                BodyLength = 6 + 2; //数据区+crc
            }
            else
            {
                //错误码
                BodyLength = 3 + 2; //数据区+crc
            }
        }

        if (SendBytes?.Length > 0)
        {
            return true;
        }
        else
        {
            return false;//不是主动请求的，直接放弃
        }
    }
}