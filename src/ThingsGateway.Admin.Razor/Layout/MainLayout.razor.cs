
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using ThingsGateway.Admin.Application;
using ThingsGateway.Core;

namespace ThingsGateway.Admin.Razor;

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
            await ToastService.Show(new ToastOption()
            {
                Title = $"{entry.Entry.Title} ",
                Content = $"{entry.Entry.Message}",
                Category = ToastCategory.Information,
                Delay = 10 * 1000,
                ForceDelay = true
            });
        }
    }

    #endregion 全局通知

    #region Gitee通知

    [Inject]
    [NotNull]
    private IDispatchService<GiteePostBody>? CommitDispatchService { get; set; }

    [NotNull]
    private string? GiteePostTitleText { get; set; }

    private async Task NotifyCommit(DispatchEntry<GiteePostBody> payload)
    {
        if (payload.CanDispatch())
        {
            var option = new ToastOption()
            {
                Category = ToastCategory.Information,
                Title = GiteePostTitleText,
                Delay = 10 * 1000,
                ForceDelay = true,
                ChildContent = BootstrapDynamicComponent.CreateComponent<CommitItem>(new Dictionary<string, object?>
                {
                    [nameof(CommitItem.Item)] = payload.Entry
                }).Render()
            };
            await ToastService.Show(option);
        }
    }

    #endregion Gitee通知

    #region 切换模块

    [Inject]
    private IUserCenterService UserCenterService { get; set; }

    [Inject]
    private ISysResourceService SysResourceService { get; set; }

    private async Task ChoiceModule()
    {
        DialogOption? op = null;

        op = new DialogOption()
        {
            IsScrolling = true,
            Size = Size.ExtraLarge,
            ShowFooter = false,
            Title = Localizer["ChoiceModule"],
            BodyTemplate = BootstrapDynamicComponent.CreateComponent<ChoiceModuleComponent>(new Dictionary<string, object?>
            {
                [nameof(ChoiceModuleComponent.ModuleList)] = AppContext.CurrentUser.ModuleList.ToList(),
                [nameof(ChoiceModuleComponent.Value)] = AppContext.CurrentUser.DefaultModule,
                [nameof(ChoiceModuleComponent.OnSave)] = new Func<long, Task>(async v =>
                {
                    if (op != null)
                    {
                        await op.CloseDialogAsync();
                    }
                    await UserCenterService.SetDefaultModule(v);
                    NavigationManager.NavigateTo("/", true);
                })
            }).Render(),
        };
        await DialogService.Show(op);
    }

    #endregion 切换模块

    #region 个人信息修改

    private Task OnUserInfoDialog() => DialogService.Show(new DialogOption()
    {
        Title = Localizer["UserCenter"],
        ShowFooter = false,
        Component = BootstrapDynamicComponent.CreateComponent<UserCenterPage>(new Dictionary<string, object?>()
        {
        })
    });

    #endregion 个人信息修改

    [Inject]
    private DialogService DialogService { get; set; }

    [Inject]
    [NotNull]
    private FullScreenService FullScreenService { get; set; }

    [Inject]
    [NotNull]
    private ToastService? ToastService { get; set; }

    [Inject]
    [NotNull]
    private IAppVersionService? VersionService { get; set; }

    [Inject]
    [NotNull]
    private IOptions<WebsiteOptions>? WebsiteOption { get; set; }

    [Inject]
    [NotNull]
    private BlazorAppContext? AppContext { get; set; }

    [Inject]
    [NotNull]
    private IMenuService? MenuService { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<MainLayout>? Localizer { get; set; }

    private string _versionString = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        GiteePostTitleText = Localizer[nameof(GiteePostTitleText)];
        _versionString = $"v{VersionService.Version}";
        DispatchService.Subscribe(Dispatch);
        CommitDispatchService.Subscribe(NotifyCommit);
        await AppContext.InitUserAsync();
        await AppContext.InitMenus(NavigationManager.ToBaseRelativePath(NavigationManager.Uri));
        await base.OnInitializedAsync();
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

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            DispatchService.UnSubscribe(Dispatch);
            CommitDispatchService.UnSubscribe(NotifyCommit);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}