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

namespace ThingsGateway.Admin.Application;

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

    /// <summary>
    /// 分类
    ///</summary>
    [SugarColumn(ColumnDescription = "分类", Length = 200, IsNullable = false)]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public virtual string Category { get; set; }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null || !(obj is UserSelectorOutput))
        {
            return false;
        }

        return Id == ((UserSelectorOutput)obj).Id;
    }
}