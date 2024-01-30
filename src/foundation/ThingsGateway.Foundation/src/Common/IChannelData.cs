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

using System.IO.Ports;

namespace ThingsGateway.Foundation;

/// <summary>
/// 通道配置
/// </summary>
public interface IChannelData
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 通道类型
    /// </summary>
    public ChannelTypeEnum ChannelType { get; set; }

    /// <summary>
    /// 远程地址，可由<see cref="IPHost.IPHost(string)"/>与<see href="IPHost.ToString()"/>相互转化
    /// </summary>
    public string? RemoteUrl { get; set; }

    /// <summary>
    /// 本地地址，可由<see cref="IPHost.IPHost(string)"/>与<see href="IPHost.ToString()"/>相互转化
    /// </summary>
    public string? BindUrl { get; set; }

    /// <summary>
    /// COM
    /// </summary>
    public string? PortName { get; set; }

    /// <summary>
    /// 波特率
    /// </summary>
    public int? BaudRate { get; set; }

    /// <summary>
    /// 数据位
    /// </summary>
    public int? DataBits { get; set; }

    /// <summary>
    /// 校验位
    /// </summary>
    public Parity? Parity { get; set; }

    /// <summary>
    /// 停止位
    /// </summary>
    public StopBits? StopBits { get; set; }

    /// <summary>
    /// DtrEnable，默认true
    /// </summary>
    public bool? DtrEnable { get; set; }

    /// <summary>
    /// RtsEnable，默认true
    /// </summary>
    public bool? RtsEnable { get; set; }
}