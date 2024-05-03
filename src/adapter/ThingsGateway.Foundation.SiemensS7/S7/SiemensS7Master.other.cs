
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
    private static byte[] GetWriteBitCommand(string address, bool data)
    {
        var result = SiemensAddress.ParseFrom(address);
        return SiemensHelper.GetWriteBitCommand(result, data);
    }

    private List<byte[]> GetReadByteCommand(string address, int length)
    {
        var from = SiemensAddress.ParseFrom(address, length);
        ushort num1 = 0;
        var listBytes = new List<byte[]>();
        while (num1 < length)
        {
            //pdu长度，重复生成报文，直至全部生成
            ushort num2 = (ushort)Math.Min(length - num1, PduLength);
            from.Length = num2;
            var result = GetReadByteCommand(new SiemensAddress[1] { from });
            listBytes.AddRange(result);
            num1 += num2;
            if (from.DataCode == (byte)S7WordLength.Timer || from.DataCode == (byte)S7WordLength.Counter)
            {
                from.Address += num2 / 2;
            }
            else
            {
                from.Address += num2 * 8;
            }
        }
        return listBytes;
    }

    private List<byte[]> GetReadByteCommand(SiemensAddress[] siemensAddress)
    {
        if (siemensAddress.Length <= 19)
        {
            return new List<byte[]>() { SiemensHelper.GetReadCommand(siemensAddress) };
        }

        List<byte[]> byteList = new();
        List<SiemensAddress[]> s7AddressDataArrayList = siemensAddress.ArraySplitByLength(19);
        for (int index = 0; index < s7AddressDataArrayList.Count; ++index)
        {
            var result = GetReadByteCommand(s7AddressDataArrayList[index]);
            byteList.AddRange(result);
        }
        return byteList;
    }

    private List<byte[]> GetWriteByteCommand(string address, byte[] value)
    {
        var s_Address = SiemensAddress.ParseFrom(address);

        return GetWriteByteCommand(s_Address, value);
    }

    /// <summary>
    /// DefaultConverter
    /// </summary>
    public static ThingsGatewayBitConverter DefaultConverter = new(BitConverter.IsLittleEndian ? EndianType.Little : EndianType.Big);

    private List<byte[]> GetWriteByteCommand(SiemensAddress address, byte[] value)
    {
        int length1 = value.Length;
        ushort index = 0;
        List<byte[]> bytes = new();
        while (index < length1)
        {
            //pdu长度，重复生成报文，直至全部生成
            ushort length2 = (ushort)Math.Min(length1 - index, PduLength);
            byte[] data = DefaultConverter.ToByte(value, index, length2);
            var result1 = SiemensHelper.GetWriteByteCommand(address, data);
            bytes.Add(result1);
            index += length2;
            address.Address += length2 * 8;
        }
        return bytes;
    }
}
