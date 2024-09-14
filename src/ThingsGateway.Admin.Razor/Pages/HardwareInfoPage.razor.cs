//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Admin.Razor;

public partial class HardwareInfoPage : IDisposable
{
    public bool Disposed { get; set; }
    private HardwareInfoService HardwareInfoService { get; set; }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    protected override void OnInitialized()
    {
        HardwareInfoService = (HardwareInfoService)NetCoreApp.RootServices.GetServices<IHostedService>().FirstOrDefault(it => it is HardwareInfoService)!;
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private async Task RunTimerAsync()
    {
        while (!Disposed)
        {
            try
            {
                if (chartInit)
                    await LineChart.Update(ChartAction.Update);

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

    private ChartDataSource? ChartDataSource { get; set; }
    private bool chartInit { get; set; }

    [Inject]
    [NotNull]
    private IStringLocalizer<HisHardwareInfo> HisHardwareInfoLocalizer { get; set; }

    private Chart LineChart { get; set; }

    private async Task<ChartDataSource> OnInit()
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
}
