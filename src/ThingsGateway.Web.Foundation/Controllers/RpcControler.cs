using Furion.DynamicApiController;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ThingsGateway.Core;
using ThingsGateway.Foundation;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 设备写入
    /// </summary>
    [ApiDescriptionSettings(CateGoryConst.ThingsGatewayOpenApi, Order = 200)]
    [Route("openApi/rpc")]
    [Description("设备写入")]
    [OpenApiPermission]
    [LoggingMonitor]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class RpcControler : IDynamicApiController
    {
        IServiceScopeFactory _scopeFactory;
        RpcCore _rpcCore { get; set; }
        public RpcControler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            using var serviceScope = _scopeFactory.CreateScope();
            _rpcCore = serviceScope.ServiceProvider.GetService<RpcCore>();
        }

        /// <summary>
        /// 写入设备
        /// </summary>
        [HttpPost("writeDeviceMethod")]
        [Description("写入设备")]
        public Task<OperResult> WriteDeviceMethod(NameVaue obj)
        {
            return _rpcCore.InvokeDeviceMethod($"WebApi-{UserManager.UserAccount}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}", obj);
        }

    }
}


