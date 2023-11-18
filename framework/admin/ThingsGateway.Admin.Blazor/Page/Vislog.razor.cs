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
/// 访问日志页面
/// </summary>
public partial class Vislog
{
    private readonly VisitLogPageInput _search = new();
    private IAppDataTable _datatable;
    /// <summary>
    /// 日志分类菜单
    /// </summary>
    private List<StringFilters> _categoryFilters { get; set; } = new();
    /// <summary>
    /// 执行结果菜单
    /// </summary>
    private List<StringFilters> _exeStatus { get; set; } = new();

    [Inject]
    private InitTimezone _initTimezone { get; set; }


    [Inject]
    AjaxService AjaxService { get; set; }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _categoryFilters.Add(new StringFilters() { Key = "登录", Value = LogConst.LOG_LOGIN });
        _categoryFilters.Add(new StringFilters() { Key = "注销", Value = LogConst.LOG_LOGOUT });
        _categoryFilters.Add(new StringFilters() { Key = "第三方登录", Value = LogConst.LOG_OPENAPILOGIN });
        _categoryFilters.Add(new StringFilters() { Key = "第三方注销", Value = LogConst.LOG_OPENAPILOGOUT });
        _exeStatus.Add(new StringFilters() { Key = "成功", Value = LogConst.LOG_SUCCESS });
        _exeStatus.Add(new StringFilters() { Key = "失败", Value = LogConst.LOG_FAIL });
        base.OnInitialized();
    }

    private async Task ClearClickAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("删除", "确定 ?");
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<VisitLogService>().DeleteAsync(_categoryFilters.Select(it => it.Value).ToArray());
            await _datatable?.QueryClickAsync();
        }
    }

    async Task DownExportAsync(VisitLogPageInput input = null)
    {
        try
        {
            await AjaxService.DownFileAsync("file/visitLog", DateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<VisitLogInput>());
        }
        finally
        {
        }
    }

    private async Task<ISqlSugarPagedList<SysVisitLog>> QueryCallAsync(VisitLogPageInput input)
    {
        var data = await _serviceScope.ServiceProvider.GetService<VisitLogService>().PageAsync(input);
        return data;
    }
}