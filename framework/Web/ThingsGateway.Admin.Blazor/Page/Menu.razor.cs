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

namespace ThingsGateway.Admin.Blazor;
/// <summary>
/// 菜单页面
/// </summary>
public partial class Menu
{
    private readonly MenuPageInput search = new();
    IAppDataTable _buttonsDatatable;
    private IAppDataTable _datatable;
    string _searchName;
    long buttonParentId;
    bool IsShowButtonList;
    List<SysResource> MenuCatalog = new();


    [CascadingParameter]
    MainLayout MainLayout { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        List<SysResource> sysResources = await App.GetService<IResourceService>().GetListByCategoryAsync(ResourceCategoryEnum.MENU);
        sysResources = sysResources.Where(it => it.TargetType == TargetTypeEnum.None).ToList();
        MenuCatalog = App.GetService<IResourceService>().ResourceListToTree(sysResources);
        await base.OnParametersSetAsync();
    }

    private async Task AddCallAsync(MenuAddInput input)
    {
        input.ParentId = search.ParentId;
        await App.GetService<MenuService>().AddAsync(input);
        await NavChangeAsync();
    }
    private async Task ButtonAddCallAsync(ButtonAddInput input)
    {
        input.ParentId = buttonParentId;
        await App.GetService<ButtonService>().AddAsync(input);
    }

    private Task ButtonDeleteCallAsync(IEnumerable<SysResource> input)
    {
        return App.GetService<ButtonService>().DeleteAsync(input.Select(a => a.Id).ToArray());
    }

    private Task ButtonEditCallAsync(ButtonEditInput input)
    {
        return App.GetService<ButtonService>().EditAsync(input);

    }

    private void ButtonFilters(List<Filters> datas)
    {
        foreach (var item in datas)
        {
            switch (item.Key)
            {
                case nameof(SysResource.Code):
                    item.Value = true;
                    break;
                case nameof(SysResource.Icon):
                case nameof(SysResource.Component):
                    item.Value = false;
                    break;
            }
        }
    }

    private async Task<ISqlSugarPagedList<SysResource>> ButtonQueryCallAsync(ButtonPageInput input)
    {
        input.ParentId = buttonParentId;
        var data = await App.GetService<ButtonService>().PageAsync(input);
        return data;
    }

    private Task DataTableQueryAsync()
    {
        return _datatable?.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<SysResource> input)
    {
        await App.GetService<MenuService>().DeleteAsync(input.Select(a => a.Id).ToArray());
        await NavChangeAsync();

    }
    private async Task EditCallAsync(MenuEditInput input)
    {
        await App.GetService<MenuService>().EditAsync(input);
        await NavChangeAsync();

    }

    private async Task<List<SysResource>> GetMenuCatalogAsync()
    {
        //获取所有菜单
        List<SysResource> sysResources = await App.GetService<ResourceService>().GetListByCategoryAsync(ResourceCategoryEnum.MENU);
        sysResources = sysResources.Where(it => it.TargetType == TargetTypeEnum.None).ToList();
        MenuCatalog = App.GetService<ResourceService>().ResourceListToTree(sysResources);
        return MenuCatalog;
    }

    private async Task NavChangeAsync()
    {
        await MainLayout.StateHasChangedAsync();
        await GetMenuCatalogAsync();
    }
    private async Task<ISqlSugarPagedList<SysResource>> QueryCallAsync(MenuPageInput input)
    {
        var data = await App.GetService<MenuService>().TreeAsync(input);
        return data.ToPagedList(input);
    }

    private async Task ShowButtonListAsync(long parentId)
    {
        buttonParentId = parentId;
        IsShowButtonList = true;
        if (_buttonsDatatable != null)
            await _buttonsDatatable.QueryClickAsync();
    }
}