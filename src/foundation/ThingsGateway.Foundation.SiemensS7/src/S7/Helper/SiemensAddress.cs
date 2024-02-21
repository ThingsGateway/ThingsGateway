//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.SiemensS7;

/// <summary>
/// 西门子PLC地址数据信息
/// </summary>
public class SiemensAddress
{
    /// <summary>
    /// 地址
    /// </summary>
    public string Address { get; set; }

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
    /// IsWString，默认是true，如果不是WString,需要填写W=false;
    /// </summary>
    public bool IsWString { get; set; } = true;

    /// <summary>
    /// Length
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// <inheritdoc cref="Address"/>
    /// </summary>
    public int AddressStart => Address.ToInt();

    /// <inheritdoc />
    public override string ToString()
    {
        if (DataCode == (byte)S7Area.TM)
        {
            return $"T{Address.ToString()}{(IsWString ? ";" : ";W=false;")}";
        }
        if (DataCode == (byte)S7Area.CT)
        {
            return $"C{Address.ToString()}{(IsWString ? ";" : ";W=false;")}";
        }

        if (DataCode == (byte)S7Area.AI)
        {
            return $"AI{GetStringAddress(AddressStart)}{(IsWString ? ";" : ";W=false;")}";
        }

        if (DataCode == (byte)S7Area.AQ)
        {
            return $"AQ{GetStringAddress(AddressStart)}{(IsWString ? ";" : ";W=false;")}";
        }

        if (DataCode == (byte)S7Area.PE)
        {
            return $"I{GetStringAddress(AddressStart)}{(IsWString ? ";" : ";W=false;")}";
        }

        if (DataCode == (byte)S7Area.PA)
        {
            return $"Q{GetStringAddress(AddressStart)}{(IsWString ? ";" : ";W=false;")}";
        }

        if (DataCode == (byte)S7Area.MK)
        {
            return $"M{GetStringAddress(AddressStart)}{(IsWString ? ";" : ";W=false;")}";
        }

        return DataCode == (byte)S7Area.DB ? $"DB{DbBlock.ToString()}.{GetStringAddress(AddressStart)}{(IsWString ? ";" : ";W=false;")}" : Address.ToString() + (IsWString ? ";" : ";W=false;");
    }

    private static string GetStringAddress(int addressStart)
    {
        return addressStart % 8 == 0 ? (addressStart / 8).ToString() : $"{addressStart / 8}.{addressStart % 8}";
    }
}