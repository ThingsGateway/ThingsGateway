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
/// 上传设备添加DTO
/// </summary>
public class UploadDeviceAddInput : UploadDeviceEditInput
{
    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string Name { get; set; }
    /// <inheritdoc/>
    [MinValue(1, ErrorMessage = "插件不能为空")]
    public override long PluginId { get; set; }

}
/// <summary>
/// 上传设备修改DTO
/// </summary>
public class UploadDeviceEditInput : UploadDevice
{

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string Name { get; set; }
    /// <inheritdoc/>
    [MinValue(1, ErrorMessage = "插件不能为空")]
    public override long PluginId { get; set; }

}

/// <summary>
/// 上传设备分页查询
/// </summary>
public class UploadDevicePageInput : BasePageInput
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