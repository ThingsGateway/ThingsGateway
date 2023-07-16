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

using Microsoft.Extensions.Logging;

using SqlSugar.DbConvert;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 后台日志表
///</summary>
[SugarTable("tg_log_backend", TableDescription = "后台日志表")]
[Tenant(SqlsugarConst.DB_Default)]
public class BackendLog : PrimaryIdEntity
{
    /// <summary>
    /// 日志时间
    /// </summary>
    [SugarColumn(ColumnName = "LogTime", ColumnDescription = "日志时间", IsNullable = false)]
    public DateTime LogTime { get; set; }
    /// <summary>
    /// 日志级别
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(50)", ColumnName = "LogLevel", ColumnDescription = "日志级别", SqlParameterDbType = typeof(EnumToStringConvert), IsNullable = false)]
    public LogLevel LogLevel { get; set; }
    /// <summary>
    /// 日志来源
    ///</summary>
    [SugarColumn(ColumnName = "LogSource", ColumnDescription = "日志来源", IsNullable = false)]
    public string LogSource { get; set; }
    /// <summary>
    /// 具体消息
    ///</summary>
    [SugarColumn(ColumnName = "LogMessage", ColumnDescription = "具体消息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string LogMessage { get; set; }
    /// <summary>
    /// 异常对象
    /// </summary>
    [SugarColumn(ColumnName = "Exception", ColumnDescription = "异常对象", IsNullable = true)]
    public string Exception { get; set; }
}