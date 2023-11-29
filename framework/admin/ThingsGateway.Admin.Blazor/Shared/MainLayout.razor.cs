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

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Admin.Blazor;

public partial class MainLayout : IDisposable
{
    private List<SysResource> _breadcrumbSysResources = new();
    private string _configCopyRight = "";
    private bool _configPageTab;
    private string _configCopyRightUrl = "";
    private string _configTitle = "";
    private bool? _drawerOpen = true;
    private PageTabs _pageTabs;
    private List<SysResource> _searchSysResources = new();
    protected IServiceScope _serviceScope { get; set; }
    private bool _changed { get; set; }

    /// <summary>
    /// IsMobile
    /// </summary>
    [CascadingParameter(Name = "IsMobile")]
    private bool _isMobile { get; set; }

    private List<NavItem> _navItems { get; set; } = new();

    private List<PageTabItem> _pageTabItems { get; set; } = new();

    [Inject]
    private IServiceScopeFactory _serviceScopeFactory { get; set; }
    [Inject]
    private UserResoures _userResoures { get; set; }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }

    /// <summary>
    /// 页面刷新
    /// </summary>
    /// <returns></returns>
    public async Task StateHasChangedAsync()
    {
        var configService = _serviceScope.ServiceProvider.GetService<IConfigService>();
        _configCopyRight = (await configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_COPYRIGHT)).ConfigValue;
        _configTitle = (await configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_TITLE)).ConfigValue;
        _configCopyRightUrl = (await configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_COPYRIGHT_URL)).ConfigValue;
        _configPageTab = (await configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_PAGETAB)).ConfigValue.ToBool(true);

        await _userResoures.InitUserAsync();
        await _userResoures.InitMenuAsync();
        _navItems = _userResoures.Menus.ParseNavItem();
        _pageTabItems = _userResoures.PageTabItems;
        _searchSysResources = _userResoures.SameLevelMenus;
        _breadcrumbSysResources = _userResoures.AllSameLevelMenuSpas;
        _changed = !_changed;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        _serviceScope = _serviceScopeFactory.CreateScope();
        await StateHasChangedAsync();
        await base.OnInitializedAsync();
    }
}