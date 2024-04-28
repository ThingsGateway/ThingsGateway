
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------



#if Admin

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ThingsGateway.Admin.Application;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Razor;

[Route("/")]
[Route("/index")]
public partial class Index : IDisposable
{
    private HardwareInfoService HardwareInfoService { get; set; }

    protected override void OnInitialized()
    {
        HardwareInfoService = (HardwareInfoService)App.RootServices.GetServices<IHostedService>().FirstOrDefault(it => it is HardwareInfoService)!;
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    public bool Disposed { get; set; }

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
                await Task.Delay((HardwareInfoService.HardwareInfoConfig ?? new()).RealInterval * 1000 + 1000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }

    #region 曲线

    private bool chartInit { get; set; }
    private Chart CPULineChart { get; set; }
    private ChartDataSource? ChartDataSource { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<HisHardwareInfo> HisHardwareInfoLocalizer { get; set; }

    private async Task<ChartDataSource> OnCPUInit()
    {
        if (ChartDataSource == null)
        {
            var hisHardwareInfos = await HardwareInfoService.GetHisHardwareInfos();
            ChartDataSource = new ChartDataSource();
            ChartDataSource.Options.Title = Localizer[nameof(HisHardwareInfo)];
            ChartDataSource.Options.X.Title = Localizer["DateTime"];
            ChartDataSource.Options.Y.Title = Localizer["Data"];
            ChartDataSource.Labels = hisHardwareInfos.Select(a => a.Date.ToString("dd HH:mm zz"));
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = HisHardwareInfoLocalizer[nameof(HisHardwareInfo.CpuUsage)],
                Data = hisHardwareInfos.Select(a => (object)a.CpuUsage),
            });
            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = HisHardwareInfoLocalizer[nameof(HisHardwareInfo.MemoryUsage)],
                Data = hisHardwareInfos.Select(a => (object)a.MemoryUsage),
            });

            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = HisHardwareInfoLocalizer[nameof(HisHardwareInfo.DriveUsage)],
                Data = hisHardwareInfos.Select(a => (object)a.DriveUsage),
            });

            ChartDataSource.Data.Add(new ChartDataset()
            {
                ShowPointStyle = false,
                Tension = 0.4f,
                PointRadius = 1,
                Label = HisHardwareInfoLocalizer[nameof(HisHardwareInfo.Temperature)],
                Data = hisHardwareInfos.Select(a => (object)a.Temperature),
            });

            ChartDataSource.Data.Add(new ChartDataset()
            {
                Tension = 0.4f,
                PointRadius = 1,
                Label = HisHardwareInfoLocalizer[nameof(HisHardwareInfo.Battery)],
                Data = hisHardwareInfos.Select(a => (object)a.Battery),
            });
        }
        else
        {
            var hisHardwareInfos = await HardwareInfoService.GetHisHardwareInfos();
            ChartDataSource.Labels = hisHardwareInfos.Select(a => a.Date.ToString("dd HH:mm zz"));
            ChartDataSource.Data[0].Data = hisHardwareInfos.Select(a => (object)a.CpuUsage);
            ChartDataSource.Data[1].Data = hisHardwareInfos.Select(a => (object)a.MemoryUsage);
            ChartDataSource.Data[2].Data = hisHardwareInfos.Select(a => (object)a.DriveUsage);
            ChartDataSource.Data[3].Data = hisHardwareInfos.Select(a => (object)a.Temperature);
            ChartDataSource.Data[4].Data = hisHardwareInfos.Select(a => (object)a.Battery);
        }
        return ChartDataSource;
    }

    #endregion 曲线

    [Inject]
    private BlazorAppContext AppContext { get; set; }

    [Inject]
    private ISysOperateLogService SysOperateLogService { get; set; }

    [Inject]
    private IStringLocalizer<Index> Localizer { get; set; }

    private IEnumerable<TimelineItem>? Items { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        var data = await SysOperateLogService.GetNewLog(AppContext.CurrentUser.Account);
        Items = data.Select(a =>
        {
            return new TimelineItem()
            {
                Content = $"{a.Name}  [IP]  {a.OpIp} [Browser] {a.OpBrowser}",

                Description = a.OpTime.ToDefaultDateTimeFormat()
            };
        });

        await base.OnParametersSetAsync();
    }
}

#endif