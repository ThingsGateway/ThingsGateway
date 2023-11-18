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

namespace ThingsGateway.Gateway.Core;

/// <summary>
/// Cache常量
/// </summary>
public class ThingsGatewayCacheConst
{
    /// <summary>
    /// 前缀
    /// </summary>
    public const string Cache_Prefix = "ThingsGateway";
    /// <summary>
    /// 采集设备
    /// </summary>
    public const string Cache_CollectDevice = Cache_Prefix + "CollectDevice";

    /// <summary>
    /// 插件
    /// </summary>
    public const string Cache_DriverPlugin = Cache_Prefix + "Cache_DriverPlugin";

    /// <summary>
    /// 上传设备
    /// </summary>
    public const string Cache_UploadDevice = Cache_Prefix + "UploadDevice";
}