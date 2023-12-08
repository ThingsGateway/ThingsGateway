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

using Furion.DependencyInjection;
using Furion.SpecificationDocument;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Admin.ApiController;

/// <summary>
/// Swagger登录授权服务
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayAdmin, Order = 200)]
[Route("Swagger")]
public class SwaggerController : IDynamicApiController, IScoped
{
    private readonly IConfigService _configService;

    /// <summary>
    /// <inheritdoc cref="SwaggerController"/>
    /// </summary>
    /// <param name="sysConfigService"></param>
    public SwaggerController(IConfigService sysConfigService)
    {
        _configService = sysConfigService;
    }

    /// <summary>
    /// Swagger登录检查
    /// </summary>
    /// <returns></returns>
    [HttpPost("CheckUrl")]
    [AllowAnonymous, NonUnify]
    public async Task<int> SwaggerCheckUrlAsync()
    {
        var enable = (await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_SWAGGERLOGIN_OPEN)).ConfigValue.ToBool(false);
        return enable ? 401 : 200;
    }

    /// <summary>
    /// Swagger登录
    /// </summary>
    /// <param name="auth"></param>
    /// <returns></returns>
    [HttpPost("SubmitUrl")]
    [AllowAnonymous, NonUnify]
    public async Task<int> SwaggerSubmitUrlAsync([FromForm] SpecificationAuth auth)
    {
        var userName = (await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_SWAGGER_NAME)).ConfigValue;
        var password = (await _configService.GetByConfigKeyAsync(ConfigConst.SYS_CONFIGBASEDEFAULT, ConfigConst.CONFIG_SWAGGER_PASSWORD)).ConfigValue;
        if (auth.UserName == userName && auth.Password == password)
        {
            return 200;
        }
        return 401;
    }
}