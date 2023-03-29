using BlazorComponent.I18n;

using Masa.Blazor;
 

namespace ThingsGateway.Web.Rcl.Core
{
    /// <summary>
    /// 标签页表示
    /// </summary>
    /// <param name="Title">标题</param>
    /// <param name="Href">Path</param>
    /// <param name="Icon">图标</param>
    public record PageTabItem(string Title, string Href, string Icon);
    /// <summary>
    /// 自定义资源管理
    /// </summary>
    public class UserResoures : IDisposable
    {
        /// <summary>
        /// 主题
        /// </summary>
        public APPThemes Themes = new();
        private IAuthService _authService;
        private CookieStorage _cookieStorage;
        private IUserCenterService _userCenterService;
        /// <inheritdoc cref="UserResoures"/>>
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
        /// <summary>
        /// 当前用户
        /// </summary>
        public SysUser CurrentUser { get; set; }
        /// <summary>
        /// 是否黑暗主题
        /// </summary>
        public bool IsDark => _masaBlazor.Theme.Dark;

        /// <summary>
        /// 全部个人菜单
        /// </summary>
        public List<SysResource> Menus { get; set; }
        /// <summary>
        /// 全部快捷方式
        /// </summary>
        public List<SysResource> WorkbenchOutputs { get; set; }
        /// <summary>
        /// 相同等级的个人菜单
        /// </summary>
        public List<SysResource> SameLevelMenus { get; private set; } = new();
        /// <summary>
        /// 全部相同等级的菜单
        /// </summary>
        public List<SysResource> AllSameLevelMenus { get; private set; } = new();
        /// <summary>
        /// 全部标签页
        /// </summary>
        public List<PageTabItem> PageTabItems = new();

        private MasaBlazor _masaBlazor { get; set; }
        private IResourceService _resourceService { get; set; }
        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// 初始化全部内容
        /// </summary>
        /// <returns></returns>
        public async Task InitAllAsync()
        {
            await InitUserAsync();
            await InitMenuAsync();
        }
        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <returns></returns>
        public async Task InitUserAsync()
        {
            CurrentUser = await _authService.GetLoginUser();
        }
        /// <summary>
        /// 初始化菜单信息
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 按钮授权检查
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsHasButtonWithRole(string code)
        {
            if (UserManager.SuperAdmin) return true;
            return CurrentUser.ButtonCodeList.Contains(code.ToLower());
        }
        /// <summary>
        /// 页面授权检查
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool IsHasPageWithRole(string code)
        {
            if (UserManager.SuperAdmin) return true;
            return AllSameLevelMenus.Select(a => a.Component).Contains(code.ToLower());
        }
        /// <summary>
        /// 设置深浅主题统一由这个方法为入口
        /// </summary>
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
                //Themes.DefalutThemes();
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