using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Foundation;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Upgrade;

public class FileHostService : BackgroundService
{
    private ILogger<FileHostService> _logger;
    public LoggerGroup _log { get; internal set; }
    public FileHostService(ILogger<FileHostService> logger)
    {
        _logger = logger;
    }

    private async Task<TcpDmtpService> GetTcpDmtpService()
    {
        // 创建新的文件日志记录器，并设置日志级别为 Trace
        var TextLogger = TextFileLogger.GetMultipleFileLogger("Logs/UpgradeLog");
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
        _log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Warning };
        _log.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        _log.AddLogger(TextLogger);
        var upgradeServerOptions = App.GetOptions<UpgradeServerOptions>();
        var service = new TcpDmtpService();
        var config = new TouchSocketConfig()//配置
               .SetListenIPHosts(new IPHost[] { new IPHost(upgradeServerOptions.UpgradeServerPort) })
               .ConfigureContainer(a =>
               {
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<FileRpcServer>();
                   });
                   a.AddLogger(_log);
                   a.AddDmtpRouteService();//添加路由策略
               })
               .ConfigurePlugins(a =>
               {
                   a.Add<TcpServiceReceiveAsyncPlugin>();
                   a.UseDmtpRpc();
                   a.UseDmtpFileTransfer()//必须添加文件传输插件
                   .SetMaxSmallFileLength(1024 * 1024 * 10);//设置小文件的最大限制长度
                   a.Add<FilePlugin>();
               })
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = "ThingsGateway"//连接验证口令。
               });

        await service.SetupAsync(config).ConfigureAwait(false);
        await service.StartAsync().ConfigureAwait(false);
        service.Logger.Info("启动文件服务");
        return service;
    }
    sealed class TcpServiceReceiveAsyncPlugin : PluginBase, ITcpConnectedPlugin, ITcpClosedPlugin
    {
        private ILog _log;
        public TcpServiceReceiveAsyncPlugin(ILog log)
        {
            _log = log;
        }

        public async Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
        {
            _log.Trace($"{client.GetIPPort()} Connected");
            await e.InvokeNext().ConfigureAwait(false);
        }

        public async Task OnTcpConnected(ITcpSession client, ConnectedEventArgs e)
        {
            _log.Trace($"{client.GetIPPort()} Connected");
            await e.InvokeNext().ConfigureAwait(false);
        }
    }
    /// <summary>
    /// 底层错误日志输出
    /// </summary>
    protected internal virtual void Log_Out(TouchSocket.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        _logger?.Log_Out(arg1, arg2, arg3, arg4);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var service = await GetTcpDmtpService().ConfigureAwait(false);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (service.ServerState != ServerState.Running)
                {

                    await service.StartAsync().ConfigureAwait(false);

                }
                await Task.Delay(30000, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _log.Exception(ex);
                try
                {
                    await service.StopAsync().ConfigureAwait(false);
                }
                catch
                {
                }
            }
        }
    }
}
