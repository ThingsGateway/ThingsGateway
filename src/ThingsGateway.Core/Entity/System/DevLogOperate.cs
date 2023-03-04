namespace ThingsGateway.Core
{
    /// <summary>
    /// 操作日志表
    ///</summary>
    [SugarTable("dev_log_operate", TableDescription = "操作日志表")]
    [Tenant(SqlsugarConst.DB_Default)]
    public class DevLogOperate : DevLogVisit
    {
        /// <summary>
        /// 类名称
        ///</summary>
        [SugarColumn(ColumnName = "ClassName", ColumnDescription = "类名称", Length = 200)]
        public string ClassName { get; set; }

        /// <summary>
        /// 具体消息
        ///</summary>
        [SugarColumn(ColumnName = "ExeMessage", ColumnDescription = "具体消息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
        public string ExeMessage { get; set; }

        /// <summary>
        /// 方法名称
        ///</summary>
        [SugarColumn(ColumnName = "MethodName", ColumnDescription = "方法名称", Length = 200)]
        public string MethodName { get; set; }

        /// <summary>
        /// 请求参数
        ///</summary>
        [SugarColumn(ColumnName = "ParamJson", ColumnDescription = "请求参数", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
        public string ParamJson { get; set; }

        /// <summary>
        /// 请求方式
        ///</summary>
        [SugarColumn(ColumnName = "ReqMethod", ColumnDescription = "请求方式", Length = 200, IsNullable = true)]
        public string ReqMethod { get; set; }

        /// <summary>
        /// 请求地址
        ///</summary>
        [SugarColumn(ColumnName = "ReqUrl", ColumnDescription = "请求地址", ColumnDataType = StaticConfig.CodeFirst_BigString)]
        public string ReqUrl { get; set; }

        /// <summary>
        /// 返回结果
        ///</summary>
        [SugarColumn(ColumnName = "ResultJson", ColumnDescription = "返回结果", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
        public string ResultJson { get; set; }
    }
}