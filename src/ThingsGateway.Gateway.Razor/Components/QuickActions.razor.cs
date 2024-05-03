
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------


using ThingsGateway.Admin.Razor;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;

/// <summary>
/// 快捷操作
/// </summary>
public partial class QuickActions
{
    private string? TooltipText { get; set; }
    private string? HeaderText { get; set; }
    private string? ReloadPluginText { get; set; }

    private string? ReloadServiceText { get; set; }
    private string? ReloadPluginConfirmText { get; set; }
    private string? ReloadServiceConfirmText { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<QuickActions>? Localizer { get; set; }

    protected override void OnInitialized()
    {
        TooltipText ??= Localizer[nameof(TooltipText)];
        HeaderText ??= Localizer[nameof(HeaderText)];

        ReloadPluginText ??= Localizer[nameof(ReloadPluginText)];
        ReloadServiceText ??= Localizer[nameof(ReloadServiceText)];
        ReloadPluginConfirmText ??= Localizer[nameof(ReloadPluginConfirmText)];
        ReloadServiceConfirmText ??= Localizer[nameof(ReloadServiceConfirmText)];
        base.OnInitialized();
    }

    private async Task ToggleOpen()
    {
        await Module!.InvokeVoidAsync("toggle", Id);
    }

    [Inject]
    private IPluginService PluginService { get; set; }

    private Task OnReloadPlugin()
    {
        PluginService.Remove();

        return Task.CompletedTask;
    }
    [Inject]
    [NotNull]
    protected BlazorAppContext? AppContext { get; set; }

    protected bool AuthorizeButton(string operate)
    {
        return AppContext.IsHasButtonWithRole("/gateway/devicestatus", operate);
    }
    private async Task OnReloadService()
    {
        try
        {
            await Task.Run(async () =>
            {
                await HostedServiceUtil.CollectDeviceHostedService.RestartAsync();
            });
        }
        finally
        {
        }
    }
}
