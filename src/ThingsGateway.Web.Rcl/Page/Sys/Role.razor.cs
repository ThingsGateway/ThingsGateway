using BlazorComponent;

using SqlSugar;

using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    public partial class Role
    {
        private IAppDataTable _datatable;
        private RolePageInput search = new();

        private async Task AddCall(RoleAddInput input)
        {
            await SysRoleService.Add(input);
        }

        private async Task DeleteCall(IEnumerable<SysRole> sysRoles)
        {
            await SysRoleService.Delete(sysRoles.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
        }

        private async Task EditCall(RoleEditInput input)
        {
            await SysRoleService.Edit(input);
        }

        private void FilterHeaders(List<DataTableHeader<SysRole>> datas)
        {
            datas.RemoveWhere(it => it.Value == nameof(SysRole.ExtJson));
            datas.RemoveWhere(it => it.Value == nameof(SysRole.Id));
            datas.RemoveWhere(it => it.Value == nameof(SysRole.Code));
            foreach (var item in datas)
            {
                item.Sortable = false;
                item.Filterable = false;
                item.Divider = false;
                item.Align = DataTableHeaderAlign.Start;
                item.CellClass = " table-minwidth ";
                switch (item.Value)
                {
                    case nameof(SysRole.Name):
                        item.Sortable = true;
                        break;

                    case nameof(SysRole.SortCode):
                        item.Sortable = true;
                        break;
                }
            }
        }

        private async Task<SqlSugarPagedList<SysRole>> QueryCall(RolePageInput input)
        {
            var data = await SysRoleService.Page(input);
            return data;
        }
    }
}