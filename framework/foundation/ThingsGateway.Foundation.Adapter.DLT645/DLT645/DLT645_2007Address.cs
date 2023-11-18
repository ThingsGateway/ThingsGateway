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

namespace ThingsGateway.Foundation.Adapter.DLT645;

/// <summary>
/// DLT645_2007Address
/// </summary>
public class DLT645_2007Address : DeviceAddressBase
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public DLT645_2007Address()
    {

    }

    /// <summary>
    /// 数据标识
    /// </summary>
    public byte[] DataId { get; set; } = new byte[0];
    /// <summary>
    /// 反转解析
    /// </summary>
    public bool Reverse { get; set; } = true;
    /// <summary>
    /// 站号信息
    /// </summary>
    public byte[] Station { get; set; } = new byte[0];


    /// <summary>
    /// 解析地址
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static DLT645_2007Address ParseFrom(string address)
    {
        DLT645_2007Address dlt645_2007Address = new();
        byte[] array;
        array = new byte[0];
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
                else if (strArray[index].Contains("r="))
                {
                    dlt645_2007Address.Reverse = strArray[index].Substring(2).ToBool(false);
                }
                else if (!strArray[index].Contains("="))
                {
                    array = strArray[index].ByHexStringToBytes().Reverse().ToArray();
                }
            }
        }
        dlt645_2007Address.DataId = array;
        return dlt645_2007Address;

    }

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
            stringGeter.Append($"s={Reverse};");
        }
        return stringGeter.ToString();
    }


}