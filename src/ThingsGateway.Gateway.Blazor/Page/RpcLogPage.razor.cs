//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Mapster;

using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Core;
using ThingsGateway.Core.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// RpcLogPage
/// </summary>
public partial class RpcLogPage
{
    private readonly RpcLogPageInput _search = new();
    private IAppDataTable _datatable;

    private async Task ClearClickAsync()
    {
        var confirm = await PopupService.OpenConfirmDialogAsync($"{AppService.I18n.T("删除")}", AppService.I18n.T("确定?"));
        if (confirm)
        {
            await _serviceScope.ServiceProvider.GetService<IRpcLogService>().DeleteAsync();
            await _datatable?.QueryClickAsync();
        }
    }

    private async Task DownExportAsync(bool isAll = false)
    {
        var query = _search?.Adapt<RpcLogInput>();
        query.All = isAll;
        await AppService.DownFileAsync("gatewayExport/rpcLog", DateTime.Now.ToFileDateTimeFormat(), query);
    }

    private async Task<SqlSugarPagedList<RpcLog>> QueryCallAsync(RpcLogPageInput input)
    {
        var data = await _serviceScope.ServiceProvider.GetService<IRpcLogService>().PageAsync(input);
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
        var dayStatisticsOutputs = await _serviceScope.ServiceProvider.GetService<IRpcLogService>().StatisticsByDayAsync(7);

        eChartsOption = new
        {
            tooltip = new { trigger = "axis" },
            legend = new { data = new[] { AppService.I18n.T("成功"), AppService.I18n.T("失败") }, left = "5%" },
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
        new { name =AppService.I18n.T( "成功"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.SuccessCount).ToArray()
        },
        new { name = AppService.I18n.T("失败"), type = "line",
                data = dayStatisticsOutputs.Select(a=>a.FailCount).ToArray()
        },
    }
        };
    }
}