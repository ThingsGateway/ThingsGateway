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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel;

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.ApiController;

/// <summary>
/// 后台登录控制器
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayAdmin, Order = 200)]
[Route("auth")]
[LoggingMonitor]
public class AuthController : IDynamicApiController
{
    private readonly IAuthService _authService;

    /// <summary>
    /// <inheritdoc cref="AuthController"/>
    /// </summary>
    /// <param name="authService"></param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// 后台登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [Description(EventSubscriberConst.Login)]
    public async Task<LoginOutput> LoginAsync(LoginInput input)
    {
        return await _authService.LoginAsync(input);
    }

    /// <summary>
    /// 后台登出
    /// </summary>
    /// <returns></returns>
    [HttpPost("logout")]
    [Description(EventSubscriberConst.LoginOut)]
    [Authorize]
    [IgnoreRolePermission]
    public async Task LogoutAsync(LoginOutIput input)
    {
        await _authService.LoginOutAsync(input.VerificatId);
    }
}