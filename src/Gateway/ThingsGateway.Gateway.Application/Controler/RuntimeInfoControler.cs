﻿//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SqlSugar;

using System.ComponentModel;

using ThingsGateway.NewLife.Extension;
namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 采集状态信息
/// </summary>
[ApiDescriptionSettings("ThingsGateway.OpenApi", Order = 200)]
[DisplayName("数据状态")]
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
    [DisplayName("获取设备信息")]
    public SqlSugarPagedList<DeviceData> GetCollectDeviceList([FromQuery] DevicePageInput input)
    {
        var data = GlobalData.ReadOnlyCollectDevices.Select(a => a.Value)
         .WhereIF(!string.IsNullOrEmpty(input.Name), u => u.Name.Contains(input.Name))
         .WhereIF(input.ChannelId != null, u => u.ChannelId == input.ChannelId)
         .WhereIF(!string.IsNullOrEmpty(input.PluginName), u => u.PluginName == input.PluginName)
         .Where(u => u.PluginType == input.PluginType)
         .ToPagedList(input);
        return data.Adapt<SqlSugarPagedList<DeviceData>>();
    }

    /// <summary>
    /// 获取实时报警信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("realAlarmList")]
    [DisplayName("获取实时报警信息")]
    public SqlSugarPagedList<AlarmVariable> GetRealAlarmList([FromQuery] VariablePageInput input)
    {
        var data = GlobalData.ReadOnlyRealAlarmVariables.Select(a => a.Value)
            .WhereIF(!input.RegisterAddress.IsNullOrEmpty(), a => a.RegisterAddress == input.RegisterAddress)
            .WhereIF(!input.Name.IsNullOrEmpty(), a => a.Name == input.Name)
            .WhereIF(input.DeviceId != null, a => a.DeviceId == input.DeviceId)
            .ToPagedList(input);
        return data.Adapt<SqlSugarPagedList<AlarmVariable>>();
    }
    /// <summary>
    /// 确认实时报警
    /// </summary>
    /// <returns></returns>
    [HttpPost("checkRealAlarm")]
    [DisplayName("确认实时报警")]
    public void CheckRealAlarm(string variableName)
    {
        if (GlobalData.ReadOnlyRealAlarmVariables.TryGetValue(variableName, out var variable))
        {
            GlobalData.AlarmHostedService.ConfirmAlarm(variable);
        }
    }

    /// <summary>
    /// 获取变量信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("variableList")]
    [DisplayName("获取变量信息")]
    public SqlSugarPagedList<VariableData> GetVariableList([FromQuery] VariablePageInput input)
    {
        var data = GlobalData.ReadOnlyVariables.Select(a => a.Value)
            .WhereIF(!input.Name.IsNullOrWhiteSpace(), a => a.Name == input.Name)
            .WhereIF(input.DeviceId != null, a => a.DeviceId == input.DeviceId)
            .WhereIF(!input.RegisterAddress.IsNullOrWhiteSpace(), a => a.RegisterAddress == input.RegisterAddress)
            .ToPagedList(input);
        return data.Adapt<SqlSugarPagedList<VariableData>>();
    }
}
