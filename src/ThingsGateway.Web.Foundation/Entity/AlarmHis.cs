using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;
/// <summary>
/// 历史报警表
/// </summary>
[IgnoreSqlTableAttribute]
[SugarTable("alarm_his", TableDescription = "历史报警表")]
[Tenant(SqlsugarConst.DB_CustomId)]
public class AlarmHis : PrimaryIdEntity
{
    [SugarColumn(ColumnName = "Name", ColumnDescription = "变量名称", IsNullable = false)]
    public string Name { get; set; }
    [SugarColumn(ColumnName = "Description", ColumnDescription = "描述", IsNullable = true)]
    public string Description { get; set; }

    [SugarColumn(ColumnName = "DeviceName", ColumnDescription = "设备名称", IsNullable = true)]
    public string DeviceName { get; set; }

    [SugarColumn(ColumnName = "VariableAddress", ColumnDescription = "变量地址")]
    public string VariableAddress { get; set; }

    [SugarColumn(ColumnName = "DataTypeEnum", ColumnDescription = "数据类型", ColumnDataType = "varchar(100)")]
    public DataTypeEnum DataTypeEnum { get; set; }

    [SugarColumn(ColumnName = "Quality", ColumnDescription = "质量戳")]
    public int Quality { get; set; }


    [SugarColumn(ColumnName = "Value", ColumnDescription = "变量值", IsNullable = false)]
    public string Value { get; set; }

    [SugarColumn(ColumnName = "AlarmCode", ColumnDescription = "报警值", IsNullable = false)]
    public string AlarmCode { get; set; }

    [SugarColumn(ColumnName = "AlarmLimit", ColumnDescription = "报警限值", IsNullable = false)]
    public string AlarmLimit { get; set; }

    [SugarColumn(ColumnName = "AlarmText", ColumnDescription = "报警文本", IsNullable = true)]
    public string AlarmText { get; set; }

    [SugarColumn(ColumnName = "AlarmTime", ColumnDescription = "报警时间", IsNullable = false)]
    public DateTime AlarmTime { get; set; }
    [SugarColumn(ColumnName = "EventTime", ColumnDescription = "事件时间", IsNullable = false)]
    public DateTime EventTime { get; set; }



    [SugarColumn(ColumnName = "AlarmTypeEnum", ColumnDescription = "报警类型", IsNullable = false, ColumnDataType = "varchar(100)")]
    public AlarmEnum AlarmTypeEnum { get; set; }


    [SugarColumn(ColumnName = "EventTypeEnum", ColumnDescription = "事件类型", IsNullable = false, ColumnDataType = "varchar(100)")]
    public EventEnum EventTypeEnum { get; set; }



}
/// <summary>
/// 报警类型
/// </summary>
public enum AlarmEnum
{
    None,
    Open,
    Close,
    HH,
    H,
    L,
    LL,
}
/// <summary>
/// 报警事件类型
/// </summary>
public enum EventEnum
{
    Alarm,
    Check,
    Finish,
}

public enum SqlDbType
{
    SqlServer,
    Mysql,
    Sqlite,
    PostgreSQL,
    Oracle,
}