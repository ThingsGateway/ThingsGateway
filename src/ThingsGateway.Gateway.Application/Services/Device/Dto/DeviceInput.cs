//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备添加DTO
/// </summary>
public class DeviceAddInput : Device, IValidatableObject
{
    /// <inheritdoc/>
    [MinValue(9, ErrorMessage = "低于最小值")]
    public override int IntervalTime { get; set; } = 1000;

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string Name { get; set; }

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override PluginTypeEnum PluginType { get; set; }

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string PluginName { get; set; }

    [MinValue(1)]
    public override long ChannelId { get; set; }

    /// <inheritdoc/>
    public override bool Enable { get; set; } = true;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsRedundant)
        {
            if (RedundantDeviceId >= 1)
            {
            }
            else
            {
                yield return new ValidationResult("启用冗余时，必须选择备用设备", new[] { nameof(RedundantDeviceId) });
            }
        }
    }
}

/// <summary>
/// 设备编辑DTO
/// </summary>
public class DeviceEditInput : DeviceAddInput
{
    /// <inheritdoc/>
    public override bool Enable { get; set; }
}

/// <summary>
/// 设备分页查询DTO
/// </summary>
public class DevicePageInput : BasePageInput
{
    /// <inheritdoc/>
    [Description("设备名称")]
    public string? Name { get; set; }

    /// <inheritdoc/>
    [Description("插件名称")]
    public string? PluginName { get; set; }

    /// <inheritdoc/>
    [Description("通道")]
    public long? ChannelId { get; set; }

    /// <inheritdoc/>
    [Description("类型")]
    public PluginTypeEnum PluginType { get; set; }
}

/// <summary>
/// 设备查询DTO
/// </summary>
public class DeviceInput
{
    /// <inheritdoc/>
    [Description("设备名称")]
    public string? Name { get; set; }

    /// <inheritdoc/>
    [Description("插件名称")]
    public string? PluginName { get; set; }

    /// <inheritdoc/>
    [Description("通道")]
    public long? ChannelId { get; set; }

    /// <inheritdoc/>
    public PluginTypeEnum PluginType { get; set; }

    /// <summary>
    /// 全部
    /// </summary>
    public bool All { get; set; }
}