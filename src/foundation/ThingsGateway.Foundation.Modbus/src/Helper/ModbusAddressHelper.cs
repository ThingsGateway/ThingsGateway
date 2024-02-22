//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife.Caching;

using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Foundation.Modbus;

/// <summary>
/// ModbusAddressUtil
/// </summary>
public static class ModbusAddressHelper
{
    /// <summary>
    /// 解析地址
    /// </summary>
    public static ModbusAddress ParseFrom(string address, byte station)
    {
        ModbusAddress modbusAddress = new()
        {
            Station = station
        };
        return ParseFrom(address, modbusAddress);
    }

    /// <summary>
    /// 解析地址
    /// </summary>
    public static ModbusAddress ParseFrom(string address, ModbusAddress modbusAddress = null, bool isCache = true)
    {
        var cacheKey = $"{nameof(ModbusAddress)}_{nameof(ParseFrom)}_{typeof(ModbusAddress).FullName}_{typeof(ModbusAddress).TypeHandle.Value}_{modbusAddress?.ToJsonString()}_{address}";
        if (isCache)
            if (Cache.Default.TryGetValue(cacheKey, out ModbusAddress mAddress))
                return mAddress!.Map<ModbusAddress>();

        modbusAddress ??= new();
        if (address.IndexOf(';') < 0)
        {
            Address(address);
        }
        else
        {
            string[] strArray = address.SplitStringBySemicolon();
            for (int index = 0; index < strArray.Length; ++index)
            {
                if (strArray[index].ToUpper().StartsWith("S="))
                {
                    if (Convert.ToInt16(strArray[index].Substring(2)) > 0)
                        modbusAddress.Station = byte.Parse(strArray[index].Substring(2));
                }
                else if (strArray[index].ToUpper().StartsWith("W="))
                {
                    if (Convert.ToInt16(strArray[index].Substring(2)) > 0)
                        modbusAddress.WriteFunction = byte.Parse(strArray[index].Substring(2));
                }
                else if (strArray[index].ToUpper().StartsWith("ID="))
                {
                    modbusAddress.SocketId = strArray[index].Substring(3);
                }
                else if (!strArray[index].Contains("="))
                {
                    Address(strArray[index]);
                }
            }
        }

        if (isCache)
            Cache.Default.Set(cacheKey, modbusAddress.Map<ModbusAddress>(), 3600);

        return modbusAddress;

        void Address(string address)
        {
            var readF = ushort.Parse(address.Substring(0, 1));
            if (readF > 4)
                throw new(ModbusConst.FunctionError);
            switch (readF)
            {
                case 0:
                    modbusAddress.ReadFunction = 1;
                    break;

                case 1:
                    modbusAddress.ReadFunction = 2;
                    break;

                case 3:
                    modbusAddress.ReadFunction = 4;
                    break;

                case 4:
                    modbusAddress.ReadFunction = 3;
                    break;
            }
            string[] strArray = address.SplitStringByDelimiter();
            if (strArray.Length > 1)
            {
                modbusAddress.Address = (ushort.Parse(strArray[0].Substring(1)) - 1).ToString() + '.' + strArray[1];
            }
            else
            {
                modbusAddress.Address = (ushort.Parse(strArray[0].Substring(1)) - 1).ToString();
            }
        }
    }
}