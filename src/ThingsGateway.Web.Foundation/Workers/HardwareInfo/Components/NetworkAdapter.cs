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

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// WMI 类：Win32 网络适配器
/// </summary>
public class NetworkAdapter
{
    /// <summary>
    /// 正在使用的网络介质。
    /// </summary>
    [Description("网络介质")]
    public string AdapterType { get; set; } = string.Empty;

    /// <summary>
    /// 对象的简短描述（单行字符串）。
    /// </summary>
    [Description("简述")]
    public string Caption { get; set; } = string.Empty;

    /// <summary>
    /// 对象的描述。
    /// </summary>
    [Description("描述")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 此网络适配器的媒体访问控制地址。
    /// MAC 地址是制造商分配给网络适配器的唯一 48 位数字。
    /// 唯一标识此网络适配器，用于映射 TCP/IP 网络通信。
    /// </summary>
    [Description("MAC地址")]
    public string MACAddress { get; set; } = string.Empty;

    /// <summary>
    ///网络适​​配器制造商的名称。
    /// </summary>
    [Description("制造商")]
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// 已知对象的标签。
    /// </summary>
    [Description("名称")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 网络连接的名称。
    /// </summary>
    [Description("网络连接名称")]
    public string NetConnectionID { get; set; } = string.Empty;

    /// <summary>
    /// 网络适​​配器的产品名称。
    /// </summary>
    [Description("产品名称")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 以比特/秒为单位估计当前带宽。
    /// </summary>
    [Description("速度(bit/s)")]
    public UInt64 Speed { get; set; }

    /// <summary>
    ///Bytes Sent/sec 是通过每个网络适配器发送字节的速率，包括帧字符。
    /// </summary>
    [Description("上传速度(byte/s)")]
    public UInt64 BytesSentPersec { get; set; }

    /// <summary>
    /// Bytes Received/sec 是通过每个网络适配器接收字节的速率，包括帧字符。
    /// </summary>
    [Description("下载速度(byte/s)")]
    public UInt64 BytesReceivedPersec { get; set; }



}
