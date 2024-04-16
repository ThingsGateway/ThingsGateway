//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Newtonsoft.Json;

using SqlSugar;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 报警变量
/// </summary>
public class AlarmVariable : PrimaryIdEntity, IDBHistoryAlarm
{
    /// <inheritdoc  cref="Variable.Name"/>
    [SugarColumn(ColumnDescription = "变量名称", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string Name { get; set; }

    /// <inheritdoc  cref="Variable.Description"/>
    [SugarColumn(ColumnDescription = "描述", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    /// <inheritdoc  cref="VariableRunTime.DeviceName"/>
    [SugarColumn(ColumnDescription = "设备名称", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string DeviceName { get; set; }

    /// <inheritdoc  cref="Variable.RegisterAddress"/>
    [SugarColumn(ColumnDescription = "变量地址")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string RegisterAddress { get; set; }

    /// <inheritdoc  cref="Variable.DataType"/>
    [SugarColumn(ColumnDescription = "数据类型")]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public DataTypeEnum DataType { get; set; }

    /// <inheritdoc  cref="VariableRunTime.AlarmCode"/>
    [SugarColumn(ColumnDescription = "报警值", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string AlarmCode { get; set; }

    /// <inheritdoc  cref="VariableRunTime.AlarmLimit"/>
    [SugarColumn(ColumnDescription = "报警限值", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string AlarmLimit { get; set; }

    /// <inheritdoc  cref="VariableRunTime.AlarmText"/>
    [SugarColumn(ColumnDescription = "报警文本", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public string? AlarmText { get; set; }

    /// <inheritdoc  cref="VariableRunTime.AlarmTime"/>
    [SugarColumn(ColumnDescription = "报警时间", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public DateTime AlarmTime { get; set; }

    /// <inheritdoc  cref="VariableRunTime.EventTime"/>
    [SugarColumn(ColumnDescription = "事件时间", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public DateTime EventTime { get; set; }

    /// <summary>
    /// 报警类型
    /// </summary>
    [SugarColumn(ColumnDescription = "报警类型", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public AlarmTypeEnum? AlarmType { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [SugarColumn(ColumnDescription = "事件类型", IsNullable = false)]
    [AutoGenerateColumn(Visible = true, Filterable = true, Sortable = true)]
    public EventTypeEnum EventType { get; set; }

    /// <inheritdoc cref="Device.Remark1"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark1 { get; set; }

    /// <inheritdoc cref="Device.Remark2"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark2 { get; set; }

    /// <inheritdoc cref="Device.Remark3"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark3 { get; set; }

    /// <inheritdoc cref="Device.Remark4"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark4 { get; set; }

    /// <inheritdoc cref="Device.Remark5"/>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Remark5 { get; set; }
}