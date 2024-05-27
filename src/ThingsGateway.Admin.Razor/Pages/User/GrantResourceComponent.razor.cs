//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class GrantResourceComponent
{
    private List<SysResource> ModuleList;

    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<long> Value { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public long Id { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public bool IsRole { get; set; }

    [Parameter]
    public EventCallback<List<long>> ValueChanged { get; set; }

    [NotNull]
    private List<TreeViewItem<SysResource>>? Items { get; set; }

    [Inject]
    [NotNull]
    private ISysResourceService? SysResourceService { get; set; }

    [Inject]
    [NotNull]
    private ISysUserService? SysUserService { get; set; }

    [Inject]
    [NotNull]
    private ISysRoleService? SysRoleService { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var items = (await SysResourceService.GetAllAsync()).Where(a => a.Category != ResourceCategoryEnum.Module).ToList();
        Items = ResourceUtil.BuildTreeItemList(items, Value, RenderTreeItem);
        ModuleList = (await SysResourceService.GetAllAsync()).Where(a => a.Category == ResourceCategoryEnum.Module).ToList();
    }

    private async Task OnTreeItemChecked(List<TreeViewItem<SysResource>> items)
    {
        Value = items.Select(a => a.Value.Id).ToList();
        if (ValueChanged.HasDelegate)
        {
            await ValueChanged.InvokeAsync(Value);
        }
    }

    private async Task OnClickClose()
    {
        if (OnCloseAsync != null)
            await OnCloseAsync();
    }

    private async Task OnClickSave()
    {
        try
        {
            GrantResourceData data = new();
            data.Id = Id;
            data.GrantInfoList = Value;

            if (IsRole)
                await SysRoleService.GrantResourceAsync(data);
            else
                await SysUserService.GrantResourceAsync(data);

            if (OnCloseAsync != null)
                await OnCloseAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }

    private string GetApp(long? moduleId) => ModuleList.FirstOrDefault(i => i.Id == moduleId)?.Title ?? ResourceConst.SpaTitle;

    private bool ModelEqualityComparer(SysResource x, SysResource y) => x.Id == y.Id;
}
