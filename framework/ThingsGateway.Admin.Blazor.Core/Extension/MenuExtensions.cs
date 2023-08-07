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

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor.Core;
/// <summary>
/// 菜单扩展
/// </summary>
public static class MenuExtensions
{
    /// <summary>
    /// 转化为NavItem
    /// </summary>
    /// <param name="menus"></param>
    /// <returns></returns>
    public static List<NavItem> Parse(this List<SysResource> menus)
    {
        List<NavItem> items = new();
        foreach (var menu in menus)
        {
            var item = menu.Parse();
            if (menu.Children?.Count > 0)
            {
                item.Children = menu.Children.Parse();
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

    private static NavItem Parse(this SysResource menu) => new()
    {
        Title = menu.Title,
        Icon = menu.Icon,
        Href = menu.Component,
        Target = menu.TargetType == TargetTypeEnum.SELF ? "_self" : "_blank",
    };

}