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

using SqlSugar;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 操作日志表
///</summary>
[SugarTable("sys_operatelog", TableDescription = "操作日志表")]
[Tenant(SqlSugarConst.DB_Log)]
public class SysOperateLog : SysVisitLog
{
    /// <summary>
    /// 类名称
    ///</summary>
    [SugarColumn(ColumnDescription = "类名称", Length = 200)]
    [DataTable(Order = 21, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string ClassName { get; set; }

    /// <summary>
    /// 具体消息
    ///</summary>
    [SugarColumn(ColumnDescription = "具体消息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [DataTable(Order = 22, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string ExeMessage { get; set; }

    /// <summary>
    /// 方法名称
    ///</summary>
    [SugarColumn(ColumnDescription = "方法名称", Length = 200)]
    [DataTable(Order = 23, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string MethodName { get; set; }

    /// <summary>
    /// 请求参数
    ///</summary>
    [SugarColumn(ColumnDescription = "请求参数", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [DataTable(Order = 24, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string ParamJson { get; set; }

    /// <summary>
    /// 请求方式
    ///</summary>
    [SugarColumn(ColumnDescription = "请求方式", Length = 200, IsNullable = true)]
    [DataTable(Order = 25, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string ReqMethod { get; set; }

    /// <summary>
    /// 请求地址
    ///</summary>
    [SugarColumn(ColumnDescription = "请求地址", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [DataTable(Order = 26, IsShow = true, Sortable = true, DefaultFilter = true, CellClass = " table-text-truncate ")]
    public string ReqUrl { get; set; }

    /// <summary>
    /// 返回结果
    ///</summary>
    [SugarColumn(ColumnDescription = "返回结果", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [DataTable(Order = 27, IsShow = true, Sortable = true, DefaultFilter = false, CellClass = " table-text-truncate ")]
    public string ResultJson { get; set; }
}