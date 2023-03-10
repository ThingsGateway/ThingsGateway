using Furion.RemoteRequest.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NewLife.Log;

using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

using UAParser;

namespace ThingsGateway.Web.Foundation
{
    public class TGHardwareInfo
    {
        public MemoryStatus MemoryStatus { get; private set; } = new MemoryStatus();
        public List<CPU> CpuList { get; private set; } = new List<CPU>();
        public List<Drive> DriveList { get; private set; } = new List<Drive>();
        public List<Volume> VolumeList => DriveList.SelectMany(a => a.PartitionList.SelectMany(b => b.VolumeList)).ToList();
        public List<Memory> MemoryList { get; private set; } = new List<Memory>();
        public List<NetworkAdapter> NetworkAdapterList { get; private set; } = new List<NetworkAdapter>();
    }
    public class TGAPPInfo
    {
        [Description("主机名称")]
        public string HostName { get; set; }
        [Description("操作系统")]
        public string SystemOs { get; set; }
        [Description("系统架构")]
        public string OsArchitecture { get; set; }
        [Description("外网地址")]
        public string RemoteIp { get; set; }
        [Description("本地地址")]
        public string LocalIp { get; set; }
        [Description("NET框架")]
        public string FrameworkDescription { get; set; }
        [Description("主机环境")]
        public string Environment { get; set; }
        [Description("Stage环境")]
        public string Stage { get; set; }
    }
    public class HardwareInfoService
    {
        public TGHardwareInfo HardwareInfo
        {
            get
            {
                var data = hardwareInfo.Adapt<TGHardwareInfo>();
                return data;
            }
        }
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
        private readonly Hardware.Info.HardwareInfo hardwareInfo = new();
        private System.Timers.Timer DelayTimer10000;
        private System.Timers.Timer DelayTimer30000;
        ILogger _logger;
        public HardwareInfoService()
        {
            DelayTimer10000 = new System.Timers.Timer(10000);
            DelayTimer10000.Elapsed += timer10000_Elapsed;
            DelayTimer10000.AutoReset = true;
            DelayTimer10000.Start();
            DelayTimer30000 = new System.Timers.Timer(30000);
            DelayTimer30000.Elapsed += timer30000_Elapsed;
            DelayTimer30000.AutoReset = true;
            DelayTimer30000.Start();
            Scoped.Create((factory, scope) => {
                var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                _logger = loggerFactory.CreateLogger(nameof(HardwareInfoService));
            });

            Task.Run(() =>
            {
                timer10000_Elapsed(null, null);
                timer30000_Elapsed(null, null);

            });

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
        private void timer10000_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                hardwareInfo.RefreshMemoryStatus();
                hardwareInfo.RefreshMemoryList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取硬件信息失败");
            }

        }
        private void timer30000_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                hardwareInfo.RefreshDriveList();
                hardwareInfo.RefreshNetworkAdapterList();
                hardwareInfo.RefreshCPUList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取硬件信息失败");
            }

        }


    }
}