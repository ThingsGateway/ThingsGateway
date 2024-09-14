//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.OpcDa.Rcw;

namespace ThingsGateway.Foundation.OpcDa.Da;

/// <summary>
/// ServerStatus
/// </summary>
public class ServerStatus
{
    /// <summary>
    /// CurrentTime
    /// </summary>
    public DateTime CurrentTime { get; internal set; } = new DateTime(0);

    /// <summary>
    /// LastUpdateTime
    /// </summary>
    public DateTime LastUpdateTime { get; internal set; } = new DateTime(0);

    /// <summary>
    /// ServerState
    /// </summary>
    public OPCSERVERSTATE ServerState { get; internal set; } = OPCSERVERSTATE.OPC_STATUS_NOCONFIG;

    /// <summary>
    /// StartTime
    /// </summary>
    public DateTime StartTime { get; internal set; } = new DateTime(0);

    /// <summary>
    /// VendorInfo
    /// </summary>
    public string VendorInfo { get; internal set; } = "UNKOWN";

    /// <summary>
    /// Version
    /// </summary>
    public string Version { get; internal set; } = "UNKOWN";
}