//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;

using Mapster;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ThingsGateway.Gateway.Application;
/// <summary>
/// 查询条件实体类
/// </summary>
public class QueryPageOptionsDto
{
    /// <summary>
    /// 获得/设置 模糊查询关键字
    /// </summary>
    public string? SearchText { get; set; }

    /// <summary>
    /// 获得 排序字段名称 由 <see cref="Table{TItem}.SortName"/> 设置
    /// </summary>
    public string? SortName { get; set; }

    /// <summary>
    /// 获得 排序方式 由 <see cref="Table{TItem}.SortOrder"/> 设置
    /// </summary>
    public SortOrder SortOrder { get; set; }

    /// <summary>
    /// 获得/设置 多列排序集合 默认为 Empty 内部为 "Name" "Age desc" 由 <see cref="Table{TItem}.SortString"/> 设置
    /// </summary>
    public List<string> SortList { get; } = new(10);

    /// <summary>
    /// 获得/设置 自定义多列排序集合 默认为 Empty 内部为 "Name" "Age desc" 由 <see cref="Table{TItem}.AdvancedSortItems"/> 设置
    /// </summary>
    public List<string> AdvancedSortList { get; } = new(10);

    /// <summary>
    /// 获得 搜索条件绑定模型 未设置 <see cref="Table{TItem}.CustomerSearchModel"/> 时为 <see cref="Table{TItem}"/> 泛型模型
    /// </summary>
    public object? SearchModel { get; set; }

    /// <summary>
    /// 获得 当前页码 首页为 第一页
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 获得 请求读取数据开始行 默认 0
    /// </summary>
    /// <remarks><see cref="Table{TItem}.ScrollMode"/> 开启虚拟滚动 <see cref="ScrollMode.Virtual"/> 时使用</remarks>
    public int StartIndex { get; set; }

    public int PageItems { get; set; } = 20;

    /// <summary>
    /// 获得 是否分页查询模式 默认为 false 由 <see cref="Table{TItem}.IsPagination"/> 设置
    /// </summary>
    public bool IsPage { get; set; }

    /// <summary>
    /// 获得 是否为虚拟滚动查询模式 默认为 false 由 <see cref="Table{TItem}.ScrollMode"/> 设置
    /// </summary>
    public bool IsVirtualScroll { get; set; }

    /// <summary>
    /// 获得 是否为首次查询 默认 false
    /// </summary>
    /// <remarks><see cref="Table{TItem}"/> 组件首次查询数据时为 true</remarks>
    public bool IsFirstQuery { get; set; }
}
public class ExportDto
{
    public QueryPageOptionsDto QueryPageOptions { get; set; } = new();
    public FilterKeyValueAction FilterKeyValueAction { get; set; } = new();
}
/// <summary>
/// 导出文件
/// </summary>
[ApiDescriptionSettings(false)]
[Route("api/gatewayExport")]
[IgnoreRolePermission]
[Authorize]
public class GatewayExportController : ControllerBase
{
    private readonly IChannelService _channelService;
    private readonly IDeviceService _deviceService;
    private readonly IVariableService _variableService;
    private readonly IImportExportService _importExportService;

    public GatewayExportController(
        IChannelService channelService,
        IDeviceService deviceService,
        IVariableService variableService,
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
    [HttpPost("businessdevice")]
    public async Task<IActionResult> DownloadBusinessDeviceAsync([FromBody] ExportDto input)
    {
        input.QueryPageOptions.IsPage = false;
        input.QueryPageOptions.IsVirtualScroll = false;
        var sheets = await _deviceService.ExportDeviceAsync(input.QueryPageOptions.Adapt<QueryPageOptions>(), PluginTypeEnum.Business, input.FilterKeyValueAction).ConfigureAwait(false);
        return await _importExportService.ExportAsync<Device>(sheets, "BusinessDevice", false).ConfigureAwait(false);

    }

    /// <summary>
    /// 下载通道
    /// </summary>
    /// <returns></returns>
    [HttpPost("channel")]
    public async Task<IActionResult> DownloadChannelAsync([FromBody] ExportDto input)
    {
        input.QueryPageOptions.IsPage = false;
        input.QueryPageOptions.IsVirtualScroll = false;

        var sheets = await _channelService.ExportChannelAsync(input.QueryPageOptions.Adapt<QueryPageOptions>(), input.FilterKeyValueAction).ConfigureAwait(false);
        return await _importExportService.ExportAsync<Channel>(sheets, "Channel", false).ConfigureAwait(false);
    }

    /// <summary>
    /// 下载设备
    /// </summary>
    /// <returns></returns>
    [HttpPost("collectdevice")]
    public async Task<IActionResult> DownloadCollectDeviceAsync([FromBody] ExportDto input)
    {
        input.QueryPageOptions.IsPage = false;
        input.QueryPageOptions.IsVirtualScroll = false;

        var sheets = await _deviceService.ExportDeviceAsync(input.QueryPageOptions.Adapt<QueryPageOptions>(), PluginTypeEnum.Collect, input.FilterKeyValueAction).ConfigureAwait(false);
        return await _importExportService.ExportAsync<Device>(sheets, "CollectDevice", false).ConfigureAwait(false);
    }

    /// <summary>
    /// 下载变量
    /// </summary>
    /// <returns></returns>
    [HttpPost("variable")]
    public async Task<IActionResult> DownloadVariableAsync([FromBody] ExportDto input)
    {
        input.QueryPageOptions.IsPage = false;
        input.QueryPageOptions.IsVirtualScroll = false;

        var sheets = await _variableService.ExportVariableAsync(input.QueryPageOptions.Adapt<QueryPageOptions>(), input.FilterKeyValueAction).ConfigureAwait(false);
        return await _importExportService.ExportAsync<Variable>(sheets, "Variable", false).ConfigureAwait(false);
    }
}
