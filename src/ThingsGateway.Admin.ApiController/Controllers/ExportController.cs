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

namespace ThingsGateway.Admin.ApiController;

/// <summary>
/// 导出文件
/// </summary>
[ApiDescriptionSettings(CateGoryConst.ThingsGatewayAdmin, Order = 200)]
[Route("export")]
[LoggingMonitor]
public class ExportController : ControllerBase
{
    private readonly IOperateLogService _operateLogService;
    private readonly IVisitLogService _visitLogService;

    /// <summary>
    /// <inheritdoc cref="ExportController"/>
    /// </summary>
    public ExportController(
        IOperateLogService operateLogService,
        IVisitLogService visitLogService
        )
    {
        _operateLogService = operateLogService;
        _visitLogService = visitLogService;
    }

    /// <summary>
    /// 下载操作日志
    /// </summary>
    /// <returns></returns>
    [HttpGet("operateLog")]
    public async Task<IActionResult> DownloadOperateLogAsync([FromQuery] OperateLogInput input)
    {
        if (input.All)
        {
            var fileStreamResult = await _operateLogService.ExportFileAsync();
            return fileStreamResult;
        }
        else
        {
            var fileStreamResult = await _operateLogService.ExportFileAsync(input);
            return fileStreamResult;
        }
    }

    /// <summary>
    /// 下载访问日志
    /// </summary>
    /// <returns></returns>
    [HttpGet("visitLog")]
    public async Task<IActionResult> DownloadVisitLogAsync([FromQuery] VisitLogInput input)
    {
        if (input.All)
        {
            var fileStreamResult = await _visitLogService.ExportFileAsync();
            return fileStreamResult;
        }
        else
        {
            var fileStreamResult = await _visitLogService.ExportFileAsync(input);
            return fileStreamResult;
        }
    }
}