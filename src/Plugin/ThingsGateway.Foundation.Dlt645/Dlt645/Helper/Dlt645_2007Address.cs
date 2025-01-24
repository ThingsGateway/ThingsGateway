//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.NewLife.Caching;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// Dlt645_2007Address
/// </summary>
public class Dlt645_2007Address : Dlt645_2007Request
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Dlt645_2007Address()
    {
    }

    public Dlt645_2007Address(Dlt645_2007Address dlt645_2007Address)
    {
        DataId = dlt645_2007Address.DataId;
        Reverse = dlt645_2007Address.Reverse;
        Station = dlt645_2007Address.Station;
    }

    /// <summary>
    /// 解析地址
    /// </summary>
    public static Dlt645_2007Address ParseFrom(string address, string defaultStation = null, bool isCache = true)
    {
        var cacheKey = $"{nameof(ParseFrom)}_{typeof(Dlt645_2007Address).FullName}_{typeof(Dlt645_2007Address).TypeHandle.Value}_{address}_{defaultStation}";
        if (isCache)
            if (MemoryCache.Instance.TryGetValue(cacheKey, out Dlt645_2007Address dAddress))
                return new(dAddress);

        Dlt645_2007Address dlt645_2007Address = new();
        if (!string.IsNullOrEmpty(defaultStation))
        {
            dlt645_2007Address.SetStation(defaultStation);
        }
        if (address.IndexOf(';') < 0)
        {
            dlt645_2007Address.SetDataId(address);
        }
        else
        {
            string[] strArray = address.SplitStringBySemicolon();

            for (int index = 0; index < strArray.Length; ++index)
            {
                if (strArray[index].StartsWith("S=", StringComparison.OrdinalIgnoreCase))
                {
                    var station = strArray[index].Substring(2);
                    if (station.Length < 12)
                        station = station.PadLeft(12, '0');
                    dlt645_2007Address.SetStation(station);
                }
                else if (strArray[index].StartsWith("R=", StringComparison.OrdinalIgnoreCase))
                {
                    dlt645_2007Address.Reverse = strArray[index].Substring(2).ToBoolean(false);
                }
                else if (!strArray[index].Contains('='))
                {
                    dlt645_2007Address.SetDataId(strArray[index]);
                }
            }
        }

        if (isCache)
            MemoryCache.Instance.Set(cacheKey, dlt645_2007Address, 3600);

        return new(dlt645_2007Address);
    }

    public void SetDataId(string dataId)
    {
        Data = dataId;
    }

    public void SetStation(string station)
    {
        StationString = station;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder stringGeter = new();
        if (Station.Length > 0)
        {
            stringGeter.Append($"s={StationString};");
        }
        if (DataId.Length > 0)
        {
            stringGeter.Append($"{Data};");
        }
        if (!Reverse)
        {
            stringGeter.Append($"r={Reverse};");
        }
        return stringGeter.ToString();
    }
}
