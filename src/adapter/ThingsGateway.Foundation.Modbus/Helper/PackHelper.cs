//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// PackHelper
/// </summary>
public class PackHelper
{
    /// <summary>
    /// 打包变量，添加到<see cref="List{T}"/>
    /// </summary>
    /// <param name="device"></param>
    /// <param name="deviceVariables"></param>
    /// <param name="maxPack">最大打包长度</param>
    /// <param name="defaultIntervalTime">默认间隔时间</param>
    /// <param name="station">默认站点</param>
    /// <param name="dtuid">默认dtuid</param>
    /// <returns></returns>
    public static List<T> LoadSourceRead<T>(IProtocol device, IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime, byte? station = null, string dtuid = null) where T : IVariableSource, new()
    {
        // 检查参数是否为空
        if (deviceVariables == null)
            throw new ArgumentNullException(nameof(deviceVariables));

        // 创建用于存储读取结果的列表
        var deviceVariableSourceReads = new List<T>();

        // 获取设备的字节转换器
        var byteConverter = device.ThingsGatewayBitConverter;

        // 循环遍历设备变量列表，为每个变量设置转换器和索引
        foreach (var item in deviceVariables)
        {
            var address = item.RegisterAddress;
            // 获取与地址关联的转换器
            IThingsGatewayBitConverter transformParameter = byteConverter.GetTransByAddress(ref address);
            // 将转换器设置到变量中
            item.ThingsGatewayBitConverter = transformParameter;
            item.Index = 0;
            // 获取变量的位偏移量
            //if (item.DataType == DataTypeEnum.Boolean)
            item.Index = device.GetBitOffset(address);
            if (item.DataType == DataTypeEnum.Byte)
                item.Index += (item.Index % 2 == 0) ? 1 : -1;
        }

        // 按照时间间隔将变量分组
        var deviceVariableRunTimeGroups = deviceVariables.GroupBy(it => it.IntervalTime ?? defaultIntervalTime);
        foreach (var group in deviceVariableRunTimeGroups)
        {
            // 将变量分组转换为字典，键为 ModbusAddress
            Dictionary<ModbusAddress, IVariable> map = group.ToDictionary(it =>
            {
                // 计算变量的字节长度
                var lastLen = (int)it.DataType.GetByteLength();
                if (lastLen <= 0)
                {
                    // 如果长度小于等于0，则根据数据类型进行处理
                    switch (it.DataType)
                    {
                        case DataTypeEnum.String:
                            // 字符串类型需特殊处理
                            lastLen = (it.ThingsGatewayBitConverter.StringLength == null ? throw new(DefaultResource.Localizer["StringTypePackError"]) : it.ThingsGatewayBitConverter.StringLength.Value);
                            break;

                        default:
                            // 默认长度为2
                            lastLen = 2;
                            break;
                    }
                }
                // 如果是数组，需要乘以数组长度
                {
                    lastLen *= it.ThingsGatewayBitConverter.ArrayLength ?? 1;
                }

                // 解析地址，并设置字节长度
                var address = it.RegisterAddress;
                var result = ModbusAddress.ParseFrom(address, station, dtuid, isCache: false);
                result.Length = (ushort)lastLen;
                return result;
            });

            // 获取所有地址
            var modbusAddressList = map.Keys;
            // 获取相同站号相同SocketId的地址
            var socketIds = modbusAddressList.Select(t => t.SocketId).Distinct();
            foreach (var socketId in socketIds)
            {
                var modbusAddressSameSocketIdList = modbusAddressList.Where(t => t.SocketId == socketId);
                // 获取所有功能码
                var functionCodes = modbusAddressSameSocketIdList.Select(t => t.FunctionCode).Distinct();
                foreach (var functionCode in functionCodes)
                {
                    // 获取相同功能码的地址
                    var modbusAddressSameFunList = modbusAddressSameSocketIdList.Where(t => t.FunctionCode == functionCode);
                    // 获取相同站号的地址
                    var stationNumbers = modbusAddressSameFunList.Select(t => t.Station).Distinct();
                    foreach (var stationNumber in stationNumbers)
                    {
                        // 获取相同站号的地址
                        var modbusAddressSameStationList = modbusAddressSameFunList.Where(t => t.Station == stationNumber);
                        var addressList = modbusAddressSameStationList.ToDictionary(t => t, t => map[t]);

                        // 加载并添加结果
                        var tempResult = LoadSourceRead<T>(addressList, functionCode, group.Key, maxPack);
                        deviceVariableSourceReads.AddRange(tempResult);
                    }
                }
            }
        }

        // 返回结果列表
        return deviceVariableSourceReads;
    }

