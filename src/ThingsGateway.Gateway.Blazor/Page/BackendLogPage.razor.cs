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
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// BackendLogPage
/// </summary>
public partial class BackendLogPage
{
    private readonly BackendLogPageInput _search = new();
    private IAppDataTable _datatable;

    private async Task ClearClickAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync($"{AppService.I18n.T("删除")}", AppService.I18n.T("确定?"));
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<IBackendLogService>().DeleteAsync();
            await _datatable?.QueryClickAsync();
        }
    }

    private async Task DownExportAsync(bool isAll = false)
    {
        var query = _search?.Adapt<BackendLogInput>();
        query.All = isAll;
        await AppService.DownFileAsync("gatewayExport/backendLog", DateTime.Now.ToFileDateTimeFormat(), query);
    }

    private async Task<SqlSugarPagedList<BackendLog>> QueryCallAsync(BackendLogPageInput input)
    {
        var data = await _serviceScope.ServiceProvider.GetService<IBackendLogService>().PageAsync(input);
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
        var dayStatisticsOutputs = await _serviceScope.ServiceProvider.GetService<IBackendLogService>().StatisticsByDayAsync(7);

        eChartsOption = new
        {
            tooltip = new { trigger = "axis" },
            legend = new { data = new[] { AppService.I18n.T("Debug"), AppService.I18n.T("Info"), AppService.I18n.T("Warn"), AppService.I18n.T("Error") }, left = "5%" },
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
        new { name =AppService.I18n.T( "Debug"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.DebugCount).ToArray()
        },
        new { name = AppService.I18n.T("Info"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.InfoCount).ToArray()
        },
        new { name = AppService.I18n.T("Warn"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.WarningCount).ToArray()
        },
        new { name = AppService.I18n.T("Error"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.ErrorCount).ToArray()
        },
    }
        };
    }
}