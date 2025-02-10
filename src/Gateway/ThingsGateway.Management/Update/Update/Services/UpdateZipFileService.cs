using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Reflection;
using System.Runtime.InteropServices;

using ThingsGateway.NewLife;
using ThingsGateway.Upgrade;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Management;

public class UpdateZipFileService : IUpdateZipFileService, IDisposable
{
    private ILogger<UpdateZipFileService> _logger;
    private IStringLocalizer Localizer;
    private LoggerGroup _log { get; set; }
    public TextFileLogger TextLogger { get; }
    public string LogPath { get; }
    public UpdateZipFileService(ILogger<UpdateZipFileService> logger)
    {
        _logger = logger;
        Localizer = App.CreateLocalizerByType(typeof(UpdateZipFileService));

        // 创建新的文件日志记录器，并设置日志级别为 Trace
        LogPath = "Logs/UpgradeLog";
        TextLogger = TextFileLogger.GetMultipleFileLogger(LogPath);
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
    }

    /// <summary>
    /// 传输限速
    /// </summary>
    public const long MaxSpeed = 1024 * 1024 * 10L;


    public async Task<List<UpdateZipFile>> GetList()
    {
        var upgradeServerOptions = App.GetOptions<UpgradeServerOptions>();
        if (!upgradeServerOptions.Enable)
            return new();
        using var client = await GetTcpDmtpClient().ConfigureAwait(false);

        //设置调用配置
        var tokenSource = new CancellationTokenSource();//可取消令箭源，可用于取消Rpc的调用
        var invokeOption = new DmtpInvokeOption()//调用配置
        {
            FeedbackType = FeedbackType.WaitInvoke,//调用反馈类型
            SerializationType = SerializationType.Json,//序列化类型
            Timeout = 5000,//调用超时设置
            Token = tokenSource.Token//配置可取消令箭
        };

        var updateZipFiles = await client.GetDmtpRpcActor().InvokeTAsync<List<UpdateZipFile>>(nameof(GetList), invokeOption, new UpdateZipFileInput()
        {
            Version = Assembly.GetEntryAssembly().GetName().Version,
            DotNetVersion = Environment.Version,
            OSPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                           RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                           RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "OSX" : "Unknown",
            Architecture = RuntimeInformation.ProcessArchitecture,
            AppName = "ThingsGateway"
        }).ConfigureAwait(false);

        return updateZipFiles.OrderByDescending(a => a.Version).ToList();
    }

    private readonly WaitLock WaitLock = new();
    public async Task Update(UpdateZipFile updateZipFile, Func<Task<bool>> check = null)
    {
        try
        {
            var upgradeServerOptions = App.GetOptions<UpgradeServerOptions>();
            if (!upgradeServerOptions.Enable)
                return;
            if (WaitLock.Waited)
            {
                _log.LogWarning("正在更新中，请稍后再试");
                return;
            }
            try
            {

                await WaitLock.WaitAsync().ConfigureAwait(false);
                RestartServerHelper.DeleteAndBackup();
                using var client = await GetTcpDmtpClient().ConfigureAwait(false);

                var result = await ClientPullFileFromService(client, updateZipFile.FilePath).ConfigureAwait(false);
                if (result)
                {
                    if (check != null)
                        result = await check.Invoke().ConfigureAwait(false);
                    if (result)
                    {
                        RestartServerHelper.ExtractUpdate();
                    }
                }
                else
                {
                }
            }
            finally
            {
                WaitLock.Release();
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex);

        }
    }

    /// <summary>
    /// 客户端从服务器下载文件。
    /// </summary>
    private static async Task<bool> ClientPullFileFromService(TcpDmtpClient client, string path)
    {
        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add(FileConst.FilePathKey, path);
        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = FileConst.UpgradePath,//客户端本地保存路径
            ResourcePath = path,//请求文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            Timeout = TimeSpan.FromSeconds(60),//传输超时时长
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            client.Logger.Info($"请求文件：{fileOperator.ResourcePath}，进度：{(fileOperator.Progress * 100).ToString("F2")}%，速度：{(fileOperator.Speed() / 1024).ToString("F2")} KB/s");
        });

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PullFileAsync
        var result = await client.GetDmtpFileTransferActor().PullFileAsync(fileOperator).ConfigureAwait(false);

        if (result.IsSuccess)
            client.Logger.Info(result.ToString());
        else
            client.Logger.Warning(result.ToString());

        return result.IsSuccess;
    }

    /// <summary>
    /// 客户端上传文件到服务器。
    /// </summary>
    private static async Task ClientPushFileFromService(TcpDmtpClient client, string serverPath, string resourcePath)
    {
        var metadata = new Metadata();//传递到服务器的元数据
        metadata.Add(FileConst.FilePathKey, serverPath);

        var fileOperator = new FileOperator//实例化本次传输的控制器，用于获取传输进度、速度、状态等。
        {
            SavePath = serverPath,//服务器本地保存路径
            ResourcePath = resourcePath,//客户端本地即将上传文件的资源路径
            Metadata = metadata,//传递到服务器的元数据
            Timeout = TimeSpan.FromSeconds(60),//传输超时时长
            TryCount = 10,//当遇到失败时，尝试次数
            FileSectionSize = 1024 * 512//分包大小，当网络较差时，应该适当减小该值
        };

        fileOperator.MaxSpeed = MaxSpeed;//设置最大限速。

        //此处的作用相当于Timer，定时每秒输出当前的传输进度和速度。
        var loopAction = LoopAction.CreateLoopAction(-1, 1000, (loop) =>
        {
            if (fileOperator.IsEnd)
            {
                loop.Dispose();
            }
            client.Logger.Info($"进度：{(fileOperator.Progress * 100).ToString("F2")}%，速度：{(fileOperator.Speed() / 1024).ToString("F2")} KB/s");
        });

        _ = loopAction.RunAsync();

        //此方法会阻塞，直到传输结束，也可以使用PushFileAsync
        var result = await client.GetDmtpFileTransferActor().PushFileAsync(fileOperator).ConfigureAwait(false);

        client.Logger.Info(result.ToString());

    }

    /// <summary>
    /// 底层错误日志输出
    /// </summary>
    protected internal virtual void Log_Out(TouchSocket.Core.LogLevel arg1, object arg2, string arg3, Exception arg4)
    {
        _logger?.Log_Out(arg1, arg2, arg3, arg4);
    }

    private async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        _log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        _log.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        _log.AddLogger(TextLogger);
        var upgradeServerOptions = App.GetOptions<UpgradeServerOptions>();
        var client = await new TouchSocketConfig()
               .SetRemoteIPHost(new IPHost($"{upgradeServerOptions.UpgradeServerIP}:{upgradeServerOptions.UpgradeServerPort}"))
               .SetDmtpOption(new DmtpOption()
               {
                   VerifyToken = upgradeServerOptions.VerifyToken
               })
               .ConfigureContainer(a =>
               {
                   a.AddLogger(_log);
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpRpc();
                   a.UseDmtpFileTransfer();//必须添加文件传输插件

                   a.Add<FilePlugin>();

                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromSeconds(3))
                   .SetMaxFailCount(3);
               })
               .BuildClientAsync<TcpDmtpClient>().ConfigureAwait(false);

        return client;
    }

    public void Dispose()
    {
        TextLogger.Dispose();
    }
}


