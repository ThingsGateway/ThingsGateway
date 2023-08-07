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

using Furion.DynamicApiController;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel;

namespace ThingsGateway.Admin.Application
{
    /// <summary>
    /// OpenApi登录控制器
    /// </summary>
    [ApiDescriptionSettings(CateGoryConst.ThingsGatewayOpenApi, Order = 200)]
    [Route("auth/openapi")]
    [LoggingMonitor]
    [Description("OpenApi登录")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OpenApiAuthController : IDynamicApiController
    {
        private readonly IOpenApiAuthService _authService;
        /// <summary>
        /// <inheritdoc cref="OpenApiAuthController"/>
        /// </summary>
        /// <param name="authService"></param>
        public OpenApiAuthController(IOpenApiAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// OpenApi登录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [Description(EventSubscriberConst.LoginOpenApi)]
        public async Task<LoginOpenApiOutput> LoginOpenApiAsync(LoginOpenApiInput input)
        {
            return await _authService.LoginOpenApiAsync(input);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost("logout")]
        [Description(EventSubscriberConst.LogoutOpenApi)]
        public async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
        }
    }
}