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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using ThingsGateway.Admin.Core;

namespace ThingsGateway.Gateway.ApiController;

/// <summary>
/// 登录控制器
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayApi, Order = 200)]
[Route("openApi/auth")]
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
    /// 登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [Description(EventSubscriberConst.Login)]
    public async Task<LoginOutput> LoginAsync(OpenApiLoginInput input)
    {
        var data = input.Adapt<LoginInput>();
        data.Device = AuthDeviceTypeEnum.Api;
        return await _authService.LoginCoreAsync(data);
    }

    /// <summary>
    /// 登出
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

/// <summary>
/// 登录输入参数
/// </summary>
public class OpenApiLoginInput
{
    /// <summary>
    /// 账号
    ///</summary>
    /// <example>apiAdmin</example>
    [Required(ErrorMessage = "账号不能为空")]
    public string Account { get; set; }

    /// <summary>
    /// 密码，需要SM2加密后传入
    ///</summary>
    ///<example>04F75DE291D453BC1B15DF350B4763FEA20B0E0EF4F9513ADD7E1923F92441F87488A1ADBF9862808916E2DFEEF828A0E3DCE24EE73BAC2EECB05C390C4E51A2F06D13EDEBE2DB30878C5D0EF757D68C37A5E203E7C20F87D1F27979B4A53C90C08AD7AB038C02</example>
    [Required(ErrorMessage = "密码不能为空")]
    public string Password { get; set; }
}