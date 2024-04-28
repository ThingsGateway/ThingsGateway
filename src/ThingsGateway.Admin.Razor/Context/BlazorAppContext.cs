
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using NewLife.Extension;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

/// <summary>
/// Context
/// </summary>
public class BlazorAppContext : IAsyncDisposable
{
    public BlazorAppContext(
        ISysResourceService sysResourceService
        , IUserCenterService userCenterService,
        ISysUserService sysUserService
        )
    {
        SysUserService = sysUserService;
        UserCenterService = userCenterService;
        ResourceService = sysResourceService;
    }

    /// <summary>
    /// 个人工作台
    /// </summary>
    public WorkbenchInfo UserWorkBench { get; private set; }

    /// <summary>
    /// 用户个人快捷方式菜单
    /// </summary>
    public IEnumerable<SysResource> UserWorkbenchOutputs { get; private set; }

    /// <summary>
    /// 全部菜单
    /// </summary>
    public IEnumerable<SysResource> AllMenus { get; private set; }

    /// <summary>
    /// 用户个人菜单
    /// </summary>
    public IEnumerable<SysResource> OwnMenus { get; private set; }

    /// <summary>
    /// 用户个人菜单
    /// </summary>
    public IEnumerable<MenuItem> OwnMenuItems { get; private set; }

    /// <summary>
    /// 用户个人菜单，非树形
    /// </summary>
    public IEnumerable<MenuItem> OwnSameLevelMenuItems { get; private set; }

    /// <summary>
    /// 当前用户
    /// </summary>
    public SysUser CurrentUser { get; private set; }

    private ISysUserService SysUserService { get; }
    private IUserCenterService UserCenterService { get; }
    private ISysResourceService ResourceService { get; }

    /// <summary>
    /// 初始化获取当前用户
    /// </summary>
    /// <returns></returns>
    public async Task InitUserAsync()
    {
        if (UserManager.UserId > 0)
        {
            CurrentUser = (await SysUserService.GetUserByIdAsync(UserManager.UserId))!;
        }
    }

    /// <summary>
    /// 获取当前的个人菜单，传入当前url，根据url判断模块，失败时使用默认模块
    /// </summary>
    /// <returns></returns>
    public async Task InitMenus(string url)
    {
        if (UserManager.UserId > 0)
        {
            url = url.StartsWith("/") ? url : $"/{url}";
            var sysResources = await ResourceService.GetAllAsync();
            AllMenus = sysResources.Where(a => a.Category == ResourceCategoryEnum.Menu);
            var module = AllMenus.FirstOrDefault(a => a.Href == url)?.Module;
            if (module == ResourceConst.SpaId)
                module = null;//SPA页面取消url传入的模块
            UserWorkBench = await UserCenterService.GetLoginWorkbenchAsync(UserManager.UserId);
            OwnMenus = await UserCenterService.GetOwnMenuAsync(UserManager.UserId, module ?? CurrentUser.DefaultModule);
            OwnMenuItems = ResourceUtil.BuildMenuTrees(OwnMenus).ToList();
            OwnSameLevelMenuItems = OwnMenus.Where(a => !a.Href.IsNullOrWhiteSpace()).Select(item => new MenuItem()
            {
                Match = item.NavLinkMatch ?? Microsoft.AspNetCore.Components.Routing.NavLinkMatch.All,
                Text = item.Title,
                Icon = item.Icon,
                Url = item.Href,
                Target = item.Target.ToString(),
            });
            UserWorkbenchOutputs = AllMenus.Where(it => UserWorkBench.Shortcuts.Contains(it.Id));
        }
    }

    /// <summary>
    /// 是否拥有按钮授权
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public bool IsHasButtonWithRole(string url, string code)
    {
        if (UserManager.SuperAdmin)
            return true;
        return CurrentUser?.ButtonCodeList?.TryGetValue(url.StartsWith("/") ? url : $"/{url}", out var titles) == true && titles.Contains(code);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}