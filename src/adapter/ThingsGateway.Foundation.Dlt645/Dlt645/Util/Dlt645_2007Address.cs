
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

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// Dlt645_2007Address
/// </summary>
public class Dlt645_2007Address
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Dlt645_2007Address()
    {
    }

    /// <summary>
    /// 数据标识
    /// </summary>
    public byte[] DataId { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 反转解析
    /// </summary>
    public bool Reverse { get; set; } = true;

    /// <summary>
    /// 站号信息
    /// </summary>
    public byte[] Station { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 作为Slave时需提供的SocketId，用于分辨Socket客户端，通常对比的是初始链接时的注册包
    /// </summary>
    public string? SocketId { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder stringGeter = new();
        if (Station.Length > 0)
        {
            stringGeter.Append($"s={Station.Reverse().ToArray().ToHexString()};");
        }
        if (DataId.Length > 0)
        {
            stringGeter.Append($"{DataId.Reverse().ToArray().ToHexString()};");
        }
        if (!Reverse)
        {
            stringGeter.Append($"r={Reverse};");
        }
        if (!string.IsNullOrEmpty(SocketId))
        {
            stringGeter.Append($"id={SocketId};");
        }
        return stringGeter.ToString();
    }

    /// <summary>
    /// 解析地址
    /// </summary>
    public static Dlt645_2007Address ParseFrom(string address, bool isCache = true)
    {
        var cacheKey = $"{nameof(ParseFrom)}_{typeof(Dlt645_2007Address).FullName}_{typeof(Dlt645_2007Address).TypeHandle.Value}_{address}";
        if (isCache)
            if (Cache.Default.TryGetValue(cacheKey, out Dlt645_2007Address dAddress))
                return dAddress;

        Dlt645_2007Address dlt645_2007Address = new();
        byte[] array;
        array = Array.Empty<byte>();
        if (address.IndexOf(';') < 0)
        {
            array = address.ByHexStringToBytes().Reverse().ToArray();
        }
        else
        {
            string[] strArray = address.SplitStringBySemicolon();

            for (int index = 0; index < strArray.Length; ++index)
            {
                if (strArray[index].ToUpper().StartsWith("S="))
                {
                    var station = strArray[index].Substring(2);
                    if (station.IsNullOrEmpty()) station = string.Empty;
                    if (station.Length < 12)
                        station = station.PadLeft(12, '0');
                    dlt645_2007Address.Station = station.ByHexStringToBytes().Reverse().ToArray();
                }
                else if (strArray[index].Contains("R="))
                {
                    dlt645_2007Address.Reverse = strArray[index].Substring(2).ToBoolean(false);
                }
                else if (strArray[index].ToUpper().StartsWith("ID="))
                {
                    dlt645_2007Address.SocketId = strArray[index].Substring(3);
                }
                else if (!strArray[index].Contains("="))
                {
                    array = strArray[index].ByHexStringToBytes().Reverse().ToArray();
                }
            }
        }
        dlt645_2007Address.DataId = array;

        if (isCache)
            Cache.Default.Set(cacheKey, dlt645_2007Address.Map<Dlt645_2007Address>(), 3600);

        return dlt645_2007Address;
    }
}
