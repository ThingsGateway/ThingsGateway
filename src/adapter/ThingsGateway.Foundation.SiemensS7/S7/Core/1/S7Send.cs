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

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class S7Send : ISendMessage
{
    public int Sign { get; set; }

    public int MaxLength => 300;

    private SiemensAddress SiemensAddress;
    private bool Read;

    public S7Send(SiemensAddress siemensAddress, ushort transactionId, bool read)
    {
        Sign = transactionId;
        SiemensAddress = siemensAddress;
        Read = read;
    }

    internal static void GetReadCommand(ref ValueByteBlock valueByteBlock, SiemensAddress[] siemensAddress)
    {
        int len = siemensAddress.Length;
        int telegramLen = len * 12 + 19;
        int parameterLen = len * 12 + 2;

        valueByteBlock.Write(S7_MULRW_HEADER);//19字节
        valueByteBlock[2] = (byte)(telegramLen / 256);
        valueByteBlock[3] = (byte)(telegramLen % 256);
        valueByteBlock[13] = (byte)(parameterLen / 256);
        valueByteBlock[14] = (byte)(parameterLen % 256);
        valueByteBlock[18] = (byte)len;

        for (int index = 0; index < len; index++)
        {
            valueByteBlock.Write(S7_MULRD_ITEM);//12字节
            if (siemensAddress[index].DataCode == (byte)S7WordLength.Counter || siemensAddress[index].DataCode == (byte)S7WordLength.Timer)
            {
                valueByteBlock[22 + (index * 12)] = siemensAddress[index].DataCode;
                valueByteBlock[23 + (index * 12)] = (byte)(siemensAddress[index].Length / 256);
                valueByteBlock[24 + (index * 12)] = (byte)(siemensAddress[index].Length % 256);
            }
            else
            {
                valueByteBlock[22 + (index * 12)] = (byte)S7WordLength.Byte;
                valueByteBlock[23 + (index * 12)] = (byte)(siemensAddress[index].Length / 256);
                valueByteBlock[24 + (index * 12)] = (byte)(siemensAddress[index].Length % 256);
            }
            valueByteBlock[25 + (index * 12)] = (byte)(siemensAddress[index].DbBlock / 256U);
            valueByteBlock[26 + (index * 12)] = (byte)(siemensAddress[index].DbBlock % 256U);
            valueByteBlock[27 + (index * 12)] = siemensAddress[index].DataCode;
            valueByteBlock[28 + (index * 12)] = (byte)(siemensAddress[index].AddressStart / 256 / 256 % 256);
            valueByteBlock[29 + (index * 12)] = (byte)(siemensAddress[index].AddressStart / 256 % 256);
            valueByteBlock[30 + (index * 12)] = (byte)(siemensAddress[index].AddressStart % 256);
        }
    }

    public void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        ushort num1 = 0;
        var listBytes = new List<ValueByteBlock>();
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
                from.AddressStart += num2 / 2;
            }
            else
            {
                from.AddressStart += num2 * 8;
            }
        }
    }
}
