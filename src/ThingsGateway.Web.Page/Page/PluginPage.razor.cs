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



namespace ThingsGateway.Web.Page
{
    public partial class PluginPage
    {
        private IAppDataTable _datatable;
        private DriverPluginPageInput search = new();

        [CascadingParameter]
        MainLayout MainLayout { get; set; }

        [Inject]
        ResourceService ResourceService { get; set; }

        private async Task AddCall(DriverPluginAddInput input)
        {
            await DriverPluginService.AddAsync(input);
        }
        private async Task datatableQuery()
        {
            await _datatable?.QueryClickAsync();
        }
        private void FilterHeaders(List<DataTableHeader<DriverPlugin>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(DriverPlugin.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(DriverPlugin.UpdateUserId));

            datas.RemoveWhere(it => it.Value == nameof(DriverPlugin.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(DriverPlugin.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(DriverPlugin.Id));
            datas.RemoveWhere(it => it.Value == nameof(DriverPlugin.ExtJson));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(DriverPlugin.AssembleName):
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
                    case nameof(DriverPlugin.CreateTime):
                    case nameof(DriverPlugin.UpdateTime):
                    case nameof(DriverPlugin.CreateUser):
                    case nameof(DriverPlugin.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<DriverPlugin>> QueryCall(DriverPluginPageInput input)
        {
            var data = await DriverPluginService.PageAsync(input);
            return data;
        }
    }
}