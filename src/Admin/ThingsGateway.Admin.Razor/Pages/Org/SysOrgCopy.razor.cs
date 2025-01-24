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

public partial class SysOrgCopy
{
    [Parameter]
    [NotNull]
    public SysOrgCopyInput? SysOrgCopyInput { get; set; }

    [NotNull]
    private List<TreeViewItem<long>> Items { get; set; }

    [Inject]
    [NotNull]
    private ISysOrgService SysOrgService { get; set; }

    private List<SelectedItem> ContainsChildBoolItems;
    private List<SelectedItem> ContainsPositionBoolItems;

    protected override async Task OnInitializedAsync()
    {
        ContainsChildBoolItems = LocalizerUtil.GetBoolItems(SysOrgCopyInput.GetType(), nameof(SysOrgCopyInput.ContainsChild));
        ContainsPositionBoolItems = LocalizerUtil.GetBoolItems(SysOrgCopyInput.GetType(), nameof(SysOrgCopyInput.ContainsPosition));
        var items = (await SysOrgService.SelectorAsync());
        Items = OrgUtil.BuildTreeIdItemList(items, new List<long> { SysOrgCopyInput.TargetId });
        await base.OnInitializedAsync();
    }
    private long Key { get; set; }
    private Task CleanParentId()
    {
        SysOrgCopyInput.TargetId = 0;
        Key = CommonUtils.GetSingleId();
        return Task.CompletedTask;
    }
}
