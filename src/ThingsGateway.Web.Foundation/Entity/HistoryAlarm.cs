#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 历史报警表
/// </summary>
[IgnoreSqlTable]
[SugarTable("historyAlarm", TableDescription = "历史报警表")]
public class HistoryAlarm : PrimaryIdEntity
{
    /// <inheritdoc  cref="MemoryVariable.Name"/>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "变量名称", IsNullable = false)]
    public string Name { get; set; }

    /// <inheritdoc  cref="MemoryVariable.Description"/>
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", IsNullable = true)]
    public string Description { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.DeviceName"/>
    [SugarColumn(ColumnName = "DeviceName", ColumnDescription = "设备名称", IsNullable = true)]
    public string DeviceName { get; set; }

    /// <inheritdoc  cref="DeviceVariable.VariableAddress"/>
    [SugarColumn(ColumnName = "VariableAddress", ColumnDescription = "变量地址")]
    public string VariableAddress { get; set; }

    /// <inheritdoc  cref="MemoryVariable.DataTypeEnum"/>
    [SugarColumn(ColumnName = "DataTypeEnum", ColumnDescription = "数据类型", ColumnDataType = "varchar(100)")]
    public DataTypeEnum DataTypeEnum { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.IsOnline"/>
    [SugarColumn(ColumnName = "Quality", ColumnDescription = "质量戳")]
    public bool IsOnline { get; set; }


    /// <inheritdoc  cref="DeviceVariableRunTime.Value"/>
    [SugarColumn(ColumnName = "Value", ColumnDescription = "变量值", IsNullable = false)]
    public string Value { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.AlarmCode"/>
    [SugarColumn(ColumnName = "AlarmCode", ColumnDescription = "报警值", IsNullable = false)]
    public string AlarmCode { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.AlarmLimit"/>
    [SugarColumn(ColumnName = "AlarmLimit", ColumnDescription = "报警限值", IsNullable = false)]
    public string AlarmLimit { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.AlarmText"/>
    [SugarColumn(ColumnName = "AlarmText", ColumnDescription = "报警文本", IsNullable = true)]
    public string AlarmText { get; set; }

    /// <inheritdoc  cref="DeviceVariableRunTime.AlarmTime"/>
    [SugarColumn(ColumnName = "AlarmTime", ColumnDescription = "报警时间", IsNullable = false)]
    public DateTime AlarmTime { get; set; }
    /// <inheritdoc  cref="DeviceVariableRunTime.EventTime"/>
    [SugarColumn(ColumnName = "EventTime", ColumnDescription = "事件时间", IsNullable = false)]
    public DateTime EventTime { get; set; }


    /// <summary>
    /// 报警类型
    /// </summary>
    [SugarColumn(ColumnName = "AlarmTypeEnum", ColumnDescription = "报警类型", IsNullable = false, ColumnDataType = "varchar(100)")]
    public AlarmEnum AlarmTypeEnum { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [SugarColumn(ColumnName = "EventTypeEnum", ColumnDescription = "事件类型", IsNullable = false, ColumnDataType = "varchar(100)")]
    public EventEnum EventTypeEnum { get; set; }



}
/// <summary>
/// 报警类型
/// </summary>
public enum AlarmEnum
{
    /// <summary>
    /// 无
    /// </summary>
    None,
    /// <summary>
    /// Bool On
    /// </summary>
    Open,
    /// <summary>
    /// Bool Off
    /// </summary>
    Close,
    /// <summary>
    /// HH
    /// </summary>
    HH,
    /// <summary>
    /// H
    /// </summary>
    H,
    /// <summary>
    /// L
    /// </summary>
    L,
    /// <summary>
    /// LL
    /// </summary>
    LL,
}
/// <summary>
/// 报警事件类型
/// </summary>
public enum EventEnum
{
    /// <summary>
    /// 无
    /// </summary>
    None,
    /// <summary>
    /// 报警产生
    /// </summary>
    Alarm,
    /// <summary>
    /// 报警确认
    /// </summary>
    Check,
    /// <summary>
    /// 报警恢复
    /// </summary>
    Finish,
}
/// <summary>
/// 数据库类型
/// </summary>
public enum SqlDbType
{
    /// <summary>
    /// SqlServer
    /// </summary>
    SqlServer,
    /// <summary>
    /// Mysql
    /// </summary>
    Mysql,
    /// <summary>
    /// Sqlite
    /// </summary>
    Sqlite,
    /// <summary>
    /// PostgreSQL
    /// </summary>
    PostgreSQL,
    /// <summary>
    /// Oracle
    /// </summary>
    Oracle,
}