using BlazorComponent;

using SqlSugar;

using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    public partial class Spa
    {
        private IAppDataTable _datatable;
        private SpaPageInput search = new();

        private async Task AddCall(SpaAddInput input)
        {
            await SpaService.Add(input);
        }

        private async Task DeleteCall(IEnumerable<SysResource> input)
        {
            await SpaService.Delete(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
        }

        private async Task EditCall(SpaEditInput input)
        {
            await SpaService.Edit(input);
        }
        private void Filters(List<Filters> datas)
        {
            foreach (var item in datas)
            {
                switch (item.Key)
                {
                    case nameof(SysResource.Code):
                    case nameof(SysResource.Category):
                    case nameof(SysResource.CreateTime):
                    case nameof(SysResource.UpdateTime):
                    case nameof(SysResource.CreateUser):
                    case nameof(SysResource.TargetType):
                    case nameof(SysResource.UpdateUser):
                        item.Value = false;
                        break;
                }
            }
        }

        private void FilterHeaders(List<DataTableHeader<SysResource>> datas)
        {

            datas.RemoveWhere(it => it.Value == nameof(SysResource.ParentId));
            datas.RemoveWhere(it => it.Value == nameof(SysResource.CreateUserId));
            datas.RemoveWhere(it => it.Value == nameof(SysResource.UpdateUserId));

            datas.RemoveWhere(it => it.Value == nameof(SysResource.IsDelete));
            datas.RemoveWhere(it => it.Value == nameof(SysResource.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(SysResource.Id));

            datas.RemoveWhere(it => it.Value == nameof(SysResource.Children));

            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(SysResource.Name):
                        item.Sortable = true;
                        break;

                    case nameof(SysResource.SortCode):
                        item.Sortable = true;
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<SysResource>> QueryCall(SpaPageInput input)
        {
            var data = await SpaService.Page(input);
            return data;
        }
    }
}