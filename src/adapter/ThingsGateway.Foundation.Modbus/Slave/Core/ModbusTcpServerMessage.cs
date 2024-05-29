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
internal class ModbusTcpServerMessage : MessageBase, IResultMessage, IModbusServerMessage
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
    public override bool CheckHeadBytes(byte[]? headBytes)
    {
        if (headBytes == null || headBytes.Length <= 0) return false;

        int num = (headBytes[4] * 256) + headBytes[5];
        if (num > 0xff + 3) return false;
        BodyLength = num;

        return true;
    }
}
