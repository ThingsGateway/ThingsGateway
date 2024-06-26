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

    public static List<List<SiemensAddress>> GroupByLength(SiemensAddress[] a, int pduLength)
    {
        List<List<SiemensAddress>> groups = new List<List<SiemensAddress>>();
        List<SiemensAddress> sortedItems = a.OrderByDescending(item => item.Length).ToList(); // 按长度降序排序

        while (sortedItems.Any())
        {
            List<SiemensAddress> currentGroup = new List<SiemensAddress>();
            int currentGroupLength = 0;

            for (int i = 0; i < sortedItems.Count; i++)
            {
                SiemensAddress item = sortedItems[i];
                if (currentGroupLength + item.Length <= pduLength) // 如果可以添加到当前组
                {
                    currentGroup.Add(item);
                    currentGroupLength += item.Length;
                    sortedItems.RemoveAt(i); // 从列表中移除已添加到组的项
                    i--; // 因为我们移除了一个元素，所以索引需要回退
                }
                else if (i == sortedItems.Count - 1) // 如果这是最后一个元素且不能添加到当前组
                {
                    // 创建一个新组并添加这个元素
                    groups.Add(new List<SiemensAddress> { item });
                    sortedItems.RemoveAt(i);
                }
            }

            if (currentGroup.Any()) // 如果当前组不为空
            {
                groups.Add(currentGroup);
            }
        }

        return groups;
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
                DataTransUtil.SpliceArray([result.Content[0], (byte)value.Length],
                inBytes
                )).ConfigureAwait(false);
        }
        return await plc.WriteAsync(address, DataTransUtil.SpliceArray<byte>(
        [
    (byte) value.Length
        ], inBytes)).ConfigureAwait(false);
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
