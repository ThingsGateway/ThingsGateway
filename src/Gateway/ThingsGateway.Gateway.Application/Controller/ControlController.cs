//------------------------------------------------------------------------------
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

using ThingsGateway.FriendlyException;

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
    /// 控制设备线程暂停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pauseBusinessThread")]
    [DisplayName("控制设备线程启停")]
    public async Task PauseDeviceThreadAsync(long id, bool pause)
    {
        if (GlobalData.Devices.TryGetValue(id, out var device))
        {
            await GlobalData.SysUserService.CheckApiDataScopeAsync(device.CreateOrgId, device.CreateUserId).ConfigureAwait(false);
            if (device.Driver != null)
            {
                device.Driver.PauseThread(pause);
                return;
            }
        }
        throw Oops.Bah("device not found");
    }
    /// <summary>
    /// 重启全部线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartAllThread")]
    [DisplayName("重启全部线程")]
    public async Task RestartAllThread()
    {
        var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
        await GlobalData.ChannelThreadManage.RestartChannelAsync(data.Select(a => a.Value)).ConfigureAwait(false);
    }

    /// <summary>
    /// 重启设备线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartThread")]
    [DisplayName("重启设备线程")]
    public async Task RestartDeviceThreadAsync(long deviceId)
    {
        if (GlobalData.Devices.TryGetValue(deviceId, out var deviceRuntime))
        {
            await GlobalData.SysUserService.CheckApiDataScopeAsync(deviceRuntime.CreateOrgId, deviceRuntime.CreateUserId).ConfigureAwait(false);
            if (GlobalData.TryGetDeviceThreadManage(deviceRuntime, out var deviceThreadManage))
            {
                await deviceThreadManage.RestartDeviceAsync(deviceRuntime, false).ConfigureAwait(false);
            }
        }
        throw Oops.Bah("device not found");
    }

    /// <summary>
    /// 写入多个变量
    /// </summary>
    [HttpPost("writeVariables")]
    [DisplayName("写入变量")]
    public async Task<Dictionary<string, OperResult>> WriteVariablesAsync(Dictionary<string, string> objs)
    {
        var data = GlobalData.ReadOnlyVariables.Where(a => objs.ContainsKey(a.Key));
        if (data != null)
            await GlobalData.SysUserService.CheckApiDataScopeAsync(data.Select(a => a.Value.CreateOrgId), data.Select(a => a.Value.CreateUserId)).ConfigureAwait(false);

        return await GlobalData.RpcService.InvokeDeviceMethodAsync($"WebApi-{UserManager.UserAccount}-{App.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}", objs).ConfigureAwait(false);

    }
}
