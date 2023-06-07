#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Furion.DynamicApiController;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Threading;

using ThingsGateway.Core;
using ThingsGateway.Foundation;

namespace ThingsGateway.Web.Foundation;

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
    CollectDeviceWorker _collectDeviceHostService { get; set; }
    IServiceScopeFactory _scopeFactory;
    /// <inheritdoc cref="RpcControler"/>
    public RpcControler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        var serviceScope = _scopeFactory.CreateScope();
        _rpcCore = serviceScope.ServiceProvider.GetService<RpcSingletonService>();
        _collectDeviceHostService = serviceScope.GetBackgroundService<CollectDeviceWorker>();
    }

    RpcSingletonService _rpcCore { get; set; }
    /// <summary>
    /// 写入变量
    /// </summary>
    [HttpPost("writeVariable")]
    [Description("写入变量")]
    public Task<OperResult> WriteDeviceMethod(NameValue obj)
    {
        return _rpcCore.InvokeDeviceMethodAsync($"WebApi-{UserManager.UserAccount}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}", obj, CancellationToken.None);
    }
    /// <summary>
    /// 写入多个变量
    /// </summary>
    [HttpPost("writeVariables")]
    [Description("写入多个变量")]
    public async Task<Dictionary<string, OperResult>> WriteDeviceMethods(List<NameValue> objs)
    {
        Dictionary<string, OperResult> operResultDict = new Dictionary<string, OperResult>();
        foreach (var obj in objs)
        {
            var result = await _rpcCore.InvokeDeviceMethodAsync($"WebApi-{UserManager.UserAccount}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}", obj, CancellationToken.None);
            operResultDict.Add(obj.Name, result);
        }
        return operResultDict;
    }

    /// <summary>
    /// 控制采集线程启停
    /// </summary>
    /// <returns></returns>
    [HttpPost("configDeviceThread")]
    [Description("控制采集线程启停")]
    public async Task ConfigDeviceThread(long id, bool isStart)
    {
        await _collectDeviceHostService.ConfigDeviceThreadAsync(id, isStart);
    }
    /// <summary>
    /// 重启采集线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("upDeviceThread")]
    [Description("重启采集线程")]
    public async Task UpDeviceThread(long id)
    {
        if(id<=0)
        {
            await _collectDeviceHostService.UpDeviceThreadAsync(id, false);
        }
        else
        {
            await _collectDeviceHostService.RestartDeviceThreadAsync();
        }
    }

}


