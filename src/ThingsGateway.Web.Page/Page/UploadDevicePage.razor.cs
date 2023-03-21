using SqlSugar;

namespace ThingsGateway.Web.Page
{
    public partial class UploadDevicePage
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
        protected override async Task OnParametersSetAsync()
        {
            DriverPlugins = DriverPluginService.GetDriverPluginChildrenList(DriverEnum.Upload);
            _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
            await base.OnParametersSetAsync();
        }

        private async Task AddCall(UploadDeviceAddInput input)
        {
            await UploadDeviceService.Add(input);
            _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }
        private async Task datatableQuery()
        {
            await _datatable?.QueryClick();
        }

        private async Task DeleteCall(IEnumerable<UploadDevice> input)
        {
            await UploadDeviceService.Delete(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
            _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }

        private async Task EditCall(UploadDeviceEditInput input)
        {
            await UploadDeviceService.Edit(input);
            _deviceGroups = UploadDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }

        private void FilterHeaders(List<DataTableHeader<UploadDevice>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.UpdateUserId));

            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.Id));
            datas.RemoveWhere(it => it.Value == nameof(UploadDevice.DevicePropertys));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(UploadDevice.Name):
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
                    case nameof(UploadDevice.CreateTime):
                    case nameof(UploadDevice.UpdateTime):
                    case nameof(UploadDevice.CreateUser):
                    case nameof(UploadDevice.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<UploadDevice>> QueryCall(UploadDevicePageInput input)
        {
            var data = await UploadDeviceService.Page(input);
            return data;
        }
    }
}