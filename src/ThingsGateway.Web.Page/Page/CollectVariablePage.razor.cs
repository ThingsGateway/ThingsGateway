#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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

        [Inject]
        IUploadDeviceService UploadDeviceService { get; set; }
        protected override async Task OnParametersSetAsync()
        {
            CollectDevices = CollectDeviceService.GetCacheList();
            UploadDevices = UploadDeviceService.GetCacheList();
            _deviceGroups = CollectDeviceService.GetTree();

            await base.OnParametersSetAsync();
        }

        private async Task AddCall(VariableAddInput input)
        {
            await VariableService.AddAsync(input);
        }
        private async Task datatableQuery()
        {
            await _datatable.QueryClickAsync();
        }

        private async Task DeleteCall(IEnumerable<CollectDeviceVariable> input)
        {
            await VariableService.DeleteAsync(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
        }

        private async Task EditCall(VariableEditInput input)
        {
            await VariableService.EditAsync(input);
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
            var data = await VariableService.PageAsync(input);
            return data;
        }
        private async Task Clear()
        {
            var confirm = await PopupService.OpenConfirmDialogAsync(T("确认"), $"清空?");
            if (confirm)
            {
                await VariableService.ClearAsync();
            }
            await datatableQuery();

        }
    }
}