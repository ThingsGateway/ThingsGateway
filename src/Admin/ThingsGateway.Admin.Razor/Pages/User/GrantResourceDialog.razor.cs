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

public partial class GrantResourceDialog
{
    private List<SysResource> ModuleList;

    [Parameter]
    [EditorRequired]
    [NotNull]
    public List<long> Value { get; set; }

    [Parameter]
    public Func<List<long>, Task> ValueChanged { get; set; }

    [NotNull]
    private List<TreeViewItem<SysResource>>? Items { get; set; }

    [Inject]
    [NotNull]
    private ISysResourceService? SysResourceService { get; set; }


    protected override async Task OnInitializedAsync()
    {
        var items = (await SysResourceService.GetAllAsync()).Where(a => a.Category != ResourceCategoryEnum.Module).OrderBy(a => a.Module).ThenBy(a => a.Id).ToList();

        Items = ResourceUtil.BuildTreeItemList(items, Value, RenderTreeItem);
        ModuleList = (await SysResourceService.GetAllAsync()).Where(a => a.Category == ResourceCategoryEnum.Module).ToList();
        await base.OnInitializedAsync();
    }

    private string GetApp(long? moduleId) => ModuleList.FirstOrDefault(i => i.Id == moduleId)?.Title;

    private static bool ModelEqualityComparer(SysResource x, SysResource y) => x.Id == y.Id;


    private async Task OnTreeItemChecked(List<TreeViewItem<SysResource>> items)
    {
        var value = items.Select(a => a.Value.Id).ToList();
        Value = value;
        if (ValueChanged != null)
        {
            await ValueChanged.Invoke(value);
        }
    }
}
