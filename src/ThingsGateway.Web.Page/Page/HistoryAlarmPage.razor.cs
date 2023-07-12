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

using Masa.Blazor;

using SqlSugar;

using System;

using TouchSocket.Core;

namespace ThingsGateway.Web.Page;

public partial class HistoryAlarmPage
{
    [Parameter]
    [SupplyParameterFromQuery]
    public string DeviceName { get; set; }

    [Inject]
    public JsInitVariables JsInitVariables { get; set; } = default!;

    HisPageInput _searchModel { get; set; } = new();

    private IAppDataTable _datatable;
    AlarmWorker AlarmHostService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AlarmHostService = ServiceExtensions.GetBackgroundService<AlarmWorker>();
        _searchModel.DeviceName = DeviceName;
        await base.OnInitializedAsync();
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

    private async Task<SqlSugarPagedList<HistoryAlarm>> QueryCall(HisPageInput input)
    {
        var result = await AlarmHostService.GetAlarmDbAsync();
        if (result.IsSuccess)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var data = await result.Content.CopyNew().Queryable<HistoryAlarm>().WhereIF(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName.Contains(input.DeviceName)).WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name)).WhereIF(input.StartTime != null, a => a.EventTime >= input.StartTime.Value).WhereIF(input.EndTime != null, a => a.EventTime <= input.EndTime.Value).OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}").OrderBy(u => u.Id) //����
                    .ToPagedListAsync(input.Current, input.Size);
                    return data;
                }
                catch (Exception ex)
                {
                    await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync("��ѯʧ�ܣ�������������", AlertTypes.Warning));
                    return new()
                    {
                        Current = 1,
                        Size = 10,
                        Pages = 0,
                        Records = new List<HistoryAlarm>(),
                        Total = 0
                    };
                }
            });
        }
        else
        {
            await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync(result.Message, AlertTypes.Warning));
            return new()
            {
                Current = 1,
                Size = 10,
                Pages = 0,
                Records = new List<HistoryAlarm>(),
                Total = 0
            };
        }
    }
}