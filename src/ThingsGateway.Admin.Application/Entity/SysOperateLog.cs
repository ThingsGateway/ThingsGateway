//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 操作日志表
///</summary>
[SugarTable("sys_operatelog", TableDescription = "操作日志表")]
[Tenant(SqlSugarConst.DB_Log)]
public class SysOperateLog
{
    /// <summary>
    /// 日志分类
    ///</summary>
    [SugarColumn(ColumnDescription = "日志分类", Length = 200)]
    [AutoGenerateColumn(Order = 1, Filterable = true, Sortable = true)]
    public LogCateGoryEnum Category { get; set; }

    /// <summary>
    /// 日志名称
    ///</summary>
    [SugarColumn(ColumnDescription = "日志名称", Length = 200)]
    [AutoGenerateColumn(Order = 2, Filterable = true, Sortable = true)]
    public string Name { get; set; }

    /// <summary>
    /// 类名称
    ///</summary>
    [SugarColumn(ColumnDescription = "类名称", Length = 200)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string ClassName { get; set; }

    /// <summary>
    /// 方法名称
    ///</summary>
    [SugarColumn(ColumnDescription = "方法名称", Length = 200)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string MethodName { get; set; }

    /// <summary>
    /// 请求参数
    ///</summary>
    [SugarColumn(ColumnDescription = "请求参数", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(ShowTips = true, Filterable = true, Sortable = true)]
    public string? ParamJson { get; set; }

    /// <summary>
    /// 请求方式
    ///</summary>
    [SugarColumn(ColumnDescription = "请求方式", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? ReqMethod { get; set; }

    /// <summary>
    /// 请求地址
    ///</summary>
    [SugarColumn(ColumnDescription = "请求地址", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string? ReqUrl { get; set; }

    /// <summary>
    /// 返回结果
    ///</summary>
    [SugarColumn(ColumnDescription = "返回结果", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(ShowTips = true, Filterable = true, Sortable = true)]
    public string? ResultJson { get; set; }

    /// <summary>
    /// 具体消息
    ///</summary>
    [SugarColumn(ColumnDescription = "具体消息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [AutoGenerateColumn(ShowTips = true, Filterable = true, Sortable = true)]
    public string? ExeMessage { get; set; }

    /// <summary>
    /// 执行状态
    ///</summary>
    [SugarColumn(ColumnDescription = "执行状态", Length = 200)]
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public bool ExeStatus { get; set; }

    /// <summary>
    /// 操作账号
    ///</summary>
    [SugarColumn(ColumnDescription = "操作账号", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public string? OpAccount { get; set; }

    /// <summary>
    /// 操作浏览器
    ///</summary>
    [SugarColumn(ColumnDescription = "操作浏览器", Length = 200)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string OpBrowser { get; set; }

    /// <summary>
    /// 操作ip
    ///</summary>
    [SugarColumn(ColumnDescription = "操作ip", Length = 200)]
    [AutoGenerateColumn(Filterable = true, Sortable = true)]
    public string? OpIp { get; set; }

    /// <summary>
    /// 操作系统
    ///</summary>
    [SugarColumn(ColumnDescription = "操作系统", Length = 200)]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public string OpOs { get; set; }

    /// <summary>
    /// 操作时间
    ///</summary>
    [SugarColumn(ColumnDescription = "操作时间")]
    [AutoGenerateColumn(Visible = true, DefaultSort = true, Sortable = true, DefaultSortOrder = SortOrder.Desc)]
    public DateTime OpTime { get; set; }

    /// <summary>
    /// 验证Id
    ///</summary>
    [SugarColumn(ColumnDescription = "验证Id")]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, Filterable = true, Sortable = true)]
    public long VerificatId { get; set; }
}