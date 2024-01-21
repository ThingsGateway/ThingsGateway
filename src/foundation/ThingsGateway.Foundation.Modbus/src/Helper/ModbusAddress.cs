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

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// Modbus协议地址
/// </summary>
public class ModbusAddress
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
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
                return data[1].ToInt();
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