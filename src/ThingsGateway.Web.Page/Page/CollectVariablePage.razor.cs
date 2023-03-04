using SqlSugar;

namespace ThingsGateway.Web.Page
{
    public partial class CollectVariablePage
    {
        private IAppDataTable _datatable;

        [CascadingParameter]
        MainLayout MainLayout { get; set; }


        [Inject]
        ResourceService ResourceService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }
        [Inject]
        IUploadDeviceService UploadDeviceService { get; set; }
        protected override async Task OnParametersSetAsync()
        {
            CollectDevices = CollectDeviceService.GetCacheListAsync();
            UploadDevices = UploadDeviceService.GetCacheListAsync();

            await base.OnParametersSetAsync();
        }

        private async Task AddCall(VariableAddInput input)
        {
            await VariableService.Add(input);
        }
        private async Task datatableQuery()
        {
            await _datatable.QueryClick();
        }

        private async Task DeleteCall(IEnumerable<CollectDeviceVariable> input)
        {
            await VariableService.Delete(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
        }

        private async Task EditCall(VariableEditInput input)
        {
            await VariableService.Edit(input);
        }

        private void FilterHeaders(List<DataTableHeader<CollectDeviceVariable>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(CollectDeviceVariable.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(CollectDeviceVariable.UpdateUserId));

            datas.RemoveWhere(it => it.Value == nameof(CollectDeviceVariable.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(CollectDeviceVariable.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(CollectDeviceVariable.Id));
            datas.RemoveWhere(it => it.Value == nameof(CollectDeviceVariable.VariablePropertys));

            datas.RemoveWhere(it => it.Value.Contains("His"));
            datas.RemoveWhere(it => it.Value.Contains("Alarm"));
            datas.RemoveWhere(it => it.Value.Contains("RestrainExpressions"));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(CollectDeviceVariable.Name):
                        item.Sortable = true;
                        break;
                }
            }
        }

        private void Filters(List<Filters> datas)
        {
            foreach (var item in datas)
            {
                switch (item.Key)
                {
                    case nameof(CollectDeviceVariable.CreateTime):
                    case nameof(CollectDeviceVariable.UpdateTime):
                    case nameof(CollectDeviceVariable.CreateUser):
                    case nameof(CollectDeviceVariable.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<CollectDeviceVariable>> QueryCall(VariablePageInput input)
        {
            var data = await VariableService.Page(input);
            return data;
        }
        private async Task Clear()
        {
            var confirm = await PopupService.OpenConfirmDialog(T("È·ÈÏ"), $"Çå¿Õ?");
            if (confirm)
            {
                await VariableService.Clear();
            }
            await datatableQuery();

        }
    }
}