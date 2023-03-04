using BlazorComponent;

using SqlSugar;

using System.Linq;

namespace ThingsGateway.Web.Rcl
{
    public partial class Menu
    {
        private IAppDataTable _datatable;
        List<SysResource> MenuCatalog = new();
        private MenuPageInput search = new();

        [CascadingParameter]
        MainLayout MainLayout { get; set; }

        [Inject]
        ResourceService ResourceService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await GetMenuCatalog();
            await base.OnInitializedAsync();
        }

        private async Task AddCall(MenuAddInput input)
        {
            input.ParentId = search.ParentId;
            await MenuService.Add(input);
            await NavChange();
        }
        private async Task datatableQuery()
        {
            await _datatable?.QueryClick();
        }

        private async Task DeleteCall(IEnumerable<SysResource> input)
        {
            await MenuService.Delete(input.ToList().ConvertAll(it => new BaseIdInput()
            { Id = it.Id }));
            await NavChange();

        }

        private async Task EditCall(MenuEditInput input)
        {
            await MenuService.Edit(input);
            await NavChange();

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

        private async Task<List<SysResource>> GetMenuCatalog()
        {
            //获取所有菜单
            List<SysResource> sysResources = await ResourceService.GetListByCategory(MenuCategoryEnum.MENU);
            sysResources = sysResources.Where(it => it.TargetType == TargetTypeEnum.CATALOG).ToList();
            MenuCatalog = sysResources.ResourceListToTree();
            return MenuCatalog;
        }

        private async Task NavChange()
        {
            await MainLayout.MenuChange();
            await GetMenuCatalog();
        }
        private async Task<SqlSugarPagedList<SysResource>> QueryCall(MenuPageInput input)
        {
            var data = await MenuService.Tree(input);
            return await data.ToPagedListAsync(input);
        }
    }
}