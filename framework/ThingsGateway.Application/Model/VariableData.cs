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

namespace ThingsGateway.Application;
/// <summary>
/// 上传DTO
/// </summary>
public class VariableData
{
    /// <inheritdoc cref="PrimaryIdEntity.Id"/>
    public long Id { get; set; }
    /// <inheritdoc cref="MemoryVariable.Name"/>
    public string Name { get; set; }
    /// <inheritdoc cref="DeviceVariable.Unit"/>
    public string Unit { get; set; }
    /// <inheritdoc cref="MemoryVariable.Description"/>
    public string Description { get; set; }
    /// <inheritdoc cref="DeviceVariableRunTime.DeviceName"/>
    public string DeviceName { get; set; }
    /// <inheritdoc cref="DeviceVariableRunTime.RawValue"/>
    public object RawValue { get; set; }
    /// <inheritdoc cref="DeviceVariableRunTime.Value"/>
    public object Value { get; set; }
    /// <inheritdoc cref="DeviceVariableRunTime.ChangeTime"/>
    public DateTime ChangeTime { get; set; }
    /// <inheritdoc cref="DeviceVariableRunTime.CollectTime"/>
    public DateTime CollectTime { get; set; }
    /// <inheritdoc cref="DeviceVariableRunTime.IsOnline"/>
    public bool IsOnline { get; set; }
    /// <inheritdoc cref="MemoryVariable.ReadExpressions"/>
    public string ReadExpressions { get; set; }
    /// <inheritdoc cref="MemoryVariable.WriteExpressions"/>
    public string WriteExpressions { get; set; }
    /// <inheritdoc cref="DeviceVariable.IntervalTime"/>
    public int IntervalTime { get; set; }
    /// <inheritdoc cref="DeviceVariable.OtherMethod"/>
    public string OtherMethod { get; set; }
    /// <inheritdoc cref="DeviceVariable.VariableAddress"/>
    public string VariableAddress { get; set; }

    /// <inheritdoc cref="MemoryVariable.ProtectTypeEnum"/>
    public int ProtectTypeEnum { get; set; }
    /// <inheritdoc cref="MemoryVariable.DataTypeEnum"/>
    public int DataTypeEnum { get; set; }
}

