//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel;

using ThingsGateway.FriendlyException;

using TouchSocket.Rpc;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 设备控制
/// </summary>
[ApiDescriptionSettings("ThingsGateway.OpenApi", Order = 200)]
[Route("openApi/control")]
[RolePermission]
[RequestAudit]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
[TouchSocket.WebApi.Router("/openApi/control/[action]")]
[TouchSocket.WebApi.EnableCors("cors")]
public class ControlController : ControllerBase, IRpcServer
{

    /// <summary>
    /// 清空全部缓存
    /// </summary>
    /// <returns></returns>
    [HttpPost("removeAllCache")]
    [DisplayName("清空全部缓存")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public void RemoveAllCache()
    {
        App.CacheService.Clear();
    }


    /// <summary>
    /// 控制设备线程暂停
    /// </summary>
    /// <returns></returns>
    [HttpPost("pauseBusinessThread")]
    [DisplayName("控制设备线程启停")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task PauseDeviceThreadAsync(long id, bool pause)
    {
        if (GlobalData.TryGetDeviceRuntime(id, out var device))
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
    /// 重启当前机构线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartScopeThread")]
    [DisplayName("重启当前机构线程")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task RestartScopeThread()
    {
        var data = await GlobalData.GetCurrentUserChannels().ConfigureAwait(false);
        await GlobalData.ChannelRuntimeService.RestartChannelAsync(data).ConfigureAwait(false);
    }

    /// <summary>
    /// 重启全部线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartAllThread")]
    [DisplayName("重启全部线程")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task RestartAllThread()
    {
        await GlobalData.ChannelRuntimeService.RestartChannelAsync(GlobalData.IdChannels.Values).ConfigureAwait(false);
    }

    /// <summary>
    /// 重启设备线程
    /// </summary>
    /// <returns></returns>
    [HttpPost("restartThread")]
    [DisplayName("重启设备线程")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task RestartDeviceThreadAsync(long deviceId)
    {
        if (GlobalData.TryGetDeviceRuntime(deviceId, out var deviceRuntime))
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
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task<Dictionary<string, Dictionary<string, OperResult>>> WriteVariablesAsync([FromBody][TouchSocket.WebApi.FromBody] Dictionary<string, Dictionary<string, string>> deviceDatas)
    {
        await GlobalData.CheckByDeviceNames(deviceDatas.Select(a => a.Key)).ConfigureAwait(false);

        return (await GlobalData.RpcService.InvokeDeviceMethodAsync($"WebApi-{UserManager.UserAccount}-{App.HttpContext?.GetRemoteIpAddressToIPv4()}", deviceDatas).ConfigureAwait(false)).ToDictionary(a => a.Key, a => a.Value.ToDictionary(b => b.Key, b => (OperResult)b.Value));


    }

    /// <summary>
    /// 保存通道
    /// </summary>
    [HttpPost("batchSaveChannel")]
    [DisplayName("保存通道")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> BatchSaveChannelAsync([FromBody][TouchSocket.WebApi.FromBody] List<Channel> channels, ItemChangedType type, bool restart = true)
    {
        return GlobalData.ChannelRuntimeService.BatchSaveChannelAsync(channels, type, restart);
    }

    /// <summary>
    /// 保存设备
    /// </summary>
    [HttpPost("batchSaveDevice")]
    [DisplayName("保存设备")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> BatchSaveDeviceAsync([FromBody][TouchSocket.WebApi.FromBody] List<Device> devices, ItemChangedType type, bool restart = true)
    {
        return GlobalData.DeviceRuntimeService.BatchSaveDeviceAsync(devices, type, restart);
    }

    /// <summary>
    /// 保存变量
    /// </summary>
    [HttpPost("batchSaveVariable")]
    [DisplayName("保存变量")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> BatchSaveVariableAsync([FromBody][TouchSocket.WebApi.FromBody] List<Variable> variables, ItemChangedType type, bool restart = true)
    {
        return GlobalData.VariableRuntimeService.BatchSaveVariableAsync(variables, type, restart);
    }

    /// <summary>
    /// 删除通道
    /// </summary>
    [HttpPost("deleteChannel")]
    [DisplayName("删除通道")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> DeleteChannelAsync([FromBody][TouchSocket.WebApi.FromBody] List<long> ids, bool restart = true)
    {
        if (ids == null || ids.Count == 0) ids = GlobalData.IdChannels.Keys.ToList();
        return GlobalData.ChannelRuntimeService.DeleteChannelAsync(ids, restart);
    }

    /// <summary>
    /// 删除设备
    /// </summary>
    [HttpPost("deleteDevice")]
    [DisplayName("删除设备")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> DeleteDeviceAsync([FromBody][TouchSocket.WebApi.FromBody] List<long> ids, bool restart = true)
    {
        if (ids == null || ids.Count == 0) ids = GlobalData.IdDevices.Keys.ToList();
        return GlobalData.DeviceRuntimeService.DeleteDeviceAsync(ids, restart);
    }

    /// <summary>
    /// 删除变量
    /// </summary>
    [HttpPost("deleteVariable")]
    [DisplayName("删除变量")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task<bool> DeleteVariableAsync([FromBody][TouchSocket.WebApi.FromBody] List<long> ids, bool restart = true)
    {
        if (ids == null || ids.Count == 0) ids = GlobalData.IdVariables.Keys.ToList();
        return GlobalData.VariableRuntimeService.DeleteVariableAsync(ids, restart);
    }

    /// <summary>
    /// 增加测试数据
    /// </summary>
    [HttpPost("insertTestData")]
    [DisplayName("增加测试数据")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task InsertTestDataAsync(int testVariableCount, int testDeviceCount, string slaveUrl, bool businessEnable, bool restart = true)
    {
        return GlobalData.VariableRuntimeService.InsertTestDataAsync(testVariableCount, testDeviceCount, slaveUrl, businessEnable, restart);
    }

    /// <summary>
    /// 增加测试Dtu数据
    /// </summary>
    [HttpPost("insertTestDtuData")]
    [DisplayName("增加测试Dtu数据")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public Task InsertTestDtuDataAsync(int testDeviceCount, string slaveUrl, bool restart = true)
    {
        return GlobalData.VariableRuntimeService.InsertTestDtuDataAsync(testDeviceCount, slaveUrl, restart);
    }

    /// <summary>
    /// 确认实时报警
    /// </summary>
    /// <returns></returns>
    [HttpPost("checkRealAlarm")]
    [RequestAudit]
    [DisplayName("确认实时报警")]
    [TouchSocket.WebApi.WebApi(Method = TouchSocket.WebApi.HttpMethodType.Post)]
    public async Task CheckRealAlarm(long variableId)
    {
        if (GlobalData.ReadOnlyRealAlarmIdVariables.TryGetValue(variableId, out var variable))
        {
            await GlobalData.SysUserService.CheckApiDataScopeAsync(variable.CreateOrgId, variable.CreateUserId).ConfigureAwait(false);
            GlobalData.AlarmHostedService.ConfirmAlarm(variableId);
        }
    }
}
