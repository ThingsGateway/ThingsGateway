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

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.Adapter.Modbus;

/// <summary>
/// PackHelper
/// </summary>
public class PackHelper
{
    /// <summary>
    /// 打包变量，添加到<see href="deviceVariableSourceReads"></see>
    /// </summary>
    /// <param name="device"></param>
    /// <param name="deviceVariables"></param>
    /// <param name="maxPack">最大打包长度</param>
    /// <returns></returns>
    public static List<T> LoadSourceRead<T, T2>(IReadWrite device, List<T2> deviceVariables, int maxPack) where T : IDeviceVariableSourceRead<IDeviceVariableRunTime>, new() where T2 : IDeviceVariableRunTime, new()
    {
        if (deviceVariables == null)
            throw new ArgumentNullException(nameof(deviceVariables));

        var deviceVariableSourceReads = new List<T>();
        var byteConverter = device.ThingsGatewayBitConverter;
        //需要先剔除额外信息，比如dataformat等
        foreach (var item in deviceVariables)
        {
            var address = item.VariableAddress;
            IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, byteConverter);
            item.ThingsGatewayBitConverter = transformParameter;
            //item.VariableAddress = address;
            item.Index = device.GetBitOffset(item.VariableAddress);
        }
        var deviceVariableRunTimeGroups = deviceVariables.GroupBy(it => it.IntervalTime);
        foreach (var group in deviceVariableRunTimeGroups)
        {
            Dictionary<ModbusAddress, T2> map = group.ToDictionary(it =>
            {
                var lastLen = it.DataTypeEnum.GetByteLength();
                if (lastLen <= 0)
                {
                    switch (it.DataTypeEnum)
                    {
                        case DataTypeEnum.String:
                            lastLen = it.ThingsGatewayBitConverter.Length == null ? throw new("数据类型为字符串时，必须指定字符串长度，才能进行打包") : it.ThingsGatewayBitConverter.Length.Value;
                            break;
                        default:
                            lastLen = 2;
                            break;
                    }
                }
                if (it.ThingsGatewayBitConverter.Length != null && it.DataTypeEnum != DataTypeEnum.String)
                {
                    lastLen *= it.ThingsGatewayBitConverter.Length.Value;
                }

                var address = it.VariableAddress;
                if (address.IndexOf('.') > 0)
                {
                    var addressSplits = address.SplitDot();
                    address = addressSplits.RemoveLast(1).ArrayToString(".");
                }
                var result = ModbusAddress.ParseFrom(address);
                result.ByteLength = lastLen;
                return result;
            });

            //获取变量的地址
            var modbusAddressList = map.Keys;

            //获取功能码
            var functionCodes = modbusAddressList.Select(t => t.ReadFunction).Distinct();
            foreach (var functionCode in functionCodes)
            {
                var modbusAddressSameFunList = modbusAddressList.Where(t => t.ReadFunction == functionCode);
                var stationNumbers = modbusAddressSameFunList.Select(t => t.Station).Distinct();
                foreach (var stationNumber in stationNumbers)
                {
                    var addressList = modbusAddressSameFunList
                        .Where(t => t.Station == stationNumber)
                        .ToDictionary(t => t, t => map[t]);

                    var tempResult = LoadSourceRead<T, T2>(addressList, functionCode, group.Key, maxPack);
                    deviceVariableSourceReads.AddRange(tempResult);
                }
            }

        }

        return deviceVariableSourceReads;
    }

    private static List<T> LoadSourceRead<T, T2>(Dictionary<ModbusAddress, T2> addressList, int functionCode, int intervalTime, int maxPack) where T : IDeviceVariableSourceRead<IDeviceVariableRunTime>, new() where T2 : IDeviceVariableRunTime, new()
    {
        List<T> sourceReads = new();
        //按地址和长度排序
        var orderByAddressEnd = addressList.Keys.OrderBy(it => it.AddressEnd);
        //按地址和长度排序
        var orderByAddressStart = addressList.Keys.OrderBy(it => it.AddressStart);
        //地址最小，在循环中更改
        var minAddress = orderByAddressStart.First().AddressStart;
        //地址最大
        var maxAddress = orderByAddressStart.Last().AddressStart;

        while (maxAddress >= minAddress)
        {
            //最大的打包长度
            int readLength = maxPack;
            if (functionCode == 1 || functionCode == 2)
            {
                readLength = maxPack * 8 * 2;
            }


            //获取当前的一组打包地址信息，
            var tempAddressEnd = orderByAddressEnd.Where(t => t.AddressEnd <= minAddress + readLength).ToList();
            //起始地址
            var startAddress = tempAddressEnd.OrderBy(it => it.AddressStart).First();
            //读取寄存器长度
            var sourceLen = tempAddressEnd.Last().AddressEnd - startAddress.AddressStart;

            T sourceRead = new()
            {
                TimerTick = new TimerTick(intervalTime),
                //这里只需要根据地址排序的第一个地址，作为实际打包报文中的起始地址
                VariableAddress = startAddress.ToString(),
                Length = sourceLen
            };
            foreach (var item in tempAddressEnd)
            {
                var readNode = addressList[item];
                if ((functionCode == -1 || functionCode == 3 || functionCode == 4) && readNode.DataTypeEnum == DataTypeEnum.Boolean)
                {
                    readNode.Index = ((item.AddressStart - startAddress.AddressStart) * 16) + readNode.Index;
                }
                else
                {
                    if (functionCode == 1 || functionCode == 2)
                        readNode.Index = item.AddressStart - startAddress.AddressStart + readNode.Index;
                    else
                        readNode.Index = ((item.AddressStart - startAddress.AddressStart) * 2) + readNode.Index;
                }

                sourceRead.DeviceVariableRunTimes.Add(readNode);
                addressList.Remove(item);
            }

            sourceReads.Add(sourceRead);
            if (orderByAddressEnd.Count() > 0)
                minAddress = orderByAddressStart.First().AddressStart;
            else
                break;
        }
        return sourceReads;
    }

}
