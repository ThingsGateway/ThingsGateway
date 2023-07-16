#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.ComponentModel.DataAnnotations;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 采集设备添加DTO
/// </summary>
public class CollectDeviceAddInput : CollectDeviceEditInput
{
    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string Name { get; set; }
    /// <inheritdoc/>
    [MinValue(1, ErrorMessage = "插件不能为空")]
    public override long PluginId { get; set; }

    /// <inheritdoc/>
    public override bool IsLogOut { get; set; } = true;
    /// <inheritdoc/>
    public override bool Enable { get; set; } = true;

}
/// <summary>
/// 采集设备编辑DTO
/// </summary>
public class CollectDeviceEditInput : CollectDevice
{

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string Name { get; set; }
    /// <inheritdoc/>
    [MinValue(1, ErrorMessage = "插件不能为空")]
    public override long PluginId { get; set; }

}
/// <summary>
/// 采集设备分页查询DTO
/// </summary>
public class CollectDevicePageInput : BasePageInput
{
    /// <inheritdoc/>
    [Description("设备名称")]
    public string Name { get; set; }
    /// <inheritdoc/>
    [Description("插件名称")]
    public string PluginName { get; set; }
    /// <inheritdoc/>
    [Description("设备组")]
    public string DeviceGroup { get; set; }
}