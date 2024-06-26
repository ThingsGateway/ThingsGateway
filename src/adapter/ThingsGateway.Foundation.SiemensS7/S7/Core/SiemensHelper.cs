//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

internal partial class SiemensHelper
{
    #region 验证

    //internal static OperResult<byte[]> AnalysisReadBit(byte[] content)
    //{
    //    int length = 1;
    //    if (content.Length < 21 || content[20] != 1)
    //        return new OperResult<byte[]>("数据块长度校验失败");
    //    byte[] numArray = new byte[length];
    //    if (content[21] == byte.MaxValue && content[22] == 3)//Bit:3;Byte:4;Counter或者Timer:9
    //    {
    //        numArray[0] = content[25];//+4
    //    }
    //    else
    //    {
    //        return new OperResult<byte[]>((int)content[21] + GetCpuError(content[21]));
    //    }
    //    return OperResult.CreateSuccessResult<byte[]>(numArray);
    //}

    internal static OperResult<AdapterResult> AnalysisReadByte(ReadOnlySpan<byte> sends, IByteBlock content)
    {
        int length = 0;
        int itemLen = (sends.Length - 19) / 12;

        //添加错误代码校验
        if (content[17] + content[18] > 0)
        {
            return new(SiemensS7Resource.Localizer["ReturnError", content[17].ToString("X2"), content[18].ToString("X2")]) { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } };
        }

