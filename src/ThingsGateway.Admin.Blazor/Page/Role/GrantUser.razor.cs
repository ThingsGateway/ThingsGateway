//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Masa.Blazor;
using Masa.Blazor.Presets;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Blazor;

public partial class GrantUser
{
    private UserSelectorInput _search { get; set; } = new();
    private UserSelectorInput _searchSelector { get; set; } = new();
    private HashSet<UserSelectorOutput> _choice;

    [Parameter]
    public long RoleId { get; set; }

    private Task<SqlSugarPagedList<UserSelectorOutput>> QueryCallAsync(UserSelectorInput input)
    {
        return _serviceScope.ServiceProvider.GetService<ISysUserService>().UserSelectorAsync(input);
    }

    private Task<SqlSugarPagedList<UserSelectorOutput>> QueryCallSelectorAsync(UserSelectorInput input)
    {
        return Task.FromResult(_choice.ToPagedList(input));
    }

    private async Task UserInitAsync()
    {
        var data = await _serviceScope.ServiceProvider.GetService<IRoleService>().OwnUserAsync(RoleId.ToInput());
        var users = await _serviceScope.ServiceProvider.GetService<ISysUserService>().GetUserListByIdListAsync(data.ToInput());
        _choice = users.Where(a => data.Contains(a.Id)).ToHashSet();
    }

    protected override async Task OnInitializedAsync()
    {
        await UserInitAsync();
        await base.OnInitializedAsync();
    }

    private async Task OnUsersSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            GrantUserInput userGrantRoleInput = new();
            userGrantRoleInput.Id = RoleId;
            userGrantRoleInput.GrantInfoList = _choice.Select(it => it.Id).ToList();
            await _serviceScope.ServiceProvider.GetService<IRoleService>().GrantUserAsync(userGrantRoleInput);
            await ClosePopupAsync(true);
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
    }
}