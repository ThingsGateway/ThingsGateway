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

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// NavItemExtensions
/// </summary>
public static class NavItemExtensions
{
    /// <inheritdoc />
    public static List<PageTabItem> PasePageTabItem(this List<SysResource> sysResources)
    {
        List<PageTabItem> pageTabItems = new();
        if (sysResources == null) return pageTabItems;
        foreach (var item in sysResources)
        {
            if ((!string.IsNullOrEmpty(item.Component)))
            {
                pageTabItems.Add(new PageTabItem(item.Title, item.Component, item.Icon ?? string.Empty));
            }
            if (item.Children != null && item.Children.Count > 0)
            {
                pageTabItems.AddRange(item.Children.PasePageTabItem());
            }
        }
        return pageTabItems;
    }

    /// <summary>
    /// 转化为NavItem
    /// </summary>
    /// <param name="menus"></param>
    /// <returns></returns>
    public static List<NavItem> ParseNavItem(this List<SysResource> menus)
    {
        List<NavItem> items = new();
        foreach (var menu in menus)
        {
            var item = menu.ParseNavItem();
            if (menu.Children?.Count > 0)
            {
                item.Children = menu.Children.ParseNavItem();
            }
            if (menu.Category == ResourceCategoryEnum.MENU || menu.Category == ResourceCategoryEnum.SPA)
            {
                items.Add(item);
            }
            else if (item.Children != null)
            {
                items.AddRange(item.Children);
            }
        }
        return items;
    }

    private static NavItem ParseNavItem(this SysResource menu) => new()
    {
        Title = menu.Title,
        Icon = menu.Icon,
        Href = menu.Component,
        Target = menu.TargetType == TargetTypeEnum.SELF ? "_self" : "_blank",
    };
}