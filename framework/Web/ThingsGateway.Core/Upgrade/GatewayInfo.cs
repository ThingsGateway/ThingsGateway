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

namespace ThingsGateway.Core;

/// <summary>
/// 网关信息
/// </summary>
public class GatewayInfo
{
    /// <summary>
    /// 软件版本
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// 获取时间
    /// </summary>
    public string UpdateTime { get; set; }
    /// <summary>
    /// 采集设备数量
    /// </summary>
    public int CollectDeviceCount { get; set; }
    /// <summary>
    /// 上传设备数量
    /// </summary>
    public int UploadDeviceCount { get; set; }
    /// <summary>
    /// 变量数量
    /// </summary>
    public int VariableCount { get; set; }

}






