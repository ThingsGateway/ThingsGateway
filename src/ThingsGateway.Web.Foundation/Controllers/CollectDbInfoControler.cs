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

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 采集设备
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayOpenApi, Order = 200)]
[Route("openApi/collectdbInfo")]
[Description("获取配置信息")]
[LoggingMonitor]
[Authorize(AuthenticationSchemes = "Bearer")]
public class CollectDbInfoControler : IDynamicApiController
{
    IServiceScopeFactory _scopeFactory;
    /// <inheritdoc cref="CollectDbInfoControler"/>
    public CollectDbInfoControler(IServiceScopeFactory scopeFactory, IVariableService variableService, ICollectDeviceService collectDeviceService)
    {
        _scopeFactory = scopeFactory;
        _variableService = variableService;
        _collectDeviceService = collectDeviceService;
    }

    IVariableService _variableService { get; set; }
    ICollectDeviceService _collectDeviceService { get; set; }
    /// <summary>
    /// 获取采集设备信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("collectDeviceList")]
    [Description("获取采集设备信息")]
    public async Task<SqlSugarPagedList<CollectDevice>> GetCollectDeviceList([FromQuery] CollectDevicePageInput input)
    {
        var data = await _collectDeviceService.PageAsync(input);
        return data;
    }

    /// <summary>
    /// 获取变量信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("variableList")]
    [Description("获取变量信息")]
    public async Task<SqlSugarPagedList<CollectDeviceVariable>> GetVariableList([FromQuery] VariablePageInput input)
    {
        var data = await _variableService.PageAsync(input);
        return data;
    }
}

