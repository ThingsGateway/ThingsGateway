//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
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
    [Inject]
    [NotNull]
    protected BlazorAppContext? AppContext { get; set; }

    private string? HeaderText { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<QuickActions>? Localizer { get; set; }

    [Inject]
    private IPluginService PluginService { get; set; }

    private string? ReloadPluginConfirmText { get; set; }
    private string? RestartText { get; set; }
    private string? ReloadServiceConfirmText { get; set; }
    private string? ReloadServiceText { get; set; }
    private string? TooltipText { get; set; }

    protected bool AuthorizeButton(string operate)
    {
        return AppContext.IsHasButtonWithRole("/gateway/devicestatus", operate);
    }

    protected override void OnInitialized()
    {
        TooltipText ??= Localizer[nameof(TooltipText)];
        HeaderText ??= Localizer[nameof(HeaderText)];

        RestartText ??= Localizer[nameof(RestartText)];
        ReloadServiceText ??= Localizer[nameof(ReloadServiceText)];
        ReloadPluginConfirmText ??= Localizer[nameof(ReloadPluginConfirmText)];
        ReloadServiceConfirmText ??= Localizer[nameof(ReloadServiceConfirmText)];

        AutoRestartThreadBoolItems = LocalizerUtil.GetBoolItems(GetType(), nameof(AutoRestartThread));
        base.OnInitialized();
    }

    #region 配置


    [Parameter]
    public bool AutoRestartThread { get; set; } = true;
    [Parameter]
    public EventCallback<bool> AutoRestartThreadChanged { get; set; }

    private async Task OnAutoRestartThreadChanged(bool autoRestartThread)
    {
        AutoRestartThread = autoRestartThread;
        if (Module != null)
            await Module!.InvokeVoidAsync("saveAutoRestartThread", autoRestartThread);
        if (AutoRestartThreadChanged.HasDelegate)
            await AutoRestartThreadChanged.InvokeAsync(autoRestartThread);
    }

    private List<SelectedItem> AutoRestartThreadBoolItems;


    private static async Task Restart()
    {
        await Task.Run(async () =>
        {
            var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
            await GlobalData.ChannelThreadManage.RestartChannelAsync(data.Select(a => a.Value));
        });
    }

    protected override async Task InvokeInitAsync()
    {
        await base.InvokeInitAsync();
        var autoRestartThread = await Module!.InvokeAsync<bool>("getAutoRestartThread");
        await OnAutoRestartThreadChanged(autoRestartThread);
    }
    #endregion
    private async Task ToggleOpen()
    {
        await Module!.InvokeVoidAsync("toggle", Id);
    }
}
