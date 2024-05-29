//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components.Routing;

using Newtonsoft.Json;

using SqlSugar;

using System.ComponentModel.DataAnnotations;

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
    [SugarColumn(ColumnDescription = "父id")]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long ParentId { get; set; } = 0;

    /// <summary>
    /// 模块
    ///</summary>
    [SugarColumn(ColumnDescription = "模块")]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false, Searchable = true)]
    public virtual long Module { get; set; }

    /// <summary>
    /// 标题
    ///</summary>
    [SugarColumn(ColumnDescription = "标题", Length = 200)]
    [Required]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true, Searchable = true)]
    public virtual string Title { get; set; }

    /// <summary>
    /// 图标
    ///</summary>
    [SugarColumn(ColumnDescription = "图标", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = false, Filterable = false)]
    public virtual string? Icon { get; set; }

    /// <summary>
    /// 编码
    ///</summary>
    [SugarColumn(ColumnDescription = "编码", Length = 200)]
    [Required]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 分类
    ///</summary>
    [SugarColumn(ColumnDescription = "分类")]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public ResourceCategoryEnum Category { get; set; } = ResourceCategoryEnum.Menu;

    /// <summary>
    /// 目标类型
    ///</summary>
    [SugarColumn(ColumnDescription = "目标类型", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual TargetEnum? Target { get; set; }

    /// <summary>
    /// 菜单匹配类型
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单匹配类型", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual NavLinkMatch? NavLinkMatch { get; set; }

    /// <summary>
    /// 路径
    ///</summary>
    [SugarColumn(ColumnDescription = "路径", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true, Searchable = true)]
    public virtual string Href { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public List<SysResource>? Children { get; set; }
}
