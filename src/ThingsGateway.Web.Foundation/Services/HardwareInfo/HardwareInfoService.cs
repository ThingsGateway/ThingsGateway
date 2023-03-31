using Furion.RemoteRequest.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using UAParser;

namespace ThingsGateway.Web.Foundation
{
    /// <summary>
    /// 硬件信息获取
    /// </summary>
    public class HardwareInfoService : ISingleton
    {
        private readonly Hardware.Info.HardwareInfo hardwareInfo = new();

        ILogger _logger;

        /// <inheritdoc cref="HardwareInfoService"/>
        public HardwareInfoService()
        {
            Scoped.Create((factory, scope) =>
            {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                _logger = loggerFactory.CreateLogger(nameof(HardwareInfoService));
            });

            _ = Task.Run(async () =>
             {
                 while (true)
                 {
                     hardwareInfo.RefreshMemoryStatus();
                     hardwareInfo.RefreshMemoryList();
                     hardwareInfo.RefreshDriveList();
                     hardwareInfo.RefreshNetworkAdapterList();
                     hardwareInfo.RefreshCPUList();
                     await Task.Delay(5000);
                 }
             });

        }

        /// <summary>
        /// 运行信息获取
        /// </summary>
        public TGAPPInfo APPInfo
        {
            get
            {
                return new()
                {
                    HostName = Environment.MachineName, // 主机名称
                    SystemOs = RuntimeInformation.OSDescription, // 操作系统
                    OsArchitecture = Environment.OSVersion.Platform.ToString() + " " + RuntimeInformation.OSArchitecture.ToString(), // 系统架构
                    RemoteIp = GetIpFromOnline(), // 外网地址
                    LocalIp = App.HttpContext?.Connection?.LocalIpAddress.ToString(), // 本地地址
                    FrameworkDescription = RuntimeInformation.FrameworkDescription, // NET框架
                    Environment = App.HostEnvironment.IsDevelopment() ? "Development" : "Production",
                    Stage = App.HostEnvironment.IsStaging() ? "Stage" : "非Stage", // 是否Stage环境
                };
            }
        }

        /// <summary>
        /// 硬件信息获取
        /// </summary>
        public TGHardwareInfo HardwareInfo
        {
            get
            {
                var data = hardwareInfo.Adapt<TGHardwareInfo>();
                return data;
            }
        }
        /// <summary>
        /// IP地址信息
        /// </summary>
        /// <returns></returns>
        public string GetIpFromOnline()
        {
            try
            {
                var url = "http://myip.ipip.net";
                var stream = url.GetAsStreamAsync().GetAwaiter().GetResult();
                var streamReader = new StreamReader(stream.Stream, stream.Encoding);
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
        /// 本地地址
        /// </summary>
        [Description("本地地址")]
        public string LocalIp { get; set; }

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
    }

    /// <inheritdoc/>
    public class TGHardwareInfo
    {
        /// <inheritdoc/>
        public List<CPU> CpuList { get; private set; } = new List<CPU>();

        /// <inheritdoc/>
        public List<Drive> DriveList { get; private set; } = new List<Drive>();

        /// <inheritdoc/>
        public List<Memory> MemoryList { get; private set; } = new List<Memory>();

        /// <inheritdoc/>
        public MemoryStatus MemoryStatus { get; private set; } = new MemoryStatus();
        /// <inheritdoc/>
        public List<NetworkAdapter> NetworkAdapterList { get; private set; } = new List<NetworkAdapter>();

        /// <inheritdoc/>
        public List<Volume> VolumeList => DriveList.SelectMany(a => a.PartitionList.SelectMany(b => b.VolumeList)).ToList();
    }
}