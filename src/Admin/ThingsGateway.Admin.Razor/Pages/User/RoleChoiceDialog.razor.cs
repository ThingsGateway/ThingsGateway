//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class RoleChoiceDialog
{
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }

    [Parameter]
    [NotNull]
    public HashSet<long> Values { get; set; }
    private HashSet<SysRole> SelectedRows { get; set; } = new();
    protected override async Task OnInitializedAsync()
    {
        SelectedRows = (await SysRoleService.GetRoleListByIdListAsync(Values)).ToHashSet();
        await base.OnInitializedAsync();
    }
    [Parameter]
    [NotNull]
    public Func<HashSet<long>, Task> ValuesChanged { get; set; }
    private async Task OnChanged(HashSet<SysRole> values)
    {
        SelectedRows = values;
        Values = SelectedRows.Select(a => a.Id).ToHashSet();
        if (ValuesChanged != null)
        {
            await ValuesChanged.Invoke(Values);
        }
    }

    [Inject]
    [NotNull]
    private ISysRoleService? SysRoleService { get; set; }
    private async Task<QueryData<SysRole>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await SysRoleService.PageAsync(options, a => a.Where(b => b.OrgId == OrgId));
        QueryData<SysRole> queryData = data.Adapt<QueryData<SysRole>>();

        return queryData;
    }
    #region 查询
    private long OrgId { get; set; }

    private async Task OrgTreeChangedAsync(long parentId)
    {
        OrgId = parentId;
        await userChoiceTable.QueryAsync();
    }
    #endregion
}
