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
public class ModbusRtuSlaveMessage : MessageBase, IResultMessage
{
    /// <summary>
    /// 当前关联的字节数组
    /// </summary>
    public ReadOnlyMemory<byte> Bytes { get; set; }

    /// <inheritdoc/>
    public override int HeaderLength => 7;

    public ModbusRequest Request { get; set; } = new();


    /// <inheritdoc/>
    public override bool CheckHead<TByteBlock>(ref TByteBlock byteBlock)
    {
        Request.Station = byteBlock.ReadByte();
        Request.FunctionCode = byteBlock.ReadByte();
        Request.StartAddress = byteBlock.ReadUInt16(EndianType.Big);
        if (Request.FunctionCode == 3 || Request.FunctionCode == 4)
        {
            Request.Length = (ushort)(byteBlock.ReadUInt16(EndianType.Big) * 2);
            BodyLength = 1;
            return true;
        }
        else if (Request.FunctionCode == 1 || Request.FunctionCode == 2)
        {
            Request.Length = byteBlock.ReadUInt16(EndianType.Big);
            BodyLength = 1;
            return true;
        }
        else if (Request.FunctionCode == 5)
        {
            Request.Data = byteBlock.AsSegmentTake(1);
            BodyLength = 1;
            return true;
        }
        else if (Request.FunctionCode == 6)
        {
            Request.Data = byteBlock.AsSegmentTake(2);
            BodyLength = 1;
            return true;
        }
        else if (Request.FunctionCode == 15)
        {
            Request.Length = byteBlock.ReadUInt16(EndianType.Big);
            BodyLength = Request.Length + 2;
            return true;
        }
        else if (Request.FunctionCode == 16)
        {
            Request.Length = (ushort)(byteBlock.ReadUInt16(EndianType.Big) * 2);
            BodyLength = Request.Length + 2;
            return true;
        }
        return false;
    }

    public override FilterResult CheckBody<TByteBlock>(ref TByteBlock byteBlock)
    {
        var pos = byteBlock.Position - HeaderLength;
        var crcLen = 0;
        Bytes = byteBlock.AsSegment(pos, HeaderLength + BodyLength);

        if (Request.FunctionCode == 15)
        {
            Request.Data = byteBlock.AsSegmentTake(Request.Length).AsSpan().ByteToBoolArray(Request.Length).Select(a => a ? (byte)0xff : (byte)0).ToArray();
        }
        else if (Request.FunctionCode == 16)
        {
            Request.Data = byteBlock.AsSegmentTake(Request.Length);
        }

        crcLen = HeaderLength + BodyLength - 2;

        var crc = CRC16Utils.Crc16Only(byteBlock.Span.Slice(pos, crcLen));

        //Crc
        var checkCrc = byteBlock.Span.Slice(pos + crcLen, 2).ToArray();
        if (crc.SequenceEqual(checkCrc))
        {
            OperCode = 0;
            return FilterResult.Success;
        }

        return FilterResult.GoOn;
    }


}
