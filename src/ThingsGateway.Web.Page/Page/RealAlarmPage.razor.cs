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

using Mapster;

using Masa.Blazor;

using SqlSugar;

using System;

using TouchSocket.Core;

namespace ThingsGateway.Web.Page;

public partial class RealAlarmPage
{
    [Inject]
    public JsInitVariables JsInitVariables { get; set; } = default!;

    bool IsAutoQuery { get; set; }

    [Parameter]
    [SupplyParameterFromQuery]
    public string DeviceName { get; set; }

    VariablePageInput _searchModel { get; set; } = new();

    private IAppDataTable _datatable;
    AlarmWorker AlarmHostService { get; set; }

    private System.Timers.Timer DelayTimer;
    protected override async Task OnInitializedAsync()
    {
        AlarmHostService = ServiceExtensions.GetBackgroundService<AlarmWorker>();
        DelayTimer = new System.Timers.Timer(5000);
        DelayTimer.Elapsed += timer_Elapsed;
        DelayTimer.AutoReset = true;
        DelayTimer.Start();
        _searchModel.DeviceName = DeviceName;
        await base.OnInitializedAsync();
    }

    async void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            if (IsAutoQuery)
                await InvokeAsync(async () => await datatableQuery());
        }
        catch (Exception ex)
        {
            await PopupService.EnqueueSnackbarAsync(ex);
        }

    }

    protected override async Task DisposeAsync(bool disposing)
    {
        await base.DisposeAsync(disposing);
        DelayTimer?.SafeDispose();
    }

    private async Task datatableQuery()
    {
        await _datatable?.QueryClickAsync();
    }

    private void FilterHeaders(List<DataTableHeader<HistoryAlarm>> datas)
    {
        datas.RemoveWhere(it => it.Value == nameof(HistoryAlarm.Id));
        foreach (var item in datas)
        {
            item.Sortable = false;
            item.Filterable = false;
            item.Divider = false;
            item.Align = DataTableHeaderAlign.Start;
            item.CellClass = " table-minwidth ";
            switch (item.Value)
            {
                case nameof(HistoryAlarm.Name):
                    item.Sortable = true;
                    break;
                case nameof(HistoryAlarm.DeviceName):
                    item.Sortable = true;
                    break;
                case nameof(HistoryAlarm.VariableAddress):
                    item.Sortable = true;
                    break;
                case nameof(HistoryAlarm.IsOnline):
                    item.Sortable = true;
                    break;
            }
        }
    }

    private void Filters(List<Filters> datas)
    {
        foreach (var item in datas)
        {
        }
    }

    private async Task<SqlSugarPagedList<HistoryAlarm>> QueryCall(VariablePageInput input)
    {
        var devices = AlarmHostService.RealAlarmDeviceVariables.Adapt<List<HistoryAlarm>>();
        var data = await devices.WhereIf(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName.Contains(input.DeviceName)).WhereIf(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name)).ToList().ToPagedListAsync(input);
        return data;
    }
}