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

using SqlSugar;

namespace ThingsGateway.Web.Page
{
    public partial class CollectDevicePage
    {
        private IAppDataTable _datatable;
        private CollectDevicePageInput search = new();


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
            DriverPlugins = DriverPluginService.GetDriverPluginChildrenList(DriverEnum.Collect);
            _deviceGroups = CollectDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
            await base.OnParametersSetAsync();
        }

        private async Task AddCall(CollectDeviceAddInput input)
        {
            await CollectDeviceService.AddAsync(input);
            _deviceGroups = CollectDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }
        private async Task datatableQuery()
        {
            await _datatable?.QueryClickAsync();
        }

        private async Task DeleteCall(IEnumerable<CollectDevice> input)
        {
            await CollectDeviceService.DeleteAsync(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
            _deviceGroups = CollectDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();
        }

        private async Task EditCall(CollectDeviceEditInput input)
        {
            await CollectDeviceService.EditAsync(input);
            _deviceGroups = CollectDeviceService.GetCacheList()?.Select(a => a.DeviceGroup)?.Where(a => a != null).Distinct()?.ToList();

        }

        private void FilterHeaders(List<DataTableHeader<CollectDevice>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.UpdateUserId));

            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.Id));
            datas.RemoveWhere(it => it.Value == nameof(CollectDevice.DevicePropertys));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(CollectDevice.Name):
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
                    case nameof(CollectDevice.CreateTime):
                    case nameof(CollectDevice.UpdateTime):
                    case nameof(CollectDevice.CreateUser):
                    case nameof(CollectDevice.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<CollectDevice>> QueryCall(CollectDevicePageInput input)
        {
            var data = await CollectDeviceService.PageAsync(input);
            return data;
        }
    }
}