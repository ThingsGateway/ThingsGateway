#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Hardware.Info;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;

using ThingsGateway.Core;

namespace ThingsGateway.Web.Foundation;

/// <summary>
/// 硬件信息获取
/// </summary>
public class HardwareInfoService : ISingleton
{
    private readonly HardwareInfo hardwareInfo;

    private ILogger _logger;

    /// <inheritdoc cref="HardwareInfoService"/>
    public HardwareInfoService()
    {
        Scoped.Create((factory, scope) =>
        {
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger(nameof(HardwareInfoService));
        });
        try
        {
            hardwareInfo = new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化硬件信息失败");
        }
        _ = Task.Run(async () =>
         {
             string currentPath = Directory.GetCurrentDirectory();
             DriveInfo drive = new DriveInfo(Path.GetPathRoot(currentPath));
             appInfo = new()
             {
                 DriveInfo = drive,
                 HostName = Environment.MachineName, // 主机名称
                 SystemOs = RuntimeInformation.OSDescription, // 操作系统
                 OsArchitecture = Environment.OSVersion.Platform.ToString() + " " + RuntimeInformation.OSArchitecture.ToString(), // 系统架构
                 RemoteIp = await GetIpFromOnlineAsync(), // 外网地址
                 FrameworkDescription = RuntimeInformation.FrameworkDescription, // NET框架
                 Environment = App.HostEnvironment.IsDevelopment() ? "Development" : "Production",
                 Stage = App.HostEnvironment.IsStaging() ? "Stage" : "非Stage", // 是否Stage环境
                 UpdateTime = DateTime.Now.ToDateTimeF(),
             };
             while (true)
             {
                 try
                 {
                     appInfo.UpdateTime = DateTime.Now.ToDateTimeF();
                     appInfo.RemoteIp = await GetIpFromOnlineAsync();

                     hardwareInfo?.RefreshMemoryStatus();
                     hardwareInfo?.RefreshMemoryList();
                     //hardwareInfo?.RefreshDriveList();
                     hardwareInfo?.RefreshNetworkAdapterList();
                     hardwareInfo?.RefreshCPUList();

                     await Task.Delay(60000);
                 }
                 catch (Exception ex)
                 {
                     _logger.LogWarning(ex, "获取硬件信息失败");
                 }
             }
         });

    }

    private TGAPPInfo appInfo = new();
    /// <summary>
    /// 运行信息获取
    /// </summary>
    public TGAPPInfo APPInfo => appInfo;

    /// <summary>
    /// 硬件信息获取
    /// </summary>
    public HardwareInfo HardwareInfo => hardwareInfo;

    /// <summary>
    /// IP地址信息
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetIpFromOnlineAsync()
    {
        try
        {
            var url = "http://myip.ipip.net";
            var stream = await new HttpClient().GetStreamAsync(url);
            var streamReader = new StreamReader(stream, Encoding.UTF8);
            var html = streamReader.ReadToEnd();
            return html.Replace("当前 IP：", "").Replace("来自于：", "");
        }
        catch (Exception)
        {
            return "";
        }
    }

}

/// <inheritdoc/>
public class TGAPPInfo
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

