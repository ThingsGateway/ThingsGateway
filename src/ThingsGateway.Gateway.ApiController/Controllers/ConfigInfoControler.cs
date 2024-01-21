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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using System.ComponentModel;

using ThingsGateway.Admin.Core;
using ThingsGateway.Core;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.ApiController;

/// <summary>
/// 采集设备
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayApi, Order = 200)]
[Route("openApi/configInfo")]
[Description("获取配置信息")]
[RolePermission]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ConfigInfoControler : IDynamicApiController
{
    private readonly IServiceScope _serviceScope;

    /// <inheritdoc cref="ConfigInfoControler"/>
    public ConfigInfoControler(IServiceScopeFactory scopeFactory,
        IChannelService channelService,
        IVariableService variableService,
        IDeviceService deviceService)
    {
        _serviceScope = scopeFactory.CreateScope();
        _variableService = variableService;
        _collectDeviceService = deviceService;
        _channelService = channelService;
    }

    private IDeviceService _collectDeviceService { get; set; }
    private IVariableService _variableService { get; set; }
    private IChannelService _channelService { get; set; }

    /// <summary>
    /// 获取通道信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("channelList")]
    [Description("获取通道信息")]
    public Task<SqlSugarPagedList<Channel>> GetChannelList([FromQuery] ChannelPageInput input)
    {
        return _channelService.PageAsync(input);
    }

    /// <summary>
    /// 获取设备信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("deviceList")]
    [Description("获取设备信息")]
    public Task<SqlSugarPagedList<Device>> GetCollectDeviceList([FromQuery] DevicePageInput input)
    {
        return _collectDeviceService.PageAsync(input);
    }

    /// <summary>
    /// 获取变量信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("variableList")]
    [Description("获取变量信息")]
    public Task<SqlSugarPagedList<Variable>> GetVariableList([FromQuery] VariablePageInput input)
    {
        return _variableService.PageAsync(input);
    }
}