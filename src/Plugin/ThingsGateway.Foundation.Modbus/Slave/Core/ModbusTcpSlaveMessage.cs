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
public class ModbusTcpSlaveMessage : MessageBase, IResultMessage
{
    /// <summary>
    /// 当前关联的字节数组
    /// </summary>
    public ReadOnlyMemory<byte> Bytes { get; set; }

    /// <inheritdoc/>
    public override int HeaderLength => 12;

    public ModbusRequest Request { get; set; } = new();

    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        Sign = byteBlock.ReadUInt16(EndianType.Big);
        byteBlock.Position += 2;
        BodyLength = byteBlock.ReadUInt16(EndianType.Big) - 6;
        Request.Station = byteBlock.ReadByte();
        Request.FunctionCode = byteBlock.ReadByte();

        Request.StartAddress = byteBlock.ReadUInt16(EndianType.Big);

        if (Request.FunctionCode == 3 || Request.FunctionCode == 4)
        {
            Request.Length = (ushort)(byteBlock.ReadUInt16(EndianType.Big) * 2);
            return true;
        }
        else if (Request.FunctionCode == 1 || Request.FunctionCode == 2)
        {
            Request.Length = byteBlock.ReadUInt16(EndianType.Big);
            return true;
        }
        else if (Request.FunctionCode == 5)
        {
            Request.Data = byteBlock.AsSegmentTake(1);
            return true;
        }
        else if (Request.FunctionCode == 6)
        {
            Request.Data = byteBlock.AsSegmentTake(2);
            return true;
        }
        else if (Request.FunctionCode == 15)
        {
            Request.Length = byteBlock.ReadUInt16(EndianType.Big);
            return true;
        }
        else if (Request.FunctionCode == 16)
        {
            Request.Length = (ushort)(byteBlock.ReadUInt16(EndianType.Big) * 2);
            return true;
        }
        return false;
    }


    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        var pos = byteBlock.Position - HeaderLength;
        Bytes = byteBlock.AsSegment(pos, HeaderLength + BodyLength);


        if (Request.FunctionCode == 15)
        {
            byteBlock.Position += 1;
            Request.Data = byteBlock.AsSegmentTake(Request.Length).AsSpan().ByteToBoolArray(Request.Length).Select(a => a ? (byte)0xff : (byte)0).ToArray();
        }
        else if (Request.FunctionCode == 16)
        {
            byteBlock.Position += 1;
            Request.Data = byteBlock.AsSegmentTake(Request.Length);
        }

        OperCode = 0;
        return FilterResult.Success;
    }

}
