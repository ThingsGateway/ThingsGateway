
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using BootstrapBlazor.Components;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NewLife;
using NewLife.Threading;

using SqlSugar;

using System.Runtime.InteropServices;

using ThingsGateway.Core;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Admin.Application;

public class HardwareInfoConfig
{
    public bool Enable { get; set; } = true;
    public int RealInterval { get; set; } = 30;
    public int HisInterval { get; set; } = 180;
    public int DaysAgo { get; set; } = 7;
}

/// <summary>
/// 硬件信息获取
/// </summary>
public class HardwareInfoService : BackgroundService
{
    private readonly ILogger _logger;

    public HardwareInfoService(ILogger<HardwareInfoService> logger)
    {
        _logger = logger;
    }

    #region 属性

    /// <summary>
    /// 运行信息获取
    /// </summary>
    public APPInfo APPInfo { get; } = new();

    public HardwareInfoConfig HardwareInfoConfig { get; private set; }

    #endregion 属性

    public async Task<List<HisHardwareInfo>> GetHisHardwareInfos()
    {
        using var db = DbContext.Db.GetConnectionScopeWithAttr<HisHardwareInfo>().CopyNew();
        return await db.Queryable<HisHardwareInfo>().ToListAsync();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            APPInfo.MachineInfo = new();
            APPInfo.MachineInfo.Init();
            APPInfo.MachineInfo.Refresh();
        }
        catch
        {
        }

        string currentPath = Directory.GetCurrentDirectory();
        DriveInfo drive = new(Path.GetPathRoot(currentPath));

        HardwareInfoConfig = App.Configuration.GetSection(nameof(HardwareInfoConfig)).Get<HardwareInfoConfig?>() ?? new HardwareInfoConfig();
        HardwareInfoConfig.DaysAgo = Math.Min(Math.Max(HardwareInfoConfig.DaysAgo, 1), 7);
        if (HardwareInfoConfig.RealInterval < 30) HardwareInfoConfig.RealInterval = 30;
        if (HardwareInfoConfig.HisInterval < 120) HardwareInfoConfig.HisInterval = 120;
        APPInfo.DriveInfo = drive;
        APPInfo.OsArchitecture = Environment.OSVersion.Platform.ToString() + " " + RuntimeInformation.OSArchitecture.ToString(); // 系统架构
        APPInfo.FrameworkDescription = RuntimeInformation.FrameworkDescription; // NET框架
        APPInfo.Environment = App.IsDevelopment ? "Development" : "Production";
        APPInfo.UUID = DESCEncryption.Encrypt(APPInfo.MachineInfo.UUID + APPInfo.MachineInfo.Guid + APPInfo.MachineInfo.DiskID);

        APPInfo.UpdateTime = TimerX.Now.ToDefaultDateTimeFormat();
        var error = false;
        DateTime hisInsertTime = default;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (HardwareInfoConfig.Enable)
                {
                    APPInfo.MachineInfo.Refresh();
                    APPInfo.UpdateTime = TimerX.Now.ToDefaultDateTimeFormat();
                    APPInfo.WorkingSet = (Environment.WorkingSet / 1024.0 / 1024.0).ToString("F2");

                    if (DateTime.Now > hisInsertTime.Add(TimeSpan.FromSeconds(HardwareInfoConfig.HisInterval)))
                    {
                        hisInsertTime = DateTime.Now;
                        using var db = DbContext.Db.GetConnectionScopeWithAttr<HisHardwareInfo>().CopyNew();
                        {
                            var his = new HisHardwareInfo()
                            {
                                Date = TimerX.Now,
                                DriveUsage = (100 - (APPInfo.DriveInfo.TotalFreeSpace * 100.00 / APPInfo.DriveInfo.TotalSize)).ToString("F2"),
                                Battery = (APPInfo.MachineInfo.Battery * 100).ToString("F2"),
                                MemoryUsage = (100 - (APPInfo.MachineInfo.AvailableMemory * 100.00 / APPInfo.MachineInfo.Memory)).ToString("F2"),
                                CpuUsage = (APPInfo.MachineInfo.CpuRate * 100).ToString("F2"),
                                Temperature = (APPInfo.MachineInfo.Temperature).ToString("F2"),
                            };
                            await db.Insertable(his).ExecuteCommandAsync();
                        }
                        var sevenDaysAgo = TimerX.Now.AddDays(-HardwareInfoConfig.DaysAgo);
                        //删除特定信息
                        await db.Deleteable<HisHardwareInfo>(a => a.Date <= sevenDaysAgo).ExecuteCommandAsync();

                    }
                }
                await Task.Delay(HardwareInfoConfig.RealInterval * 1000, stoppingToken).ConfigureAwait(false);
                error = false;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (!error)
                    _logger.LogWarning(ex, App.CreateLocalizerByType(this.GetType())["GetHardwareInfoFail"]);
                error = true;
            }
        }
    }
}

/// <inheritdoc/>
public class APPInfo
{
    /// <summary>
    /// 当前磁盘信息
    /// </summary>
    public DriveInfo DriveInfo { get; set; }

    /// <summary>
    /// 硬件信息获取
    /// </summary>
    public MachineInfo? MachineInfo { get; set; }

    /// <summary>
    /// 主机环境
    /// </summary>
    public string Environment { get; set; }

    /// <summary>
    /// NET框架
    /// </summary>
    public string FrameworkDescription { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    public string OsArchitecture { get; set; }

    /// <summary>
    /// 唯一编码
    /// </summary>
    public string UUID { get; set; }

    /// <summary>
    /// 进程占用内存
    /// </summary>
    [AutoGenerateColumn(Ignore = true)]
    public string WorkingSet { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public string UpdateTime { get; set; }


}

[SugarTable("tg_hardwareinfo", TableDescription = "硬件信息历史表")]
[Tenant(SqlSugarConst.DB_HardwareInfo)]
public class HisHardwareInfo
{
    [SugarColumn(ColumnDescription = "磁盘使用率")]
    public string DriveUsage { get; set; }

    [SugarColumn(ColumnDescription = "内存使用率")]
    public string MemoryUsage { get; set; }

    [SugarColumn(ColumnDescription = "CPU使用率")]
    public string CpuUsage { get; set; }

    [SugarColumn(ColumnDescription = "温度")]
    public string Temperature { get; set; }

    [SugarColumn(ColumnDescription = "电池")]
    public string Battery { get; set; }

    [SugarColumn(ColumnDescription = "时间")]
    public DateTime Date { get; set; }
}
