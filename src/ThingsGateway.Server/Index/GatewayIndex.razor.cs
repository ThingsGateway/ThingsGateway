
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

#pragma warning disable CA2007 // 考虑对等待的任务调用 ConfigureAwait



using BootstrapBlazor.Components;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using System.Diagnostics.CodeAnalysis;

using ThingsGateway.Admin.Application;
using ThingsGateway.Admin.Razor;
using ThingsGateway.Extension;
using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Gateway.Razor;


[Authorize]
[IgnoreRolePermission]
[Route("/")]
[Route("/index")]
public partial class GatewayIndex : IDisposable
{
    #region 曲线

    [Inject]
    private IHardwareJob HardwareJob { get; set; }

    protected override void OnInitialized()
    {
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    public bool Disposed;

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                if (chartInit)
                    await CPULineChart.Update(ChartAction.Update);

                await InvokeAsync(StateHasChanged);
                await Task.Delay(30000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }

    #region CPU曲线

    private bool chartInit { get; set; }
    private Chart CPULineChart { get; set; }
    private ChartDataSource? ChartDataSource { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<HistoryHardwareInfo> HistoryHardwareInfoLocalizer { get; set; }

    private async Task<ChartDataSource> OnCPUInit()
    {
        if (ChartDataSource == null)
        {
            var hisHardwareInfos = await HardwareJob.GetHistoryHardwareInfos();
            ChartDataSource = new ChartDataSource();
            ChartDataSource.Options.Title = Localizer[nameof(HistoryHardwareInfo)];
            ChartDataSource.Options.X.Title = Localizer["DateTime"];
            ChartDataSource.Options.Y.Title = Localizer["Data"];
            ChartDataSource.Labels = hisHardwareInfos.Select(a => a.Date.ToString("dd HH:mm zz"));
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = HistoryHardwareInfoLocalizer[nameof(HistoryHardwareInfo.CpuUsage)],
                Data = hisHardwareInfos.Select(a => (object)a.CpuUsage),
            });
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = HistoryHardwareInfoLocalizer[nameof(HistoryHardwareInfo.MemoryUsage)],
                Data = hisHardwareInfos.Select(a => (object)a.MemoryUsage),
            });

            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = HistoryHardwareInfoLocalizer[nameof(HistoryHardwareInfo.DriveUsage)],
                Data = hisHardwareInfos.Select(a => (object)a.DriveUsage),
            });

            ChartDataSource.Data.Add(new ChartDataset()
            {
                ShowPointStyle = false,
                Tension = 0.4f,
                PointRadius = 1,
                Label = HistoryHardwareInfoLocalizer[nameof(HistoryHardwareInfo.Temperature)],
                Data = hisHardwareInfos.Select(a => (object)a.Temperature),
            });

            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = HistoryHardwareInfoLocalizer[nameof(HistoryHardwareInfo.Battery)],
                Data = hisHardwareInfos.Select(a => (object)a.Battery),
            });
        }
        else
        {
            var hisHardwareInfos = await HardwareJob.GetHistoryHardwareInfos();
            ChartDataSource.Labels = hisHardwareInfos.Select(a => a.Date.ToString("dd HH:mm zz"));
            ChartDataSource.Data[0].Data = hisHardwareInfos.Select(a => (object)a.CpuUsage);
            ChartDataSource.Data[1].Data = hisHardwareInfos.Select(a => (object)a.MemoryUsage);
            ChartDataSource.Data[2].Data = hisHardwareInfos.Select(a => (object)a.DriveUsage);
            ChartDataSource.Data[3].Data = hisHardwareInfos.Select(a => (object)a.Temperature);
            ChartDataSource.Data[4].Data = hisHardwareInfos.Select(a => (object)a.Battery);
        }
        return ChartDataSource;
    }

    #endregion CPU曲线

    #endregion 曲线

    [Inject]
    private BlazorAppContext AppContext { get; set; }

    [Inject]
    private ISysOperateLogService SysOperateLogService { get; set; }

    [Inject]
    private IBackendLogService BackendLogService { get; set; }

    [Inject]
    private IRpcLogService RpcLogService { get; set; }

    [Inject]
    private IStringLocalizer<GatewayIndex> Localizer { get; set; }

    private IEnumerable<TimelineItem>? SysOperateLogItems { get; set; }
    private IEnumerable<TimelineItem>? BackendLogItems { get; set; }
    private IEnumerable<TimelineItem>? RpcLogItems { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var data = await SysOperateLogService.GetNewLog(AppContext.CurrentUser.Account);
        SysOperateLogItems = data.Select(a =>
        {
            return new TimelineItem()
            {
                Content = $"{a.Name}  [IP]  {a.OpIp} [Browser] {a.OpBrowser}",

                Description = a.OpTime.ToDefaultDateTimeFormat()
            };
        });

        var data1 = await BackendLogService.GetNewLog();
        BackendLogItems = data1.Select(a =>
        {
            return new TimelineItem()
            {
                Content = $"{a.LogLevel}  [Msg]  {a.LogMessage}",

                Description = a.LogTime.ToDefaultDateTimeFormat()
            };
        });

        var data2 = await RpcLogService.GetNewLog();
        RpcLogItems = data2.Select(a =>
        {
            return new TimelineItem()
            {
                Content = $"{a.OperateObject}  [Source]  {a.OperateSource} ",

                Description = a.LogTime.ToDefaultDateTimeFormat()
            };
        });

        await base.OnParametersSetAsync();
    }
}

