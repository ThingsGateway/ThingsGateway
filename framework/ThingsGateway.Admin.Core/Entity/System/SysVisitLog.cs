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
/// 访问日志表
///</summary>
[SugarTable("sys_visitlog", TableDescription = "访问日志表")]
[Tenant(SqlSugarConst.DB_Default)]
public class SysVisitLog : BaseEntity
{
    /// <summary>
    /// 日志分类
    ///</summary>
    [SugarColumn(ColumnName = "Category", ColumnDescription = "日志分类", Length = 200)]
    [DataTable(Order = 1, IsShow = true, Sortable = true, DefaultFilter = false)]
    public string Category { get; set; }

    /// <summary>
    /// 执行状态
    ///</summary>
    [SugarColumn(ColumnName = "ExeStatus", ColumnDescription = "执行状态", Length = 200)]
    [DataTable(Order = 3, IsShow = true, Sortable = true, DefaultFilter = false)]
    public string ExeStatus { get; set; }

    /// <summary>
    /// 日志名称
    ///</summary>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "日志名称", Length = 200)]
    [DataTable(Order = 2, IsShow = true, Sortable = true, DefaultFilter = false)]
    public string Name { get; set; }

    /// <summary>
    /// 操作人账号
    ///</summary>
    [SugarColumn(ColumnName = "OpAccount", ColumnDescription = "操作人账号", Length = 200, IsNullable = true)]
    [DataTable(Order = 4, IsShow = true, Sortable = true, DefaultFilter = false)]
    public string OpAccount { get; set; }

    /// <summary>
    /// 操作浏览器
    ///</summary>
    [SugarColumn(ColumnName = "OpBrowser", ColumnDescription = "操作浏览器", Length = 200)]
    [DataTable(Order = 5, IsShow = true, Sortable = true, DefaultFilter = false)]
    public string OpBrowser { get; set; }

    /// <summary>
    /// 操作ip
    ///</summary>
    [SugarColumn(ColumnName = "OpIp", ColumnDescription = "操作ip", Length = 200)]
    [DataTable(Order = 6, IsShow = true, Sortable = true, DefaultFilter = false)]
    public string OpIp { get; set; }

    /// <summary>
    /// 操作系统
    ///</summary>
    [SugarColumn(ColumnName = "OpOs", ColumnDescription = "操作系统", Length = 200)]
    [DataTable(Order = 7, IsShow = true, Sortable = true, DefaultFilter = false)]
    public string OpOs { get; set; }

    /// <summary>
    /// 操作时间
    ///</summary>
    [SugarColumn(ColumnName = "OpTime", ColumnDescription = "操作时间")]
    [DataTable(Order = 8, IsShow = true, Sortable = true, DefaultFilter = false)]
    public DateTimeOffset OpTime { get; set; }

    /// <summary>
    /// 验证Id
    ///</summary>
    [SugarColumn(ColumnName = "VerificatId", ColumnDescription = "验证Id")]
    [DataTable(Order = 9, IsShow = true, Sortable = true, DefaultFilter = true)]
    public long VerificatId { get; set; }
}