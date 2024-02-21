//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Blazor;

public partial class User
{
    private readonly UserPageInput _search = new();
    private IAppDataTable _datatable;

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }

    private async Task AddCallAsync(UserAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<ISysUserService>().AddAsync(input);
    }

    private async Task DeleteCallAsync(IEnumerable<SysUser> sysUsers)
    {
        await _serviceScope.ServiceProvider.GetService<ISysUserService>().DeleteAsync(sysUsers.Adapt<List<BaseIdInput>>());
        await MainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(UserEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<ISysUserService>().EditAsync(input);
        await MainLayout.StateHasChangedAsync();
    }

    private Task<SqlSugarPagedList<SysUser>> QueryCallAsync(UserPageInput input)
    {
        return _serviceScope.ServiceProvider.GetService<ISysUserService>().PageAsync(input);
    }

    private async Task UserStatusChangeAsync(SysUser context, bool enable)
    {
        try
        {
            if (enable)
                await _serviceScope.ServiceProvider.GetService<ISysUserService>().EnableUserAsync(context.Id.ToInput());
            else
                await _serviceScope.ServiceProvider.GetService<ISysUserService>().DisableUserAsync(context.Id.ToInput());
        }
        finally
        {
            await _datatable?.QueryClickAsync();
            await MainLayout.StateHasChangedAsync();
        }
    }

    private async Task ResetPasswordAsync(SysUser sysUser)
    {
        await _serviceScope.ServiceProvider.GetService<ISysUserService>().ResetPasswordAsync(sysUser.Id.ToInput());
        await PopupService.EnqueueSnackbarAsync(new(AppService.I18n.T("成功"), AlertTypes.Success));
        await MainLayout.StateHasChangedAsync();
    }

    private async Task GrantRoleAsync(SysUser context)
    {
        var result = (bool?)await PopupService.OpenAsync(typeof(GrantRole), new Dictionary<string, object?>()
        {
            {nameof(GrantRole.UserId),context.Id},
});
        if (result == true)
            await MainLayout.StateHasChangedAsync();
    }

    private async Task GrantResourceAsync(SysUser context)
    {
        var result = (bool?)await PopupService.OpenAsync(typeof(GrantResource), new Dictionary<string, object?>()
        {
            {nameof(GrantResource.Id),context.Id},
            {nameof(GrantApi.IsRole),false}
});
        if (result == true)
            await MainLayout.StateHasChangedAsync();
    }

    private async Task GrantApiAsync(SysUser context)
    {
        var result = (bool?)await PopupService.OpenAsync(typeof(GrantApi), new Dictionary<string, object?>()
        {
            {nameof(GrantApi.Id),context.Id},
            {nameof(GrantApi.IsRole),false}
});
        if (result == true)
            await MainLayout.StateHasChangedAsync();
    }
}