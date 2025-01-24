//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class SysResourcePage
{
    private ResourceTableSearchModel CustomerSearchModel { get; set; } = new ResourceTableSearchModel();

    private List<SelectedItem> ModuleSelectedItems { get; set; }

    private List<TreeViewItem<SysResource>> MenuTreeItems { get; set; }
    private List<SelectedItem> MenuItems { get; set; }

    [CascadingParameter(Name = "ReloadMenu")]
    private Func<Task>? ReloadMenu { get; set; }

    [CascadingParameter(Name = "ReloadUser")]
    private Func<Task>? ReloadUser { get; set; }

    [Inject]
    [NotNull]
    private ISysResourceService? SysResourceService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        CustomerSearchModel.Module = (await SysResourceService.GetAllAsync()).FirstOrDefault(a => a.Category == ResourceCategoryEnum.Module)?.Id ?? ResourceConst.SystemId;
        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        ModuleSelectedItems = ResourceUtil.BuildModuleSelectList((await SysResourceService.GetAllAsync())).ToList();
        MenuItems = ResourceUtil.BuildMenuSelectList((await SysResourceService.GetAllAsync())).Concat(new List<SelectedItem>() { new("0", AdminLocalizer["Root"]) }).ToList();

        await base.OnParametersSetAsync();
    }

    #region 查询

    private async Task<QueryData<SysResource>> OnQueryAsync(QueryPageOptions options)
    {
        MenuTreeItems = new List<TreeViewItem<SysResource>>() { new TreeViewItem<SysResource>(new SysResource()) { Text = AdminLocalizer["Root"] } }.Concat(ResourceUtil.BuildTreeItemList((await SysResourceService.GetAllAsync()).Where(a => a.Module == CustomerSearchModel.Module), new(), null)).ToList();

        var data = await SysResourceService.PageAsync(options, CustomerSearchModel);
        return data;
    }

    #endregion 查询

    #region 修改
    private List<SysResource> SelectedRows { get; set; } = new();
    private long CopyModule { get; set; }
    private long ChangeParentId { get; set; }

    private async Task OnCopy()
    {
        try
        {
            await SysResourceService.CopyAsync(SelectedRows.Select(a => a.Id), CopyModule);
            await table.QueryAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }

    }
    private async Task OnChangeParent()
    {
        try
        {
            await SysResourceService.ChangeParentAsync(SelectedRows.Select(a => a.Id).FirstOrDefault(), ChangeParentId);
            await table.QueryAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
        }

    }

    private async Task<bool> Delete(IEnumerable<SysResource> sysResources)
    {
        try
        {
            var result = await SysResourceService.DeleteResourceAsync(sysResources.Select(a => a.Id));
            if (ReloadUser != null)
            {
                await ReloadUser();
            }
            return result;
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    private async Task<bool> Save(SysResource sysResource, ItemChangedType itemChangedType)
    {
        try
        {
            if (itemChangedType == ItemChangedType.Add && sysResource.Category != ResourceCategoryEnum.Module)
                sysResource.Module = CustomerSearchModel.Module;
            var result = await SysResourceService.SaveResourceAsync(sysResource, itemChangedType);
            if (ReloadUser != null)
            {
                await ReloadUser();
            }
            return result;
        }
        catch (Exception ex)
        {
            await ToastService.Warn(ex);
            return false;
        }
    }

    #endregion 修改

    #region 树节点

    private static bool ModelEqualityComparer(SysResource x, SysResource y) => x.Id == y.Id;

    private async Task<IEnumerable<TableTreeNode<SysResource>>> OnTreeExpand(SysResource menu)
    {
        var sysResources = await SysResourceService.GetAllAsync();
        var result = ResourceUtil.BuildTableTrees(sysResources, menu.Id);
        return result;
    }

    private static async Task<IEnumerable<TableTreeNode<SysResource>>> TreeNodeConverter(IEnumerable<SysResource> items)
    {
        await Task.CompletedTask;
        var result = ResourceUtil.BuildTableTrees(items, 0);
        return result;
    }

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
