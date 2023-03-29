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