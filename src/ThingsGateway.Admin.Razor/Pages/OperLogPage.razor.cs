//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class OperLogPage
{
    [Inject]
    [NotNull]
    private ISysOperateLogService? SysOperateLogService { get; set; }

    #region 曲线

    private bool chartInit { get; set; }
    private Chart LineChart { get; set; }
    private ChartDataSource? ChartDataSource { get; set; }

    private async Task<ChartDataSource> OnInit()
    {
        if (ChartDataSource == null)
        {
            var dayStatisticsOutputs = await SysOperateLogService.StatisticsByDayAsync(7);
            ChartDataSource = new ChartDataSource();
            ChartDataSource.Options.Title = Localizer[nameof(SysOperateLog)];
            ChartDataSource.Options.X.Title = Localizer["Date"];
            ChartDataSource.Options.Y.Title = Localizer["Count"];
            ChartDataSource.Labels = dayStatisticsOutputs.Select(a => a.Date);
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = Localizer["Operate"],
                Data = dayStatisticsOutputs.Select(a => (object)a.OperateCount),
            });
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = Localizer["Exception"],
                Data = dayStatisticsOutputs.Select(a => (object)a.ExceptionCount),
            });
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = Localizer["Login"],
                Data = dayStatisticsOutputs.Select(a => (object)a.LoginCount),
            });
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = Localizer["Logout"],
                Data = dayStatisticsOutputs.Select(a => (object)a.LogoutCount),
            });
        }
        else
        {
            var dayStatisticsOutputs = await SysOperateLogService.StatisticsByDayAsync(7);
            ChartDataSource.Labels = dayStatisticsOutputs.Select(a => a.Date);
            ChartDataSource.Data[0].Data = dayStatisticsOutputs.Select(a => (object)a.OperateCount);
            ChartDataSource.Data[1].Data = dayStatisticsOutputs.Select(a => (object)a.ExceptionCount);
            ChartDataSource.Data[2].Data = dayStatisticsOutputs.Select(a => (object)a.LoginCount);
            ChartDataSource.Data[3].Data = dayStatisticsOutputs.Select(a => (object)a.LogoutCount);
        }
        return ChartDataSource;
    }

    #endregion 曲线

    #region 查询

    private OperateLogPageInput CustomerSearchModel { get; set; } = new OperateLogPageInput();

    private async Task<QueryData<SysOperateLog>> OnQueryAsync(QueryPageOptions options)
    {
        if (chartInit)
            await LineChart.Update(ChartAction.Update);
        var data = await SysOperateLogService.PageAsync(options);
        return data;
    }

    #endregion 查询

    #region 导出

    [Inject]
    [NotNull]
    private ITableExport? TableExport { get; set; }

    private async Task ExcelExportAsync(ITableExportContext<SysOperateLog> context)
    {
        // 自定义导出模板导出当前页面数据为 Excel 方法
        // 使用 BootstrapBlazor 内置服务 ITableExcelExport 实例方法 ExportAsync 进行导出操作
        // 导出数据使用 context 传递来的 Rows/Columns 即为当前页数据
        var ret = await TableExport.ExportExcelAsync(context.Rows, context.Columns, $"OperLog_{DateTime.Now:yyyyMMddHHmmss}.xlsx");

        // 返回 true 时自动弹出提示框
        await ToastService.Default(ret);
    }

    #endregion 导出
}
