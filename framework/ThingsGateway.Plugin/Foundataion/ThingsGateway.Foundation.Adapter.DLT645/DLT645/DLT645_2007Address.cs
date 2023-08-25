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

using ThingsGateway.Foundation.Extension.Byte;

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
    /// <inheritdoc/>
    public DLT645_2007Address(string address, ushort len)
    {
        Parse(address, len);
    }

    /// <inheritdoc/>
    public DLT645_2007Address(string address, byte[] station)
    {
        Station = station;
        Parse(address, 0);
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

    /// <inheritdoc/>
    public override void Parse(string address, int length)
    {
        var result = ParseFrom(address, length);
        if (result.IsSuccess)
        {
            Length = result.Content.Length;
            AddressStart = result.Content.AddressStart;
            DataId = result.Content.DataId;
            Station = result.Content.Station;
        }
        else
        {
            throw new Exception(result.Message);
        }
    }
    /// <summary>
    /// 解析地址
    /// </summary>
    /// <param name="address"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static OperResult<DLT645_2007Address> ParseFrom(string address, int length)
    {

        try
        {
            DLT645_2007Address dLT645_2007Address = new();
            byte[] array;
            array = new byte[0];
            dLT645_2007Address.Length = length;
            if (address.IndexOf(';') < 0)
            {
                array = address.ByHexStringToBytes().Reverse().ToArray();
            }
            else
            {
                string[] strArray = address.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                for (int index = 0; index < strArray.Length; ++index)
                {
                    if (strArray[index].ToUpper().StartsWith("S="))
                    {
                        var station = strArray[index].Substring(2);
                        if (station.IsNullOrEmpty()) return new OperResult<DLT645_2007Address>("通讯地址不能为空");
                        if (station.Length < 12)
                            station = station.PadLeft(12, '0');
                        dLT645_2007Address.Station = station.ByHexStringToBytes().Reverse().ToArray();
                    }
                    else if (!strArray[index].Contains("r="))
                    {
                        dLT645_2007Address.Reverse = strArray[index].Substring(2).GetBoolValue();
                    }
                    else if (!strArray[index].Contains("="))
                    {
                        array = strArray[index].ByHexStringToBytes().Reverse().ToArray();
                    }
                }
            }
            dLT645_2007Address.DataId = array;
            return OperResult.CreateSuccessResult(dLT645_2007Address);

        }
        catch (Exception ex)
        {
            return new OperResult<DLT645_2007Address>(ex);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder stringGeter = new();
        if (Station.Length > 0)
        {
            stringGeter.Append("s=" + Station.Reverse().ToArray().ToHexString() + ";");
        }
        if (DataId.Length > 0)
        {
            stringGeter.Append(DataId.Reverse().ToArray().ToHexString() + ";");
        }
        if (!Reverse)
        {
            stringGeter.Append("s=" + Reverse.ToString() + ";");
        }
        return stringGeter.ToString();
    }


}