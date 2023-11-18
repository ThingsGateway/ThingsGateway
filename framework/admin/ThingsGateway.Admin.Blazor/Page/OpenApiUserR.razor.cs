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
/// OpenApiUserR
/// </summary>
public partial class OpenApiUserR
{
    private readonly OpenApiUserPageInput _search = new();
    private IAppDataTable _datatable;
    private List<OpenApiPermissionTreeSelector> _allRouters;
    private long _choiceUserId;
    private bool _isShowRoles;
    private List<OpenApiPermissionTreeSelector> _rolesChoice = new();
    private string _searchName;

    private async Task AddCallAsync(OpenApiUserAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<OpenApiUserService>().AddAsync(input);
    }

    private async Task DeleteCallAsync(IEnumerable<OpenApiUser> users)
    {
        await _serviceScope.ServiceProvider.GetService<OpenApiUserService>().DeleteAsync(users.Select(a => a.Id).ToArray());
    }

    private async Task EditCallAsync(OpenApiUserEditInput users)
    {
        await _serviceScope.ServiceProvider.GetService<OpenApiUserService>().EditAsync(users);
    }

    private List<OpenApiPermissionTreeSelector> GetRouters()
    {
        _allRouters = PermissionUtil.OpenApiPermissionTreeSelector().ToList();
        return _allRouters;
    }

    private async Task OnRolesSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            OpenApiUserGrantPermissionInput userGrantRoleInput = new();
            userGrantRoleInput.Id = _choiceUserId;
            userGrantRoleInput.PermissionList = _rolesChoice.Select(it => it.ApiRoute).ToList();
            await _serviceScope.ServiceProvider.GetService<OpenApiUserService>().GrantRoleAsync(userGrantRoleInput);
            _isShowRoles = false;
            await _datatable?.QueryClickAsync();
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
    }
    private async Task<ISqlSugarPagedList<OpenApiUser>> QueryCallAsync(OpenApiUserPageInput input)
    {
        return await _serviceScope.ServiceProvider.GetService<OpenApiUserService>().PageAsync(input);
    }

    private async Task UserStatusChangeAsync(OpenApiUser context, bool enable)
    {
        try
        {
            if (enable)
                await _serviceScope.ServiceProvider.GetService<OpenApiUserService>().EnableUserAsync(context.Id);
            else
                await _serviceScope.ServiceProvider.GetService<OpenApiUserService>().DisableUserAsync(context.Id);
        }
        finally
        {
            await _datatable?.QueryClickAsync();
        }
    }
}