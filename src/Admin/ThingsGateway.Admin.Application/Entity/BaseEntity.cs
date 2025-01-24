//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using SqlSugar;

using System.Diagnostics.CodeAnalysis;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 主键id基类
/// </summary>
public abstract class PrimaryIdEntity : IPrimaryIdEntity
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [SugarColumn(ColumnDescription = "Id", IsPrimaryKey = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenEdit = false, IsVisibleWhenAdd = false, Sortable = true, DefaultSort = true, DefaultSortOrder = SortOrder.Asc)]
    public virtual long Id { get; set; }
}

/// <summary>
/// 主键实体基类
/// </summary>
public abstract class PrimaryKeyEntity : PrimaryIdEntity
{
    /// <summary>
    /// 拓展信息
    /// </summary>
    [SugarColumn(ColumnDescription = "扩展信息", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual string? ExtJson { get; set; }
}

public interface IBaseEntity
{
    DateTime? CreateTime { get; set; }
    string? CreateUser { get; set; }
    long CreateUserId { get; set; }
    bool IsDelete { get; set; }
    int? SortCode { get; set; }
    DateTime? UpdateTime { get; set; }
    string? UpdateUser { get; set; }
    long? UpdateUserId { get; set; }
}

/// <summary>
/// 框架实体基类
/// </summary>
public abstract class BaseEntity : PrimaryKeyEntity, IBaseEntity
{
    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public virtual DateTime? CreateTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [SugarColumn(ColumnDescription = "创建人", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [NotNull]
    [AutoGenerateColumn(Ignore = true)]
    public virtual string? CreateUser { get; set; }

    /// <summary>
    /// 创建者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者Id", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long CreateUserId { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    [SugarColumn(ColumnDescription = "软删除", IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual bool IsDelete { get; set; } = false;


    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Visible = false, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public virtual DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    [SugarColumn(ColumnDescription = "更新人", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual string? UpdateUser { get; set; }

    /// <summary>
    /// 修改者Id
    /// </summary>
    [SugarColumn(ColumnDescription = "修改者Id", IsOnlyIgnoreInsert = true, IsNullable = true)]
    [IgnoreExcel]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long? UpdateUserId { get; set; }

    /// <summary>
    /// 排序码
    ///</summary>
    [SugarColumn(ColumnDescription = "排序码", IsNullable = true)]
    [AutoGenerateColumn(Visible = false, DefaultSort = true, Sortable = true, DefaultSortOrder = SortOrder.Asc)]
    [IgnoreExcel]
    public int? SortCode { get; set; }
}

public interface IBaseDataEntity
{
    long CreateOrgId { get; set; }
}

/// <summary>
/// 业务数据实体基类(数据权限)
/// </summary>
public abstract class BaseDataEntity : BaseEntity, IBaseDataEntity
{
    /// <summary>
    /// 创建者部门Id
    /// </summary>
    [SugarColumn(ColumnDescription = "创建者部门Id", IsOnlyIgnoreUpdate = true, IsNullable = true)]
    [AutoGenerateColumn(Ignore = true)]
    [IgnoreExcel]
    public virtual long CreateOrgId { get; set; }
}
