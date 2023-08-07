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
/// 操作日志
/// </summary>
public partial class Oplog
{
    private readonly OperateLogPageInput search = new();
    private IAppDataTable _datatable;
    private List<StringFilters> CategoryFilters { get; set; } = new();
    private List<StringFilters> ExeStatus { get; set; } = new();
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        CategoryFilters.Add(new StringFilters() { Key = "操作", Value = LogConst.LOG_OPERATE });
        CategoryFilters.Add(new StringFilters() { Key = "第三方操作", Value = LogConst.LOG_OPENAPIOPERATE });
        ExeStatus.Add(new StringFilters() { Key = "成功", Value = LogConst.LOG_SUCCESS });
        ExeStatus.Add(new StringFilters() { Key = "失败", Value = LogConst.LOG_FAIL });
    }

    private async Task ClearClick()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync("删除", "确定 ?");
        if (confirm)
        {
            await OperateLogService.DeleteAsync(CategoryFilters.Select(it => it.Value).ToArray());
            await _datatable?.QueryClickAsync();
        }
    }

    private Task<SqlSugarPagedList<SysOperateLog>> QueryCallAsync(OperateLogPageInput input)
    {
        input.Account = search.Account;
        input.Category = search.Category;
        input.ExeStatus = search.ExeStatus;
        return OperateLogService.PageAsync(input);
    }
    [Inject]
    AjaxService AjaxService { get; set; }
    async Task DownExportAsync(OperateLogPageInput input = null)
    {
        try
        {
            await AjaxService.DownFileAsync("file/operateLog", SysDateTimeExtensions.CurrentDateTime.ToFileDateTimeFormat(), input.Adapt<OperateLogInput>());
        }
        finally
        {
        }
    }
}