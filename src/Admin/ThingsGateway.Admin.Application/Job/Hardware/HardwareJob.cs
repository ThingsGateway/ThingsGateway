// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Runtime.InteropServices;

using ThingsGateway.DataEncryption;
using ThingsGateway.Extension;
using ThingsGateway.NewLife;
using ThingsGateway.NewLife.Threading;
using ThingsGateway.Schedule;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 获取硬件信息作业任务
/// </summary>
[JobDetail("hardware_log", Description = "获取硬件信息", GroupName = "Hardware", Concurrent = false)]
[PeriodSeconds(30, TriggerId = "trigger_hardware", Description = "获取硬件信息", RunOnStart = true)]
public class HardwareJob : IJob, IHardwareJob
{
    private readonly ILogger _logger;
    private readonly IStringLocalizer _localizer;

    /// <inheritdoc/>
    public HardwareJob(ILogger<HardwareJob> logger, IStringLocalizer<HardwareJob> localizer, IOptions<HardwareInfoOptions> options)
    {
        _logger = logger;
        _localizer = localizer;
        HardwareInfoOptions = options.Value;
    }
    #region 属性

    /// <summary>
    /// 运行信息获取
    /// </summary>
    public HardwareInfo HardwareInfo { get; } = new();

    /// <inheritdoc/>
    public HardwareInfoOptions HardwareInfoOptions { get; private set; }

    #endregion 属性

    /// <inheritdoc/>
    public async Task<List<HistoryHardwareInfo>> GetHistoryHardwareInfos()
    {
        using var db = DbContext.Db.GetConnectionScopeWithAttr<HistoryHardwareInfo>().CopyNew();
        return await db.Queryable<HistoryHardwareInfo>().ToListAsync().ConfigureAwait(false);
    }

    private bool error = false;
    private DateTime hisInsertTime = default;

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        if (HardwareInfoOptions.Enable)
        {
            try
            {
                if (HardwareInfo.MachineInfo == null)
                {
                    await MachineInfo.RegisterAsync().ConfigureAwait(false);
                    HardwareInfo.MachineInfo = MachineInfo.Current;

                    string currentPath = Directory.GetCurrentDirectory();
                    DriveInfo drive = new(Path.GetPathRoot(currentPath));

                    HardwareInfoOptions.DaysAgo = Math.Min(Math.Max(HardwareInfoOptions.DaysAgo, 1), 7);
                    if (HardwareInfoOptions.HistoryInterval < 60000) HardwareInfoOptions.HistoryInterval = 60000;
                    HardwareInfo.DriveInfo = drive;
                    HardwareInfo.OsArchitecture = Environment.OSVersion.Platform.ToString() + " " + RuntimeInformation.OSArchitecture.ToString(); // 系统架构
                    HardwareInfo.FrameworkDescription = RuntimeInformation.FrameworkDescription; // NET框架
                    HardwareInfo.Environment = App.HostEnvironment.IsDevelopment() ? "Development" : "Production";
                    HardwareInfo.UUID = DESEncryption.Encrypt(HardwareInfo.MachineInfo.UUID + HardwareInfo.MachineInfo.Guid + HardwareInfo.MachineInfo.DiskID);

                    HardwareInfo.UpdateTime = TimerX.Now.ToDefaultDateTimeFormat();

                }
            }
            catch
            {
            }
            try
            {
                HardwareInfo.MachineInfo.Refresh();
                HardwareInfo.UpdateTime = TimerX.Now.ToDefaultDateTimeFormat();
                HardwareInfo.WorkingSet = (Environment.WorkingSet / 1024.0 / 1024.0).ToString("F2");
                error = false;
            }
            catch (Exception ex)
            {
                if (!error)
                    _logger.LogWarning(ex, _localizer["GetHardwareInfoFail"]);
                error = true;
            }

            try
            {
                if (HardwareInfoOptions.Enable)
                {
                    if (DateTime.Now > hisInsertTime.Add(TimeSpan.FromMilliseconds(HardwareInfoOptions.HistoryInterval)))
                    {
                        hisInsertTime = DateTime.Now;
                        using var db = DbContext.Db.GetConnectionScopeWithAttr<HistoryHardwareInfo>().CopyNew();
                        {
                            var his = new HistoryHardwareInfo()
                            {
                                Date = TimerX.Now,
                                DriveUsage = (100 - (HardwareInfo.DriveInfo.TotalFreeSpace * 100.00 / HardwareInfo.DriveInfo.TotalSize)).ToString("F2"),
                                Battery = (HardwareInfo.MachineInfo.Battery * 100).ToString("F2"),
                                MemoryUsage = (100 - (HardwareInfo.MachineInfo.AvailableMemory * 100.00 / HardwareInfo.MachineInfo.Memory)).ToString("F2"),
                                CpuUsage = (HardwareInfo.MachineInfo.CpuRate * 100).ToString("F2"),
                                Temperature = (HardwareInfo.MachineInfo.Temperature).ToString("F2"),
                            };
                            await db.Insertable(his).ExecuteCommandAsync(stoppingToken).ConfigureAwait(false);
                        }
                        var sevenDaysAgo = TimerX.Now.AddDays(-HardwareInfoOptions.DaysAgo);
                        //删除特定信息
                        await db.Deleteable<HistoryHardwareInfo>(a => a.Date <= sevenDaysAgo).ExecuteCommandAsync(stoppingToken).ConfigureAwait(false);
                    }
                }
                error = false;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (!error)
                    _logger.LogWarning(ex, _localizer["GetHardwareInfoFail"]);
                error = true;
            }
        }
    }

}
