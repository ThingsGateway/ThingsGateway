//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Components;

/// <summary>
/// NavItemExtensions
/// </summary>
public static class NavItemExtension
{
    /// <summary>
    /// PasePageTabItem
    /// </summary>
    /// <param name="nav"></param>
    /// <returns></returns>
    public static List<PageTabItem> ParsePageTabItem(this List<NavItem> nav)
    {
        List<PageTabItem> pageTabItems = new();
        if (nav == null) return pageTabItems;
        foreach (var item in nav)
        {
            if ((!string.IsNullOrEmpty(item.Href)))
            {
                pageTabItems.Add(new PageTabItem(item.Title, item.Href, item.Icon ?? string.Empty));
            }
            if (item.HasChildren())
            {
                pageTabItems.AddRange(item.Children.ParsePageTabItem());
            }
        }
        return pageTabItems;
    }
}