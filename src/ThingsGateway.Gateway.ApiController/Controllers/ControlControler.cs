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

using ThingsGateway.Admin.Core;
using ThingsGateway.Foundation;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.ApiController;

/// <summary>
/// 设备控制
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayApi, Order = 200)]
[Route("openApi/control")]
[Description("设备控制")]
[RolePermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ControlControler : IDynamicApiController
{
    private readonly IServiceScope _serviceScope;

    /// <inheritdoc cref="ControlControler"/>
    public ControlControler(IServiceScopeFactory scopeFactory)
    {
        _serviceScope = scopeFactory.CreateScope();
        _rpcService = _serviceScope.ServiceProvider.GetService<IRpcService>();
        _collectDeviceWorker = WorkerUtil.GetWoker<CollectDeviceWorker>();
        _businessDeviceWorker = WorkerUtil.GetWoker<BusinessDeviceWorker>();
    }

    private IRpcService _rpcService { get; set; }
    private CollectDeviceWorker _collectDeviceWorker { get; set; }
    private BusinessDeviceWorker _businessDeviceWorker { get; set; }

    /// <summary>
    /// 控制采集线程启停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pasueCollectThread")]
    [Description("控制采集线程启停")]
    public void PasueCollectThread(long id, bool isStart)
    {
        _collectDeviceWorker.PasueThread(id, isStart);
    }

    /// <summary>
    /// 控制业务线程启停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pasueBusinessThread")]
    [Description("控制业务线程启停")]
    public void PasueBusinessThread(long id, bool isStart)
    {
        _businessDeviceWorker.PasueThread(id, isStart);
    }

    /// <summary>
    /// 重启采集线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartCollectThread")]
    [Description("重启采集线程")]
    public async Task RestartCollectDeviceThread(long id)
    {
        if (id <= 0)
        {
            await _collectDeviceWorker.RestartAsync();
        }
        else
        {
            await _collectDeviceWorker.RestartChannelThreadAsync(id, true);
        }
    }

    /// <summary>
    /// 重启业务线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartBusinessThread")]
    [Description("重启业务线程")]
    public async Task RestartBusinessDeviceThread(long id)
    {
        if (id <= 0)
        {
            await _businessDeviceWorker.RestartAsync();
        }
        else
        {
            await _businessDeviceWorker.RestartChannelThreadAsync(id, true);
        }
    }

    /// <summary>
    /// 写入多个变量
    /// </summary>
    [HttpPost("writeVariables")]
    [Description("写入变量")]
    public async Task<Dictionary<string, OperResult>> WriteDeviceMethods(Dictionary<string, string> objs)
    {
        var result = await _rpcService.InvokeDeviceMethodAsync($"WebApi-{UserManager.UserAccount}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}", objs);
        return result;
    }
}