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

using System.Collections.Generic;
using System.Linq;

using ThingsGateway.Application;
using ThingsGateway.Application.Extensions;
using ThingsGateway.Foundation;

namespace ThingsGateway.Siemens
{
    internal static class S7Helper
    {
        internal static List<DeviceVariableSourceRead> LoadSourceRead(this List<DeviceVariableRunTime> deviceVariables, SiemensS7PLC siemensS7Net)
        {
            var byteConverter = siemensS7Net.ThingsGatewayBitConverter;
            var result = new List<DeviceVariableSourceRead>();

            //需要先剔除额外信息，比如dataformat等
            foreach (var item in deviceVariables)
            {
                var address = item.VariableAddress;

                IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, byteConverter);
                item.ThingsGatewayBitConverter = transformParameter;
                //item.VariableAddress = address;

                item.Index = siemensS7Net.GetBitOffset(item.VariableAddress);
            }
            //按读取间隔分组
            var tags = deviceVariables.GroupBy(it => it.IntervalTime);
            foreach (var item in tags)
            {
                Dictionary<SiemensAddress, DeviceVariableRunTime> map = item.ToDictionary(it =>
                {

                    var lastLen = it.DataTypeEnum.GetByteLength(); ;
                    if (lastLen <= 0)
                    {
                        if (it.DataTypeEnum.GetSystemType() == typeof(bool))
                        {
                            lastLen = 2;
                        }
                        else if (it.DataTypeEnum.GetSystemType() == typeof(string))
                        {
                            lastLen = it.ThingsGatewayBitConverter.StringLength;
                        }
                        else if (it.DataTypeEnum.GetSystemType() == typeof(object))
                        {
                            lastLen = 1;
                        }
                    }

                    var s7Address = SiemensAddress.ParseFrom(it.VariableAddress);
                    if (s7Address.IsSuccess)
                    {
                        if ((s7Address.Content.DataCode == (byte)S7WordLength.Counter || s7Address.Content.DataCode == (byte)S7WordLength.Timer) && lastLen == 1)
                        {
                            lastLen = 2;
                        }
                    }
                    //这里把每个变量的应读取长度都写入变量地址实体中
                    return SiemensAddress.ParseFrom(it.VariableAddress, (ushort)lastLen).Content;

                });

                //获取变量的地址
                var modbusAddressList = map.Keys.ToList();

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
                        var tempResult = LoadSourceRead(addressList, functionCode, item.Key, siemensS7Net);
                        //添加到总连读包
                        result.AddRange(tempResult);
                    }
                }
            }
            return result;
        }

        private static List<DeviceVariableSourceRead> LoadSourceRead(Dictionary<SiemensAddress, DeviceVariableRunTime> addressList, int functionCode, int timeInterval, SiemensS7PLC siemensS7Net)
        {

            List<DeviceVariableSourceRead> sourceReads = new();
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
                DeviceVariableSourceRead sourceRead = new(timeInterval)
                {
                    Address = tempAddress.OrderBy(it => it.AddressStart).First().ToString(),
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
                    sourceRead.DeviceVariables.Add(readNode);
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
}
