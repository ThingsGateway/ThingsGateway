//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Management;

/// <summary>
/// 冗余配置
/// </summary>
public class RedundancyOptions
{
    /// <summary>
    /// 获取或设置是否启用冗余。
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// 获取或设置远程 URI，用于通信。
    /// </summary>
    public string MasterUri { get; set; }

    /// <summary>
    /// 获取或设置是否为主设备。
    /// </summary>
    public bool IsMaster { get; set; }

    /// <summary>
    /// 获取或设置用于验证的令牌。
    /// </summary>
    [Required]
    public string VerifyToken { get; set; }

    /// <summary>
    /// 获取或设置心跳间隔。
    /// </summary>
    [MinValue(3000)]
    public int HeartbeatInterval { get; set; }

    /// <summary>
    /// 获取或设置允许的最大错误计数。
    /// </summary>
    [MinValue(1)]
    public int MaxErrorCount { get; set; }

    /// <summary>
    /// 获取或设置是否为启动业务的设备。
    /// </summary>
    public bool IsStartBusinessDevice { get; set; } = true;

    /// <summary>
    /// 获取或设置冗余数据同步间隔(ms)。
    /// </summary>
    [MinValue(1000)]
    public int SyncInterval { get; set; }
}
