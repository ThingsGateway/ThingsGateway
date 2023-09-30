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

using Furion;

using Hardware.Info;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 硬件信息获取
/// </summary>
public class HardwareInfoWorker : BackgroundService
{
    /// <summary>
    /// 硬件信息获取
    /// </summary>
    public HardwareInfo HardwareInfo { get; private set; }

    private readonly ILogger<HardwareInfoWorker> _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc cref="HardwareInfoWorker"/>
    public HardwareInfoWorker(ILogger<HardwareInfoWorker> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
    }

    private EasyLock easyLock = new();

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { easyLock.Release(); easyLock = null; });
        await base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 运行信息获取
    /// </summary>
    public APPInfo APPInfo { get; } = new();

    /// <summary>
    /// IP地址信息
    /// </summary>
    /// <returns></returns>
    private async Task<string> GetIpFromOnlineAsync()
    {
        try
        {
            var url = "http://myip.ipip.net";
            using var httpClient = new HttpClient();
            using var stream = await httpClient.GetStreamAsync(url);
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            var html = streamReader.ReadToEnd();
            return html.Replace("当前 IP：", "").Replace("来自于：", "");
        }
        catch (Exception)
        {
            return "";
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await easyLock?.WaitAsync();

        try
        {
            HardwareInfo = new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化硬件信息失败");
        }
        string currentPath = Directory.GetCurrentDirectory();
        DriveInfo drive = new(Path.GetPathRoot(currentPath));

        APPInfo.DriveInfo = drive;
        APPInfo.HostName = Environment.MachineName; // 主机名称
        APPInfo.SystemOs = RuntimeInformation.OSDescription; // 操作系统
        APPInfo.OsArchitecture = Environment.OSVersion.Platform.ToString() + " " + RuntimeInformation.OSArchitecture.ToString(); // 系统架构
        APPInfo.RemoteIp = await GetIpFromOnlineAsync(); // 外网地址
        APPInfo.FrameworkDescription = RuntimeInformation.FrameworkDescription; // NET框架
        APPInfo.Environment = App.HostEnvironment.IsDevelopment() ? "Development" : "Production";
        APPInfo.Stage = App.HostEnvironment.IsStaging() ? "Stage" : "非Stage"; // 是否Stage环境
        APPInfo.UpdateTime = DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                APPInfo.UpdateTime = DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat();
                APPInfo.RemoteIp = await GetIpFromOnlineAsync();

                HardwareInfo?.RefreshMemoryStatus();
                HardwareInfo?.RefreshMemoryList();
                HardwareInfo?.RefreshNetworkAdapterList();
                HardwareInfo?.RefreshCPUList();
                //10秒更新一次
                await Task.Delay(10000, stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "获取硬件信息失败");
            }
        }

    }
}

/// <inheritdoc/>
public class APPInfo
{
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
    /// 主机名称
    /// </summary>
    [Description("主机名称")]
    public string HostName { get; set; }

    /// <summary>
    /// 系统架构
    /// </summary>
    [Description("系统架构")]
    public string OsArchitecture { get; set; }

    /// <summary>
    /// 外网地址
    /// </summary>
    [Description("外网地址")]
    public string RemoteIp { get; set; }

    /// <summary>
    /// Stage环境
    /// </summary>
    [Description("Stage环境")]
    public string Stage { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    [Description("操作系统")]
    public string SystemOs { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [Description("更新时间")]
    public string UpdateTime { get; set; }

    /// <summary>
    /// 当前磁盘信息
    /// </summary>
    [Description("当前磁盘信息")]
    public DriveInfo DriveInfo { get; set; }
}

