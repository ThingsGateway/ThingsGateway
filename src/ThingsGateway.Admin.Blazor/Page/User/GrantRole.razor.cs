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

public partial class GrantRole
{
    private RoleSelectorInput _search { get; set; } = new();
    private RoleSelectorInput _searchSelector { get; set; } = new();
    private HashSet<SysRole> _choice;

    [Parameter]
    public long UserId { get; set; }

    private Task<SqlSugarPagedList<SysRole>> QueryCallAsync(RoleSelectorInput input)
    {
        return _serviceScope.ServiceProvider.GetService<IRoleService>().RoleSelectorAsync(input);
    }

    private Task<SqlSugarPagedList<SysRole>> QueryCallSelectorAsync(RoleSelectorInput input)
    {
        return Task.FromResult(_choice.ToPagedList(input));
    }

    private async Task UserInitAsync()
    {
        var data = await _serviceScope.ServiceProvider.GetService<ISysUserService>().OwnRoleAsync(UserId.ToInput());
        var users = await _serviceScope.ServiceProvider.GetService<IRoleService>().GetRoleListByIdListAsync(data.ToInput());
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
            UserGrantRoleInput userGrantRoleInput = new();
            userGrantRoleInput.Id = UserId;
            userGrantRoleInput.RoleIdList = _choice.Select(it => it.Id).ToList();
            await _serviceScope.ServiceProvider.GetService<ISysUserService>().GrantRoleAsync(userGrantRoleInput);
            await ClosePopupAsync(true);
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
    }
}