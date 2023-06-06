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

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// Cache常量
/// </summary>
public class ThingsGatewayCacheConst
{
    /// <summary>
    /// 采集设备
    /// </summary>
    public const string Cache_CollectDevice = CacheConst.Cache_Prefix_Web + "CollectDevice";

    /// <summary>
    /// 设备变量组
    /// </summary>
    public const string Cache_DeviceVariableGroup = CacheConst.Cache_Prefix_Web + "DeviceVariableGroup";

    /// <summary>
    /// 设备变量Id
    /// </summary>
    public const string Cache_DeviceVariableId = CacheConst.Cache_Prefix_Web + "DeviceVariableId";

    /// <summary>
    /// 设备变量名称
    /// </summary>
    public const string Cache_DeviceVariableName = CacheConst.Cache_Prefix_Web + "DeviceVariableName";
    /// <summary>
    /// 插件
    /// </summary>
    public const string Cache_DriverPlugin = CacheConst.Cache_Prefix_Web + "Cache_DriverPlugin";
    /// <summary>
    /// 上传设备
    /// </summary>
    public const string Cache_UploadDevice = CacheConst.Cache_Prefix_Web + "UploadDevice";
}