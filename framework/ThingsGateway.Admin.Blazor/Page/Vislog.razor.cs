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

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Blazor.Core;
using ThingsGateway.Admin.Core;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 访问日志页面
/// </summary>
public partial class Vislog
{
    private IAppDataTable _datatable;
    private readonly VisitLogPageInput search = new();
    /// <summary>
    /// 日志分类菜单
    /// </summary>
    public List<StringFilters> CategoryFilters { get; set; } = new();
    /// <summary>
    /// 执行结果菜单
    /// </summary>
    public List<StringFilters> ExeStatus { get; set; } = new();

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        CategoryFilters.Add(new StringFilters() { Key = "登录", Value = LogConst.LOG_LOGIN });
        CategoryFilters.Add(new StringFilters() { Key = "注销", Value = LogConst.LOG_LOGOUT });
        CategoryFilters.Add(new StringFilters() { Key = "第三方登录", Value = LogConst.LOG_OPENAPILOGIN });
        CategoryFilters.Add(new StringFilters() { Key = "第三方注销", Value = LogConst.LOG_OPENAPILOGOUT });
        ExeStatus.Add(new StringFilters() { Key = "成功", Value = LogConst.LOG_SUCCESS });
        ExeStatus.Add(new StringFilters() { Key = "失败", Value = LogConst.LOG_FAIL });
        base.OnInitialized();
    }

    private async Task ClearClickAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("删除", "确定 ?");
        if (confirm)
        {
            await App.GetService<VisitLogService>().DeleteAsync(CategoryFilters.Select(it => it.Value).ToArray());
            await _datatable?.QueryClickAsync();
        }
    }

    private async Task<SqlSugarPagedList<SysVisitLog>> QueryCallAsync(VisitLogPageInput input)
    {
        var data = await App.GetService<VisitLogService>().PageAsync(input);
        return data;
    }
    [Inject]
    AjaxService AjaxService { get; set; }
    async Task DownExportAsync(VisitLogPageInput input = null)
    {
        try
        {
            await AjaxService.DownFileAsync("file/visitLog", SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<VisitLogInput>());
        }
        finally
        {
        }
    }
}