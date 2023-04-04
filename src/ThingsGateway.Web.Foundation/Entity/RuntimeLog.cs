using Microsoft.Extensions.Logging;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 运行日志表
    ///</summary>
    [SugarTable("tg_log_runtime", TableDescription = "运行日志表")]
    [Tenant(SqlsugarConst.DB_Default)]
    public class RuntimeLog : PrimaryIdEntity
    {
        /// <summary>
        /// 日志时间
        /// </summary>
        [SugarColumn(ColumnName = "LogTime", ColumnDescription = "日志时间", IsNullable = false)]
        public DateTime LogTime { get; set; }
        /// <summary>
        /// 日志级别
        /// </summary>
        [SugarColumn(ColumnName = "LogLevel", ColumnDescription = "日志级别", IsNullable = false)]
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
}