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

using Microsoft.Extensions.Logging;



namespace ThingsGateway.Application;

/// <summary>
/// 后台日志表
///</summary>
[SugarTable("tg_log_backend", TableDescription = "后台日志表")]
[Tenant(SqlSugarConst.DB_Default)]
public class BackendLog : PrimaryIdEntity
{
    /// <summary>
    /// 日志时间
    /// </summary>
    [SugarColumn(ColumnName = "LogTime", ColumnDescription = "日志时间", IsNullable = false)]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public DateTimeOffset LogTime { get; set; }
    /// <summary>
    /// 日志级别
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(50)", ColumnName = "LogLevel", ColumnDescription = "日志级别", SqlParameterDbType = typeof(SqlSugar.DbConvert.EnumToStringConvert), IsNullable = false)]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public LogLevel LogLevel { get; set; }
    /// <summary>
    /// 日志来源
    ///</summary>
    [SugarColumn(ColumnName = "LogSource", ColumnDescription = "日志来源", IsNullable = false)]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string LogSource { get; set; }
    /// <summary>
    /// 具体消息
    ///</summary>
    [SugarColumn(ColumnName = "LogMessage", ColumnDescription = "具体消息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [DataTable(Order = 4, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string LogMessage { get; set; }
    /// <summary>
    /// 异常对象
    /// </summary>
    [SugarColumn(ColumnName = "Exception", ColumnDescription = "异常对象", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [DataTable(Order = 5, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string Exception { get; set; }
}