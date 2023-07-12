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

using SqlSugar;

using System;

using TouchSocket.Core;

namespace ThingsGateway.Web.Page
{
    public partial class HistoryValuePage
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public string DeviceName { get; set; }

        [Inject]
        public JsInitVariables JsInitVariables { get; set; } = default!;

        HisPageInput _searchModel { get; set; } = new();

        private IAppDataTable _datatable;
        HistoryValueWorker HistoryValueHostService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            HistoryValueHostService = ServiceExtensions.GetBackgroundService<HistoryValueWorker>();
            _searchModel.DeviceName = DeviceName;
            await base.OnInitializedAsync();
        }

        private async Task datatableQuery()
        {
            await _datatable?.QueryClickAsync();
        }

        private void FilterHeaders(List<DataTableHeader<HistoryValue>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(HistoryValue.Id));
            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(HistoryValue.Name):
                        item.Sortable = true;
                        break;
                    case nameof(HistoryValue.Quality):
                        item.Sortable = true;
                        break;
                    case nameof(HistoryValue.CollectTime):
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

        private async Task<SqlSugarPagedList<HistoryValue>> QueryCall(HisPageInput input)
        {
            var result = await HistoryValueHostService.GetHisDbAsync();
            if (result.IsSuccess)
            {
                try
                {
                    return await Task.Run(async () =>
                    {
                        var data = await result.Content.CopyNew().Queryable<HistoryValue>().WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name.Contains(input.Name)).WhereIF(input.StartTime != null, a => a.CollectTime >= input.StartTime).WhereIF(input.EndTime != null, a => a.CollectTime <= input.EndTime).OrderByIF(!string.IsNullOrEmpty(input.SortField), $"{input.SortField} {input.SortOrder}").ToPagedListAsync(input.Current, input.Size);
                        return data;
                    });
                }
                catch (Exception ex)
                {
                    await InvokeAsync(async () => await PopupService.EnqueueSnackbarAsync("查询失败，请检查网络连接", AlertTypes.Warning));
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
}