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
using ThingsGateway.Extension.Generic;
using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Admin.Razor;

public partial class SysRoleEdit
{

    [Inject]
    private IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }


    [Parameter]
    [NotNull]
    public SysRole? Model { get; set; }

    [NotNull]
    private List<TreeViewItem<SysOrg>> Items { get; set; }
    [Inject]
    [NotNull]
    private ISysOrgService? SysOrgService { get; set; }

    private static bool ModelEqualityComparer(SysOrg x, SysOrg y) => x.Id == y.Id;

    private Task OnTreeItemChecked(List<TreeViewItem<SysOrg>> items)
    {
        Model.DefaultDataScope.ScopeDefineOrgIdList = items.Select(a => a.Value.Id).ToList();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private List<SysOrg>? items;
    [Inject]
    private BlazorAppContext AppContext { get; set; }
    protected override async Task OnInitializedAsync()
    {
        items = (await SysOrgService.SelectorAsync());
        Items = OrgUtil.BuildTreeItemList(items, Model.DefaultDataScope.ScopeDefineOrgIdList).ToList();

        OrgItems = OrgUtil.BuildTreeIdItemList(items, new List<long> { Model.OrgId });

        if (!AppContext.CurrentUser.IsGlobal)
            Model.Category = RoleCategoryEnum.Org;
        await base.OnInitializedAsync();
    }

    private string SearchText;
    private async Task<List<TreeViewItem<SysOrg>>> OnClickSearch(string searchText)
    {
        SearchText = searchText;
        var items = (await SysOrgService.SelectorAsync());
        items = items.WhereIf(!searchText.IsNullOrEmpty(), a => a.Name.Contains(searchText)).ToList();
        return OrgUtil.BuildTreeItemList(items, Model.DefaultDataScope.ScopeDefineOrgIdList);
    }
    [NotNull]
    private List<TreeViewItem<long>> OrgItems { get; set; }


}
