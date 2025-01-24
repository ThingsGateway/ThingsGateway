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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 导出文件
/// </summary>
[ApiDescriptionSettings(false)]
[Route("api/gatewayExport")]
[IgnoreRolePermission]
[Authorize]
public class GatewayExportController : ControllerBase
{
    private readonly IChannelRuntimeService _channelService;
    private readonly IDeviceRuntimeService _deviceService;
    private readonly IVariableRuntimeService _variableService;
    private readonly IImportExportService _importExportService;

    public GatewayExportController(
        IChannelRuntimeService channelService,
        IDeviceRuntimeService deviceService,
        IVariableRuntimeService variableService,
        IImportExportService importExportService
        )
    {
        _channelService = channelService;
        _deviceService = deviceService;
        _variableService = variableService;
        _importExportService = importExportService;
    }

    /// <summary>
    /// 下载设备
    /// </summary>
    /// <returns></returns>
    [HttpPost("device")]
    public async Task<IActionResult> DownloadDeviceAsync([FromBody] ExportFilter input)
    {
        input.QueryPageOptions.IsPage = false;
        input.QueryPageOptions.IsVirtualScroll = false;
        var sheets = await _deviceService.ExportDeviceAsync(input).ConfigureAwait(false);
        return await _importExportService.ExportAsync<Device>(sheets, "Device", false).ConfigureAwait(false);

    }

    /// <summary>
    /// 下载通道
    /// </summary>
    /// <returns></returns>
    [HttpPost("channel")]
    public async Task<IActionResult> DownloadChannelAsync([FromBody] ExportFilter input)
    {
        input.QueryPageOptions.IsPage = false;
        input.QueryPageOptions.IsVirtualScroll = false;

        var sheets = await _channelService.ExportChannelAsync(input).ConfigureAwait(false);
        return await _importExportService.ExportAsync<Channel>(sheets, "Channel", false).ConfigureAwait(false);
    }


    /// <summary>
    /// 下载变量
    /// </summary>
    /// <returns></returns>
    [HttpPost("variable")]
    public async Task<IActionResult> DownloadVariableAsync([FromBody] ExportFilter input)
    {
        input.QueryPageOptions.IsPage = false;
        input.QueryPageOptions.IsVirtualScroll = false;

        var sheets = await _variableService.ExportVariableAsync(input).ConfigureAwait(false);
        return await _importExportService.ExportAsync<Variable>(sheets, "Variable", false).ConfigureAwait(false);
    }
}
