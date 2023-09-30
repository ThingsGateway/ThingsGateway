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

using Furion.DynamicApiController;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using SqlSugar;

using System.ComponentModel;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 采集状态信息
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayOpenApi, Order = 200)]
[Route("openApi/collectInfo")]
[Description("获取采集信息")]
[OpenApiPermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CollectInfoControler : IDynamicApiController
{
    readonly IServiceScopeFactory _scopeFactory;
    /// <inheritdoc cref="CollectInfoControler"/>
    public CollectInfoControler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        var serviceScope = _scopeFactory.CreateScope();
        _globalDeviceData = serviceScope.ServiceProvider.GetService<GlobalDeviceData>();
        _alarmHostService = BackgroundServiceUtil.GetBackgroundService<AlarmWorker>();
    }

    AlarmWorker _alarmHostService { get; set; }
    GlobalDeviceData _globalDeviceData { get; set; }
    /// <summary>
    /// 获取设备信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("collectDeviceList")]
    [Description("获取设备信息")]
    public List<DeviceData> GetCollectDeviceList()
    {
        return _globalDeviceData.CollectDevices.Adapt<List<DeviceData>>();
    }

    /// <summary>
    /// 获取变量信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("collectVariableList")]
    [Description("获取变量信息")]
    public Task<SqlSugarPagedList<VariableData>> GetDeviceVariableList([FromQuery] VariablePageInput input)
    {
        var data = _globalDeviceData.AllVariables
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name == input.Name)
            .WhereIF(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName == input.DeviceName)
            .ToPagedList(input);
        return Task.FromResult(data.Adapt<SqlSugarPagedList<VariableData>>());
    }

    /// <summary>
    /// 获取实时报警信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("realAlarmList")]
    [Description("获取实时报警信息")]
    public Task<SqlSugarPagedList<VariableData>> GetRealAlarmList([FromQuery] VariablePageInput input)
    {
        var data = _alarmHostService.RealAlarmDeviceVariables
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name == input.Name)
            .WhereIF(!input.DeviceName.IsNullOrEmpty(), a => a.DeviceName == input.DeviceName)
            .ToPagedList(input);
        return Task.FromResult(data.Adapt<SqlSugarPagedList<VariableData>>());
    }

}

