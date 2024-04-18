﻿//------------------------------------------------------------------------------
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

using NewLife.Extension;

using SqlSugar;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 采集状态信息
/// </summary>
[Route("openApi/runtimeInfo")]
[RolePermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class RuntimeInfoControler : ControllerBase
{
    /// <summary>
    /// 获取设备信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("deviceList")]
    public List<DeviceData> GetCollectDeviceList()
    {
        return GlobalData.CollectDevices.Values.Adapt<List<DeviceData>>();
    }

    /// <summary>
    /// 获取变量信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("variableList")]
    public async Task<SqlSugarPagedList<VariableData>> GetVariableList([FromQuery] VariablePageInput input)
    {
        var data = GlobalData.ReadOnlyVariables.Values
            .WhereIF(!input.Name.IsNullOrWhiteSpace(), a => a.Name == input.Name)
            .WhereIF(input.DeviceId != null, a => a.DeviceId == input.DeviceId)
            .WhereIF(!input.RegisterAddress.IsNullOrWhiteSpace(), a => a.RegisterAddress == input.RegisterAddress)
            .ToPagedList(input);
        return await Task.FromResult(data.Adapt<SqlSugarPagedList<VariableData>>());
    }

    /// <summary>
    /// 获取实时报警信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("realAlarmList")]
    public async Task<SqlSugarPagedList<VariableData>> GetRealAlarmList([FromQuery] VariablePageInput input)
    {
        var data = GlobalData.ReadOnlyRealAlarmVariables
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name == input.Name)
            .WhereIF(input.DeviceId != null, a => a.DeviceId == input.DeviceId)
            .ToPagedList(input);
        return await Task.FromResult(data.Adapt<SqlSugarPagedList<VariableData>>());
    }
}