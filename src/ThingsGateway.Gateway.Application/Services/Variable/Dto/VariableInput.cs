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
/// 添加变量DTO
/// </summary>
public class VariableAddInput : Variable, IValidatableObject
{
    /// <inheritdoc/>
    public override ProtectTypeEnum ProtectType { get; set; } = ProtectTypeEnum.ReadWrite;

    public override bool RpcWriteEnable { get; set; } = true;

    /// <inheritdoc/>
    public override DataTypeEnum DataType { get; set; } = DataTypeEnum.Int16;

    /// <inheritdoc/>
    public override bool Enable { get; set; } = true;

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DeviceId != 0)
            if (string.IsNullOrEmpty(RegisterAddress) && string.IsNullOrEmpty(OtherMethod))
                yield return new ValidationResult("变量地址或特殊方法不能同时为空", new[] { nameof(RegisterAddress) });
    }

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string Name { get; set; }

    /// <inheritdoc/>
    [MinValue(1, ErrorMessage = "不能为空")]
    public override long DeviceId { get; set; }

    /// <inheritdoc/>
    [MinValue(9, ErrorMessage = "低于最小值")]
    public override int? IntervalTime { get; set; }
}

/// <summary>
/// 修改变量DTO
/// </summary>
public class VariableEditInput : VariableAddInput
{
    /// <inheritdoc/>
    public override ProtectTypeEnum ProtectType { get; set; }

    /// <inheritdoc/>
    public override DataTypeEnum DataType { get; set; }
}

/// <summary>
/// 报警变量分页查询参数
/// </summary>
public class AlarmVariablePageInput : BasePageInput
{
    /// <inheritdoc/>
    [Description("变量名称")]
    public string Name { get; set; }

    /// <inheritdoc/>
    [Description("设备名称")]
    public string? DeviceName { get; set; }

    /// <inheritdoc/>
    [Description("变量地址")]
    public string RegisterAddress { get; set; }
}

/// <summary>
/// 变量分页查询参数
/// </summary>
public class VariablePageInput : BasePageInput
{
    /// <inheritdoc/>
    [Description("变量名称")]
    public string Name { get; set; }

    /// <inheritdoc/>
    [Description("设备名称")]
    public long? DeviceId { get; set; }

    /// <inheritdoc/>
    [Description("变量地址")]
    public string RegisterAddress { get; set; }

    /// <inheritdoc/>
    [Description("业务设备名称")]
    public long? BusinessDeviceId { get; set; }
}

/// <summary>
/// 变量查询参数
/// </summary>
public class VariableInput
{
    /// <inheritdoc/>
    [Description("设备名称")]
    public long? DeviceId { get; set; }

    /// <inheritdoc/>
    [Description("变量地址")]
    public string RegisterAddress { get; set; }

    /// <inheritdoc/>
    [Description("变量名称")]
    public string Name { get; set; }

    /// <inheritdoc/>
    [Description("业务设备名称")]
    public long? BusinessDeviceId { get; set; }

    /// <summary>
    /// 全部
    /// </summary>
    public bool All { get; set; }
}