#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Admin.Blazor;

public partial class MainLayout
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
    /// <summary>
    /// ҳ��ˢ��
    /// </summary>
    /// <returns></returns>
    public async Task StateHasChangedAsync()
    {
        _configCopyRight = (await _serviceScope.ServiceProvider.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_COPYRIGHT)).ConfigValue;
        _configTitle = (await _serviceScope.ServiceProvider.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_TITLE)).ConfigValue;
        _configCopyRightUrl = (await _serviceScope.ServiceProvider.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_COPYRIGHT_URL)).ConfigValue;
        _configPageTab = (await _serviceScope.ServiceProvider.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_PAGETAB)).ConfigValue.ToBool(true);

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