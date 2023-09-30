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
/// 角色按钮资源
/// </summary>
public class RoleGrantResourceButton
{
    /// <summary>
    /// 按钮id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }
}

/// <summary>
/// 授权菜单类
/// </summary>
public class RoleGrantResourceMenu
{
    /// <summary>
    /// 菜单下按钮集合
    /// </summary>
    public List<RoleGrantResourceButton> Button { get; set; } = new List<RoleGrantResourceButton>();

    /// <summary>
    /// 菜单id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父id
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 父名称
    /// </summary>
    public string ParentName { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string Title { get; set; }
}

/// <summary>
/// Blazor Server的组件路由内容
/// </summary>
public class PermissionTreeSelector
{
    /// <summary>
    /// 路由名称
    /// </summary>
    public string ApiRoute { get; set; }
}