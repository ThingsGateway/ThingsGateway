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
using Furion.SpecificationDocument;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ThingsGateway.Application
{
    /// <summary>
    /// 系统登录授权服务
    /// </summary>
    [ApiDescriptionSettings(CateGoryConst.ThingsGatewayCore, Order = 200)]
    [Route("Swagger")]
    public class SwaggerController : IDynamicApiController, IScoped
    {
        private readonly IMemoryCache _cache;
        private readonly ConfigService _configService;
        /// <summary>
        /// <inheritdoc cref="SwaggerController"/>
        /// </summary>
        /// <param name="sysConfigService"></param>
        /// <param name="cache"></param>
        public SwaggerController(ConfigService sysConfigService,
            IMemoryCache cache)
        {
            _cache = cache;
            _configService = sysConfigService;
        }

        /// <summary>
        /// Swagger登录检查
        /// </summary>
        /// <returns></returns>
        [HttpPost("CheckUrl")]
        [AllowAnonymous, NonUnify]
        public int SwaggerCheckUrl()
        {
            return _cache.Get<bool>(CacheConst.SwaggerLogin) ? 200 : 401;
        }

        /// <summary>
        /// Swagger登录
        /// </summary>
        /// <param name="auth"></param>
        /// <returns></returns>
        [HttpPost("SubmitUrl")]
        [AllowAnonymous, NonUnify]
        public async Task<int> SwaggerSubmitUrl([FromForm] SpecificationAuth auth)
        {
            var userName = (await _configService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_SWAGGER_NAME)).ConfigValue;
            var password = (await _configService.GetByConfigKey(CateGoryConst.Config_SYS_BASE, DevConfigConst.SYS_DEFAULT_SWAGGER_PASSWORD)).ConfigValue;
            if (auth.UserName == userName && auth.Password == password)
            {
                _cache.Set<bool>(CacheConst.SwaggerLogin, true);
                return 200;
            }
            return 401;
        }
    }
}