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

using Microsoft.AspNetCore.Mvc;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Core;
using ThingsGateway.Application;

namespace ThingsGateway.ApiController;

/// <summary>
/// 后台登录控制器
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayAdmin, Order = 200)]
[Route("gatewayFile")]
[LoggingMonitor]
public class FileController : IDynamicApiController
{
    private readonly IRpcLogService _rpcLogService;
    private readonly IBackendLogService _backendLogService;
    private readonly ICollectDeviceService _collectDeviceService;
    private readonly IVariableService _variableService;
    private readonly IUploadDeviceService _uploadDeviceService;

    /// <summary>
    /// <inheritdoc cref="FileController"/>
    /// </summary>
    public FileController(
        IRpcLogService rpcLogService,
        IBackendLogService backendLogService,
        ICollectDeviceService collectDeviceService,
        IUploadDeviceService uploadDeviceService,
        IVariableService variableService
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
    /// 下载RPC日志
    /// </summary>
    /// <returns></returns>
    [HttpGet("rpcLog")]
    public async Task<IActionResult> DownloadRpcLogAsync([FromQuery] RpcLogInput input)
    {
        var memoryStream = await _rpcLogService.ExportFileAsync(input);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"rpcLog{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }
    /// <summary>
    /// 下载后台日志
    /// </summary>
    /// <returns></returns>
    [HttpGet("backendLog")]
    public async Task<IActionResult> DownloadBackendLogAsync([FromQuery] BackendLogInput input)
    {
        var memoryStream = await _backendLogService.ExportFileAsync(input);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"backendLog{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }


    /// <summary>
    /// 导出采集设备
    /// </summary>
    /// <returns></returns>
    [HttpGet("collectDevice")]
    public async Task<IActionResult> DownloadCollectDeviceAsync([FromQuery] CollectDeviceInput input)
    {
        var memoryStream = await _collectDeviceService.ExportFileAsync(input);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"collectDevice{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }
    /// <summary>
    /// 导出上传设备
    /// </summary>
    /// <returns></returns>
    [HttpGet("uploadDevice")]
    public async Task<IActionResult> DownloadUploadDeviceAsync([FromQuery] UploadDeviceInput input)
    {
        var memoryStream = await _uploadDeviceService.ExportFileAsync(input);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"uploadDevice{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
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
        memoryStream.Seek(0, SeekOrigin.Begin);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"deviceVariable{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }
    /// <summary>
    /// 导出内存变量
    /// </summary>
    /// <returns></returns>
    [HttpGet("memoryVariable")]
    public async Task<IActionResult> DownloadMemoryVariableAsync([FromQuery] MemoryVariableInput input)
    {
        var memoryStream = await _variableService.ExportFileAsync(input);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var data = new FileStreamResult(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = $"memoryVariable{SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat()}.xlsx"
        };
        return data;
    }
}