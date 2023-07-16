﻿#region copyright
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

using BlazorComponent.I18n;

using Masa.Blazor;

using NewLife.Serialization;

namespace ThingsGateway.Web.Rcl.Core
{
    public record PageTabItem(string Title, string Href, string Icon);
    /// <summary>
    /// 用户菜单等资源管理
    /// </summary>
    public class UserResoures : IDisposable
    {
        public APPThemes Themes = new();
        private IAuthService _authService;
        private CookieStorage _cookieStorage;
        private IUserCenterService _userCenterService;

        public UserResoures(CookieStorage cookieStorage, MasaBlazor masaBlazor, IHttpContextAccessor httpContextAccessor,
        IAuthService authService,
            ResourceService resourceService,
            IUserCenterService userCenterService)
        {
            _authService = authService;
            _userCenterService = userCenterService;
            _masaBlazor = masaBlazor;
            _cookieStorage = cookieStorage;
            _resourceService = resourceService;
            if (httpContextAccessor.HttpContext is not null) InitCookie(httpContextAccessor.HttpContext.Request.Cookies);
        }

        public SysUser CurrentUser { get; set; }
        public bool IsDark => _masaBlazor.Theme.Dark;

        public List<SysResource> Menus { get; set; }
        public List<SysResource> WorkbenchOutputs { get; set; }
        public List<SysResource> SameLevelMenus { get; private set; } = new();
        public List<SysResource> AllSameLevelMenus { get; private set; } = new();
        public List<PageTabItem> PageTabItems = new();

        private MasaBlazor _masaBlazor { get; set; }
        private IResourceService _resourceService { get; set; }
        public void Dispose()
        {

        }
        public async Task InitAllAsync()
        {
            await InitUserAsync();
            await InitMenuAsync();
        }
        public async Task InitUserAsync()
        {
            CurrentUser = await _authService.GetLoginUser();
        }
        public async Task InitMenuAsync()
        {
            var ids = await _userCenterService.GetLoginWorkbench();
            AllSameLevelMenus = (await _resourceService.GetListAsync(new List<MenuCategoryEnum>() { MenuCategoryEnum.MENU, MenuCategoryEnum.SPA }));
            WorkbenchOutputs = AllSameLevelMenus?
                .Where(it => ids.Contains(it.Id))?.ToList();
            Menus = await _userCenterService.GetOwnMenu();
            SameLevelMenus = Menus.TreeToList();
            PageTabItems = AllSameLevelMenus.SameLevelMenuPasePageTab();
        }

        public bool IsHasButtonWithRole(string code)
        {
            if (UserManager.SuperAdmin) return true;
            return CurrentUser.ButtonCodeList.Contains(code.ToLower());
        }
        public bool IsHasPageWithRole(string code)
        {
            if (UserManager.SuperAdmin) return true;
            return AllSameLevelMenus.Select(a => a.Component).Contains(code.ToLower());
        }
        //设置深浅主题统一由这个方法为入口
        public void SetDarkOrLightTheme()
        {
            Themes.IsDark = !Themes.IsDark;
            SetMasaTheme();
        }

        private void InitCookie(IRequestCookieCollection cookies)
        {
            APPThemes theme = null;
            try
            {
                theme = cookies[BlazorConst.ThemeCookieKey].ToJsonEntity<APPThemes>();
            }
            catch
            {
            }
            //设置主题
            if (theme != null)
            {
                Themes = theme;
            }
            else
            {
                //设置默认主题
                //Themes.DefaultThemes();
            }
            SetMasaTheme();
        }

        private void SetMasaTheme()
        {
            var oldTheme = _masaBlazor.Theme;
            //Themes.Dark?.SetMasaThemesOp(oldTheme.Themes.Dark);
            //Themes.Light?.SetMasaThemesOp(oldTheme.Themes.Light);
            //oldTheme.Dark = Themes.IsDark;
            if (_masaBlazor.Theme.Dark != Themes.IsDark)
                _masaBlazor.ToggleTheme();
            //_masaBlazor.Theme = oldTheme;
            _cookieStorage?.SetItemAsync(BlazorConst.ThemeCookieKey, Themes.ToJson());
        }
    }
}