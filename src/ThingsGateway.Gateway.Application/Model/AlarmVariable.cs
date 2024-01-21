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

using Newtonsoft.Json;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 报警变量
/// </summary>
public class AlarmVariable : PrimaryIdEntity
{
    /// <inheritdoc  cref="Variable.Name"/>
    [SugarColumn(ColumnDescription = "变量名称", IsNullable = false)]
    [DataTable(Order = 1, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string Name { get; set; }

    /// <inheritdoc  cref="Variable.Description"/>
    [SugarColumn(ColumnDescription = "描述", IsNullable = true)]
    [DataTable(Order = 2, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    /// <inheritdoc  cref="VariableRunTime.DeviceName"/>
    [SugarColumn(ColumnDescription = "设备名称", IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string DeviceName { get; set; }

    /// <inheritdoc  cref="Variable.RegisterAddress"/>
    [SugarColumn(ColumnDescription = "变量地址")]
    [DataTable(Order = 4, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string RegisterAddress { get; set; }

    /// <inheritdoc  cref="Variable.DataType"/>
    [SugarColumn(ColumnDescription = "数据类型", ColumnDataType = "varchar(100)")]
    [DataTable(Order = 5, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public DataTypeEnum DataType { get; set; }

    /// <inheritdoc  cref="VariableRunTime.AlarmCode"/>
    [SugarColumn(ColumnDescription = "报警值", IsNullable = false)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string AlarmCode { get; set; }

    /// <inheritdoc  cref="VariableRunTime.AlarmLimit"/>
    [SugarColumn(ColumnDescription = "报警限值", IsNullable = false)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string AlarmLimit { get; set; }

    /// <inheritdoc  cref="VariableRunTime.AlarmText"/>
    [SugarColumn(ColumnDescription = "报警文本", IsNullable = true)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string? AlarmText { get; set; }

    /// <inheritdoc  cref="VariableRunTime.AlarmTime"/>
    [SugarColumn(ColumnDescription = "报警时间", IsNullable = false)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public DateTime AlarmTime { get; set; }

    /// <inheritdoc  cref="VariableRunTime.EventTime"/>
    [SugarColumn(ColumnDescription = "事件时间", IsNullable = false)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public DateTime EventTime { get; set; }

    /// <summary>
    /// 报警类型
    /// </summary>
    [SugarColumn(ColumnDescription = "报警类型", IsNullable = false, ColumnDataType = "varchar(100)")]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public AlarmTypeEnum? AlarmType { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [SugarColumn(ColumnDescription = "事件类型", IsNullable = false, ColumnDataType = "varchar(100)")]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
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