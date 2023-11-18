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

using SqlSugar;



namespace ThingsGateway.Admin.Blazor;
/// <summary>
/// 角色页面
/// </summary>
public partial class Role
{
    private readonly RolePageInput _search = new();
    private List<UserSelectorOutput> _allUsers;
    private long _choiceRoleId;
    private IAppDataTable _datatable;
    private bool _isShowResuorces;
    private bool _isShowUsers;
    private List<RoleGrantResourceMenu> _resTreeSelectors = new();
    private List<RelationRoleResuorce> _roleHasResuorces = new();
    private List<UserSelectorOutput> _usersChoice;

    [CascadingParameter]
    private MainLayout _mainLayout { get; set; }
    private string _searchKey { get; set; }

    private async Task AddCallAsync(RoleAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<RoleService>().AddAsync(input);
    }
    private async Task DeleteCallAsync(IEnumerable<SysRole> sysRoles)
    {
        await _serviceScope.ServiceProvider.GetService<RoleService>().DeleteAsync(sysRoles.Select(a => a.Id).ToArray());
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(RoleEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<RoleService>().EditAsync(input);
        await _mainLayout.StateHasChangedAsync();
    }
    private async Task OnRoleHasResuorcesSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            GrantResourceInput userGrantRoleInput = new();
            var data = new List<SysResource>();
            userGrantRoleInput.Id = _choiceRoleId;
            userGrantRoleInput.GrantInfoList = _roleHasResuorces;
            await _serviceScope.ServiceProvider.GetService<RoleService>().GrantResourceAsync(userGrantRoleInput);
            _isShowResuorces = false;
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        await _mainLayout.StateHasChangedAsync();
    }
    private async Task OnUsersSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            GrantUserInput userGrantRoleInput = new();
            userGrantRoleInput.Id = _choiceRoleId;
            userGrantRoleInput.GrantInfoList = _usersChoice.Select(it => it.Id).ToList();
            await _serviceScope.ServiceProvider.GetService<RoleService>().GrantUserAsync(userGrantRoleInput);
            _isShowUsers = false;
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
        await _mainLayout.StateHasChangedAsync();
    }

    private async Task<ISqlSugarPagedList<SysRole>> QueryCallAsync(RolePageInput input)
    {
        return await _serviceScope.ServiceProvider.GetService<RoleService>().PageAsync(input);
    }

    private async Task ResuorceInitAsync()
    {
        _resTreeSelectors = (await _serviceScope.ServiceProvider.GetService<ResourceService>().GetRoleGrantResourceMenusAsync());
        _roleHasResuorces = (await _serviceScope.ServiceProvider.GetService<RoleService>().OwnResourceAsync(_choiceRoleId))?.GrantInfoList;
    }

    private async Task<List<UserSelectorOutput>> UserInitAsync()
    {
        _allUsers = await _serviceScope.ServiceProvider.GetService<SysUserService>().UserSelectorAsync(_searchKey);
        var data = await _serviceScope.ServiceProvider.GetService<RoleService>().OwnUserAsync(_choiceRoleId);
        _usersChoice = _allUsers.Where(a => data.Contains(a.Id)).ToList();
        return _allUsers;
    }
}