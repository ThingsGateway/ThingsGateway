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

using LiteDB;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NewLife;

using Newtonsoft.Json;

using System.ComponentModel;
using System.Runtime.InteropServices;

using ThingsGateway.Admin.Core.Utils;
using ThingsGateway.Cache;
using ThingsGateway.Core.Extension;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 硬件信息获取
/// </summary>
public class HardwareInfoWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _appLifetime;

    private readonly ILogger _logger;

    private EasyLock _easyLock = new();
    protected IServiceScope _serviceScope;

    /// <inheritdoc cref="HardwareInfoWorker"/>
    public HardwareInfoWorker(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime appLifetime)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
        _logger = _serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("硬件信息服务");
        _appLifetime = appLifetime;
    }

    #region 属性

    /// <summary>
    /// 运行信息获取
    /// </summary>
    public APPInfo APPInfo { get; } = new();

    #endregion 属性

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { _easyLock.Release(); _easyLock = null; });
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _easyLock?.WaitAsync();

        await GetInfoAsync(stoppingToken);
    }

    private const string _cache_HardwareInfo = $"{ThingsGatewayCacheConst.Cache_Prefix}Cache_HardwareInfo";

    private async Task GetInfoAsync(CancellationToken stoppingToken)
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

        var enable = App.GetConfig<bool?>("HardwareInfo:Enable") ?? true;
        var timeInterval = App.GetConfig<int?>("HardwareInfo:TimeInterval") ?? 10000;
        if (timeInterval < 5000) timeInterval = 5000;
        APPInfo.DriveInfo = drive;
        APPInfo.OsArchitecture = Environment.OSVersion.Platform.ToString() + " " + RuntimeInformation.OSArchitecture.ToString(); // 系统架构
        APPInfo.FrameworkDescription = RuntimeInformation.FrameworkDescription; // NET框架
        APPInfo.Environment = App.HostEnvironment.IsDevelopment() ? "Development" : "Production";
        APPInfo.Stage = App.HostEnvironment.IsStaging() ? "Stage" : "非Stage"; // 是否Stage环境
        APPInfo.UUID = CryptogramUtil.Sm4Encrypt(APPInfo.MachineInfo.UUID + APPInfo.MachineInfo.Guid + APPInfo.MachineInfo.DiskID);

        APPInfo.UpdateTime = DateTimeUtil.TimerXNow.ToDefaultDateTimeFormat();
        TimeTick timeTick = new TimeTick(180000);//3分钟更新一次历史数据
        var error = false;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (enable)
                {
                    APPInfo.MachineInfo.Refresh();
                    APPInfo.UpdateTime = DateTimeUtil.TimerXNow.ToDefaultDateTimeFormat();

                    //添加历史信息到redis
                    if (cache != null && timeTick.IsTickHappen())
                    {
                        var his = new HisHardwareInfo()
                        {
                            Date = DateTimeUtil.TimerXNow,
                            DownlinkSpeed = (APPInfo.MachineInfo.DownlinkSpeed / 1024).ToString("F2"),
                            DriveUsage = (100 - (APPInfo.DriveInfo.TotalFreeSpace * 100.00 / APPInfo.DriveInfo.TotalSize)).ToString("F2"),
                            Battery = (APPInfo.MachineInfo.Battery * 100).ToString("F2"),
                            MemoryUsage = (100 - (APPInfo.MachineInfo.AvailableMemory * 100.00 / APPInfo.MachineInfo.Memory)).ToString("F2"),
                            CpuUsage = (APPInfo.MachineInfo.CpuRate * 100).ToString("F2"),
                            Temperature = (APPInfo.MachineInfo.Temperature).ToString("F2"),
                            UplinkSpeed = (APPInfo.MachineInfo.UplinkSpeed / 1024).ToString("F2"),
                        };
                        cache.Add(his);
                    }
                    var sevenDaysAgo = DateTimeUtil.TimerXNow.AddDays(-7);
                    //删除特定信息
                    var deleteCount = cache.DeleteMany(Query.LT(nameof(HisHardwareInfo.Date), sevenDaysAgo));
                    if (deleteCount > 0)
                        cache.InitDb();
                }
                //10秒更新一次
                await Task.Delay(timeInterval, stoppingToken);
                error = false;
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (!error)
                    _logger.LogWarning(ex, "获取硬件信息失败");
                error = true;
            }
        }
    }

    public List<HisHardwareInfo> GetHis()
    {
        var data = cache.GetAll();
        return data.ToList();
    }

    private string cacheKey => $"{typeof(HisHardwareInfo).FullName}";
    private LiteDBCache<HisHardwareInfo> cache => LiteDBCacheUtil.GetDB<HisHardwareInfo>(nameof(APPInfo), cacheKey, false);
}

/// <inheritdoc/>
public class APPInfo
{
    /// <summary>
    /// 当前磁盘信息
    /// </summary>
    [Description("当前磁盘信息")]
    public DriveInfo DriveInfo { get; set; }

    /// <summary>
    /// 硬件信息获取
    /// </summary>
    public MachineInfo? MachineInfo { get; set; }

    /// <summary>
    /// 主机环境
    /// </summary>
    [Description("主机环境")]
    public string Environment { get; set; }

    /// <summary>
    /// NET框架
    /// </summary>
    [Description("NET框架")]
    public string FrameworkDescription { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    [Description("系统架构")]
    public string OsArchitecture { get; set; }

    /// <summary>
    /// Stage环境
    /// </summary>
    [Description("Stage环境")]
    public string Stage { get; set; }

    /// <summary>
    /// 唯一编码
    /// </summary>
    [Description("唯一编码")]
    public string UUID { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Description("更新时间")]
    public string UpdateTime { get; set; }
}

/// <inheritdoc/>
public class HisHardwareInfo : PrimaryIdEntity
{
    [JsonIgnore]
    public override long Id { get; set; }

    [Description("磁盘使用率")]
    public string DriveUsage { get; set; }

    [Description("内存使用率")]
    public string MemoryUsage { get; set; }

    [Description("CPU使用率")]
    public string CpuUsage { get; set; }

    [Description("温度")]
    public string Temperature { get; set; }

    [Description("电池")]
    public string Battery { get; set; }

    [Description("上载速度")]
    public string UplinkSpeed { get; set; }

    [Description("下载速度")]
    public string DownlinkSpeed { get; set; }

    [Description("时间")]
    public DateTime Date { get; set; }
}