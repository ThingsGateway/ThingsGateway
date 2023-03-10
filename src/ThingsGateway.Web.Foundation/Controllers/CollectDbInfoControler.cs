using Furion.DynamicApiController;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 采集设备
    /// </summary>
    [ApiDescriptionSettings(CateGoryConst.ThingsGatewayOpenApi, Order = 200)]
    [Route("openApi/collectdbInfo")]
    [Description("获取配置信息")]
    [LoggingMonitor]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CollectDbInfoControler : IDynamicApiController
    {
        IServiceScopeFactory _scopeFactory;
        IVariableService _variableService { get; set; }
        public CollectDbInfoControler(IServiceScopeFactory scopeFactory, IVariableService variableService)
        {
            _scopeFactory = scopeFactory;
            _variableService = variableService;
        }

        /// <summary>
        /// 获取变量信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("variableList")]
        [Description("获取变量信息")]
        public async Task<SqlSugarPagedList<CollectDeviceVariable>> GetVariableList([FromQuery] VariablePageInput input)
        {
            var data = await _variableService.Page(input);

            return data;
        }


    }
}

