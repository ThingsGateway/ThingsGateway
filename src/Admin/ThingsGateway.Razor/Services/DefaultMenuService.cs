//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Razor;

public class DefaultMenuService : IMenuService
{
    private MenuOptions MenuOptions;
    public DefaultMenuService(IOptions<MenuOptions> options)
    {
        MenuOptions = options?.Value ?? new();
    }
    public IEnumerable<MenuItem>? MenuItems => MenuOptions.MenuItems;
    public IEnumerable<MenuItem>? AllOwnMenuItems => MenuOptions.MenuItems;

    public IEnumerable<MenuItem>? SameLevelMenuItems => GetSameLevelMenuItems(MenuOptions.MenuItems).Where(a => !a.Url.IsNullOrWhiteSpace());

    private static IEnumerable<MenuItem> GetSameLevelMenuItems(IEnumerable<MenuItem> items) => items.Concat(items.SelectMany(i => i.Items.Any() ? GetSameLevelMenuItems(i.Items) : i.Items));
}
