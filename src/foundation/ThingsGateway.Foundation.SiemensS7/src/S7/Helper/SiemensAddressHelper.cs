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

using NewLife.Caching;

using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Foundation.SiemensS7;

using TouchSocket.Core;

internal static class SiemensAddressHelper
{
    /// <summary>
    /// 解析地址
    /// </summary>
    /// <returns></returns>
    public static SiemensAddress ParseFrom(string address, int len)
    {
        SiemensAddress s7AddressData = ParseFrom(address);
        s7AddressData.Length = len;
        return s7AddressData;
    }

    /// <summary>
    /// 获取起始地址
    /// </summary>
    /// <param name="address"></param>
    /// <param name="isCounterOrTimer"></param>
    /// <returns></returns>
    public static int GetAddressStart(string address, bool isCounterOrTimer = false)
    {
        if (address.IndexOf('.') < 0)
        {
            return isCounterOrTimer ? Convert.ToInt32(address) : Convert.ToInt32(address) * 8;
        }

        string[] strArray = address.Split('.');
        return Convert.ToInt32(strArray[0]) * 8;
    }

    /// <summary>
    /// 获取bit
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static byte GetBitCode(string address)
    {
        if (address.IndexOf('.') < 0)
        {
            return 0;
        }
        string[] strArray = address.Split('.');
        return Convert.ToByte(strArray[1]);
    }

    /// <summary>
    /// 解析地址
    /// </summary>
    public static SiemensAddress ParseFrom(string address, bool isCache = true)
    {
        var cacheKey = $"{nameof(SiemensAddress)}_{nameof(ParseFrom)}_{typeof(SiemensAddress).FullName}_{typeof(SiemensAddress).TypeHandle.Value}_{address}";
        if (isCache)
            if (Cache.Default.TryGetValue(cacheKey, out SiemensAddress sAddress))
                return sAddress!.Map<SiemensAddress>();

        SiemensAddress s7AddressData = new();
        address = address.ToUpper();
        string[] strArr = address.SplitStringBySemicolon();
        for (int index = 0; index < strArr.Length; ++index)
        {
            if (strArr[index].StartsWith("W="))
            {
                s7AddressData.IsWString = strArr[index].Substring(2).ToBool(true);
            }
            else if (!strArr[index].Contains("="))
            {
                s7AddressData.DbBlock = 0;

                if (strArr[index].StartsWith("AI"))
                {
                    s7AddressData.DataCode = (byte)S7Area.AI;
                    if (strArr[index].StartsWith("AIX") || strArr[index].StartsWith("AIB") || strArr[index].StartsWith("AIW") || strArr[index].StartsWith("AID"))
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(3)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(3));
                    }
                    else
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(2)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(2));
                    }
                }
                else if (strArr[index].StartsWith("AQ"))
                {
                    s7AddressData.DataCode = (byte)S7Area.AQ;
                    if (strArr[index].StartsWith("AQX") || strArr[index].StartsWith("AQB") || strArr[index].StartsWith("AQW") || strArr[index].StartsWith("AQD"))
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(3)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(3));
                    }
                    else
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(2)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(2));
                    }
                }
                else if (strArr[index][0] == 'I')
                {
                    s7AddressData.DataCode = (byte)S7Area.PE;
                    if (strArr[index].StartsWith("IX") || strArr[index].StartsWith("IB") || strArr[index].StartsWith("IW") || strArr[index].StartsWith("ID"))
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(2)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(2));
                    }
                    else
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(1)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(1));
                    }
                }
                else if (strArr[index][0] == 'Q')
                {
                    s7AddressData.DataCode = (byte)S7Area.PA;
                    if (strArr[index].StartsWith("QX") || strArr[index].StartsWith("QB") || strArr[index].StartsWith("QW") || strArr[index].StartsWith("QD"))
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(2)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(2));
                    }
                    else
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(1)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(1));
                    }
                }
                else if (strArr[index][0] == 'M')
                {
                    s7AddressData.DataCode = (byte)S7Area.MK;
                    if (strArr[index].StartsWith("MX") || strArr[index].StartsWith("MB") || strArr[index].StartsWith("MW") || strArr[index].StartsWith("MD"))
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(2)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(2));
                    }
                    else
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(1)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(1));
                    }
                }
                else if (strArr[index][0] == 'D' || strArr[index].Substring(0, 2) == "DB")
                {
                    s7AddressData.DataCode = (byte)S7Area.DB;
                    string[] strArray = strArr[index].Split('.');
                    s7AddressData.DbBlock = strArray[index][1] != 'B' ? Convert.ToUInt16(strArray[0].Substring(1)) : Convert.ToUInt16(strArray[0].Substring(2));
                    string address1 = strArr[index].Substring(strArr[index].IndexOf('.') + 1);
                    if (address1.StartsWith("DBX") || address1.StartsWith("DBB") || address1.StartsWith("DBW") || address1.StartsWith("DBD"))
                    {
                        address1 = address1.Substring(3);
                    }

                    s7AddressData.Address = GetAddressStart(address1).ToString();
                    s7AddressData.BitCode = GetBitCode(address1);
                }
                else if (strArr[index][0] == 'T')
                {
                    s7AddressData.DataCode = (byte)S7Area.TM;
                    s7AddressData.Address = GetAddressStart(strArr[index].Substring(1), true).ToString();
                    s7AddressData.BitCode = GetBitCode(strArr[index].Substring(1));
                }
                else if (strArr[index][0] == 'C')
                {
                    s7AddressData.DataCode = (byte)S7Area.CT;
                    s7AddressData.Address = GetAddressStart(strArr[index].Substring(1), true).ToString();
                    s7AddressData.BitCode = GetBitCode(strArr[index].Substring(1));
                }
                else if (strArr[index][0] == 'V')
                {
                    s7AddressData.DataCode = (byte)S7Area.DB;
                    s7AddressData.DbBlock = 1;
                    if (strArr[index].StartsWith("VB") || strArr[index].StartsWith("VW") || strArr[index].StartsWith("VD") || strArr[index].StartsWith("VX"))
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(2)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(2));
                    }
                    else
                    {
                        s7AddressData.Address = GetAddressStart(strArr[index].Substring(1)).ToString();
                        s7AddressData.BitCode = GetBitCode(strArr[index].Substring(1));
                    }
                }
            }
        }

        if (isCache)
            Cache.Default.Set(cacheKey, s7AddressData.Map<SiemensAddress>(), 3600);

        return s7AddressData;
    }
}