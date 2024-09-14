//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Foundation.SiemensS7;

internal partial class SiemensHelper
{
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

    internal static string GetCpuError(ushort Error)
    {
        return Error switch
        {
            0x01 => SiemensS7Resource.Localizer["ERROR1"],
            0x03 => SiemensS7Resource.Localizer["ERROR3"],
            0x05 => SiemensS7Resource.Localizer["ERROR5"],
            0x06 => SiemensS7Resource.Localizer["ERROR6"],
            0x07 => SiemensS7Resource.Localizer["ERROR7"],
            0x0a => SiemensS7Resource.Localizer["ERROR10"],
            _ => "Unknown",
        };
        ;
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
            if (result1.Content[0] == 0 || result1.Content[0] == byte.MaxValue)
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
        return await plc.WriteAsync(address, DataTransUtil.SpliceArray([(byte)value.Length], inBytes)).ConfigureAwait(false);
    }
}
