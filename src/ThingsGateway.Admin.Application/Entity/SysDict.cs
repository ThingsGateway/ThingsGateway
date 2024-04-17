
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

using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Admin.Application;

[SugarTable("sys_dict", TableDescription = "字典表")]
[Tenant(SqlSugarConst.DB_Admin)]
public class SysDict : BaseEntity
{
    /// <summary>
    /// 类型
    ///</summary>
    [SugarColumn(ColumnDescription = "类型", Length = 200)]
    [AutoGenerateColumn(Ignore = true, Filterable = true, Sortable = true)]
    public virtual DictTypeEnum DictType { get; set; }

    /// <summary>
    /// 分类
    ///</summary>
    [SugarColumn(ColumnDescription = "分类", Length = 200)]
    [Required]
    [AutoGenerateColumn(Searchable = true, Filterable = true, Sortable = true)]
    public string Category { get; set; }

    /// <summary>
    /// 名称
    ///</summary>
    [SugarColumn(ColumnDescription = "名称", Length = 200)]
    [Required]
    [AutoGenerateColumn(Searchable = true, Filterable = true, Sortable = true)]
    public virtual string Name { get; set; }

    /// <summary>
    /// 代码
    ///</summary>
    [SugarColumn(ColumnDescription = "代码", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    [Required]
    [AutoGenerateColumn(Searchable = true, Filterable = true, Sortable = true)]
    public virtual string Code { get; set; }

    /// <summary>
    /// 描述
    ///</summary>
    [SugarColumn(ColumnDescription = "描述", Length = 200, IsNullable = true)]
    public string Remark { get; set; }
}