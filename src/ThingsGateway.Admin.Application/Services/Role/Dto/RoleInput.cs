
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Admin.Application;

/// <summary>
/// 角色授权资源参数
/// </summary>
public class GrantResourceData
{
    /// <summary>
    /// 角色Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 授权资源信息
    /// </summary>
    public IEnumerable<long> GrantInfoList { get; set; } = Enumerable.Empty<long>();
}

/// <summary>
/// 角色拥有权限输出
/// </summary>
public class GrantPermissionData
{
    /// <summary>
    /// 角色Id/用户Id
    /// </summary>
    public virtual long Id { get; set; }

    /// <summary>
    /// 已授权权限信息
    /// </summary>
    public virtual IEnumerable<RelationRolePermission> GrantInfoList { get; set; } = Enumerable.Empty<RelationRolePermission>();
}

/// <summary>
/// 角色授权用户参数
/// </summary>
public class GrantUserOrRoleInput
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 授权权限信息
    /// </summary>
    public IEnumerable<long> GrantInfoList { get; set; } = Enumerable.Empty<long>();
}