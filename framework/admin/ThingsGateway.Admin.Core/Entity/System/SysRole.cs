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
/// 系统角色表
///</summary>
[SugarTable("sys_role", TableDescription = "系统角色表")]
[Tenant(SqlSugarConst.DB_Admin)]
public class SysRole : BaseEntity
{
    /// <summary>
    /// 编码
    ///</summary>
    [SugarColumn(ColumnDescription = "编码", Length = 200)]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public string Code { get; set; }

    /// <summary>
    /// 名称
    ///</summary>
    [SugarColumn(ColumnDescription = "名称", Length = 200)]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public virtual string Name { get; set; }
}

/// <summary>
/// SYS_ROLE_HAS_RESOURCE
/// 角色有哪些资源扩展
/// </summary>
public class RelationRoleResuorce
{
    /// <summary>
    /// 按钮信息
    /// </summary>
    public List<long> ButtonInfo { get; set; } = new List<long>();

    /// <summary>
    /// 菜单ID
    /// </summary>
    public long MenuId { get; set; }
}

/// <summary>
/// 角色拥有的资源输出
/// </summary>
public class RoleOwnResourceOutput
{
    /// <summary>
    /// 已授权资源信息
    /// </summary>
    public virtual List<RelationRoleResuorce> GrantInfoList { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public virtual long Id { get; set; }
}

/// <summary>
/// SYS_ROLE_HAS_PERMISSION
/// 角色权限关系扩展
/// </summary>
public class RelationRolePermission
{
    /// <summary>
    /// 接口Url
    /// </summary>
    public string ApiUrl { get; set; }
}