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

using BlazorComponent;

using Furion;

using Masa.Blazor;

namespace ThingsGateway.Gateway.Blazor;

/// <inheritdoc/>
public partial class ConfigPage
{
    private List<SysConfig> _alarmConfig = new();
    private List<SysConfig> _hisConfig = new();
    StringNumber tab;

    /// <inheritdoc/>
    public ConfigPage()
    {
        AlarmHostService = BackgroundServiceUtil.GetBackgroundService<AlarmWorker>();
        HistoryValueHostService = BackgroundServiceUtil.GetBackgroundService<HistoryValueWorker>();
    }

    AlarmWorker AlarmHostService { get; set; }

    HistoryValueWorker HistoryValueHostService { get; set; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        _alarmConfig = await App.GetService<IConfigService>().GetListByCategoryAsync(ThingsGatewayConfigConst.ThingGateway_AlarmConfig_Base);
        _hisConfig = await App.GetService<IConfigService>().GetListByCategoryAsync(ThingsGatewayConfigConst.ThingGateway_HisConfig_Base);
        await base.OnParametersSetAsync();
    }

    private async Task OnAlarmSaveAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("确认", "保存配置后将重启报警服务，是否确定?");
        if (confirm)
        {
            await App.GetService<ConfigService>().EditBatchAsync(_alarmConfig);
            await AlarmHostService.RestartAsync();
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
        }
    }

    private async Task OnHisSaveAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("确认", "保存配置后将重启历史服务，是否确定?");
        if (confirm)
        {
            await App.GetService<ConfigService>().EditBatchAsync(_hisConfig);
            await HistoryValueHostService.RestartAsync();
            await PopupService.EnqueueSnackbarAsync("成功", AlertTypes.Success);
        }
    }

}