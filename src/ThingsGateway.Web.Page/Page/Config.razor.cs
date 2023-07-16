#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Masa.Blazor;

namespace ThingsGateway.Web.Page;

public partial class Config
{
    private List<DevConfig> _alarmConfig = new();
    private List<DevConfig> _hisConfig = new();
    StringNumber tab;

    public Config()
    {
        AlarmHostService = ServiceExtensions.GetBackgroundService<AlarmWorker>();
        HistoryValueHostService = ServiceExtensions.GetBackgroundService<HistoryValueWorker>();
    }

    AlarmWorker AlarmHostService { get; set; }

    HistoryValueWorker HistoryValueHostService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _alarmConfig = await ConfigService.GetListByCategory(ThingsGatewayConst.ThingGateway_AlarmConfig_Base);
        _hisConfig = await ConfigService.GetListByCategory(ThingsGatewayConst.ThingGateway_HisConfig_Base);
        await base.OnInitializedAsync();
    }
    private async Task OnAlarmSave()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync(T("确认"), T("保存配置后将重启报警服务，是否确定?"));
        if (confirm)
        {
            await ConfigService.EditBatch(_alarmConfig);
            AlarmHostService.Restart();
            await PopupService.EnqueueSnackbarAsync(T("成功"), AlertTypes.Success);
        }
    }

    private async Task OnHisSave()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync(T("确认"), T("保存配置后将重启历史服务，是否确定?"));
        if (confirm)
        {
            await ConfigService.EditBatch(_hisConfig);
            HistoryValueHostService.Restart();
            await PopupService.EnqueueSnackbarAsync(T("成功"), AlertTypes.Success);
        }
    }

}