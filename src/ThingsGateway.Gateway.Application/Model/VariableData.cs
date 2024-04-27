
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using Newtonsoft.Json;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 变量业务变化数据
/// </summary>
public class VariableData : IPrimaryIdEntity
{
    /// <inheritdoc cref="PrimaryIdEntity.Id"/>
    public long Id { get; set; }

    /// <inheritdoc cref="Variable.Name"/>
    public string Name { get; set; }

    /// <inheritdoc cref="VariableRunTime.DeviceName"/>
    public string DeviceName { get; set; }

    /// <inheritdoc cref="VariableRunTime.Value"/>
    public object Value { get; set; }

    /// <inheritdoc cref="VariableRunTime.ChangeTime"/>
    public DateTime ChangeTime { get; set; }

    /// <inheritdoc cref="VariableRunTime.CollectTime"/>
    public DateTime CollectTime { get; set; }

    /// <inheritdoc cref="VariableRunTime.IsOnline"/>
    public bool IsOnline { get; set; }

    /// <inheritdoc cref="VariableRunTime.LastErrorMessage"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? LastErrorMessage { get; set; }

    /// <inheritdoc cref="Device.Remark1"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Remark1 { get; set; }

    /// <inheritdoc cref="Device.Remark2"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Remark2 { get; set; }

    /// <inheritdoc cref="Device.Remark3"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Remark3 { get; set; }

    /// <inheritdoc cref="Device.Remark4"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Remark4 { get; set; }

    /// <inheritdoc cref="Device.Remark5"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string Remark5 { get; set; }
}

/// <summary>
/// 变量基础数据
/// </summary>
public class VariableBasicData : VariableData
{
    /// <inheritdoc cref="Variable.Unit"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Unit { get; set; }

    /// <inheritdoc cref="Variable.Description"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Description { get; set; }

    /// <inheritdoc cref="Variable.ProtectType"/>
    public ProtectTypeEnum ProtectType { get; set; }

    /// <inheritdoc cref="Variable.DataType"/>
    public DataTypeEnum DataType { get; set; }
}

internal class VariableDataWithValue : IPrimaryIdEntity
{
    /// <inheritdoc cref="PrimaryIdEntity.Id"/>
    public long Id { get; set; }

    /// <inheritdoc cref="VariableRunTime.Value"/>
    public object RawValue { get; set; }

    /// <inheritdoc cref="VariableRunTime.CollectTime"/>
    public DateTime CollectTime { get; set; }

    /// <inheritdoc cref="VariableRunTime.IsOnline"/>
    public bool IsOnline { get; set; }

    /// <inheritdoc cref="VariableRunTime.LastErrorMessage"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? LastErrorMessage { get; set; }
}
