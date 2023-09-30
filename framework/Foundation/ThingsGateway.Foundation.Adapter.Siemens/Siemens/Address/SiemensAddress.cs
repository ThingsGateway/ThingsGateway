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

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.Adapter.Siemens;
/// <summary>
/// 区域
/// </summary>
public enum S7Area : byte
{
    /// <inheritdoc/>
    PE = 0x81,
    /// <inheritdoc/>
    PA = 0x82,
    /// <inheritdoc/>
    MK = 0x83,
    /// <inheritdoc/>
    DB = 0x84,
    /// <inheritdoc/>
    CT = 0x1C,
    /// <inheritdoc/>
    TM = 0x1D,
    /// <inheritdoc/>
    AI = 0X06,
    /// <inheritdoc/>
    AQ = 0x07,
}

/// <summary>
/// 西门子PLC地址数据信息
/// </summary>
public class SiemensAddress : DeviceAddressBase
{
    /// <summary>
    /// bit位偏移
    /// </summary>
    public byte BitCode { get; set; }

    /// <summary>
    /// 数据块代码
    /// </summary>
    public byte DataCode { get; set; }
    /// <summary>
    /// DB块数据信息
    /// </summary>
    public ushort DbBlock { get; set; }

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
    /// <returns></returns>
    public static SiemensAddress ParseFrom(string address, int len)
    {
        SiemensAddress s7AddressData = ParseFrom(address);
        s7AddressData.Length = len;
        return s7AddressData;
    }
    /// <summary>
    /// 解析地址
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public static SiemensAddress ParseFrom(string address)
    {
        SiemensAddress s7AddressData = new();

        address = address.ToUpper();
        address = address.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

        s7AddressData.DbBlock = 0;
        if (address.StartsWith("AI"))
        {
            s7AddressData.DataCode = (byte)S7Area.AI;
            if (address.StartsWith("AIX") || address.StartsWith("AIB") || address.StartsWith("AIW") || address.StartsWith("AID"))
            {
                s7AddressData.Address = GetAddressStart(address.Substring(3)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(3));
            }
            else
            {
                s7AddressData.Address = GetAddressStart(address.Substring(2)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(2));
            }
        }
        else if (address.StartsWith("AQ"))
        {
            s7AddressData.DataCode = (byte)S7Area.AQ;
            if (address.StartsWith("AQX") || address.StartsWith("AQB") || address.StartsWith("AQW") || address.StartsWith("AQD"))
            {
                s7AddressData.Address = GetAddressStart(address.Substring(3)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(3));
            }
            else
            {
                s7AddressData.Address = GetAddressStart(address.Substring(2)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(2));
            }
        }
        else if (address[0] == 'I')
        {
            s7AddressData.DataCode = (byte)S7Area.PE;
            if (address.StartsWith("IX") || address.StartsWith("IB") || address.StartsWith("IW") || address.StartsWith("ID"))
            {
                s7AddressData.Address = GetAddressStart(address.Substring(2)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(2));
            }
            else
            {
                s7AddressData.Address = GetAddressStart(address.Substring(1)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(1));
            }
        }
        else if (address[0] == 'Q')
        {
            s7AddressData.DataCode = (byte)S7Area.PA;
            if (address.StartsWith("QX") || address.StartsWith("QB") || address.StartsWith("QW") || address.StartsWith("QD"))
            {
                s7AddressData.Address = GetAddressStart(address.Substring(2)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(2));
            }
            else
            {
                s7AddressData.Address = GetAddressStart(address.Substring(1)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(1));
            }
        }
        else if (address[0] == 'M')
        {
            s7AddressData.DataCode = (byte)S7Area.MK;
            if (address.StartsWith("MX") || address.StartsWith("MB") || address.StartsWith("MW") || address.StartsWith("MD"))
            {
                s7AddressData.Address = GetAddressStart(address.Substring(2)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(2));
            }
            else
            {
                s7AddressData.Address = GetAddressStart(address.Substring(1)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(1));
            }
        }
        else if (address[0] == 'D' || address.Substring(0, 2) == "DB")
        {
            s7AddressData.DataCode = (byte)S7Area.DB;
            string[] strArray = address.Split('.');
            s7AddressData.DbBlock = address[1] != 'B' ? Convert.ToUInt16(strArray[0].Substring(1)) : Convert.ToUInt16(strArray[0].Substring(2));
            string address1 = address.Substring(address.IndexOf('.') + 1);
            if (address1.StartsWith("DBX") || address1.StartsWith("DBB") || address1.StartsWith("DBW") || address1.StartsWith("DBD"))
            {
                address1 = address1.Substring(3);
            }

            s7AddressData.Address = GetAddressStart(address1).ToString();
            s7AddressData.BitCode = GetBitCode(address1);
        }
        else if (address[0] == 'T')
        {
            s7AddressData.DataCode = (byte)S7Area.TM;
            s7AddressData.Address = GetAddressStart(address.Substring(1), true).ToString();
            s7AddressData.BitCode = GetBitCode(address.Substring(1));
        }
        else if (address[0] == 'C')
        {
            s7AddressData.DataCode = (byte)S7Area.CT;
            s7AddressData.Address = GetAddressStart(address.Substring(1), true).ToString();
            s7AddressData.BitCode = GetBitCode(address.Substring(1));
        }
        else if (address[0] == 'V')
        {
            s7AddressData.DataCode = (byte)S7Area.DB;
            s7AddressData.DbBlock = 1;
            if (address.StartsWith("VB") || address.StartsWith("VW") || address.StartsWith("VD") || address.StartsWith("VX"))
            {
                s7AddressData.Address = GetAddressStart(address.Substring(2)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(2));
            }
            else
            {
                s7AddressData.Address = GetAddressStart(address.Substring(1)).ToString();
                s7AddressData.BitCode = GetBitCode(address.Substring(1));
            }
        }
        else
        {
            throw new Exception("解析错误，无相关变量类型");
        }

        return s7AddressData;
    }


    /// <summary>
    /// Length
    /// </summary>
    public int Length { get; set; }
    /// <summary>
    /// <inheritdoc cref="DeviceAddressBase.Address"/>
    /// </summary>
    public int AddressStart => Address.ToInt();

    /// <inheritdoc />
    public override string ToString()
    {
        if (DataCode == (byte)S7Area.TM)
        {
            return "T" + Address.ToString();
        }
        if (DataCode == (byte)S7Area.CT)
        {
            return "C" + Address.ToString();
        }

        if (DataCode == (byte)S7Area.AI)
        {
            return "AI" + GetStringAddress(AddressStart);
        }

        if (DataCode == (byte)S7Area.AQ)
        {
            return "AQ" + GetStringAddress(AddressStart);
        }

        if (DataCode == (byte)S7Area.PE)
        {
            return "I" + GetStringAddress(AddressStart);
        }

        if (DataCode == (byte)S7Area.PA)
        {
            return "Q" + GetStringAddress(AddressStart);
        }

        if (DataCode == (byte)S7Area.MK)
        {
            return "M" + GetStringAddress(AddressStart);
        }

        return DataCode == (byte)S7Area.DB ? "DB" + DbBlock.ToString() + "." + GetStringAddress(AddressStart) : Address.ToString();
    }

    private static string GetStringAddress(int addressStart)
    {
        return addressStart % 8 == 0 ? (addressStart / 8).ToString() : string.Format("{0}.{1}", addressStart / 8, addressStart % 8);
    }

}