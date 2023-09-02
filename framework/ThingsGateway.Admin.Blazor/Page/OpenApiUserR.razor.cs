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
/// OpenApiUserR
/// </summary>
public partial class OpenApiUserR
{
    private readonly OpenApiUserPageInput search = new();
    private IAppDataTable _datatable;
    private List<OpenApiPermissionTreeSelector> AllRouters;
    long ChoiceUserId;
    bool IsShowRoles;
    List<OpenApiPermissionTreeSelector> RolesChoice = new();
    string SearchName;

    private Task AddCallAsync(OpenApiUserAddInput input)
    {
        return App.GetService<OpenApiUserService>().AddAsync(input);
    }

    private async Task DeleteCallAsync(IEnumerable<OpenApiUser> users)
    {
        await App.GetService<OpenApiUserService>().DeleteAsync(users.Select(a => a.Id).ToArray());
    }

    private Task EditCallAsync(OpenApiUserEditInput users)
    {
        return App.GetService<OpenApiUserService>().EditAsync(users);
    }

    private List<OpenApiPermissionTreeSelector> GetRouters()
    {
        AllRouters = PermissionUtil.OpenApiPermissionTreeSelector().ToList();
        return AllRouters;
    }

    private async Task OnRolesSaveAsync(ModalActionEventArgs args)
    {
        try
        {
            OpenApiUserGrantPermissionInput userGrantRoleInput = new();
            userGrantRoleInput.Id = ChoiceUserId;
            userGrantRoleInput.PermissionList = RolesChoice.Select(it => it.ApiRoute).ToList();
            await App.GetService<OpenApiUserService>().GrantRoleAsync(userGrantRoleInput);
            IsShowRoles = false;
            await _datatable?.QueryClickAsync();
        }
        catch (Exception ex)
        {
            args.Cancel();
            await PopupService.EnqueueSnackbarAsync(ex, false);
        }
    }
    private Task<SqlSugarPagedList<OpenApiUser>> QueryCallAsync(OpenApiUserPageInput input)
    {
        return App.GetService<OpenApiUserService>().PageAsync(input);
    }

    private async Task UserStatusChangeAsync(OpenApiUser context, bool enable)
    {
        try
        {
            if (enable)
                await App.GetService<OpenApiUserService>().EnableUserAsync(context.Id);
            else
                await App.GetService<OpenApiUserService>().DisableUserAsync(context.Id);
        }
        finally
        {
            await _datatable?.QueryClickAsync();
        }
    }
}