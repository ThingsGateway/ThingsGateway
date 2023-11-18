#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Masa.Blazor;
using Masa.Blazor.Presets;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 用户界面
/// </summary>
public partial class User
{
    private readonly UserPageInput _search = new();
    private List<SysRole> _allRoles;
    private long _choiceUserId;
    private IAppDataTable _datatable;
    private bool _isShowRoles;
    private List<SysRole> _rolesChoice = new();
    private string _searchName;
    [CascadingParameter]
    private MainLayout _mainLayout { get; set; }


    private async Task AddCallAsync(UserAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<SysUserService>().AddAsync(input);
    }
    private async Task DeleteCallAsync(IEnumerable<SysUser> users)
    {
        await _serviceScope.ServiceProvider.GetService<SysUserService>().DeleteAsync(users.Select(a => a.Id).ToArray());
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(UserEditInput users)
    {
        await _serviceScope.ServiceProvider.GetService<SysUserService>().EditAsync(users);
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task OnRolesSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            UserGrantRoleInput userGrantRoleInput = new();
            userGrantRoleInput.Id = _choiceUserId;
            userGrantRoleInput.RoleIdList = _rolesChoice.Select(it => it.Id).ToList();
            await _serviceScope.ServiceProvider.GetService<SysUserService>().GrantRoleAsync(userGrantRoleInput);
            _isShowRoles = false;
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        await _mainLayout.StateHasChangedAsync();
    }
    private async Task<ISqlSugarPagedList<SysUser>> QueryCallAsync(UserPageInput input)
    {
        return await _serviceScope.ServiceProvider.GetService<SysUserService>().PageAsync(input);
    }

    private async Task ResetPasswordAsync(SysUser sysUser)
    {
        await _serviceScope.ServiceProvider.GetService<SysUserService>().ResetPasswordAsync(sysUser.Id);
        await PopupService.EnqueueSnackbarAsync(new("成功", AlertTypes.Success));
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task RoleInitAsync()
    {
        _allRoles = await _serviceScope.ServiceProvider.GetService<RoleService>().RoleSelectorAsync();
        var data = await _serviceScope.ServiceProvider.GetService<RoleService>().GetRoleIdListByUserIdAsync(_choiceUserId);
        _rolesChoice = _allRoles.Where(a => data.Contains(a.Id)).ToList();
    }
    private async Task UserStatusChangeAsync(SysUser context, bool enable)
    {
        try
        {
            if (enable)
                await _serviceScope.ServiceProvider.GetService<SysUserService>().EnableUserAsync(context.Id);
            else
                await _serviceScope.ServiceProvider.GetService<SysUserService>().DisableUserAsync(context.Id);
        }
        finally
        {
            await _datatable?.QueryClickAsync();
            await _mainLayout.StateHasChangedAsync();
        }
    }
}