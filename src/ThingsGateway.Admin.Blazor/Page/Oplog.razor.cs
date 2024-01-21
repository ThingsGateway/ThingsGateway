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

using ThingsGateway.Core;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Blazor;

/// <summary>
/// 操作日志页面
/// </summary>
public partial class Oplog
{
    private readonly OperateLogPageInput _search = new();
    private IAppDataTable _datatable;

    private async Task ClearClickAsync()
    {
        var str = _search.Category == CateGoryConst.Log_OPERATE ? AppService.I18n.T("操作日志") : AppService.I18n.T("异常日志");
        var confirm = await PopupService.OpenConfirmDialogAsync($"{AppService.I18n.T("删除")}   {str}", AppService.I18n.T("确定?"));
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<IOperateLogService>().DeleteAsync(_search.Category);
            await _datatable?.QueryClickAsync();
        }
    }

    private async Task DownExportAsync(bool isAll = false)
    {
        var query = _search?.Adapt<OperateLogInput>();
        query.All = isAll;
        await AppService.DownFileAsync("export/operateLog", DateTime.Now.ToFileDateTimeFormat(), query);
    }

    private async Task<SqlSugarPagedList<SysOperateLog>> QueryCallAsync(OperateLogPageInput input)
    {
        var data = await _serviceScope.ServiceProvider.GetService<IOperateLogService>().PageAsync(input);
        return data;
    }

    protected override async Task OnInitializedAsync()
    {
        await Task.Run(GetOption);
        await base.OnInitializedAsync();
    }

    private object eChartsOption = new();

    private async Task GetOption()
    {
        var dayStatisticsOutputs = await _serviceScope.ServiceProvider.GetService<IOperateLogService>().StatisticsByDayAsync(7);

        eChartsOption = new
        {
            tooltip = new { trigger = "axis" },
            legend = new { data = new[] { AppService.I18n.T("操作"), AppService.I18n.T("异常") }, left = "5%" },
            grid = new { left = "3%", right = "4%", bottom = "3%", containLabel = true },
            toolbox = new { feature = new { saveAsImage = new { } } },
            xAxis = new
            {
                type = "category",
                boundaryGap = false,
                data = dayStatisticsOutputs.Select(a => a.Date).ToArray()
            },
            yAxis = new { type = "value" },
            series = new[]
    {
        new { name = AppService.I18n.T("操作"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.OperateCount).ToArray()
        },
        new { name = AppService.I18n.T("异常"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.ExceptionCount).ToArray()
        },
    }
        };
    }
}