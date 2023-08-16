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

using ThingsGateway.Application;
using ThingsGateway.Application.Extensions;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Modbus;

internal static class ModbusHelper
{
    internal static List<DeviceVariableSourceRead> LoadSourceRead(this List<DeviceVariableRunTime> deviceVariables, IReadWriteDevice device, int MaxPack)
    {
        var byteConverter = device.ThingsGatewayBitConverter;
        var result = new List<DeviceVariableSourceRead>();
        //需要先剔除额外信息，比如dataformat等
        foreach (var item in deviceVariables)
        {
            var address = item.VariableAddress;

            IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, byteConverter);
            item.ThingsGatewayBitConverter = transformParameter;
            item.VariableAddress = address;
            item.Index = device.GetBitOffset(item.VariableAddress);
        }

        //按读取间隔分组
        var tags = deviceVariables.GroupBy(it => it.IntervalTime);
        foreach (var item in tags)
        {
            Dictionary<ModbusAddress, DeviceVariableRunTime> map = item.ToDictionary(it =>
            {
                var lastLen = it.DataTypeEnum.GetByteLength();
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
                        lastLen = 2;
                    }
                }
                var address = it.VariableAddress;
                if (address.IndexOf('.') > 0)
                {
                    var addressSplits = address.SplitDot();

                    address = addressSplits.RemoveLast(1).ArrayToString(".");
                }

                var result = new ModbusAddress(address, (ushort)lastLen);
                if (result == null)
                {
                }

                return result;
            });

            //获取变量的地址
            var modbusAddressList = map.Keys.ToList();

            //获取功能码
            var functionCodes = modbusAddressList.Select(t => t.ReadFunction).Distinct();
            foreach (var functionCode in functionCodes)
            {
                var modbusAddressSameFunList = modbusAddressList
                    .Where(t => t.ReadFunction == functionCode).ToList();
                var stationNumbers = modbusAddressSameFunList
                    .Select(t => t.Station).Distinct().ToList();
                foreach (var stationNumber in stationNumbers)
                {
                    var addressList = modbusAddressSameFunList.Where(t => t.Station == stationNumber)
                        .ToDictionary(t => t, t => map[t]);
                    var tempResult = LoadSourceRead(addressList, functionCode, item.Key, MaxPack);
                    result.AddRange(tempResult);
                }
            }
        }
        return result;
    }

    private static List<DeviceVariableSourceRead> LoadSourceRead(Dictionary<ModbusAddress, DeviceVariableRunTime> addressList, int functionCode, int timeInterval, int MaxPack)
    {
        List<DeviceVariableSourceRead> sourceReads = new();
        //按地址和长度排序
        var orderByAddressAndLen = addressList.Keys.OrderBy(it => it.AddressStart + Math.Ceiling(it.Length / 2.0)).ToList();
        //按地址和长度排序
        var orderByAddress = addressList.Keys.OrderBy(it => it.AddressStart).ToList();
        //地址最小，在循环中更改
        var minAddress = orderByAddress.First().AddressStart;
        //地址最大
        var maxAddress = orderByAddress.Last().AddressStart;

        while (maxAddress >= minAddress)
        {
            //最大的打包长度
            int readLength = MaxPack;
            if (functionCode == 1 || functionCode == 2)
            {
                readLength = MaxPack * 8 * 2;
            }
            //获取当前的一组打包地址信息，
            var tempAddressAndLen = orderByAddressAndLen.Where(t => (t.AddressStart + (t.Length / 2.0)) <= minAddress + readLength).ToList();
            //起始地址
            var startAddress = tempAddressAndLen.OrderBy(it => it.AddressStart).First();
            //读取寄存器长度
            var sourceLen = tempAddressAndLen.Last().AddressStart + (int)Math.Ceiling(tempAddressAndLen.Last().Length / 2.0) - startAddress.AddressStart;

            DeviceVariableSourceRead sourceRead = new(timeInterval)
            {
                //这里只需要根据地址排序的第一个地址，作为实际打包报文中的起始地址
                Address = startAddress.ToString(),
                Length = sourceLen
            };
            foreach (var item in tempAddressAndLen)
            {
                var readNode = addressList[item];
                if ((functionCode == -1 || functionCode == 3 || functionCode == 4) &&
                    readNode.DataTypeEnum == DataTypeEnum.Boolean)
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


                sourceRead.DeviceVariables.Add(readNode);
                orderByAddressAndLen.Remove(item);
                orderByAddress.Remove(item);
            }

            sourceReads.Add(sourceRead);
            if (orderByAddressAndLen.Count > 0)
                minAddress = orderByAddress.First().AddressStart;
            else
                break;
        }
        return sourceReads;
    }

}
