//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait
using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;

namespace ThingsGateway.Razor;

public partial class MainLayout : IDisposable
{
    #region 全局通知

    [Inject]
    [NotNull]
    private IDispatchService<MessageItem>? DispatchService { get; set; }

    private async Task Dispatch(DispatchEntry<MessageItem> entry)
    {
        if (entry.Entry != null)
        {
            await InvokeAsync(async () =>
            {
                await ToastService.Show(new ToastOption()
                {
                    Title = $"{entry.Entry.Title} ",
                    Content = $"{entry.Entry.Message}",
                    Category = entry.Entry.Category,
                    Delay = 10 * 1000,
                    ForceDelay = true
                });
            });
        }
    }

    #endregion 全局通知
    [Inject]
    private IStringLocalizer<ThingsGateway.Admin.Razor._Imports> AdminLocalizer { get; set; }

    #region 切换模块

    [Inject]
    private ISysResourceService SysResourceService { get; set; }

    [Inject]
    private IUserCenterService UserCenterService { get; set; }

    private async Task ChoiceModule()
    {
        DialogOption? op = null;

        op = new DialogOption()
        {
            IsScrolling = false,
            Size = Size.ExtraLarge,
            ShowFooter = false,
            Title = Localizer["ChoiceModule"],
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<ChoiceModuleComponent>(new Dictionary<string, object?>
            {
                [nameof(ChoiceModuleComponent.ModuleList)] = AppContext.CurrentUser.ModuleList.ToList(),
                [nameof(ChoiceModuleComponent.Value)] = AppContext.CurrentModuleId,
                [nameof(ChoiceModuleComponent.OnSave)] = async (long v, bool s) =>
                {
                    if (op != null)
                    {
                        await op.CloseDialogAsync();
                    }
                    if (s)
                    {
                        await UserCenterService.SetDefaultModule(v);
                        NavigationManager.NavigateTo("/", true);
                    }
                    else
                    {
                        var filteredByA = AppContext.AllMenus.Where(p => p.Module != v);
                        var filteredByB = AppContext.AllMenus.Where(p => p.Module == v);
                        var uniqueAValues = new HashSet<string>();
                        string finalResult = null;
                        foreach (var pair in filteredByA)
                        {
                            uniqueAValues.Add(pair.Href);
                        }
                        foreach (var pair in filteredByB)
                        {
                            if (uniqueAValues.Add(pair.Href)) // 如果添加成功，说明a值没有重复
                            {
                                finalResult = pair.Href;
                                break;
                            }
                        }
                        NavigationManager.NavigateTo(finalResult ?? "/", true);
                    }
                }
            }).Render(),
        };
        await DialogService.Show(op);
    }

    #endregion 切换模块

    #region 个人信息修改

    private Task OnUserInfoDialog()
    {
        return DialogService.Show(new DialogOption()
        {
            IsScrolling = false,
            Title = Localizer["UserCenter"],
            ShowFooter = false,
            Component = BootstrapDynamicComponent.CreateComponent<UserCenterPage>(new Dictionary<string, object?>()
            {
            })
        });
    }

    #endregion 个人信息修改

    #region 注销

    [Inject]
    private AjaxService AjaxService { get; set; }
    [Inject]
    private IAppService AppService { get; set; }
    [Inject]
    [NotNull]
    private IAuthRazorService? AuthRazorService { get; set; }

    private async Task LogoutAsync()
    {

        try
        {

            var ret = await AuthRazorService.LoginOutAsync();
            if (ret.Code != 200)
            {
                await ToastService.Error(Localizer["LoginErrorh1"], $"{ret.Msg}");
            }
            else
            {
                await ToastService.Information(Localizer["LoginSuccessh1"], Localizer["LoginSuccessc1"]);
                await Task.Delay(1000);
                var url = AppService.GetReturnUrl(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
                await AjaxService.Goto(url);
            }
        }
        catch
        {
            await ToastService.Error(Localizer["LoginErrorh2"], Localizer["LoginErrorc2"]);
        }
    }

    #endregion 注销

    private string _versionString = string.Empty;

    [Inject]
    [NotNull]
    private BlazorAppContext? AppContext { get; set; }

    [Inject]
    private DialogService DialogService { get; set; }

    [Inject]
    [NotNull]
    private FullScreenService FullScreenService { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<MainLayout>? Localizer { get; set; }

    [Inject]
    [NotNull]
    private IMenuService? MenuService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    [Inject]
    [NotNull]
    private IAppVersionService? VersionService { get; set; }

    [Inject]
    [NotNull]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        _versionString = $"v{VersionService.Version}";
        DispatchService.Subscribe(Dispatch);
        await AppContext.InitUserAsync();
        await AppContext.InitMenus(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));

        StateHasChanged();
        await base.OnInitializedAsync();
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            DispatchService.UnSubscribe(Dispatch);
        }
    }

    private async Task ReloadMenu()
    {
        await AppContext.InitMenus(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
        await InvokeAsync(StateHasChanged);
    }

    private async Task ReloadUser()
    {
        await AppContext.InitUserAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task ShowAbout()
    {
        DialogOption? op = null;

        op = new DialogOption()
        {
            IsScrolling = false,
            Size = Size.Medium,
            ShowFooter = false,
            Title = Localizer["About"],
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<About>().Render(),
        };
        await DialogService.Show(op);
    }
}
