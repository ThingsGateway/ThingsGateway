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

using System.Diagnostics;
using System.IO;

using ThingsGateway.Core;
using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Dmtp;
using ThingsGateway.Foundation.Dmtp.FileTransfer;
using ThingsGateway.Foundation.Dmtp.Rpc;

namespace ThingsGateway.UpgradeManger;

/// <summary>
/// UpgradeManger
/// </summary>
public partial class UpgradeManger
{
    /// <summary>
    /// UpgradeMangerConfig
    /// </summary>
    public UpgradeMangerConfig Config;

    /// <summary>
    /// Messages
    /// </summary>
    public ConcurrentLinkedList<(Microsoft.Extensions.Logging.LogLevel level, string message)> Messages = new();

    /// <summary>
    /// TcpDmtpService
    /// </summary>
    public TcpDmtpService TcpDmtpService;
    /// <summary>
    /// LogMessage
    /// </summary>
    public LoggerGroup LogMessage;
    /// <inheritdoc/>
    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            LogMessage = new LoggerGroup() { LogLevel = LogLevel.Trace };
            LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = LogLevel.Trace });

            var dataString = FileUtil.ReadFile($"{AppContext.BaseDirectory}UpgradeMangerConfig.json");//读取文件
            Config = dataString.FromJsonString<UpgradeMangerConfig>();
        }
        catch (Exception ex)
        {
            LogMessage.LogError(ex, "程序初始化配置失败");
        }
        TcpDmtpService = CreateTcpDmtpService(Config);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                TcpDmtpService.Start();
                await Task.Delay(10000, stoppingToken);
            }
            catch (TaskCanceledException)
            {

            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                LogMessage.LogError(ex, ToString());
            }
        }
        TcpDmtpService.Dispose();
    }

    private TcpDmtpService CreateTcpDmtpService(UpgradeMangerConfig autoUpdateConfig)
    {
        var config = new TouchSocketConfig()
       .SetListenIPHosts(autoUpdateConfig.UpdateServerUri)
       .SetVerifyToken(autoUpdateConfig.VerifyToken)
       .ConfigureContainer(a =>
       {
           a.AddLogger(LogMessage);
           a.AddDmtpRouteService();//添加路由策略
       })
       .ConfigurePlugins(a =>
       {
           a.UseDmtpFileTransfer();//必须添加文件传输插件
           a.UseDmtpHeartbeat()//使用Dmtp心跳
           .SetTick(TimeSpan.FromSeconds(3))
           .SetMaxFailCount(3);
           a.UseDmtpRpc();
       });
        TcpDmtpService service = new TcpDmtpService();
        service.Connecting = (client, e) =>
        {
            service.Logger.Info($"{client.GetIPPort()}：Connecting");
        };//有客户端正在连接
        service.Connected = (client, e) => { service.Logger.Info($"{client.GetIPPort()}：Connected"); };//有客户端连接
        service.Disconnected = (client, e) => { service.Logger.Info($"{client.GetIPPort()}：Disconnected"); };//有客户端断开连接
        service.Setup(config);
        return service;

    }

    /// <summary>
    /// 底层日志输出
    /// </summary>
    private void LogOut(ThingsGateway.Foundation.LogLevel logLevel, object source, string message, Exception exception)
    {
        Messages.Add(((Microsoft.Extensions.Logging.LogLevel)logLevel,
            $"{DateTimeExtensions.CurrentDateTime.ToDefaultDateTimeFormat()} - {message} {exception}"));
        if (Messages.Count > 2500)
        {
            Messages.Clear();
        }

    }


    /// <summary>
    /// DBUpload
    /// </summary>
    /// <param name="tcpDmtpSocketClient"></param>
    /// <returns></returns>
    public async Task DBUpload(TcpDmtpSocketClient tcpDmtpSocketClient)
    {
        var folderPath = $"{AppContext.BaseDirectory}ThingsGatewayDB/";
        // 检查文件夹是否存在
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var metadata = new Metadata();//传递到客户端的元数据
        metadata.Add(FilePluginUtil.DmtpType, DmtpTypeEnum.GatewayDB.ToString());

        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = $"{folderPath}/{tcpDmtpSocketClient.IP}-{tcpDmtpSocketClient.Port}ThingsGateway.db",//服务器本地保存路径
            ResourcePath = string.Empty,//请求客户端文件的资源路径
            Metadata = metadata,//传递到客户端的元数据
            Timeout = TimeSpan.FromSeconds(60),//传输超时时长
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.Result.ResultCode != ResultCode.Default)
            {
                loop.Dispose();
            }
            LogMessage.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed() / 1024}kb/s");
        });

        //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
        var result = await tcpDmtpSocketClient.GetDmtpFileTransferActor().PullFileAsync(fileOperator);

        loopAction.Run();

        LogMessage.Info($" {result.ResultCode}，具体消息：{result.Message}");
        if (result.ResultCode != ResultCode.Success)
            throw new Exception(result.Message);
        OpenFile(folderPath);
    }

    /// <summary>
    /// DBUpload
    /// </summary>
    /// <returns></returns>
    public async Task FileDown(TcpDmtpSocketClient tcpDmtpSocketClient, string folderPath)
    {

        string[] files = Directory.GetFiles(folderPath, "", SearchOption.AllDirectories);
        await files.ParallelForEachAsync(async (file, cancellationToken) =>
        {
            try
            {
                if (!Path.GetRelativePath(folderPath, file).Contains("FileTemp"))
                {


                    var metadata = new Metadata();//传递到客户端的元数据
                    metadata.Add(FilePluginUtil.DmtpType, DmtpTypeEnum.File.ToString());
                    metadata.Add(FilePluginUtil.FileName, Path.GetRelativePath(folderPath, file));

                    var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
                    {
                        SavePath = string.Empty,//客户端本地保存路径
                        ResourcePath = file,//服务器文件的资源路径
                        Metadata = metadata,//传递到客户端的元数据
                        Timeout = TimeSpan.FromSeconds(60),//传输超时时长
                        TryCount = 10,//当遇到失败时，尝试次数
                        FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
                    };

                    //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
                    var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
                    {
                        if (fileOperator.Result.ResultCode != ResultCode.Default)
                        {
                            loop.Dispose();
                        }
                        LogMessage.Info($"进度：{fileOperator.Progress}，速度：{fileOperator.Speed() / 1024}kb/s");
                    });

                    //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
                    var result = await tcpDmtpSocketClient.GetDmtpFileTransferActor().PushFileAsync(fileOperator);

                    loopAction.Run();
                    LogMessage.Info($" {result.ResultCode}，具体消息：{result.Message}");
                    if (result.ResultCode != ResultCode.Success)
                        throw new Exception(result.Message);
                }
            }
            catch (Exception ex)
            {
                LogMessage.LogWarning(ex, "传输出错");
            }
        }, 10);


    }


    /// <summary>
    /// 打开指定目录下的文件
    /// </summary>
    private static void OpenFile(string folderPath)
    {

        Process process = new Process();

        if (OperatingSystem.IsWindows())
        {
            process.StartInfo.FileName = "explorer.exe";
            process.StartInfo.Arguments = folderPath.Replace("/", "\\");
        }
        else if (OperatingSystem.IsMacOS())
        {
            process.StartInfo.FileName = "open";
            process.StartInfo.Arguments = folderPath;
        }
        else if (OperatingSystem.IsLinux())
        {
            process.StartInfo.FileName = "xdg-open";
            process.StartInfo.Arguments = folderPath;
        }
        else
        {
            throw new NotSupportedException("不支持的操作系统");
        }

        process.Start();
    }
}
