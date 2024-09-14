//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusTcpSend : ISendMessage
{
    private ModbusAddress ModbusAddress;

    private bool Read;

    public ModbusTcpSend(ModbusAddress modbusAddress, ushort transactionId, bool read, ushort protocolId = 0)
    {
        TransactionId = transactionId;
        Sign = transactionId;
        ProtocolId = protocolId;
        ModbusAddress = modbusAddress;
        Read = read;
    }

    public int MaxLength => 300;

    /// <summary>
    /// 协议标识符
    /// </summary>
    public ushort ProtocolId { get; set; }

    public int Sign { get; set; }

    /// <summary>
    /// 事务处理标识符。即序号
    /// </summary>
    public ushort TransactionId { get; set; }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        if (!Read)
        {
            if (ModbusAddress.WriteFunctionCode == null)
            {
                ModbusAddress.WriteFunctionCode = (byte)(ModbusAddress.FunctionCode == 1 ? 5 : 6);
            }
            if (ModbusAddress.Data.Length > 2 && ModbusAddress.WriteFunctionCode < 15)
            {
                ModbusAddress.WriteFunctionCode = (byte)(ModbusAddress.FunctionCode == 1 ? 15 : 16);
            }
        }
        var f = Read ? ModbusAddress.FunctionCode : ModbusAddress.WriteFunctionCode;

        byteBlock.WriteUInt16(TransactionId, EndianType.Big);
        byteBlock.WriteUInt16(ProtocolId, EndianType.Big);
        if (f <= 4)
        {
            byteBlock.WriteUInt16(6, EndianType.Big);
            byteBlock.WriteByte(ModbusAddress.Station);
            byteBlock.WriteByte((byte)f);
            byteBlock.WriteUInt16(ModbusAddress.StartAddress, EndianType.Big);
            byteBlock.WriteUInt16(ModbusAddress.Length, EndianType.Big);
        }
        else if (f == 5 || f == 6)
        {
            byteBlock.WriteUInt16(6, EndianType.Big);
            byteBlock.WriteByte(ModbusAddress.Station);
            byteBlock.WriteByte((byte)f);
            byteBlock.WriteUInt16(ModbusAddress.StartAddress, EndianType.Big);
            byteBlock.Write(ModbusAddress.Data.Span);
        }
        else if (f == 15 || f == 16)
        {
            byteBlock.WriteUInt16((ushort)(ModbusAddress.Data.Length + 7), EndianType.Big);
            byteBlock.WriteByte(ModbusAddress.Station);
            byteBlock.WriteByte((byte)f);
            byteBlock.WriteUInt16(ModbusAddress.StartAddress, EndianType.Big);
            byteBlock.WriteUInt16((ushort)Math.Ceiling(f == 15 ? ModbusAddress.Data.Length * 8 : ModbusAddress.Data.Length / 2.0), EndianType.Big);
            byteBlock.WriteByte((byte)ModbusAddress.Data.Length);
            byteBlock.Write(ModbusAddress.Data.Span);
        }
        else
        {
            throw new System.InvalidOperationException(ModbusResource.Localizer["ModbusError1"]);
        }
    }
}
