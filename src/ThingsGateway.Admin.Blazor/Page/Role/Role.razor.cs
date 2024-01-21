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

using Mapster;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 角色页面
/// </summary>
public partial class Role
{
    private readonly RolePageInput _search = new();
    private IAppDataTable _datatable;

    [CascadingParameter(Name = "MainLayout")]
    private IMainLayout MainLayout { get; set; }

    private async Task AddCallAsync(RoleAddInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IRoleService>().AddAsync(input);
    }

    private async Task DeleteCallAsync(IEnumerable<SysRole> sysRoles)
    {
        await _serviceScope.ServiceProvider.GetService<IRoleService>().DeleteAsync(sysRoles.Adapt<List<BaseIdInput>>());
        await MainLayout.StateHasChangedAsync();
    }

    private async Task EditCallAsync(RoleEditInput input)
    {
        await _serviceScope.ServiceProvider.GetService<IRoleService>().EditAsync(input);
        await MainLayout.StateHasChangedAsync();
    }

    private Task<SqlSugarPagedList<SysRole>> QueryCallAsync(RolePageInput input)
    {
        return _serviceScope.ServiceProvider.GetService<IRoleService>().PageAsync(input);
    }

    private async Task GrantResourceAsync(SysRole context)
    {
        var result = (bool?)await PopupService.OpenAsync(typeof(GrantResource), new Dictionary<string, object?>()
        {
            {nameof(GrantResource.Id),context.Id},
            {nameof(GrantResource.IsRole),true}
});
        if (result == true)
            await MainLayout.StateHasChangedAsync();
    }

    private async Task GrantUserAsync(SysRole context)
    {
        var result = (bool?)await PopupService.OpenAsync(typeof(GrantUser), new Dictionary<string, object?>()
        {
            {nameof(GrantUser.RoleId),context.Id},
});
        if (result == true)
            await MainLayout.StateHasChangedAsync();
    }

    private async Task GrantApiAsync(SysRole context)
    {
        var result = (bool?)await PopupService.OpenAsync(typeof(GrantApi), new Dictionary<string, object?>()
        {
            {nameof(GrantApi.Id),context.Id},
            {nameof(GrantApi.IsRole),true}
});
        if (result == true)
            await MainLayout.StateHasChangedAsync();
    }
}