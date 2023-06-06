#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// WMI 类：Win32 物理内存
/// </summary>
public class Memory
{
    /// <summary>
    /// 总容量（以字节为单位）
    /// </summary>
    [Description("总容量")]
    public UInt64 Capacity { get; set; }

    /// <summary>
    /// 制造商
    /// </summary>
    [Description("制造商")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 此设备的最大工作电压，以毫伏为单位，如果电压未知，则为 0。
    /// </summary>
    [Description("最大工作电压")]
    public UInt32 MaxVoltage { get; set; }

    /// <summary>
    /// 此设备的最小工作电压，以毫伏为单位，如果电压未知，则为 0。
    /// </summary>
    [Description("最小工作电压")]
    public UInt32 MinVoltage { get; set; }

    /// <summary>
    /// 物理内存的速度（以纳秒为单位）。
    /// </summary>
    [Description("速度(ns)")]
    public UInt32 Speed { get; set; }

}
