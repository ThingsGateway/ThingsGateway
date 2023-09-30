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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 添加变量DTO
/// </summary>
public class VariableAddInput : VariableEditInput
{
    /// <inheritdoc/>
    public override ProtectTypeEnum ProtectTypeEnum { get; set; } = ProtectTypeEnum.ReadOnly;
}
/// <summary>
/// 添加变量DTO
/// </summary>
public class DeviceVariableAddInput : VariableAddInput
{
    /// <inheritdoc/>
    public override int IntervalTime { get; set; } = 1000;
    /// <inheritdoc/>
    public override bool IsMemoryVariable { get; set; } = false;
    /// <inheritdoc/>
    public override DataTypeEnum DataTypeEnum { get; set; } = DataTypeEnum.Int16;

}
/// <summary>
/// 添加变量DTO
/// </summary>
public class MemoryVariableAddInput : DeviceVariable
{
    /// <inheritdoc/>
    public override bool IsMemoryVariable { get; set; } = true;
}

/// <summary>
/// 修改变量DTO
/// </summary>
public class VariableEditInput : DeviceVariable, IValidatableObject
{

    /// <inheritdoc/>
    [Required(ErrorMessage = "不能为空")]
    public override string Name { get; set; }

    /// <inheritdoc/>
    [MinValue(1, ErrorMessage = "不能为空")]
    public override long DeviceId { get; set; }

    /// <inheritdoc/>
    [MinValue(9, ErrorMessage = "低于最小值")]
    public override int IntervalTime { get; set; }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(VariableAddress) && string.IsNullOrEmpty(OtherMethod))
            yield return new ValidationResult("变量地址或特殊方法不能同时为空", new[] { nameof(VariableAddress) });
    }

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
    public string DeviceName { get; set; }
    /// <inheritdoc/>
    [Description("变量地址")]
    public string VariableAddress { get; set; }
    /// <inheritdoc/>
    [Description("上传设备名称")]
    public string UploadDeviceName { get; set; }

    /// <inheritdoc/>
    public virtual bool IsMemoryVariable { get; set; }
}

/// <summary>
/// 变量分页查询参数
/// </summary>
public class DeviceVariablePageInput : VariablePageInput
{
    /// <inheritdoc/>
    public override bool IsMemoryVariable { get; set; } = false;
}
/// <summary>
/// 变量分页查询参数
/// </summary>
public class DeviceVariableInput : MemoryVariableInput
{
    /// <inheritdoc/>
    [Description("设备名称")]
    public string DeviceName { get; set; }
    /// <inheritdoc/>
    [Description("变量地址")]
    public string VariableAddress { get; set; }
    /// <inheritdoc/>
    public override bool IsMemoryVariable { get; set; } = false;
}

/// <summary>
/// 变量分页查询参数
/// </summary>
public class MemoryVariablePageInput : VariablePageInput
{
    /// <inheritdoc/>
    public override bool IsMemoryVariable { get; set; } = true;
}

/// <summary>
/// 变量分页查询参数
/// </summary>
public class MemoryVariableInput
{
    /// <inheritdoc/>
    [Description("变量名称")]
    public string Name { get; set; }
    /// <inheritdoc/>
    [Description("上传设备名称")]
    public string UploadDeviceName { get; set; }
    /// <inheritdoc/>
    public virtual bool IsMemoryVariable { get; set; } = true;
}

