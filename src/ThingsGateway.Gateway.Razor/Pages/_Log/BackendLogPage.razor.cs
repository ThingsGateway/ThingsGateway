
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------


using ThingsGateway.Gateway.Application;
using ThingsGateway.Razor;

namespace ThingsGateway.Gateway.Razor;

public partial class BackendLogPage
{
    [Inject]
    [NotNull]
    private IBackendLogService? BackendLogService { get; set; }

    #region 曲线

    private Chart LineChart { get; set; }
    private ChartDataSource? ChartDataSource { get; set; }

    private async Task<ChartDataSource> OnInit()
    {
        if (ChartDataSource == null)
        {
            var dayStatisticsOutputs = await BackendLogService.StatisticsByDayAsync(7);
            ChartDataSource = new ChartDataSource();
            ChartDataSource.Options.Title = Localizer[nameof(BackendLog)];
            ChartDataSource.Options.X.Title = Localizer["Date"];
            ChartDataSource.Options.Y.Title = Localizer["Count"];
            ChartDataSource.Labels = dayStatisticsOutputs.Select(a => a.Date);
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                Label = Localizer["Debug"],
                Data = dayStatisticsOutputs.Select(a => (object)a.DebugCount),
            });
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                Label = Localizer["Info"],
                Data = dayStatisticsOutputs.Select(a => (object)a.InfoCount),
            });
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                Label = Localizer["Warn"],
                Data = dayStatisticsOutputs.Select(a => (object)a.WarningCount),
            });
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                Label = Localizer["Error"],
                Data = dayStatisticsOutputs.Select(a => (object)a.ErrorCount),
            });
        }
        else
        {
            var dayStatisticsOutputs = await BackendLogService.StatisticsByDayAsync(7);
            ChartDataSource.Labels = dayStatisticsOutputs.Select(a => a.Date);
            ChartDataSource.Data[0].Data = dayStatisticsOutputs.Select(a => (object)a.DebugCount);
            ChartDataSource.Data[1].Data = dayStatisticsOutputs.Select(a => (object)a.InfoCount);
            ChartDataSource.Data[2].Data = dayStatisticsOutputs.Select(a => (object)a.WarningCount);
            ChartDataSource.Data[3].Data = dayStatisticsOutputs.Select(a => (object)a.ErrorCount);
        }
        return ChartDataSource;
    }

    #endregion 曲线

    #region 查询

    private BackendLogPageInput CustomerSearchModel { get; set; } = new BackendLogPageInput();

    private async Task<QueryData<BackendLog>> OnQueryAsync(QueryPageOptions options)
    {
        var data = await BackendLogService.PageAsync(options);
        return data;
    }

    #endregion 查询

    #region 导出

    [Inject]
    [NotNull]
    private ITableExport? TableExport { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<BackendLog> context)
    {
        var ret = await TableExport.ExportExcelAsync(context.Rows, context.Columns, $"BackendLog_{DateTime.Now:yyyyMMddHHmmss}.xlsx");

        // 返回 true 时自动弹出提示框
        await ToastService.Default(ret);
    }

    #endregion 导出
}