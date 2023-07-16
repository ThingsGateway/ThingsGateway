#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Web.Rcl.Core
{
    public static class MenuExtensions
    {
        public static List<NavItem> Parse(this List<SysResource> menus)
        {
            List<NavItem> items = new List<NavItem>();
            foreach (var menu in menus)
            {
                var item = menu.Parse();
                if (menu.Children.Count > 0)
                {
                    item.Children = menu.Children.Parse();
                }
                if (menu.Category == MenuCategoryEnum.MENU || menu.Category == MenuCategoryEnum.SPA)
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

        public static NavItem Parse(this SysResource menu) => new()
        {
            Title = menu.Title,
            Icon = menu.Icon,
            Href = menu.Component,
            Target = menu.TargetType == TargetTypeEnum.SELF ? "_self" : "_blank",
        };

        public static List<PageTabItem> SameLevelMenuPasePageTab(this List<SysResource> nav)
        {
            List<PageTabItem> pageTabItems = new List<PageTabItem>();
            if (nav == null) return pageTabItems;
            foreach (var item in nav)
            {
                if ((item.Category == MenuCategoryEnum.MENU || item.Category == MenuCategoryEnum.SPA) && item.TargetType == TargetTypeEnum.SELF)
                {
                    if (item.Icon == null)
                        pageTabItems.Add(new PageTabItem(item.Title, item.Component, ""));
                    else
                        pageTabItems.Add(new PageTabItem(item.Title, item.Component, item.Icon));
                }
            }
            return pageTabItems;
        }
    }
}