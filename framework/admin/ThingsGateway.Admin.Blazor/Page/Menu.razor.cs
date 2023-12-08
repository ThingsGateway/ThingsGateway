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

using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 菜单页面
/// </summary>
public partial class Menu
{
    private readonly MenuPageInput _search = new();
    private long _buttonParentId;
    private IAppDataTable _buttonsDatatable;
    private IAppDataTable _datatable;
    private bool _isShowButtonList;
    private List<SysResource> _menuCatalog = new();
    private string _searchName;

    [CascadingParameter]
    private MainLayout _mainLayout { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        List<SysResource> sysResources = await _serviceScope.ServiceProvider.GetService<IResourceService>().GetListByCategoryAsync(ResourceCategoryEnum.MENU);
        sysResources = sysResources.Where(it => it.TargetType == TargetTypeEnum.None).ToList();
        _menuCatalog = _serviceScope.ServiceProvider.GetService<IResourceService>().ResourceListToTree(sysResources);
        await base.OnParametersSetAsync();
    }

    private async Task AddCallAsync(MenuAddInput input)
    {
        input.ParentId = _search.ParentId;
        await _serviceScope.ServiceProvider.GetService<IMenuService>().AddAsync(input);
        await NavChangeAsync();
    }

    private async Task ButtonAddCallAsync(ButtonAddInput input)
    {
        input.ParentId = _buttonParentId;
        await _serviceScope.ServiceProvider.GetService<IButtonService>().AddAsync(input);
    }

    private async Task ButtonDeleteCallAsync(IEnumerable<SysResource> input)
    {
        await _serviceScope.ServiceProvider.GetService<IButtonService>().DeleteAsync(input.Select(a => a.Id).ToArray());
    }

    private async Task ButtonEditCallAsync(ButtonEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IButtonService>().EditAsync(input);
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
        input.ParentId = _buttonParentId;
        var data = await _serviceScope.ServiceProvider.GetService<IButtonService>().PageAsync(input);
        return data;
    }

    private async Task DataTableQueryAsync()
    {
        await _datatable?.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<SysResource> input)
    {
        await _serviceScope.ServiceProvider.GetService<IMenuService>().DeleteAsync(input.Select(a => a.Id).ToArray());
        await NavChangeAsync();
    }

    private async Task EditCallAsync(MenuEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IMenuService>().EditAsync(input);
        await NavChangeAsync();
    }

    private async Task<List<SysResource>> GetMenuCatalogAsync()
    {
        //获取所有菜单
        List<SysResource> sysResources = await _serviceScope.ServiceProvider.GetService<ResourceService>().GetListByCategoryAsync(ResourceCategoryEnum.MENU);
        sysResources = sysResources.Where(it => it.TargetType == TargetTypeEnum.None).ToList();
        _menuCatalog = _serviceScope.ServiceProvider.GetService<ResourceService>().ResourceListToTree(sysResources);
        return _menuCatalog;
    }

    private async Task NavChangeAsync()
    {
        await _mainLayout.StateHasChangedAsync();
        await GetMenuCatalogAsync();
    }

    private async Task<ISqlSugarPagedList<SysResource>> QueryCallAsync(MenuPageInput input)
    {
        var data = await _serviceScope.ServiceProvider.GetService<IMenuService>().TreeAsync(input);
        return data.ToPagedList(input);
    }

    private async Task ShowButtonListAsync(long parentId)
    {
        _buttonParentId = parentId;
        _isShowButtonList = true;
        if (_buttonsDatatable != null)
            await _buttonsDatatable.QueryClickAsync();
    }
}