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
using Furion.Logging.Extensions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ThingsGateway.Foundation.Dmtp;
using ThingsGateway.Foundation.Dmtp.FileTransfer;
using ThingsGateway.Foundation.Dmtp.Rpc;
using ThingsGateway.Foundation.Rpc;
using ThingsGateway.Foundation.Sockets;

using Yitter.IdGenerator;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// UpgradeWorker
/// </summary>
public partial class UpgradeWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <inheritdoc cref="UpgradeWorker"/>
    public UpgradeWorker(ILogger<UpgradeWorker> logger, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
    }
    /// <summary>
    /// 服务状态
    /// </summary>
    public OperResult StatuString { get; set; } = new OperResult("初始化");


    #region worker服务
    private TcpDmtpClient TcpDmtpClient;
    private EasyLock easyLock = new();

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { easyLock.Release(); easyLock = null; });
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await easyLock?.WaitAsync();

        var config = App.GetConfig<UpgradeConfig>("UpgradeConfig");
        if (config == null || (!config.ConfigEnable && !config.FileEnable))
        {
            _logger.LogInformation("不启用自动更新");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(10000, stoppingToken);
                }
                catch
                {

                }
            }
        }
        else
        {
            TcpDmtpClient = GetTcpDmtpClient(config);
            TcpDmtpClient.Connected = (client, e) =>
            {
                TcpDmtpClient.ResetId(config.Name ?? YitIdHelper.NextId().ToString());
            };
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    try
                    {
                        await TcpDmtpClient.ConnectAsync();
                        StatuString.ErrorCode = 0;
                        StatuString.Message = "成功";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "连接服务器失败");
                        StatuString.ErrorCode = 999;
                        StatuString.Message = "连接服务器失败";
                    }

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
                    _logger.LogWarning(ex, ToString());
                }
            }
        }

    }
    private void LogOut(ThingsGateway.Foundation.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    private TcpDmtpClient GetTcpDmtpClient(UpgradeConfig autoUpdateConfig)
    {
        TcpDmtpClient client = new TcpDmtpClient();
        var config = new TouchSocketConfig()
               .SetRemoteIPHost(autoUpdateConfig.UpdateServerUri)
               .SetVerifyToken(autoUpdateConfig.VerifyToken)
               .ConfigureContainer(a =>
               {
                   a.AddEasyLogger(LogOut);
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpFileTransfer();//必须添加文件传输插件
                   a.Add<FilePlugin>();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromSeconds(3))
                   .SetMaxFailCount(3);
                   a.UseDmtpRpc()
                    .ConfigureRpcStore(store =>
                    {
                        store.Container.RegisterSingleton<IHostApplicationLifetime>(_appLifetime);
                        store.Container.RegisterSingleton(client);
                        store.RegisterServer<ReverseCallbackServer>();
                    });
               });
        client.Setup(config);
        return client;
    }




    #endregion






}
