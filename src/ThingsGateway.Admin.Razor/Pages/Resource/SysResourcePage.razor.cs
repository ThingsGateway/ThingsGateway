//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class SysResourcePage
{
    [Inject]
    [NotNull]
    private ISysResourceService? SysResourceService { get; set; }

    private List<SelectedItem> ModuleSelectedItems { get; set; }
    private IEnumerable<SelectedItem> ParementSelectedItems { get; set; }

    private ResourceSearchInput CustomerSearchModel { get; set; } = new ResourceSearchInput();

    [CascadingParameter(Name = "ReloadMenu")]
    private Func<Task>? ReloadMenu { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        ModuleSelectedItems = ResourceUtil.BuildModuleSelectList((await SysResourceService.GetAllAsync())).ToList();
        ModuleSelectedItems.Add(new SelectedItem(ResourceConst.SpaId.ToString(), ResourceConst.SpaTitle));
        ParementSelectedItems = ResourceUtil.BuildMenuSelectList((await SysResourceService.GetAllAsync())).Concat(new List<SelectedItem>() { new("0", Localizer["Root"]) }).ToList();
        await base.OnParametersSetAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        CustomerSearchModel.Module = (await SysResourceService.GetAllAsync()).FirstOrDefault(a => a.Category == ResourceCategoryEnum.Module)?.Id ?? ResourceConst.SystemId;
        await base.OnInitializedAsync();
    }

    #region 查询

    private async Task<QueryData<SysResource>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await SysResourceService.PageAsync(options, CustomerSearchModel);
        return data;
    }

    #endregion 查询

    #region 修改

    private async Task<bool> Save(SysResource sysResource, ItemChangedType itemChangedType)
    {
        try
        {
            if (itemChangedType == ItemChangedType.Add && sysResource.Category != ResourceCategoryEnum.Module)
                sysResource.Module = CustomerSearchModel.Module;
            return await SysResourceService.SaveResourceAsync(sysResource, itemChangedType);
        }
        catch (Exception ex)
        {
            await ToastService.Warning(string.Empty, $"{ex.Message}");
            return false;
        }
    }

    private async Task<bool> Delete(IEnumerable<SysResource> sysResources)
    {
        try
        {
            return await SysResourceService.DeleteResourceAsync(sysResources.Select(a => a.Id));
        }
        catch (Exception ex)
        {
            await ToastService.Warning(string.Empty, $"{ex.Message}");
            return false;
        }
    }

    #endregion 修改

    #region 树节点

    private async Task<IEnumerable<TableTreeNode<SysResource>>> OnTreeExpand(SysResource menu)
    {
        var sysResources = await SysResourceService.GetAllAsync();
        var result = ResourceUtil.BuildTableTrees(sysResources, menu.Id);
        return result;
    }

    private async Task<IEnumerable<TableTreeNode<SysResource>>> TreeNodeConverter(IEnumerable<SysResource> items)
    {
        await Task.CompletedTask;
        var result = ResourceUtil.BuildTableTrees(items, 0);
        return result;
    }

    private bool ModelEqualityComparer(SysResource x, SysResource y) => x.Id == y.Id;

    #endregion 树节点

    #region 更新页面

    private async Task OnAfterModifyAsync()
    {
        if (ReloadMenu != null)
        {
            await ReloadMenu();
        }
        await OnParametersSetAsync();
    }

    #endregion 更新页面
}