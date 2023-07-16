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