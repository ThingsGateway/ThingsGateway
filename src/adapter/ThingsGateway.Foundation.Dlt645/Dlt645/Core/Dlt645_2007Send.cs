//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class Dlt645_2007Send : ISendMessage
{
    public int Sign { get; set; }

    public int MaxLength => 300;

    internal Dlt645_2007Address Dlt645_2007Address { get; }
    public int SendHeadCodeIndex { get; private set; }

    private bool Read;

    public Dlt645_2007Send(Dlt645_2007Address dlt645_2007Address, ushort sign, bool read)
    {
        Sign = sign;
        Dlt645_2007Address = dlt645_2007Address;
        Read = read;
    }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        var f = Read ? ModbusAddress.FunctionCode : ModbusAddress.WriteFunctionCode;

        if (f <= 4)
        {
            byteBlock.WriteByte(ModbusAddress.Station);
            byteBlock.WriteByte((byte)f);
            byteBlock.WriteUInt16(ModbusAddress.StartAddress, EndianType.Big);
            byteBlock.WriteUInt16(ModbusAddress.Length, EndianType.Big);
        }
        else if (f == 5 || f == 6)
        {
            byteBlock.WriteByte(ModbusAddress.Station);
            byteBlock.WriteByte((byte)f);
            byteBlock.WriteUInt16(ModbusAddress.StartAddress, EndianType.Big);
            byteBlock.Write(ModbusAddress.Data.Span);
        }
        else if (f == 15 || f == 16)
        {
            byteBlock.WriteByte(ModbusAddress.Station);
            byteBlock.WriteByte((byte)f);
            byteBlock.WriteUInt16(ModbusAddress.StartAddress, EndianType.Big);
            byteBlock.WriteUInt16((ushort)Math.Ceiling(ModbusAddress.Data.Length / 2.0), EndianType.Big);
            byteBlock.WriteByte((byte)ModbusAddress.Data.Length);
            byteBlock.Write(ModbusAddress.Data.Span);
        }
        else
        {
            throw new System.InvalidOperationException(ModbusResource.Localizer["ModbusError1"]);
        }
        byteBlock.Write(CRC16Utils.Crc16Only(byteBlock.Span));
    }
}
