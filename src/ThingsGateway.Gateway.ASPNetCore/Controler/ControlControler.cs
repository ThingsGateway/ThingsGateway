//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel;

using ThingsGateway.Admin.Application;
using ThingsGateway.ASPNetCore;
using ThingsGateway.Foundation;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备控制
/// </summary>
[ApiDescriptionSettings("ThingsGateway.OpenApi", Order = 200)]
[Route("openApi/control")]
[RolePermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ControlControler : ControllerBase
{
    public ControlControler(IRpcService rpcService)
    {
        _rpcService = rpcService;
    }

    private IRpcService _rpcService { get; set; }

    /// <summary>
    /// 控制业务线程启停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pasueBusinessThread")]
    [DisplayName("控制业务线程启停")]
    public void PasueBusinessThread(long id, bool isStart)
    {
        HostedServiceUtil.BusinessDeviceHostedService.PasueThread(id, isStart);
    }

    /// <summary>
    /// 控制采集线程启停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pasueCollectThread")]
    [DisplayName("控制采集线程启停")]
    public void PasueCollectThread(long id, bool isStart)
    {
        HostedServiceUtil.CollectDeviceHostedService.PasueThread(id, isStart);
    }

    /// <summary>
    /// 重启业务线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartBusinessThread")]
    [DisplayName("重启业务线程")]
    public Task RestartBusinessDeviceThread(long id)
    {
        if (id <= 0)
        {
            return HostedServiceUtil.BusinessDeviceHostedService.RestartAsync();
        }
        else
        {
            return HostedServiceUtil.BusinessDeviceHostedService.RestartChannelThreadAsync(id, true);
        }
    }

    /// <summary>
    /// 重启采集线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartCollectThread")]
    [DisplayName("重启采集线程")]
    public Task RestartCollectDeviceThread(long id)
    {
        if (id <= 0)
        {
            return HostedServiceUtil.CollectDeviceHostedService.RestartAsync();
        }
        else
        {
            return HostedServiceUtil.CollectDeviceHostedService.RestartChannelThreadAsync(id, true);
        }
    }

    /// <summary>
    /// 写入多个变量
    /// </summary>
    [HttpPost("writeVariables")]
    [DisplayName("写入变量")]
    public Task<Dictionary<string, OperResult>> WriteDeviceMethods(Dictionary<string, string> objs)
    {
        return _rpcService.InvokeDeviceMethodAsync($"WebApi-{UserManager.UserAccount}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}", objs);

    }
}
