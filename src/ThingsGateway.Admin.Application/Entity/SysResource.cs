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

using System.ComponentModel;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 系统资源表
///</summary>
[SugarTable("sys_resource", TableDescription = "系统资源表")]
[Tenant(SqlSugarConst.DB_Admin)]
public class SysResource : BaseEntity
{
    /// <summary>
    /// 父id
    ///</summary>
    [SugarColumn(ColumnDescription = "父id", IsNullable = true)]
    public virtual long? ParentId { get; set; }

    /// <summary>
    /// 标题
    ///</summary>
    [SugarColumn(ColumnDescription = "标题", Length = 200)]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public virtual string Title { get; set; }

    /// <summary>
    /// 编码
    ///</summary>
    [SugarColumn(ColumnDescription = "编码", Length = 200, IsNullable = true)]
    [DataTable(Order = 5, IsShow = true, Sortable = true, DefaultFilter = true)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 分类
    ///</summary>
    [SugarColumn(ColumnDescription = "分类", Length = 200)]
    [DataTable(Order = 4, IsShow = true, Sortable = true, DefaultFilter = true)]
    public string Category { get; set; }

    /// <summary>
    /// 菜单类型
    ///</summary>
    [SugarColumn(ColumnDescription = "菜单类型", ColumnDataType = "varchar(50)")]
    [DataTable(Order = 4, IsShow = true, Sortable = true, DefaultFilter = true)]
    public virtual MenuTypeEnum MenuType { get; set; }

    /// <summary>
    /// 路径
    ///</summary>
    [SugarColumn(ColumnDescription = "路径", Length = 200, IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public virtual string Href { get; set; }

    /// <summary>
    /// 图标
    ///</summary>
    [SugarColumn(ColumnDescription = "图标", Length = 200, IsNullable = true)]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public virtual string Icon { get; set; }

    /// <summary>
    /// 是否隐藏
    /// </summary>
    [SugarColumn(ColumnDescription = "隐藏", Length = 200)]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public bool Hidden { get; set; } = false;

    /// <summary>
    /// 子节点
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<SysResource> Children { get; set; }
}

/// <summary>
/// 链接跳转类型
/// </summary>
public enum MenuTypeEnum
{
    /// <summary>
    /// 目录
    /// </summary>
    [Description("目录")]
    CATALOG = 0,

    /// <summary>
    /// 组件
    /// </summary>
    [Description("组件")]
    MENU,

    /// <summary>
    /// 内链
    /// </summary>
    [Description("内链")]
    IFRAME,

    /// <summary>
    /// 外链
    /// </summary>
    [Description("外链")]
    LINK,
}