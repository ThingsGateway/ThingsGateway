﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备控制
/// </summary>
[ApiDescriptionSettings("ThingsGateway.OpenApi", Order = 200)]
[Route("openApi/control")]
[RolePermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ControlController : ControllerBase
{
    private ISysUserService _sysUserService;
    public ControlController(IRpcService rpcService, ISysUserService sysUserService)
    {
        _sysUserService = sysUserService;
        _rpcService = rpcService;
    }

    private IRpcService _rpcService { get; set; }


    /// <summary>
    /// 清空全部缓存
    /// </summary>
    /// <returns></returns>
    [HttpPost("removeAllCache")]
    [DisplayName("清空全部缓存")]
    public void RemoveAllCache()
    {
        App.CacheService.Clear();
    }

    /// <summary>
    /// 删除通道/设备缓存
    /// </summary>
    /// <returns></returns>
    [HttpPost("removeCache")]
    [DisplayName("删除通道/设备缓存")]
    public void RemoveCache()
    {
        App.GetService<IDeviceService>().DeleteDeviceFromCache();
        App.GetService<IChannelService>().DeleteChannelFromCache();
    }


    /// <summary>
    /// 控制业务线程启停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pauseBusinessThread")]
    [DisplayName("控制业务线程启停")]
    public async Task PauseBusinessThread(long id, bool isStart)
    {
        var data = GlobalData.BusinessDevices.FirstOrDefault(a => a.Value.Id == id).Value;
        if (data != null)
            await _sysUserService.CheckApiDataScopeAsync(data.CreateOrgId, data.CreateUserId).ConfigureAwait(false);
        GlobalData.BusinessDeviceHostedService.PauseThread(id, isStart);
    }

    /// <summary>
    /// 控制采集线程启停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pauseCollectThread")]
    [DisplayName("控制采集线程启停")]
    public async Task PauseCollectThread(long id, bool isStart)
    {
        var data = GlobalData.CollectDevices.FirstOrDefault(a => a.Value.Id == id).Value;
        if (data != null)
            await _sysUserService.CheckApiDataScopeAsync(data.CreateOrgId, data.CreateUserId).ConfigureAwait(false);
        GlobalData.CollectDeviceHostedService.PauseThread(id, isStart);
    }

    /// <summary>
    /// 重启业务线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartBusinessThread")]
    [DisplayName("重启业务线程")]
    public async Task RestartBusinessDeviceThread(long id)
    {
        if (id <= 0)
        {
            await GlobalData.BusinessDeviceHostedService.RestartAsync().ConfigureAwait(false);
        }
        else
        {
            var data = GlobalData.BusinessDevices.FirstOrDefault(a => a.Value.Id == id).Value;
            if (data != null)
                await _sysUserService.CheckApiDataScopeAsync(data.CreateOrgId, data.CreateUserId).ConfigureAwait(false);
            await GlobalData.BusinessDeviceHostedService.RestartChannelThreadAsync(id, true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 重启采集线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartCollectThread")]
    [DisplayName("重启采集线程")]
    public async Task RestartCollectDeviceThread(long id)
    {
        if (id <= 0)
        {
            await GlobalData.CollectDeviceHostedService.RestartAsync().ConfigureAwait(false);
        }
        else
        {
            var data = GlobalData.CollectDevices.FirstOrDefault(a => a.Value.Id == id).Value;
            if (data != null)
                await _sysUserService.CheckApiDataScopeAsync(data.CreateOrgId, data.CreateUserId).ConfigureAwait(false);
            await GlobalData.CollectDeviceHostedService.RestartChannelThreadAsync(id, true).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 写入多个变量
    /// </summary>
    [HttpPost("writeVariables")]
    [DisplayName("写入变量")]
    public async Task<Dictionary<string, OperResult>> WriteDeviceMethods(Dictionary<string, string> objs)
    {

        var data = GlobalData.ReadOnlyVariables.Where(a => objs.ContainsKey(a.Key));
        if (data != null)
            await _sysUserService.CheckApiDataScopeAsync(data.Select(a => a.Value.CreateOrgId), data.Select(a => a.Value.CreateUserId)).ConfigureAwait(false);

        return await _rpcService.InvokeDeviceMethodAsync($"WebApi-{UserManager.UserAccount}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}", objs).ConfigureAwait(false);

    }
}
