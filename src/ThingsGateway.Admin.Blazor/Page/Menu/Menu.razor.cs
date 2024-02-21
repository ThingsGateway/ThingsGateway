//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 菜单页面
/// </summary>
public partial class Menu
{
    private readonly MenuPageInput _search = new();
    private IAppDataTable _datatable;
    private List<SysResource> _menuCatalog = new();
    private string _searchName;

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <returns></returns>
    protected override async Task OnParametersSetAsync()
    {
        await GetMenuCatalogAsync();
        await base.OnParametersSetAsync();
    }

    private async Task ShowButtonListAsync(long parentId)
    {
        await PopupService.OpenAsync(typeof(Button), new Dictionary<string, object?>()
        {
            {nameof(Button.ParentId),parentId },
});
        await MainLayout.StateHasChangedAsync();
    }

    private async Task AddCallAsync(MenuAddInput input)
    {
        input.ParentId = _search.ParentId;
        await _serviceScope.ServiceProvider.GetService<IMenuService>().AddAsync(input);
        await NavChangeAsync();
    }

    private Task DataTableQueryAsync()
    {
        return _datatable?.QueryClickAsync();
    }

    private async Task DeleteCallAsync(IEnumerable<SysResource> input)
    {
        await _serviceScope.ServiceProvider.GetService<IMenuService>().DeleteAsync(input.Adapt<List<BaseIdInput>>());
        await NavChangeAsync();
    }

    private async Task EditCallAsync(MenuEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IMenuService>().EditAsync(input);
        await NavChangeAsync();
    }

    private async Task GetMenuCatalogAsync()
    {
        //获取所有菜单
        List<SysResource> sysResources = await _serviceScope.ServiceProvider.GetService<IResourceService>().GetListByCategoryAsync(CateGoryConst.Resource_MENU);
        sysResources = sysResources.Where(it => it.MenuType == MenuTypeEnum.CATALOG).ToList();
        _menuCatalog = _serviceScope.ServiceProvider.GetService<IMenuService>().ConstructMenuTrees(sysResources);
    }

    private async Task NavChangeAsync()
    {
        await MainLayout.StateHasChangedAsync();
        await GetMenuCatalogAsync();
    }

    private async Task<SqlSugarPagedList<SysResource>> QueryCallAsync(MenuPageInput input)
    {
        var data = await _serviceScope.ServiceProvider.GetService<IMenuService>().GetListAsync(input);
        return data.ToPagedList(input);
    }
}