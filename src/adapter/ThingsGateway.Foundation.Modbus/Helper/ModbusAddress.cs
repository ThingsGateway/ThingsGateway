
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using NewLife.Caching;

using System.Text;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// Modbus协议地址
/// </summary>
public class ModbusAddress
{
    public ModbusAddress()
    {
    }

    /// <summary>
    /// 可能带小数点的地址表示
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// 起始地址
    /// </summary>
    public ushort AddressStart => (ushort)Address.ToInt();

    /// <summary>
    /// 读取功能码
    /// </summary>
    public byte ReadFunction { get; set; }

    /// <summary>
    /// 站号信息
    /// </summary>
    public byte Station { get; set; }

    /// <summary>
    /// 写入功能码
    /// </summary>
    public byte? WriteFunction { get; set; }

    /// <summary>
    /// BitIndex
    /// </summary>
    public int? BitIndex
    {
        get
        {
            var data = Address?.SplitStringByDelimiter();
            if (data?.Length == 2)
            {
                return Convert.ToInt32(data[1]);
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// 打包临时写入，需要读取的字节长度
    /// </summary>
    public int? ByteLength { get; set; }

    /// <summary>
    /// 读取终止
    /// </summary>
    public uint AddressEnd => (ushort)(AddressStart + (ByteLength != null ? (Math.Ceiling(ByteLength!.Value / 2.0) > 0 ? Math.Ceiling(ByteLength!.Value / 2.0) : 1) : 1));

    /// <summary>
    /// 作为Slave时需提供的SocketId，用于分辨Socket客户端，通常对比的是初始链接时的注册包
    /// </summary>
    public string? SocketId { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder stringGeter = new();
        if (Station > 0)
        {
            stringGeter.Append($"s={Station};");
        }
        if (WriteFunction > 0)
        {
            stringGeter.Append($"w={WriteFunction};");
        }
        if (!string.IsNullOrEmpty(SocketId))
        {
            stringGeter.Append($"id={SocketId};");
        }
        stringGeter.Append($"{GetFunctionString(ReadFunction)}{AddressStart + 1}{(BitIndex != null ? $".{BitIndex}" : null)}");
        return stringGeter.ToString();
    }

    private string GetFunctionString(int readF)
    {
        return readF switch
        {
            1 => "0",
            2 => "1",
            3 => "4",
            4 => "3",
            _ => "4",
        };
    }

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
        if (address.IsNullOrWhiteSpace()) { return modbusAddress; }
        var cacheKey = $"{nameof(ModbusAddress)}_{nameof(ParseFrom)}_{typeof(ModbusAddress).FullName}_{typeof(ModbusAddress).TypeHandle.Value}_{modbusAddress?.ToJsonString()}_{address}";
        if (isCache)
            if (Cache.Default.TryGetValue(cacheKey, out ModbusAddress mAddress))
                return mAddress;

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
                throw new(ModbusResource.Localizer["FunctionError"]);
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
