using Furion.DynamicApiController;

using Microsoft.AspNetCore.Mvc;

using ThingsGateway.Application.Services.Auth;

namespace ThingsGateway.Web.Entry.Controllers
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
        private readonly OpenApiAuthService _authService;

        public OpenApiAuthController(OpenApiAuthService authService)
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
        public async Task<LoginOpenApiOutPut> LoginLoginOpenApi(LoginOpenApiInput input)
        {
            return await _authService.LoginOpenApi(input);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost("loginOut")]
        [Description(EventSubscriberConst.LoginOutOpenApi)]
        public async Task LoginOut()
        {
            await _authService.LoginOut();
        }
    }
}