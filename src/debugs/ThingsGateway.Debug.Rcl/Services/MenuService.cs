
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using BootstrapBlazor.Components;

using ThingsGateway.Razor;

using TouchSocket.Core;

namespace ThingsGateway.Admin.Razor;

/// <inheritdoc/>
public class MenuConfigs : AppConfigBase
{
    public static MenuConfigs Default;

    static MenuConfigs()
    {
        Default = AppConfigBase.GetNewDefault<MenuConfigs>();
    }

    public List<MenuItem> MenuItems { get; set; } = new();

    public MenuConfigs() : base(AppContext.BaseDirectory.CombinePath("menu.config.json"))
    {
    }
}

public class MenuService : IMenuService
{
    public IEnumerable<MenuItem>? MenuItems => MenuConfigs.Default.MenuItems;

    public IEnumerable<MenuItem>? SameLevelMenuItems => GetSameLevelMenuItems(MenuConfigs.Default.MenuItems).Where(a => !a.Url.IsNullOrWhiteSpace());

    private static IEnumerable<MenuItem> GetSameLevelMenuItems(IEnumerable<MenuItem> items) => items.Concat(items.SelectMany(i => i.Items.Any() ? GetSameLevelMenuItems(i.Items) : i.Items));
}