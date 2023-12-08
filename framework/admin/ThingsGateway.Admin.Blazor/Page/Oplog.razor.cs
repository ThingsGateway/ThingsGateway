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

using Mapster;

using Microsoft.Extensions.DependencyInjection;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 操作日志
/// </summary>
public partial class Oplog
{
    private readonly OperateLogPageInput _search = new();
    private IAppDataTable _datatable;

    [Inject]
    private AjaxService _ajaxService { get; set; }

    private List<StringFilters> _categoryFilters { get; set; } = new();
    private List<StringFilters> _exeStatus { get; set; } = new();

    [Inject]
    private InitTimezone _initTimezone { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _categoryFilters.Add(new StringFilters() { Key = "操作", Value = LogConst.LOG_OPERATE });
        _categoryFilters.Add(new StringFilters() { Key = "第三方操作", Value = LogConst.LOG_OPENAPIOPERATE });
        _exeStatus.Add(new StringFilters() { Key = "成功", Value = LogConst.LOG_SUCCESS });
        _exeStatus.Add(new StringFilters() { Key = "失败", Value = LogConst.LOG_FAIL });
    }

    private async Task ClearClick()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("删除", "确定 ?");
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<IOperateLogService>().DeleteAsync(_categoryFilters.Select(it => it.Value).ToArray());
            await _datatable?.QueryClickAsync();
        }
    }

    private async Task DownExportAsync(OperateLogPageInput input = null)
    {
        try
        {
            await _ajaxService.DownFileAsync("file/operateLog", DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<OperateLogInput>());
        }
        finally
        {
        }
    }

    private async Task<ISqlSugarPagedList<SysOperateLog>> QueryCallAsync(OperateLogPageInput input)
    {
        input.Account = _search.Account;
        input.Category = _search.Category;
        input.ExeStatus = _search.ExeStatus;
        return await _serviceScope.ServiceProvider.GetService<IOperateLogService>().PageAsync(input);
    }
}