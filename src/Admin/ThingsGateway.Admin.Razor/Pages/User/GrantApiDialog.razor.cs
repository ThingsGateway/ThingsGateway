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

public partial class GrantApiDialog
{
    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<string> Value { get; set; }

    [Parameter]
    public Func<List<string>, Task> ValueChanged { get; set; }

    [NotNull]
    private List<TreeViewItem<OpenApiPermissionTreeSelector>>? Items { get; set; }

    [Inject]
    [NotNull]
    private IApiPermissionService? ApiPermissionService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        var items = ApiPermissionService.ApiPermissionTreeSelector();
        Items = OpenApiUtil.BuildTreeItemList(items, Value, RenderTreeItem);
    }

    private static bool ModelEqualityComparer(OpenApiPermissionTreeSelector x, OpenApiPermissionTreeSelector y) => x.ApiRoute == y.ApiRoute;





    private async Task OnTreeItemChecked(List<TreeViewItem<OpenApiPermissionTreeSelector>> items)
    {
        var value = items.Where(a => a.Items == null || a.Items.Count <= 0).Select(a => a.Value.ApiRoute).ToList();
        Value = value;
        if (ValueChanged != null)
        {
            await ValueChanged.Invoke(value);
        }
    }
}
