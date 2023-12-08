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

using Microsoft.AspNetCore.Mvc;

namespace ThingsGateway.Gateway.ApiController;

/// <summary>
/// 文件下载
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayAdmin, Order = 200)]
[Route("gatewayFile")]
[LoggingMonitor]
public class FileController : IDynamicApiController
{
    private readonly IBackendLogService _backendLogService;
    private readonly ICollectDeviceService _collectDeviceService;
    private readonly IRpcLogService _rpcLogService;
    private readonly IUploadDeviceService _uploadDeviceService;
    private readonly VariableService _variableService;

    /// <summary>
    /// <inheritdoc cref="FileController"/>
    /// </summary>
    public FileController(
        IRpcLogService rpcLogService,
        IBackendLogService backendLogService,
        ICollectDeviceService collectDeviceService,
        IUploadDeviceService uploadDeviceService,
        VariableService variableService
        )
    {
        _rpcLogService = rpcLogService;
        _backendLogService = backendLogService;
        _collectDeviceService = collectDeviceService;
        _uploadDeviceService = uploadDeviceService;
        _variableService = variableService;
        _variableService = variableService;
    }

    /// <summary>
    /// 下载后台日志
    /// </summary>
    /// <returns></returns>
    [HttpGet("backendLog")]
    public async Task<IActionResult> DownloadBackendLogAsync([FromQuery] BackendLogInput input)
    {
        var memoryStream = await _backendLogService.ExportFileAsync(input);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"backendLog{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }

    /// <summary>
    /// 导出采集设备
    /// </summary>
    /// <returns></returns>
    [HttpGet("collectDevice")]
    public async Task<IActionResult> DownloadCollectDeviceAsync([FromQuery] DeviceInput input)
    {
        var memoryStream = await _collectDeviceService.ExportFileAsync(input);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"collectDevice{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }

    /// <summary>
    /// 导出采集变量
    /// </summary>
    /// <returns></returns>
    [HttpGet("deviceVariable")]
    public async Task<IActionResult> DownloadDeviceVariableAsync([FromQuery] DeviceVariableInput input)
    {
        var memoryStream = await _variableService.ExportFileAsync(input);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"deviceVariable{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }

    /// <summary>
    /// 下载RPC日志
    /// </summary>
    /// <returns></returns>
    [HttpGet("rpcLog")]
    public async Task<IActionResult> DownloadRpcLogAsync([FromQuery] RpcLogInput input)
    {
        var memoryStream = await _rpcLogService.ExportFileAsync(input);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"rpcLog{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }

    /// <summary>
    /// 导出上传设备
    /// </summary>
    /// <returns></returns>
    [HttpGet("uploadDevice")]
    public async Task<IActionResult> DownloadUploadDeviceAsync([FromQuery] DeviceInput input)
    {
        var memoryStream = await _uploadDeviceService.ExportFileAsync(input);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"uploadDevice{DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }
}