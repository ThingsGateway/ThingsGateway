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

using Furion;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using System.ComponentModel;

namespace ThingsGateway.Gateway.ApiController;

/// <summary>
/// 设备控制
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayOpenApi, Order = 200)]
[Route("openApi/rpc")]
[Description("变量写入")]
[OpenApiPermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class RpcControler : IDynamicApiController
{
    private readonly IServiceScope _serviceScope;

    /// <inheritdoc cref="RpcControler"/>
    public RpcControler(IServiceScopeFactory scopeFactory)
    {
        _serviceScope = scopeFactory.CreateScope();
        _rpcSingletonService = _serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        CollectDeviceHostService = BackgroundServiceUtil.GetBackgroundService<CollectDeviceWorker>();
    }

    private RpcSingletonService _rpcSingletonService { get; set; }
    private CollectDeviceWorker CollectDeviceHostService { get; set; }

    /// <summary>
    /// 控制采集线程启停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pasueThread")]
    [Description("控制采集线程启停")]
    public void PasueThread(long id, bool isStart)
    {
        CollectDeviceHostService.PasueThread(id, isStart);
    }

    /// <summary>
    /// 重启采集线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("upDeviceThread")]
    [Description("重启采集线程")]
    public async Task UpDeviceThread(long id)
    {
        if (id <= 0)
        {
            await CollectDeviceHostService.RestartDeviceThreadAsync();
        }
        else
        {
            await CollectDeviceHostService.UpDeviceThreadAsync(id, true);
        }
    }

    /// <summary>
    /// 写入多个变量
    /// </summary>
    [HttpPost("writeVariables")]
    [Description("写入变量")]
    public async Task<Dictionary<string, OperResult>> WriteDeviceMethods(Dictionary<string, string> objs)
    {
        var result = await _rpcSingletonService.InvokeDeviceMethodAsync($"WebApi-{UserManager.UserAccount}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}", objs);
        return result;
    }
}