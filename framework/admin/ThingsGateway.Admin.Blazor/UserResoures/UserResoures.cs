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

using Masa.Blazor;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 当前用户资源
/// </summary>
public class UserResoures
{
    private readonly CookieStorage _cookieStorage;
    private IRequestCookieCollection _cookies;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="cookieStorage"></param>
    /// <param name="masaBlazor"></param>
    /// <param name="httpContextAccessor"></param>
    public UserResoures(CookieStorage cookieStorage, MasaBlazor masaBlazor, IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor httpContextAccessor)
    {
        _masaBlazor = masaBlazor;
        _cookieStorage = cookieStorage;
        _serviceScope = serviceScopeFactory.CreateScope();
        if (httpContextAccessor.HttpContext is not null)
            _cookies = httpContextAccessor.HttpContext.Request.Cookies;
    }

    /// <summary>
    /// 当前的菜单与单页列表
    /// </summary>
    public List<SysResource> AllSameLevelMenuSpas { get; private set; } = new();

    /// <summary>
    /// 当前用户
    /// </summary>
    public SysUser CurrentUser { get; private set; }

    /// <summary>
    /// 是否深色主图
    /// </summary>
    public bool IsDark => _masaBlazor.Theme.Dark;

    /// <summary>
    /// 当前菜单
    /// </summary>
    public List<SysResource> Menus { get; private set; }

    /// <summary>
    /// 当前的Tab列表
    /// </summary>
    public List<PageTabItem> PageTabItems { get; private set; } = new();

    /// <summary>
    /// 当前的菜单列表
    /// </summary>
    public List<SysResource> SameLevelMenus { get; private set; } = new();

    /// <summary>
    /// 当前工作台
    /// </summary>
    public List<SysResource> WorkbenchOutputs { get; private set; }

    private MasaBlazor _masaBlazor { get; set; }
    private IServiceScope _serviceScope { get; set; }
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
        if (UserManager.UserId > 0)
        {
            var ids = await _serviceScope.ServiceProvider.GetService<IUserCenterService>().GetLoginWorkbenchAsync();
            AllSameLevelMenuSpas = (await _serviceScope.ServiceProvider.GetService<IResourceService>().GetListByCategorysAsync(new List<ResourceCategoryEnum>() { ResourceCategoryEnum.MENU, ResourceCategoryEnum.SPA }));
            WorkbenchOutputs = AllSameLevelMenuSpas?
                .Where(it => ids.Contains(it.Id))?.ToList();
            Menus = await _serviceScope.ServiceProvider.GetService<IUserCenterService>().GetOwnMenuAsync();
            SameLevelMenus = _serviceScope.ServiceProvider.GetService<IResourceService>().ResourceTreeToList(Menus);
            PageTabItems = AllSameLevelMenuSpas.PasePageTabItem();
        }
    }

    /// <summary>
    /// 初始化获取当前用户
    /// </summary>
    /// <returns></returns>
    public async Task InitUserAsync()
    {
        if (UserManager.UserId > 0)
            CurrentUser = await _serviceScope.ServiceProvider.GetService<AuthService>().GetLoginUserAsync();
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
            if (UserManager.IsSuperAdmin)
                return true;
            return CurrentUser?.ButtonCodeList?.Contains(code.ToLower()) == true;
        }
        else
        {
            return true;
        }
    }
    /// <summary>
    /// 是否拥有页面授权
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public bool IsHasPageWithRole(string code)
    {
        if (UserManager.UserId > 0)
        {
            if (UserManager.IsSuperAdmin) return true;
            return AllSameLevelMenuSpas.Select(a => a.Component).Contains(code.ToLower());
        }
        else
        {
            return true;
        }

    }

    /// <summary>
    /// 设置深浅主题统一由这个方法为入口
    /// </summary>
    public async Task SetMasaThemeAsync(bool? isDark = null)
    {
        if (_masaBlazor.Theme.Dark != isDark || isDark == null)
            _masaBlazor.ToggleTheme();
        await _cookieStorage?.SetItemAsync(BlazorResourceConst.ThemeCookieKey, _masaBlazor.Theme.Dark.ToJsonString());
    }

    public async Task InitCookieAsync()
    {
        try
        {
            var theme = _cookies[BlazorResourceConst.ThemeCookieKey].FromJsonString<bool>();
            //设置主题
            await SetMasaThemeAsync(theme);
        }
        catch
        {
            await SetMasaThemeAsync(false);
        }
    }
}