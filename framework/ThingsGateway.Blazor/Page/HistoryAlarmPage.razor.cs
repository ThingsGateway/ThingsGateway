#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using BlazorComponent;

using Masa.Blazor;

using Microsoft.AspNetCore.Components;

using SqlSugar;

using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

namespace ThingsGateway.Blazor;

/// <summary>
/// ��ʷ����ҳ��
/// </summary>
public partial class HistoryAlarmPage
{
    private IAppDataTable _datatable;


    AlarmWorker AlarmHostService { get; set; }
    [Inject]
    InitTimezone InitTimezone { get; set; }

    HisPageInput SearchModel { get; set; } = new();
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        AlarmHostService = ServiceHelper.GetBackgroundService<AlarmWorker>();
        await base.OnInitializedAsync();
    }

    private async Task DatatableQuery()
    {
        await _datatable?.QueryClickAsync();
    }
    private async Task<SqlSugarPagedList<HistoryAlarm>> QueryCallAsync(HisPageInput input)
    {
        var result = await AlarmHostService.GetAlarmDbAsync();
        if (result.IsSuccess)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var query = result.Content.CopyNew().Queryable<HistoryAlarm>().
                    WhereIF(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName.Contains(input.DeviceName))
                    .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name))
                    .WhereIF(input.StartTime != null, a => a.EventTime >= input.StartTime.Value)
                    .WhereIF(input.EndTime != null, a => a.EventTime <= input.EndTime.Value);

                    for (int i = 0; i < input.SortField.Count; i++)
                    {
                        query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
                    }
                    var data = await query.ToPagedListAsync(input.Current, input.Size);
                    return data;
                }
                catch (Exception ex)
                {
                    await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync("��ѯʧ�ܣ������������ӣ�" + ex.Message, AlertTypes.Warning));
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