
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

public partial class GrantApiComponent
{
    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<string> Value { get; set; }

    [Parameter]
    public EventCallback<List<string>> ValueChanged { get; set; }

    [Parameter]
    [EditorRequired]
    [NotNull]
    public long Id { get; set; }

    [Parameter]
    [NotNull]
    public bool IsRole { get; set; }

    [NotNull]
    private List<TreeViewItem<OpenApiPermissionTreeSelector>>? Items { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    [Inject]
    [NotNull]
    private ISysUserService? SysUserService { get; set; }

    [Inject]
    [NotNull]
    private ISysRoleService? SysRoleService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var items = (ResourceUtil.ApiPermissionTreeSelector());
        Items = ResourceUtil.BuildTreeItemList(items, Value, RenderTreeItem);
    }

    private async Task OnTreeItemChecked(List<TreeViewItem<OpenApiPermissionTreeSelector>> items)
    {
        Value = items.Select(a => a.Value.ApiRoute).ToList();
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
            GrantPermissionData data = new();
            data.Id = Id;
            data.GrantInfoList = Value.Select(a => new RelationRolePermission() { ApiUrl = a });
            if (IsRole)
                await SysRoleService.GrantApiPermissionAsync(data);
            else
                await SysUserService.GrantApiPermissionAsync(data);
            if (OnCloseAsync != null)
                await OnCloseAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }

    private bool ModelEqualityComparer(OpenApiPermissionTreeSelector x, OpenApiPermissionTreeSelector y) => x.ApiRoute == y.ApiRoute;
}