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

public partial class UserChoiceDialog
{
    [Inject]
    [NotNull]
    public IStringLocalizer<ThingsGateway.Admin.Razor._Imports>? AdminLocalizer { get; set; }

    [Parameter]
    [NotNull]
    public HashSet<long> Values { get; set; }
    private HashSet<UserSelectorOutput> SelectedRows { get; set; } = new();
    protected override async Task OnInitializedAsync()
    {
        SelectedRows = (await SysUserService.GetUserListByIdListAsync(Values)).ToHashSet();
        await base.OnInitializedAsync();
    }
    [Parameter]
    [NotNull]
    public Func<HashSet<long>, Task> ValuesChanged { get; set; }
    private async Task OnChanged(HashSet<UserSelectorOutput> values)
    {
        SelectedRows = values;
        Values = SelectedRows.Select(a => a.Id).ToHashSet();
        if (ValuesChanged != null)
        {
            await ValuesChanged.Invoke(Values);
        }
    }

    [Parameter]
    public int MaxCount { get; set; } = 0;

    [Inject]
    [NotNull]
    private ISysUserService? SysUserService { get; set; }
    private async Task<QueryData<UserSelectorOutput>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await SysUserService.PageAsync(options, new UserSelectorInput()
        {
            RoleId = RoleId,
            OrgId = OrgId,
            PositionId = PositionId,
        });
        QueryData<UserSelectorOutput> queryData = data.Adapt<QueryData<UserSelectorOutput>>();

        return queryData;
    }
    #region 查询
    private long RoleId { get; set; }
    private long OrgId { get; set; }
    private long PositionId { get; set; }
    private async Task RoleTreeChangedAsync(long parentId)
    {
        RoleId = parentId;
        await userChoiceTable.QueryAsync();
    }
    private async Task PositionTreeChangedAsync(long parentId)
    {
        PositionId = parentId;
        await userChoiceTable.QueryAsync();
    }
    private async Task OrgTreeChangedAsync(long parentId)
    {
        OrgId = parentId;
        await userChoiceTable.QueryAsync();
    }
    #endregion
}