    private static List<T> LoadSourceRead<T>(Dictionary<ModbusAddress, IVariable> addressList, int functionCode, int intervalTime, int maxPack) where T : IVariableSource, new()
    {
        // 用于存储读取结果的列表
        List<T> sourceReads = new();

        // 按地址结束位置排序
        var orderByAddressEnd = addressList.Keys.OrderBy(it => it.AddressEnd);
        // 按地址开始位置排序
        var orderByAddressStart = addressList.Keys.OrderBy(it => it.StartAddress);
        // 获取地址最小值
        var minAddress = orderByAddressStart.First().StartAddress;
        // 获取地址最大值
        var maxAddress = orderByAddressStart.Last().StartAddress;

        // 循环直到处理完所有地址
        while (maxAddress >= minAddress)
        {
            // 初始化读取长度为最大打包长度
            int readLength = maxPack;
            // 对于功能码为 1 或 2 的情况
            if (functionCode == 1 || functionCode == 2)
            {
                readLength = maxPack * 8 * 2;
            }

            // 获取当前一组打包地址信息，使得地址不超过读取长度
            var tempAddressEnd = orderByAddressEnd.Where(t => t.AddressEnd <= minAddress + readLength);
            // 获取起始地址（当前组打包地址中起始地址最小的）
            var startAddress = tempAddressEnd.OrderBy(it => it.StartAddress).First();
            // 计算读取寄存器长度
            var sourceLen = tempAddressEnd.Last().AddressEnd - startAddress.StartAddress;

            // 创建一个新的变量源读取对象
            T sourceRead = new()
            {
                TimeTick = new TimeTick(intervalTime),
                // 将当前组打包地址中的起始地址作为实际打包报文中的起始地址
                RegisterAddress = startAddress.ToString(),
                Length = sourceLen.ToInt()
            };

            // 遍历当前组打包地址中的每个地址
            foreach (var item in tempAddressEnd)
            {
                var readNode = addressList[item];
                // 如果功能码为 -1、3 或 4，并且数据类型为布尔型，则根据位偏移计算索引
                if ((functionCode == -1 || functionCode == 3 || functionCode == 4) && readNode.DataType == DataTypeEnum.Boolean)
                {
                    readNode.Index = ((item.StartAddress - startAddress.StartAddress) * 16) + readNode.Index;
                }
                else
                {
                    // 其他情况根据功能码和地址计算索引
                    if (functionCode == 1 || functionCode == 2)
                        readNode.Index = item.StartAddress - startAddress.StartAddress + readNode.Index;
                    else
                        readNode.Index = ((item.StartAddress - startAddress.StartAddress) * 2) + readNode.Index;
                }

                // 将读取节点添加到变量源读取对象中，并从地址列表中移除
                sourceRead.AddVariable(readNode);
                addressList.Remove(item);
            }

            // 将变量源读取对象添加到结果列表中
            sourceReads.Add(sourceRead);

            // 更新最小地址值，如果还有地址未处理，则继续循环；否则跳出循环
            if (orderByAddressEnd.Count() > 0)
                minAddress = orderByAddressStart.First().StartAddress;
            else
                break;
        }

        // 返回结果列表
        return sourceReads;
    }
}
