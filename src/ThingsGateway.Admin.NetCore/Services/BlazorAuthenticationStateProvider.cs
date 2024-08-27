//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Authorization;

using System.Security.Claims;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.NetCore;

/// <inheritdoc/>
public class BlazorAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISysDictService _sysDictService;
    private readonly ISysRoleService _sysRoleService;
    private readonly ISysUserService _sysUserService;
    private readonly IVerificatInfoService _verificatInfoService;
    private readonly NetCoreAppService _appService;

    public BlazorAuthenticationStateProvider(IVerificatInfoService verificatInfoService,
        NetCoreAppService netCoreAppService,
        ISysUserService sysUserService, ISysRoleService sysRoleService, ISysDictService sysDictService)
    {
        _sysUserService = sysUserService;
        _sysRoleService = sysRoleService;
        _sysDictService = sysDictService;
        _verificatInfoService = verificatInfoService;
        _appService = netCoreAppService;
    }

    private AuthenticationState currentUser = new AuthenticationState(new ClaimsPrincipal());
    public void UserChanged()
    {
        currentUser = new AuthenticationState(_appService.User);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(currentUser);
    }


}
