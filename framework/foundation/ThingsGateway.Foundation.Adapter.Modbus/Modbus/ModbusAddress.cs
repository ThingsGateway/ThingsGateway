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

using System.Text;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.Adapter.Modbus;

/// <summary>
/// Modbus协议地址
/// </summary>
public class ModbusAddress : DeviceAddressBase
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public ModbusAddress()
    {
    }

    /// <summary>
    /// 读取功能码
    /// </summary>
    public ushort AddressStart => Address.ToUShort();

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
    public byte WriteFunction { get; set; }

    /// <summary>
    /// 打包临时写入，需要读取的字节长度
    /// </summary>
    public int ByteLength { get; set; }

    /// <summary>
    /// BitIndex
    /// </summary>
    public int BitIndex => (int)(Address.SplitStringByDelimiter().LastOrDefault().ToInt());

    /// <summary>
    /// 读取功能码
    /// </summary>
    public ushort AddressEnd => (ushort)(AddressStart + (Math.Ceiling(ByteLength / 2.0) > 0 ? Math.Ceiling(ByteLength / 2.0) : 1));

    /// <summary>
    /// 作为Slave时需提供的SocketId，用于分辨Socket客户端，通常对比的是初始链接时的注册包
    /// </summary>
    public string SocketId { get; set; }

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
    public static ModbusAddress ParseFrom(string address, ModbusAddress modbusAddress = null)
    {
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

        return modbusAddress;

        void Address(string address)
        {
            var readF = ushort.Parse(address.Substring(0, 1));
            if (readF > 4)
                throw new("功能码错误");
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
            modbusAddress.Address = (double.Parse(address.Substring(1)) - 1).ToString();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder stringGeter = new();
        if (Station > 0)
        {
            stringGeter.Append($"s={Station.ToString()};");
        }
        if (WriteFunction > 0)
        {
            stringGeter.Append($"w={WriteFunction.ToString()};");
        }
        if (!string.IsNullOrEmpty(SocketId))
        {
            stringGeter.Append($"id={SocketId};");
        }
        stringGeter.Append(GetFunctionString(ReadFunction) + (AddressStart + 1).ToString());
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
}