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

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// Layout
/// </summary>
public partial class MainLayout
{
    private static readonly string[] selfPatterns =
    {
    };
    bool Changed { get; set; }
    private bool? _drawerOpen = true;

    private PageTabs _pageTabs;

    private string CONFIG_COPYRIGHT = "";

    private string CONFIG_COPYRIGHT_URL = "";

    private string CONFIG_TITLE = "";
    /// <summary>
    /// IsMobile
    /// </summary>
    [CascadingParameter(Name = "IsMobile")]
    public bool IsMobile { get; set; }



    private List<NavItem> Navs { get; set; } = new();

    [Inject]
    private UserResoures UserResoures { get; set; }

    /// <summary>
    /// 页面刷新
    /// </summary>
    /// <returns></returns>
    public async Task StateHasChangedAsync()
    {
        CONFIG_COPYRIGHT = (await App.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_COPYRIGHT)).ConfigValue;
        CONFIG_TITLE = (await App.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_TITLE)).ConfigValue;
        CONFIG_COPYRIGHT_URL = (await App.GetService<IConfigService>().GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_COPYRIGHT_URL)).ConfigValue;
        await UserResoures.InitUserAsync();
        await UserResoures.InitMenuAsync();
        Navs = UserResoures.Menus.Parse();
        Changed = !Changed;
        await InvokeAsync(StateHasChanged);
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        await StateHasChangedAsync();
        await base.OnInitializedAsync();
    }

}