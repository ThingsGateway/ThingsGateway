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
/// 历史报警表
/// </summary>
[IgnoreSqlTable]
[SugarTable("historyAlarm", TableDescription = "历史报警表")]
public class HistoryAlarm : PrimaryIdEntity
{
    /// <inheritdoc  cref="MemoryVariable.Name"/>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "变量名称", IsNullable = false)]
    [DataTable(Order = 1, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string Name { get; set; }

    /// <inheritdoc  cref="MemoryVariable.Description"/>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", IsNullable = true)]
    [DataTable(Order = 2, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string Description { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.DeviceName"/>
    [SugarColumn(ColumnName = "DeviceName", ColumnDescription = "设备名称", IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string DeviceName { get; set; }

    /// <inheritdoc  cref="DeviceVariable.VariableAddress"/>
    [SugarColumn(ColumnName = "VariableAddress", ColumnDescription = "变量地址")]
    [DataTable(Order = 4, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string VariableAddress { get; set; }

    /// <inheritdoc  cref="MemoryVariable.DataTypeEnum"/>
    [SugarColumn(ColumnName = "DataTypeEnum", ColumnDescription = "数据类型", ColumnDataType = "varchar(100)")]
    [DataTable(Order = 5, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public DataTypeEnum DataTypeEnum { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.IsOnline"/>
    [SugarColumn(ColumnName = "IsOnline", ColumnDescription = "是否在线")]
    [DataTable(Order = 6, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public bool IsOnline { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.Value"/>
    [SugarColumn(ColumnName = "Value", ColumnDescription = "变量值", IsNullable = false)]
    [DataTable(Order = 7, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string Value { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.AlarmCode"/>
    [SugarColumn(ColumnName = "AlarmCode", ColumnDescription = "报警值", IsNullable = false)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string AlarmCode { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.AlarmLimit"/>
    [SugarColumn(ColumnName = "AlarmLimit", ColumnDescription = "报警限值", IsNullable = false)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string AlarmLimit { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.AlarmText"/>
    [SugarColumn(ColumnName = "AlarmText", ColumnDescription = "报警文本", IsNullable = true)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string AlarmText { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.AlarmTime"/>
    [SugarColumn(ColumnName = "AlarmTime", ColumnDescription = "报警时间", IsNullable = false)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public DateTimeOffset AlarmTime { get; set; }


    /// <inheritdoc  cref="DeviceVariableRunTime.EventTime"/>
    [SugarColumn(ColumnName = "EventTime", ColumnDescription = "事件时间", IsNullable = false)]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public DateTimeOffset EventTime { get; set; }


    /// <summary>
    /// 报警类型
    /// </summary>
    [SugarColumn(ColumnName = "AlarmTypeEnum", ColumnDescription = "报警类型", IsNullable = false, ColumnDataType = "varchar(100)")]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public AlarmEnum AlarmTypeEnum { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [SugarColumn(ColumnName = "EventTypeEnum", ColumnDescription = "事件类型", IsNullable = false, ColumnDataType = "varchar(100)")]
    [DataTable(Order = 8, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public EventEnum EventTypeEnum { get; set; }

}
