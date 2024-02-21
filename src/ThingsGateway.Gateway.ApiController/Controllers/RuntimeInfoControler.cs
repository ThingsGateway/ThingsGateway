//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using System.ComponentModel;

using ThingsGateway.Admin.Core;
using ThingsGateway.Core;
using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 采集状态信息
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayApi, Order = 200)]
[Route("openApi/runtimeInfo")]
[Description("获取运行态信息")]
[RolePermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class RuntimeInfoControler : IDynamicApiController
{
    private readonly IServiceScope _serviceScope;

    /// <inheritdoc cref="RuntimeInfoControler"/>
    public RuntimeInfoControler(IServiceScopeFactory scopeFactory)
    {
        _serviceScope = scopeFactory.CreateScope();
        _globalData = _serviceScope.ServiceProvider.GetService<GlobalData>();
        _alarmHostService = WorkerUtil.GetWoker<AlarmWorker>();
    }

    private AlarmWorker _alarmHostService { get; set; }
    private GlobalData _globalData { get; set; }

    /// <summary>
    /// 获取设备信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("deviceList")]
    [Description("获取设备信息")]
    public List<DeviceData> GetCollectDeviceList()
    {
        return _globalData.CollectDevices.Adapt<List<DeviceData>>();
    }

    /// <summary>
    /// 获取变量信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("variableList")]
    [Description("获取变量信息")]
    public async Task<SqlSugarPagedList<VariableData>> GetDeviceVariableList([FromQuery] VariablePageInput input)
    {
        var data = _globalData.AllVariables
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name == input.Name)
            .WhereIF(input.DeviceId != null, a => a.DeviceId == input.DeviceId)
            .ToPagedList(input);
        return await Task.FromResult(data.Adapt<SqlSugarPagedList<VariableData>>());
    }

    /// <summary>
    /// 获取实时报警信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("realAlarmList")]
    [Description("获取实时报警信息")]
    public async Task<SqlSugarPagedList<VariableData>> GetRealAlarmList([FromQuery] VariablePageInput input)
    {
        var data = _alarmHostService.RealAlarmVariables
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name == input.Name)
            .WhereIF(input.DeviceId != null, a => a.DeviceId == input.DeviceId)
            .ToPagedList(input);
        return await Task.FromResult(data.Adapt<SqlSugarPagedList<VariableData>>());
    }
}