//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 角色授权资源树输出
/// </summary>
public class ResTreeSelector
{
    /// <summary>
    /// 菜单集合
    /// </summary>
    public List<RoleGrantResourceMenu> Menu { get; set; }

    /// <summary>
    /// 授权菜单类
    /// </summary>
    public class RoleGrantResourceMenu
    {
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

        /// <summary>
        /// 菜单下按钮集合
        /// </summary>
        public List<RoleGrantResourceButton> Button { get; set; } = new List<RoleGrantResourceButton>();
    }

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
}

public class PermissionTreeSelector
{
    /// <summary>
    /// 接口描述
    /// </summary>
    public string ApiName { get; set; }

    /// <summary>
    /// 路由名称
    /// </summary>
    public string ApiRoute { get; set; }

    /// <summary>
    /// 权限名称
    /// </summary>
    public string PermissionName { get; set; }
}

/// <summary>
/// Api授权资源树
/// </summary>
public class OpenApiPermissionTreeSelector
{
    /// <summary>
    /// 接口描述
    /// </summary>
    [Description("Api说明")]
    public string ApiName { get; set; }

    /// <summary>
    /// 路由名称
    /// </summary>
    [Description("Api路径")]
    public string ApiRoute { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public List<OpenApiPermissionTreeSelector> Children { get; set; }

    /// <summary>
    /// ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父ID
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 权限名称
    /// </summary>
    [Description("权限名称")]
    public string PermissionName { get; set; }

    /// <summary>
    /// 多个树转列表
    /// </summary>
    public static List<OpenApiPermissionTreeSelector> TreeToList(IList<OpenApiPermissionTreeSelector> data)
    {
        List<OpenApiPermissionTreeSelector> list = new();
        foreach (var item in data)
        {
            list.Add(item);
            if (item.Children != null && item.Children.Count > 0)
            {
                list.AddRange(TreeToList(item.Children));
            }
        }
        return list;
    }
}