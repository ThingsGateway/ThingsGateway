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

using Microsoft.AspNetCore.Components.Web;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class GrantUserComponent
{
    [Parameter]
    public long RoleId { get; set; }

    [CascadingParameter]
    private Func<Task>? OnCloseAsync { get; set; }

    private List<UserSelectorOutput> SelectedAddRows { get; set; } = new();
    private List<UserSelectorOutput> SelectedDeleteRows { get; set; } = new();
    private HashSet<UserSelectorOutput> SelectedRows { get; set; } = new();

    [Inject]
    [NotNull]
    private ISysRoleService? SysRoleService { get; set; }

    [Inject]
    [NotNull]
    private ISysUserService? SysUserService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var data = await SysRoleService.OwnUserAsync(RoleId);
        var users = await SysUserService.GetUserListByIdListAsync(data);
        SelectedRows = users.Where(a => data.Contains(a.Id)).ToHashSet();
        await base.OnInitializedAsync();
    }

    private async Task<QueryData<UserSelectorOutput>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await SysUserService.PageAsync(options);
        QueryData<UserSelectorOutput> queryData = data.Adapt<QueryData<UserSelectorOutput>>();

        return queryData;
    }

    private async Task OnSave(MouseEventArgs args)
    {
        try
        {
            GrantUserOrRoleInput userGrantRoleInput = new();
            userGrantRoleInput.Id = RoleId;
            userGrantRoleInput.GrantInfoList = SelectedRows.Select(it => it.Id);
            await SysRoleService.GrantUserAsync(userGrantRoleInput);
            if (OnCloseAsync != null)
                await OnCloseAsync();
            await ToastService.Default();
        }
        catch (Exception ex)
        {
            await ToastService.Warning(ex.Message);
        }
    }
}
