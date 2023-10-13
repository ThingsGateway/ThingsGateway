#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Foundation.Adapter.Siemens;

internal static class PackHelper
{
    public static List<T> LoadSourceRead<T, T2>(SiemensS7PLC siemensS7Net, List<T2> deviceVariables, int maxPack) where T : IDeviceVariableSourceRead<IDeviceVariableRunTime>, new() where T2 : IDeviceVariableRunTime, new()
    {
        var byteConverter = siemensS7Net.ThingsGatewayBitConverter;
        var result = new List<T>();

        //需要先剔除额外信息，比如dataformat等
        foreach (var item in deviceVariables)
        {
            var address = item.VariableAddress;

            IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, byteConverter);
            item.ThingsGatewayBitConverter = transformParameter;
            //item.VariableAddress = address;//需要使用过滤后的地址
            item.Index = siemensS7Net.GetBitOffset(item.VariableAddress);
        }
        //按读取间隔分组
        var tags = deviceVariables.GroupBy(it => it.IntervalTime);
        foreach (var item in tags)
        {
            Dictionary<SiemensAddress, T2> map = item.ToDictionary(it =>
            {
                try
                {
                    var s7Address = SiemensAddress.ParseFrom(it.VariableAddress);
                    var lastLen = it.DataTypeEnum.GetByteLength();
                    if (lastLen <= 0)
                    {
                        switch (it.DataTypeEnum)
                        {
                            case DataTypeEnum.String:
                                if (it.ThingsGatewayBitConverter.Length == null)
                                {
                                    throw new("数据类型为字符串时，必须指定字符串长度，才能进行打包");
                                }
                                else
                                {
                                    if (siemensS7Net.CurrentPlc == SiemensEnum.S200Smart)
                                    {
                                        if (s7Address.IsWString)
                                        {
                                            //字符串在S200Smart中，第一个字节不属于实际内容
                                            it.Index += 1;
                                            lastLen = it.ThingsGatewayBitConverter.Length.Value + 1;
                                        }
                                        else
                                        {
                                            lastLen = it.ThingsGatewayBitConverter.Length.Value;
                                        }
                                    }
                                    else
                                    {
                                        if (s7Address.IsWString)
                                        {
                                            //字符串在S7中，前两个字节不属于实际内容
                                            it.Index += 2;
                                            lastLen = it.ThingsGatewayBitConverter.Length.Value + 2;
                                        }
                                        else
                                        {
                                            lastLen = it.ThingsGatewayBitConverter.Length.Value;
                                        }
                                    }
                                }
                                break;
                            default:
                                lastLen = 1;
                                break;
                        }
                    }

                    if ((s7Address.DataCode == (byte)S7WordLength.Counter || s7Address.DataCode == (byte)S7WordLength.Timer) && lastLen == 1)
                    {
                        lastLen = 2;
                    }
                    if (it.ThingsGatewayBitConverter.Length != null && it.DataTypeEnum != DataTypeEnum.String)
                    {
                        lastLen *= it.ThingsGatewayBitConverter.Length.Value;
                    }

                    //这里把每个变量的应读取长度都写入变量地址实体中
                    s7Address.Length = lastLen;
                    return s7Address;
                }
                catch (Exception ex)
                {
                    siemensS7Net.Logger.LogWarning(ex, "解析地址错误");
                }
                return null;
            });

            //获取变量的地址
            var modbusAddressList = map.Keys.Where(a => a != null).ToList();

            //获取S7数据代码
            var functionCodes = modbusAddressList.Select(t => t.DataCode).Distinct();
            foreach (var functionCode in functionCodes)
            {
                //相同数据代码的变量集合
                var modbusAddressSameFunList = modbusAddressList
                    .Where(t => t.DataCode == functionCode);
                //相同数据代码的变量集合中的不同DB块
                var stationNumbers = modbusAddressSameFunList
                    .Select(t => t.DbBlock).Distinct();
                foreach (var stationNumber in stationNumbers)
                {
                    var addressList = modbusAddressSameFunList.Where(t => t.DbBlock == stationNumber)
                        .ToDictionary(t => t, t => map[t]);
                    //循环对数据代码，站号都一样的变量进行分配连读包
                    var tempResult = LoadSourceRead<T, T2>(addressList, functionCode, item.Key, siemensS7Net);
                    //添加到总连读包
                    result.AddRange(tempResult);
                }
            }
        }
        return result;
    }

    private static List<T> LoadSourceRead<T, T2>(Dictionary<SiemensAddress, T2> addressList, int functionCode, int intervalTime, SiemensS7PLC siemensS7Net) where T : IDeviceVariableSourceRead<IDeviceVariableRunTime>, new() where T2 : IDeviceVariableRunTime, new()
    {

        List<T> sourceReads = new();
        //实际地址与长度排序
        var addresss = addressList.Keys.OrderBy(it =>
        {
            int address = 0;
            if (it.DataCode == (byte)S7WordLength.Counter || it.DataCode == (byte)S7WordLength.Timer)
            {
                address = it.AddressStart * 2;
            }
            else
            {
                address = it.AddressStart / 8;
            }
            return address + it.Length;
        }).ToList();
        var minAddress = addresss.First().AddressStart;
        var maxAddress = addresss.Last().AddressStart;
        while (maxAddress >= minAddress)
        {
            //这里直接避免末位变量长度超限的情况，pdu长度-8
            int readLength = siemensS7Net.PDULength == 0 ? 200 : siemensS7Net.PDULength - 8;
            List<SiemensAddress> tempAddress = new();
            if (functionCode == (byte)S7WordLength.Counter || functionCode == (byte)S7WordLength.Timer)
            {
                tempAddress = addresss.Where(t => t.AddressStart >= minAddress && ((t.AddressStart) + t.Length) <= ((minAddress) + readLength)).ToList();
                while ((tempAddress.Last().AddressStart * 2) + tempAddress.Last().Length - (tempAddress.First().AddressStart * 2) > readLength)
                {
                    tempAddress.Remove(tempAddress.Last());
                }
            }
            else
            {
                tempAddress = addresss.Where(t => t.AddressStart >= minAddress && ((t.AddressStart) + t.Length) <= ((minAddress) + readLength)).ToList();
                while ((tempAddress.Last().AddressStart / 8) + tempAddress.Last().Length - (tempAddress.First().AddressStart / 8) > readLength)
                {
                    tempAddress.Remove(tempAddress.Last());
                }
            }

            //读取寄存器长度
            int lastAddress = 0;
            int firstAddress = 0;
            if (functionCode == (byte)S7WordLength.Counter || functionCode == (byte)S7WordLength.Timer)
            {
                lastAddress = tempAddress.Last().AddressStart * 2;
                firstAddress = tempAddress.First().AddressStart * 2;
            }
            else
            {
                lastAddress = tempAddress.Last().AddressStart / 8;
                firstAddress = tempAddress.First().AddressStart / 8;
            }
            var sourceLen = lastAddress + tempAddress.Last().Length - firstAddress;
            T sourceRead = new()
            {
                TimerTick = new(intervalTime),
                VariableAddress = tempAddress.OrderBy(it => it.AddressStart).First().ToString(),
                Length = sourceLen
            };
            foreach (var item in tempAddress)
            {
                var readNode = addressList[item];
                if (functionCode == (byte)S7WordLength.Counter || functionCode == (byte)S7WordLength.Timer)
                {
                    if (readNode.DataTypeEnum == DataTypeEnum.Boolean)
                    {
                        readNode.Index = (((item.AddressStart * 2) - (tempAddress.First().AddressStart * 2)) * 8) + readNode.Index;
                    }
                    else
                    {
                        readNode.Index = (item.AddressStart * 2) - (tempAddress.First().AddressStart * 2) + readNode.Index;
                    }
                }
                else
                {
                    if (readNode.DataTypeEnum == DataTypeEnum.Boolean)
                    {
                        readNode.Index = (((item.AddressStart / 8) - (tempAddress.First().AddressStart / 8)) * 8) + readNode.Index;
                    }
                    else
                    {
                        readNode.Index = (item.AddressStart / 8) - (tempAddress.First().AddressStart / 8) + readNode.Index;
                    }
                }
                sourceRead.DeviceVariableRunTimes.Add(readNode);
                addresss.Remove(item);
            }
            sourceReads.Add(sourceRead);
            if (addresss.Count > 0)
                minAddress = addresss.First().AddressStart;
            else
                break;
        }
        return sourceReads;
    }

}
