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

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 用户界面
/// </summary>
public partial class User
{
    private readonly UserPageInput search = new();
    private IAppDataTable _datatable;
    private List<SysRole> AllRoles;
    long ChoiceUserId;
    bool IsShowRoles;
    List<SysRole> RolesChoice = new();
    string SearchName;
    [CascadingParameter]
    MainLayout MainLayout { get; set; }


    private Task AddCallAsync(UserAddInput input)
    {
        return App.GetService<SysUserService>().AddAsync(input);
    }
    private async Task DeleteCallAsync(IEnumerable<SysUser> users)
    {
        await App.GetService<SysUserService>().DeleteAsync(users.Select(a => a.Id).ToArray());
        await MainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(UserEditInput users)
    {
        await App.GetService<SysUserService>().EditAsync(users);
        await MainLayout.StateHasChangedAsync();
    }

    private async Task OnRolesSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            UserGrantRoleInput userGrantRoleInput = new();
            userGrantRoleInput.Id = ChoiceUserId;
            userGrantRoleInput.RoleIdList = RolesChoice.Select(it => it.Id).ToList();
            await App.GetService<SysUserService>().GrantRoleAsync(userGrantRoleInput);
            IsShowRoles = false;
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        await MainLayout.StateHasChangedAsync();
    }
    private Task<SqlSugarPagedList<SysUser>> QueryCallAsync(UserPageInput input)
    {
        return App.GetService<SysUserService>().PageAsync(input);
    }

    private async Task ResetPasswordAsync(SysUser sysUser)
    {
        await App.GetService<SysUserService>().ResetPasswordAsync(sysUser.Id);
        await PopupService.EnqueueSnackbarAsync(new("成功", AlertTypes.Success));
        await MainLayout.StateHasChangedAsync();
    }

    private async Task RoleInitAsync()
    {
        AllRoles = await App.GetService<RoleService>().RoleSelectorAsync();
        var data = await App.GetService<RoleService>().GetRoleIdListByUserIdAsync(ChoiceUserId);
        RolesChoice = AllRoles.Where(a => data.Contains(a.Id)).ToList();
    }
    private async Task UserStatusChangeAsync(SysUser context, bool enable)
    {
        try
        {
            if (enable)
                await App.GetService<SysUserService>().EnableUserAsync(context.Id);
            else
                await App.GetService<SysUserService>().DisableUserAsync(context.Id);
        }
        finally
        {
            await _datatable?.QueryClickAsync();
            await MainLayout.StateHasChangedAsync();
        }
    }
}