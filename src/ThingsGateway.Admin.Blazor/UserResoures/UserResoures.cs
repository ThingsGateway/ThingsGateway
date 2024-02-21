//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 当前用户资源
/// </summary>
public class UserResoures
{
    /// <summary>
    /// 当前全部菜单
    /// </summary>
    public List<SysResource> AllSameLevelMenuSpas { get; private set; } = new();

    /// <summary>
    /// 当前用户
    /// </summary>
    public SysUser CurrentUser { get; private set; }

    /// <summary>
    /// 当前用户菜单构建树
    /// </summary>
    public List<SysResource> UserMenus { get; private set; }

    /// <summary>
    /// 当前用户Tab列表
    /// </summary>
    public List<PageTabItem> UserPageTabItems { get; private set; } = new();

    /// <summary>
    /// 当前用户NavItem列表
    /// </summary>
    public List<NavItem> UserNavItems { get; private set; } = new();

    /// <summary>
    /// 当前用户菜单列表
    /// </summary>
    public List<SysResource> UserSameLevelMenus { get; private set; } = new();

    /// <summary>
    /// 当前用户工作台菜单
    /// </summary>
    public List<SysResource> UserWorkbenchOutputs { get; private set; }

    /// <summary>
    /// 当前用户工作台Id
    /// </summary>
    public RelationUserWorkBench RelationUserWorkBench { get; private set; }

    private IServiceScopeFactory _serviceScopeFactory { get; set; }

    public UserResoures(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// 初始化获取全部资源
    /// </summary>
    /// <returns></returns>
    public async Task InitAllAsync()
    {
        await InitUserAsync();
        await InitMenuAsync();
    }

    /// <summary>
    /// 初始化获取当前菜单资源
    /// </summary>
    /// <returns></returns>
    public async Task InitMenuAsync()
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();
        AllSameLevelMenuSpas = await serviceScope.ServiceProvider.GetService<IResourceService>().GetMenuAndSpaListAsync();
        if (UserManager.UserId > 0)
        {
            RelationUserWorkBench = await serviceScope.ServiceProvider.GetService<IUserCenterService>().GetLoginWorkbenchAsync(UserManager.UserId);

            UserWorkbenchOutputs = AllSameLevelMenuSpas?
                .Where(it => RelationUserWorkBench.Shortcut.Contains(it.Id))?.ToList();

            var data = await serviceScope.ServiceProvider.GetService<IUserCenterService>().GetOwnMenuAsync(UserManager.UserId);

            UserMenus = data.menuTree;

            UserSameLevelMenus = data.menu;
            UserNavItems = UserMenus.ParseNavItem();
            UserPageTabItems = UserNavItems.ParsePageTabItem();
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
            using var serviceScope = _serviceScopeFactory.CreateScope();
            CurrentUser = await serviceScope.ServiceProvider.GetService<ISysUserService>().GetUserByIdAsync(UserManager.UserId);
        }
    }

    /// <summary>
    /// 是否拥有按钮授权
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public bool IsHasButtonWithRole(string code)
    {
        if (UserManager.UserId > 0)
        {
            if (UserManager.SuperAdmin)
                return true;
            return CurrentUser?.ButtonCodeList?.Contains(code) == true;
        }
        else
        {
            return false;
        }
    }
}