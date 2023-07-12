#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
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