//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using SqlSugar;

using ThingsGateway.Admin.Application;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Admin.Razor;

public partial class OrgTree : IDisposable
{
    [Parameter]
    [NotNull]
    public long Value { get; set; }

    [Parameter]
    public Func<long, Task> ValueChanged { get; set; }

    [NotNull]
    private List<TreeViewItem<SysOrg>> Items { get; set; }


    [Inject]
    private IStringLocalizer<ThingsGateway.Admin.Razor._Imports> AdminLocalizer { get; set; }
    [Inject]
    [NotNull]
    private ISysOrgService? SysOrgService { get; set; }

    private static bool ModelEqualityComparer(SysOrg x, SysOrg y) => x.Id == y.Id;

    private async Task OnTreeItemClick(TreeViewItem<SysOrg> item)
    {
        var value = item.Value.Id;
        Value = value;
        if (ValueChanged != null)
        {
            await ValueChanged.Invoke(value);
        }
    }


    private List<TreeViewItem<SysOrg>> ZItem;
    protected override async Task OnInitializedAsync()
    {
        ZItem = new List<TreeViewItem<SysOrg>>() {new TreeViewItem<SysOrg>(new SysOrg(){ })
        {
            Text = AdminLocalizer["All"],
            IsActive = Value == 0,
            IsExpand = false,
            CheckedState = Value == 0 ? CheckboxState.Checked : CheckboxState.UnChecked
        } };
        var items = (await SysOrgService.SelectorAsync());
        Items = ZItem.Concat(OrgUtil.BuildTreeItemList(items, new List<long> { Value })).ToList();
        context = ExecutionContext.Capture();
        DispatchService.Subscribe(Refresh);
        await base.OnInitializedAsync();
    }
    private ExecutionContext? context;
    private async Task Notify()
    {
        var current = ExecutionContext.Capture();
        try
        {
            ExecutionContext.Restore(context);
            await InvokeAsync(async () =>
            {
                await OnClickSearch(SearchText);
            });
        }
        finally
        {
            ExecutionContext.Restore(current);
        }
    }

    private async Task Refresh(DispatchEntry<SysOrg> entry)
    {
        await Notify();
    }

    [Inject]
    private IDispatchService<SysOrg> DispatchService { get; set; }
    private string SearchText;

    private async Task<List<TreeViewItem<SysOrg>>> OnClickSearch(string searchText)
    {
        SearchText = searchText;
        var items = (await SysOrgService.SelectorAsync());
        items = items.WhereIF(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText)).ToList();
        return ZItem.Concat(OrgUtil.BuildTreeItemList(items, new List<long> { Value })).ToList();
    }

    public void Dispose()
    {
        context?.Dispose();
        DispatchService.UnSubscribe(Refresh);
    }
}