        if (content.Length >= 22)
        {
            //添加返回代码校验
            if (content[21] != 0xff)
            {
                return new(SiemensS7Resource.Localizer["ReturnCode", content[21].ToString("X2")]) { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } };
            }
        }

        for (int index = 0; index < itemLen; index++)
        {
            if (sends[22 + (index * 12)] >= (byte)S7WordLength.Word)
            {
                length += ((sends[23 + (index * 12)] * 256) + sends[24 + (index * 12)]) * 2;
            }
            else
            {
                length += (sends[23 + (index * 12)] * 256) + sends[24 + (index * 12)];
            }
        }

        if (content[20] != itemLen)
        {
            return new(SiemensS7Resource.Localizer["DataLengthError"]);
        }

        byte[] dataArray = new byte[length];
        int index1 = 0;
        int dataIndex = 0;
        for (int index2 = 21; index2 < content.Length; index2++)
        {
            if (index2 + 1 < content.Length)
            {
                int s7len;
                if (sends[22 + (index1 * 12)] >= (byte)S7WordLength.Word)
                {
                    s7len = ((sends[23 + (index1 * 12)] * 256) + sends[24 + (index1 * 12)]) * 2;
                }
                else
                {
                    s7len = (sends[23 + (index1 * 12)] * 256) + sends[24 + (index1 * 12)];
                }
                if (content[index2] == byte.MaxValue && content[index2 + 1] == 4)//Bit:3;Byte:4;Counter或者Timer:9
                {
                    Array.Copy(content.AsSegment().Array, index2 + 4, dataArray, dataIndex, s7len);
                    index2 += s7len + 3;
                    dataIndex += s7len;
                    index1++;
                }
                else if (content[index2] == byte.MaxValue && content[index2 + 1] == 9)//Counter或者Timer:9
                {
                    int num = (content[index2 + 2] * 256) + content[index2 + 3];
                    if (num % 3 == 0)
                    {
                        for (int index3 = 0; index3 < num / 3; index3++)
                        {
                            Array.Copy(content.AsSegment().Array, index2 + 5 + (3 * index3), dataArray, dataIndex, 2);
                            dataIndex += 2;
                        }
                    }
                    else
                    {
                        for (int index4 = 0; index4 < num / 5; index4++)
                        {
                            Array.Copy(content.AsSegment().Array, index2 + 7 + (5 * index4), dataArray, dataIndex, 2);
                            dataIndex += 2;
                        }
                    }
                    index2 += num + 4;
                    index1++;
                }
                else
                {
                    return new(SiemensS7Resource.Localizer["ValidateDataError", content[index2], GetCpuError(content[index2])])
                    { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } };
                }
            }
        }
        return new() { Content = new AdapterResult() { Content = dataArray, FilterResult = FilterResult.Success } }; ;
    }

    internal static OperResult<AdapterResult> AnalysisWrite(IByteBlock content)
    {
        if (content.Length < 22 || content[21] != byte.MaxValue)
        {
            if (content.Length < 22)
                return new("ValidateDataError") { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } };
            return new(SiemensS7Resource.Localizer["ValidateDataError", content[21], GetCpuError(content[21])])
            { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } };
        }
        else
        {
            return new() { Content = new AdapterResult() { Content = Array.Empty<byte>(), FilterResult = FilterResult.Success } }; ;
        }
    }

    #endregion 验证

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

    internal static void GetWriteBitCommand(ref ValueByteBlock valueByteBlock, SiemensAddress address, bool data)
    {
        int len = 1;
        int telegramLen = 16 + 19 + len;//最后的1是写入值的byte数量
        int parameterLen = 12 + 2;

        valueByteBlock.Write(S7_MULRW_HEADER);//19字节
        valueByteBlock[2] = (byte)(telegramLen / 256);
        valueByteBlock[3] = (byte)(telegramLen % 256);
        valueByteBlock[13] = (byte)(parameterLen / 256);
        valueByteBlock[14] = (byte)(parameterLen % 256);
        valueByteBlock[15] = (byte)((4 + len) / 256);
        valueByteBlock[16] = (byte)((4 + len) % 256);
        valueByteBlock[17] = 5;
        valueByteBlock[18] = (byte)1;
        //写入Item与读取大致相同
        valueByteBlock.Position = 19;
        valueByteBlock.WriteByte((byte)18);
        valueByteBlock.WriteByte((byte)10);
        valueByteBlock.WriteByte((byte)16);
        valueByteBlock.WriteByte((byte)S7WordLength.Bit);
        valueByteBlock.WriteByte((byte)(len / 256));
        valueByteBlock.WriteByte((byte)(len % 256));
        valueByteBlock.WriteByte((byte)(address.DbBlock / 256));
        valueByteBlock.WriteByte((byte)(address.DbBlock % 256));
        valueByteBlock.WriteByte((byte)(address.DataCode));
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) / 256 / 256));
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) / 256));
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) % 256));
        //后面跟的是写入的数据信息
        valueByteBlock.WriteByte((byte)(0));
        valueByteBlock.WriteByte((byte)(3));//Bit:3;Byte:4;Counter或者Timer:9
        valueByteBlock.WriteByte((byte)(len / 256));
        valueByteBlock.WriteByte((byte)(len % 256));
        valueByteBlock.WriteByte((byte)(data ? 1 : 0));
    }

    internal static void GetWriteByteCommand(ref ValueByteBlock valueByteBlock, SiemensAddress address, ReadOnlySpan<byte> data)
    {
        int len = data.Length;
        int telegramLen = 16 + 19 + len;//最后的1是写入值的byte数量
        int parameterLen = 12 + 2;

        valueByteBlock.Write(S7_MULRW_HEADER);//19字节

        valueByteBlock[2] = (byte)(telegramLen / 256);
        valueByteBlock[3] = (byte)(telegramLen % 256);
        valueByteBlock[13] = (byte)(parameterLen / 256);
        valueByteBlock[14] = (byte)(parameterLen % 256);
        valueByteBlock[15] = (byte)((4 + len) / 256);
        valueByteBlock[16] = (byte)((4 + len) % 256);
        valueByteBlock[17] = 5;
        valueByteBlock[18] = (byte)1;
        //写入Item与读取大致相同

        //写入Item与读取大致相同
        valueByteBlock.Position = 19;
        valueByteBlock.WriteByte((byte)18);
        valueByteBlock.WriteByte((byte)10);
        valueByteBlock.WriteByte((byte)16);
        valueByteBlock.WriteByte((byte)S7WordLength.Byte);
        valueByteBlock.WriteByte((byte)(len / 256));
        valueByteBlock.WriteByte((byte)(len % 256));
        valueByteBlock.WriteByte((byte)(address.DbBlock / 256));
        valueByteBlock.WriteByte((byte)(address.DbBlock % 256));
        valueByteBlock.WriteByte((byte)(address.DataCode));
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) / 256 / 256));
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) / 256));
        valueByteBlock.WriteByte((byte)((address.AddressStart + address.BitCode) % 256));
        //后面跟的是写入的数据信息
        valueByteBlock.WriteByte((byte)(0));
        valueByteBlock.WriteByte((byte)(4));//Bit:3;Byte:4;Counter或者Timer:9
        valueByteBlock.WriteByte((byte)(len * 8 / 256));
        valueByteBlock.WriteByte((byte)(len * 8 % 256));
        valueByteBlock.Write(data);
    }

    internal static async ValueTask<OperResult<string>> ReadStringAsync(SiemensS7Master plc, string address, Encoding encoding, CancellationToken cancellationToken)
    {
        //先读取一次获取长度，再读取实际值
        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            var result1 = await plc.ReadAsync(address, 2, cancellationToken).ConfigureAwait(false);
            if (!result1.IsSuccess)
            {
                return new OperResult<string>(result1);
            }
            if (result1.Content[0] == (byte)0 || result1.Content[0] == byte.MaxValue)
            {
                return new OperResult<string>(SiemensS7Resource.Localizer["NotString"]);
            }
            var result2 = await plc.ReadAsync(address, 2 + result1.Content[1], cancellationToken).ConfigureAwait(false);
            if (!result2.IsSuccess)
            {
                return new OperResult<string>(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content, 2, result2.Content.Length - 2));
            }
        }
        else
        {
            var result1 = await plc.ReadAsync(address, 1, cancellationToken).ConfigureAwait(false);
            if (!result1.IsSuccess)
                return new OperResult<string>(result1);
            var result2 = await plc.ReadAsync(address, 1 + result1.Content[0], cancellationToken).ConfigureAwait(false);
            if (!result2.IsSuccess)
            {
                return new OperResult<string>(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content, 1, result2.Content.Length - 1));
            }
        }
    }

    internal static async ValueTask<OperResult> WriteAsync(SiemensS7Master plc, string address, string value, Encoding encoding)
    {
        value ??= string.Empty;
        byte[] inBytes = encoding.GetBytes(value);
        if (plc.SiemensS7Type != SiemensTypeEnum.S200Smart)
        {
            var result = await plc.ReadAsync(address, 2).ConfigureAwait(false);
            if (!result.IsSuccess) return result;
            if (result.Content[0] == byte.MaxValue) return new OperResult<string>(SiemensS7Resource.Localizer["NotString"]);
            if (result.Content[0] == 0) result.Content[0] = 254;
            if (value.Length > result.Content[0]) return new OperResult<string>(SiemensS7Resource.Localizer["WriteDataLengthMore"]);
            return await plc.WriteAsync(
                address,
                DataTransUtil.SpliceArray(new byte[2] { result.Content[0], (byte)value.Length },
                inBytes
                )).ConfigureAwait(false);
        }
        return await plc.WriteAsync(address, DataTransUtil.SpliceArray<byte>(new byte[1]
        {
    (byte) value.Length
        }, inBytes)).ConfigureAwait(false);
    }

    private static string GetCpuError(ushort Error)
    {
        return Error switch
        {
            0x05 => SiemensS7Resource.Localizer["ERROR1"],
            0x06 => SiemensS7Resource.Localizer["ERROR2"],
            0x07 => SiemensS7Resource.Localizer["ERROR3"],
            0x0a or 0xd209 => SiemensS7Resource.Localizer["ERROR4"],
            0x8500 => SiemensS7Resource.Localizer["ERROR5"],
            0xdc01 => SiemensS7Resource.Localizer["ERROR6"],
            0x8104 => SiemensS7Resource.Localizer["ERROR7"],
            0xd241 => SiemensS7Resource.Localizer["ERROR8"],
            0xd602 => SiemensS7Resource.Localizer["ERROR9"],
            0xd604 or 0xd605 => SiemensS7Resource.Localizer["ERROR10"],
            _ => "Unknown",
        };
        ;
    }
}
