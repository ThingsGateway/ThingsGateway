using Microsoft.Extensions.Logging;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Extension;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Modbus
{
    internal static class ModbusHelper
    {
        internal static OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(this List<CollectVariableRunTime> deviceVariables, ILogger _logger, IThingsGatewayBitConverter byteConverter, int MaxPack)
        {
            var result = new List<DeviceVariableSourceRead>();
            try
            {
                //需要先剔除额外信息，比如dataformat等
                foreach (var item in deviceVariables)
                {
                    var address = item.VariableAddress;

                    IThingsGatewayBitConverter transformParameter = ByteConverterHelper.GetTransByAddress(
                     ref address, byteConverter, out int length, out BcdFormat bCDFormat);
                    item.ThingsGatewayBitConverter = transformParameter;
                    item.StringLength = length;
                    item.StringBcdFormat = bCDFormat;
                    item.VariableAddress = address;

                    int bitIndex = 0;
                    string[] addressSplits = new string[] { address };
                    if (address.IndexOf('.') > 0)
                    {
                        addressSplits = address.SplitDot();
                        try
                        {
                            bitIndex = Convert.ToInt32(addressSplits.Last());

                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "自动分包方法获取Bit失败");
                        }
                    }
                    item.Index = bitIndex;
                }

                //按读取间隔分组
                var tags = deviceVariables.GroupBy(it => it.IntervalTime);
                foreach (var item in tags)
                {
                    Dictionary<ModbusAddress, CollectVariableRunTime> map = item.ToDictionary(it =>
                    {
                        var lastLen = it.DataTypeEnum.GetByteLength();
                        if (lastLen <= 0)
                        {
                            if (it.DataTypeEnum.GetNetType() == typeof(bool))
                            {
                                lastLen = 2;
                            }
                            else if (it.DataTypeEnum.GetNetType() == typeof(string))
                            {
                                lastLen = it.StringLength;
                            }
                            else if (it.DataTypeEnum.GetNetType() == typeof(object))
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
                            result.AddRange(tempResult.Content);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "自动分包失败");
            }
            return OperResult.CreateSuccessResult(result);
        }

        private static OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(Dictionary<ModbusAddress, CollectVariableRunTime> addressList, int functionCode, int timeInterval, int MaxPack)
        {
            List<DeviceVariableSourceRead> sourceReads = new List<DeviceVariableSourceRead>();
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
                    Length = sourceLen.ToString()
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
            return OperResult.CreateSuccessResult(sourceReads);
        }

    }
}
