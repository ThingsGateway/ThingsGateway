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

using SqlSugar;

using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;


namespace ThingsGateway.Blazor;
/// <summary>
/// ʱ���ҳ��
/// </summary>
public partial class HistoryValuePage
{

    HisPageInput SearchModel { get; set; } = new();

    private IAppDataTable _datatable;
    HistoryValueWorker HistoryValueHostService { get; set; }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        HistoryValueHostService = ServiceHelper.GetBackgroundService<HistoryValueWorker>();
        await base.OnInitializedAsync();
    }

    private async Task DatatableQuery()
    {
        await _datatable?.QueryClickAsync();
    }




    private async Task<SqlSugarPagedList<HistoryValue>> QueryCallAsync(HisPageInput input)
    {
        var result = await HistoryValueHostService.GetHisDbAsync();
        if (result.IsSuccess)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    var query = result.Content.CopyNew().Queryable<HistoryValue>()
                    .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name))
                    .WhereIF(input.StartTime != null, a => a.CollectTime >= input.StartTime.Value)
                    .WhereIF(input.EndTime != null, a => a.CollectTime <= input.EndTime.Value);

                    for (int i = 0; i < input.SortField.Count; i++)
                    {
                        query = query.OrderByIF(!string.IsNullOrEmpty(input.SortField[i]), $"{input.SortField[i]} {(input.SortDesc[i] ? "desc" : "asc")}");
                    }
                    var data = await query.ToPagedListAsync(input.Current, input.Size);
                    return data;
                });
            }
            catch (Exception ex)
            {
                await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync("��ѯʧ�ܣ������������ӣ�" + ex.Message, AlertTypes.Warning));
                return new()
                {
                    Current = 1,
                    Size = 10,
                    Pages = 0,
                    Records = new List<HistoryValue>(),
                    Total = 0
                };
            }
        }
        else
        {
            await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync(result.Message, AlertTypes.Warning));
            return new()
            {
                Current = 1,
                Size = 10,
                Pages = 0,
                Records = new List<HistoryValue>(),
                Total = 0
            };
        }
    }
}