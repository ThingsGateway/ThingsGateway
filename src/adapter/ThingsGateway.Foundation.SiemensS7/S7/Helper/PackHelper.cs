//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.SiemensS7;

internal static class PackHelper
{
    /// <summary>
    /// 加载源数据并打包成连续读取的数据包列表
    /// </summary>
    /// <typeparam name="T">变量源类型</typeparam>
    /// <param name="device">SiemensS7Master实例，表示Siemens S7主控设备</param>
    /// <param name="deviceVariables">IVariable接口列表，表示设备的变量列表</param>
    /// <param name="maxPack">int，最大打包长度</param>
    /// <param name="defaultIntervalTime">int，默认间隔时间</param>
    /// <returns>List&lt;T&gt;，包含打包后的源数据列表</returns>
    public static List<T> LoadSourceRead<T>(SiemensS7Master device, IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime) where T : IVariableSource, new()
    {
        var byteConverter = device.ThingsGatewayBitConverter;
        var result = new List<T>();

        // 需要先剔除额外信息，比如dataformat等
        foreach (var item in deviceVariables)
        {
            var address = item.RegisterAddress;

            // 根据地址获取转换参数
            IThingsGatewayBitConverter transformParameter = byteConverter.GetTransByAddress(ref address);
            item.ThingsGatewayBitConverter = transformParameter;
            item.Index = 0;
            if (item.DataType == DataTypeEnum.Boolean)
                item.Index = device.GetBitOffsetDefault(address);
        }

        // 按读取间隔分组
        var tags = deviceVariables.GroupBy(it => it.IntervalTime ?? defaultIntervalTime);

        foreach (var item in tags)
        {
            // 使用字典存储地址和对应的变量
            Dictionary<SiemensAddress, IVariable> map = item.ToDictionary(it =>
            {
                // 解析SiemensAddress对象
                var s7Address = SiemensAddress.ParseFrom(it.RegisterAddress);
                int lastLen = it.DataType.GetByteLength();

                // 处理特殊情况下的长度
                if (lastLen <= 0)
                {
                    switch (it.DataType)
                    {
                        case DataTypeEnum.String:
                            if (it.ThingsGatewayBitConverter.StringLength == null)
                            {
                                throw new Exception(DefaultResource.Localizer["StringTypePackError"]);
                            }
                            else
                            {
                                // 根据不同的设备类型和地址类型，调整字符串长度
                                if (device.SiemensS7Type == SiemensTypeEnum.S200Smart)
                                {
                                    if (it.ThingsGatewayBitConverter.IsVariableStringLength)
                                    {
                                        // 字符串在S200Smart中，第一个字节不属于实际内容
                                        it.Index += 1;
                                        lastLen = it.ThingsGatewayBitConverter.StringLength.Value + 1;
                                    }
                                    else
                                    {
                                        lastLen = it.ThingsGatewayBitConverter.StringLength.Value;
                                    }
                                }
                                else
                                {
                                    if (it.ThingsGatewayBitConverter.IsVariableStringLength)
                                    {
                                        // 字符串在S7中，前两个字节不属于实际内容
                                        it.Index += 2;
                                        lastLen = it.ThingsGatewayBitConverter.StringLength.Value + 2;
                                    }
                                    else
                                    {
                                        lastLen = it.ThingsGatewayBitConverter.StringLength.Value;
                                    }
                                }
                            }
                            break;

                        default:
                            lastLen = 1;
                            break;
                    }
                }

                // 对于计数器和定时器，长度需调整
                if ((s7Address.DataCode == (byte)S7WordLength.Counter || s7Address.DataCode == (byte)S7WordLength.Timer) && lastLen == 1)
                {
                    lastLen = 2;
                }
                if (it.ThingsGatewayBitConverter.ArrayLength != null)
                {
                    if (it.DataType == DataTypeEnum.Boolean)
                    {
                        var len = Math.Ceiling((decimal)it.ThingsGatewayBitConverter.ArrayLength.Value / 8);
                        lastLen *= (int)len;
                    }
                    else
                    {
                        lastLen *= it.ThingsGatewayBitConverter.ArrayLength.Value;
                    }
                }

                // 将变量的应读取长度写入SiemensAddress实例中
                s7Address.Length = lastLen;
                return s7Address;
            });

            // 获取变量的地址列表
            var s7AddressList = map.Keys.Where(a => a != null);

            // 获取S7数据代码
            var functionCodes = s7AddressList.Select(t => t.DataCode).Distinct();

            foreach (var functionCode in functionCodes)
            {
                // 获取相同数据代码的变量集合
                var s7AddressSameFunList = s7AddressList.Where(t => t.DataCode == functionCode);

                // 获取相同数据代码的变量集合中的不同DB块
                var dbNumbers = s7AddressSameFunList.Select(t => t.DbBlock).Distinct();

                foreach (var stationNumber in dbNumbers)
                {
                    var addressList = s7AddressSameFunList.Where(t => t.DbBlock == stationNumber)
                        .ToDictionary(t => t, t => map[t]);

                    // 对相同数据代码和站号的变量进行分配连读包
                    var tempResult = LoadSourceRead<T>(addressList!, functionCode, item.Key, device);

                    // 添加到总连读包中
                    result.AddRange(tempResult);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 从地址列表中加载源数据并将其打包成连续读取的数据包列表。
    /// </summary>
    /// <typeparam name="T">变量源类型</typeparam>
    /// <param name="addressList">SiemensAddress和IVariable的字典，表示地址和对应的变量</param>
    /// <param name="functionCode">数据代码</param>
    /// <param name="intervalTime">间隔时间</param>
    /// <param name="siemensS7Net">SiemensS7Master实例，表示Siemens S7主控设备</param>
    /// <returns>包含打包后的源数据列表</returns>
    private static List<T> LoadSourceRead<T>(Dictionary<SiemensAddress, IVariable> addressList, int functionCode, int intervalTime, SiemensS7Master siemensS7Net) where T : IVariableSource, new()
    {
        List<T> sourceReads = new(); // 用于存储打包后的源数据列表

        // 实际地址与长度排序
        var addresses = addressList.Keys.OrderBy(it =>
        {
            int address = 0;
            if (it.DataCode == (byte)S7WordLength.Counter || it.DataCode == (byte)S7WordLength.Timer)
            {
                address = it.AddressStart * 2; // 如果是计数器或计时器，则地址乘以2
            }
            else
            {
                address = it.AddressStart / 8; // 否则地址除以8
            }
            return address + it.Length; // 返回排序后的地址加上长度
        }).ToList();

        var minAddress = addresses.First().AddressStart; // 获取最小地址
        var maxAddress = addresses.Last().AddressStart; // 获取最大地址

        while (maxAddress >= minAddress) // 循环，直到最大地址小于最小地址
        {
            int readLength = siemensS7Net.PduLength == 0 ? 200 : siemensS7Net.PduLength; // 读取长度为PDU长度减去28，避免超出限制

            List<SiemensAddress> tempAddresses = new(); // 临时地址列表用于存储分配给单个数据包的地址

            if (functionCode == (byte)S7WordLength.Counter || functionCode == (byte)S7WordLength.Timer)
            {
                // 如果是计数器或计时器
                tempAddresses = addresses.Where(t => t.AddressStart >= minAddress && ((t.AddressStart) + t.Length) <= ((minAddress) + readLength / 2)).ToList();

                while ((tempAddresses.Last().AddressStart * 2) + tempAddresses.Last().Length - (tempAddresses.First().AddressStart * 2) > readLength)
                {
                    tempAddresses.Remove(tempAddresses.Last()); // 移除超出限制的地址
                }
            }
            else
            {
                tempAddresses = addresses.Where(t => t.AddressStart >= minAddress && ((t.AddressStart) + t.Length) <= ((minAddress) + readLength * 8)).ToList();

                while ((tempAddresses.Last().AddressStart / 8) + tempAddresses.Last().Length - (tempAddresses.First().AddressStart / 8) > readLength)
                {
                    tempAddresses.Remove(tempAddresses.Last());
                }
            }

            // 计算寄存器长度
            int lastAddress = 0;
            int firstAddress = 0;

            if (functionCode == (byte)S7WordLength.Counter || functionCode == (byte)S7WordLength.Timer)
            {
                lastAddress = tempAddresses.Last().AddressStart * 2;
                firstAddress = tempAddresses.First().AddressStart * 2;
            }
            else
            {
                lastAddress = tempAddresses.Last().AddressStart / 8;
                firstAddress = tempAddresses.First().AddressStart / 8;
            }

            var sourceLen = lastAddress + tempAddresses.Last().Length - firstAddress; // 计算源长度

            T sourceRead = new() // 创建一个新的源读取对象
            {
                TimeTick = new(intervalTime), // 设置时间戳
                RegisterAddress = tempAddresses.OrderBy(it => it.AddressStart).First().ToString(), // 获取地址并按地址排序
                Length = sourceLen // 设置源长度
            };

            foreach (var item in tempAddresses) // 遍历临时地址列表
            {
                var readNode = addressList[item]; // 获取地址对应的变量

                if (functionCode == (byte)S7WordLength.Counter || functionCode == (byte)S7WordLength.Timer)
                {
                    if (readNode.DataType == DataTypeEnum.Boolean)
                    {
                        // 计算索引（针对计数器和计时器的布尔型变量）
                        readNode.Index = (((item.AddressStart * 2) - (tempAddresses.First().AddressStart * 2)) * 8) + readNode.Index;
                    }
                    else
                    {
                        // 计算索引（针对计数器和计时器的非布尔型变量）
                        readNode.Index = (item.AddressStart * 2) - (tempAddresses.First().AddressStart * 2) + readNode.Index;
                    }
                }
                else
                {
                    if (readNode.DataType == DataTypeEnum.Boolean)
                    {
                        // 计算索引（针对非计数器和计时器的布尔型变量）
                        readNode.Index = (((item.AddressStart / 8) - (tempAddresses.First().AddressStart / 8)) * 8) + readNode.Index;
                    }
                    else
                    {
                        // 计算索引（针对非计数器和计时器的非布尔型变量）
                        readNode.Index = (item.AddressStart / 8) - (tempAddresses.First().AddressStart / 8) + readNode.Index;
                    }
                }

                sourceRead.AddVariable(readNode); // 将变量添加到源读取对象中
                addresses.Remove(item); // 从地址列表中移除已处理的地址
            }

            sourceReads.Add(sourceRead); // 将源读取对象添加到源数据列表中

            if (addresses.Count > 0)
            {
                minAddress = addresses.First().AddressStart; // 更新最小地址
            }
            else
            {
                break; // 如果地址列表为空，则退出循环
            }
        }

        return sourceReads; // 返回打包后的源数据列表
    }
}
