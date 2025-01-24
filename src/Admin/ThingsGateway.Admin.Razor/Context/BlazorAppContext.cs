//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using ThingsGateway.Admin.Application;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Admin.Razor;

/// <summary>
/// Context
/// </summary>
public class BlazorAppContext
{
    public IStringLocalizer TitleLocalizer;

    public BlazorAppContext(ISysResourceService sysResourceService, IUserCenterService userCenterService, ISysUserService sysUserService)
    {
        SysUserService = sysUserService;
        UserCenterService = userCenterService;
        ResourceService = sysResourceService;
    }

    /// <summary>
    /// 全部菜单
    /// </summary>
    public IEnumerable<SysResource> AllMenus { get; private set; }

    /// <summary>
    /// 当前用户
    /// </summary>
    public SysUser CurrentUser { get; private set; }

    /// <summary>
    /// 用户个人菜单
    /// </summary>
    public IEnumerable<MenuItem> OwnMenuItems { get; private set; }

    /// <summary>
    /// 不同模块的菜单
    /// </summary>
    public IEnumerable<MenuItem> AllOwnMenuItems { get; private set; }

    /// <summary>
    /// 用户个人菜单，多个模块
    /// </summary>
    public IEnumerable<SysResource> OwnMenus { get; private set; }

    /// <summary>
    /// 用户个人菜单，非树形
    /// </summary>
    public IEnumerable<MenuItem> OwnSameLevelMenuItems { get; private set; }

    /// <summary>
    /// 个人工作台
    /// </summary>
    public WorkbenchInfo UserWorkBench { get; private set; }

    /// <summary>
    /// 用户个人快捷方式菜单
    /// </summary>
    public IEnumerable<SysResource> UserWorkbenchOutputs { get; private set; }

    public IEnumerable<SysResource> AllResource { get; private set; }

    private ISysResourceService ResourceService { get; }
    private ISysUserService SysUserService { get; }
    private IUserCenterService UserCenterService { get; }
    public long CurrentModuleId { get; private set; }

    /// <summary>
    /// 获取当前的个人菜单，传入当前url，根据url判断模块，失败时使用默认模块
    /// </summary>
    /// <returns></returns>
    public async Task InitMenus(string url, long? moduleId = null)
    {
        if (UserManager.UserId > 0)
        {
            url = url.StartsWith('/') ? url : $"/{url}";
            var sysResources = (await ResourceService.GetAllAsync()).Adapt<List<SysResource>>();
            if (TitleLocalizer != null)
            {
                sysResources.ForEach(a =>
                {
                    a.Title = TitleLocalizer[a.Title];
                });
            }
            AllResource = sysResources;
            CurrentUser.ModuleList = AllResource.Where(a => CurrentUser.ModuleList.Select(a => a.Id).ToHashSet().Contains(a.Id)).ToList();
            AllMenus = sysResources.Where(a => a.Category == ResourceCategoryEnum.Menu);

            if (moduleId == null)
            {
                moduleId = AllMenus.FirstOrDefault(a => a.Href == url)?.Module;
                if (moduleId != null)
                {
                    CurrentModuleId = CurrentUser.ModuleList.Select(a => a.Id).Contains(moduleId.Value) ? moduleId.Value : CurrentUser.ModuleList.Select(a => a.Id).FirstOrDefault();
                }
                else
                {
                    CurrentModuleId = CurrentUser.ModuleList.Select(a => a.Id).FirstOrDefault();

                }

            }
            else
            {
                CurrentModuleId = moduleId.Value;
            }
            UserWorkBench = await UserCenterService.GetLoginWorkbenchAsync(UserManager.UserId);
            OwnMenus = (await UserCenterService.GetOwnMenuAsync(UserManager.UserId, 0)).Adapt<List<SysResource>>();

            if (TitleLocalizer != null)
            {
                foreach (var a in OwnMenus)
                {
                    a.Title = TitleLocalizer[a.Title];
                }
            }
            var ownMenus = OwnMenus.Where(a => a.Module == CurrentModuleId);
            OwnMenuItems = ResourceUtil.BuildMenuTrees(ownMenus).ToList();
            AllOwnMenuItems = ResourceUtil.BuildMenuTrees(OwnMenus).ToList();
            OwnSameLevelMenuItems = ownMenus.Where(a => !a.Href.IsNullOrWhiteSpace()).Select(item => new MenuItem()
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
    /// 是否拥有按钮授权
    /// </summary>
    /// <param name="url"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public bool IsHasButtonWithRole(string url, string code)
    {
        if (UserManager.SuperAdmin)
            return true;
        url ??= string.Empty;
        if (!url.IsNullOrWhiteSpace())
        {
            var data = CurrentUser?.ButtonCodeList?.TryGetValue(url.StartsWith('/') ? url : $"/{url}", out var titles) == true && titles.Contains(code);
            return data;
        }
        else
        {
            var data = CurrentUser?.ButtonCodeList?.TryGetValue(url, out var titles) == true && titles.Contains(code);
            return data;
        }
    }
}
