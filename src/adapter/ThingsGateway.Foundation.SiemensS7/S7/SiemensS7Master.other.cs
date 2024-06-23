//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

/// <inheritdoc/>
public partial class SiemensS7Master : ProtocolBase
{
    private List<ReadOnlyMemory<byte>> GetReadByteCommand(ref ValueByteBlock valueByteBlock, string address, int length)
    {
        var from = SiemensAddress.ParseFrom(address, length);
        ushort num1 = 0;
        var listBytes = new List<ReadOnlyMemory<byte>>();
        while (num1 < length)
        {
            //pdu长度，重复生成报文，直至全部生成
            ushort num2 = (ushort)Math.Min(length - num1, PduLength);
            from.Length = num2;
            var result = GetReadByteCommand(ref valueByteBlock, new SiemensAddress[1] { from });
            listBytes.AddRange(result);
            num1 += num2;
            if (from.DataCode == (byte)S7WordLength.Timer || from.DataCode == (byte)S7WordLength.Counter)
            {
                from.AddressStart += num2 / 2;
            }
            else
            {
                from.AddressStart += num2 * 8;
            }
        }
        return listBytes;
    }

    private List<ReadOnlyMemory<byte>> GetReadByteCommand(ref ValueByteBlock valueByteBlock, SiemensAddress[] siemensAddress)
    {
        if (siemensAddress.Length <= 19)
        {
            return new List<ReadOnlyMemory<byte>>() { SiemensHelper.GetReadCommand(ref valueByteBlock, siemensAddress) };
        }

        List<ReadOnlyMemory<byte>> byteList = new();
        List<SiemensAddress[]> s7AddressDataArrayList = siemensAddress.ArraySplitByLength(19);
        for (int index = 0; index < s7AddressDataArrayList.Count; ++index)
        {
            var result = GetReadByteCommand(ref valueByteBlock, s7AddressDataArrayList[index]);
            byteList.AddRange(result);
        }
        return byteList;
    }

    private List<ReadOnlyMemory<byte>> GetWriteByteCommand(ref ValueByteBlock valueByteBlock, SiemensAddress address, byte[] value)
    {
        int length1 = value.Length;
        ushort index = 0;
        List<ReadOnlyMemory<byte>> bytes = new();
        while (index < length1)
        {
            //pdu长度，重复生成报文，直至全部生成
            ushort length2 = (ushort)Math.Min(length1 - index, PduLength);
            ReadOnlySpan<byte> data = new ReadOnlySpan<byte>(value, index, length2);
            var result1 = SiemensHelper.GetWriteByteCommand(ref valueByteBlock, address, data);
            bytes.Add(result1);
            index += length2;
            address.AddressStart += length2 * 8;
        }
        return bytes;
    }
}
