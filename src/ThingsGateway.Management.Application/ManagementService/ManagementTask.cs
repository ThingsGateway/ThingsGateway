//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using System.Text.Json.Serialization.Metadata;

using ThingsGateway.Foundation.Common.Json.Extension;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Management;

public partial class ManagementTask : AsyncDisposableObject
{
    private ILog LogMessage;
    private ILogger _logger;
    public TextFileLogger TextLogger { get; private set; }

    public IClientCollection<TcpDmtpSessionClient>? GetKeys()
    {
        if (!_managementConfig.Enable) return null;
        if (_managementConfig.IsServer)
            return _tcpDmtpService.Clients;
        else
            return null;
    }

    public IDmtpActorObject? GetClient(string id = null)
    {
        if (!_managementConfig.Enable) return null;
        if (_managementConfig.IsServer)
            return _tcpDmtpService?.TryGetClient(id, out var result) == true ? result : null;
        else
            return _tcpDmtpClient?.Online == true ? _tcpDmtpClient : null;
    }

    public bool Started => _managementConfig.Enable && (_tcpDmtpClient?.Online == true || _tcpDmtpService?.ServerState == ServerState.Running);


    internal const string LogPathFormat = "Logs/ManagementTask/{0}";

    public ManagementTask(ILogger logger, ManagementConfig managementConfig)
    {
        _logger = logger;
        TextLogger = TextFileLogger.GetMultipleFileLogger(string.Format(LogPathFormat, managementConfig.Name.SanitizeFileName()));
        TextLogger.LogLevel = TouchSocket.Core.LogLevel.Trace;
        var log = new LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        log?.AddLogger(new EasyLogger(Log_Out) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        log?.AddLogger(TextLogger);
        LogMessage = log;

        _managementConfig = managementConfig;

    }

    private void Log_Out(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_managementConfig.Enable) return;

        if (_managementConfig.IsServer)
        {
            _tcpDmtpService ??= await GetTcpDmtpService().ConfigureAwait(false);
        }
        else
        {
            _tcpDmtpClient ??= await GetTcpDmtpClient().ConfigureAwait(false);
        }
        try
        {
            await EnsureChannelOpenAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogMessage?.LogWarning(ex, "Start");
        }
        finally
        {
        }

    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_managementConfig.Enable) return;

        if (_tcpDmtpClient != null)
        {
            await _tcpDmtpClient.CloseAsync().ConfigureAwait(false);
            _tcpDmtpClient.Dispose();
            _tcpDmtpClient = null;
        }
        if (_tcpDmtpService != null)
        {
            await _tcpDmtpService.ClearAsync().ConfigureAwait(false);
            _tcpDmtpService.Dispose();
            _tcpDmtpService = null;
        }

    }

    private TcpDmtpClient? _tcpDmtpClient;
    private TcpDmtpService? _tcpDmtpService;
    private ManagementConfig _managementConfig;

    private async Task<TcpDmtpClient> GetTcpDmtpClient()
    {
        var tcpDmtpClient = new TcpDmtpClient();
        var config = new TouchSocketConfig()
               .SetRemoteIPHost(_managementConfig.ServerUri)
               .SetAdapterOption(a => a.MaxPackageSize = 1024 * 1024 * 1024)
               .SetDmtpOption(a => a.VerifyToken = _managementConfig.VerifyToken)
               .ConfigureContainer(a =>
               {
                   a.AddDmtpRouteService();//添加路由策略
                   a.AddLogger(LogMessage);
               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();


                   a.UseReconnection<TcpDmtpClient>(options =>
                   {
                       options.PollingInterval = TimeSpan.FromMilliseconds(_managementConfig.HeartbeatInterval);
                       options.LogReconnection = true;
                       options.UseDmtpCheckAction();
                       options.CheckAction = async (c) =>
                       {
                           using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                           if ((await c.PingAsync(cts.Token).ConfigureAwait(false)).IsSuccess)
                           {
                               return ConnectionCheckResult.Alive;
                           }
                           else
                           {
                               return ConnectionCheckResult.Dead;
                           }
                       };
                       options.ConnectAction = async (client, cancellationToken) =>
                       {
                           var attempts = 0;
                           var currentInterval = options.BaseInterval;

                           while (options.MaxRetryCount < 0 || attempts < options.MaxRetryCount)
                           {
                               if (cancellationToken.IsCancellationRequested)
                               {
                                   return;
                               }
                               if (client.GetPauseReconnection())
                               {
                                   continue;
                               }

                               attempts++;

                               try
                               {
                                   if (client.Online)
                                   {
                                       options.OnSuccessed?.Invoke(client);
                                       return;
                                   }

                                   await client.ConnectAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                                   options.OnSuccessed?.Invoke(client);

                                   if (options.LogReconnection)
                                   {
                                       client.Logger?.Info(options, $"重连成功，尝试次数: {attempts}");
                                   }
                                   return;
                               }
                               catch (Exception ex)
                               {
                                   if (cancellationToken.IsCancellationRequested)
                                   {
                                       return;
                                   }
                                   options.OnFailed?.Invoke(client, attempts, ex);

                                   if (options.LogReconnection)
                                   {
                                       client.Logger?.Warning(options, $"重连失败，尝试次数: {attempts}，错误: {ex.Message}");
                                   }

                                   if (options.MaxRetryCount > 0 && attempts >= options.MaxRetryCount)
                                   {
                                       options.OnGiveUp?.Invoke(client, attempts);
                                       if (options.LogReconnection)
                                       {
                                           client.Logger?.Error(options, $"达到最大重连次数 {options.MaxRetryCount}，放弃重连");
                                       }
                                       return;
                                   }

                                   // 计算下次重连间隔
                                   currentInterval = CalculateNextInterval(options, attempts, currentInterval);

                                   await Task.Delay(currentInterval, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                               }
                           }
                       };


                   });

                   a.UseDmtpRpc(a => a.ConfigureDefaultSerializationSelector(b =>
                   {
                       b.UseSystemTextJson(json =>
                       {
                           json.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                           json.Converters.Add(new SystemTextJsonByteArrayToNumberArrayConverter());
                           json.Converters.Add(new JTokenSystemTextJsonConverter());
                           json.Converters.Add(new JValueSystemTextJsonConverter());
                           json.Converters.Add(new JObjectSystemTextJsonConverter());
                           json.Converters.Add(new JArraySystemTextJsonConverter());
                           json.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                       });
                   }));

                   a.UseDmtpFileTransfer();//必须添加文件传输插件


                   a.Add<FilePlugin>();

                   a.AddDmtpCreatedChannelPlugin(() =>
                   {
                       return tcpDmtpClient.ResetIdAsync($"{_managementConfig.Name}:{ManagementGlobalData.HardwareJob.HardwareInfo.UUID}", tcpDmtpClient.ClosedToken);
                   });
               });

        await tcpDmtpClient.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpClient;
    }
    /// <summary>
    /// 计算下次重连间隔
    /// </summary>
    /// <param name="reconnectionOption">option</param>
    /// <param name="attemptCount">当前尝试次数</param>
    /// <param name="currentInterval">当前间隔</param>
    /// <returns>下次重连间隔</returns>
    private static TimeSpan CalculateNextInterval(ReconnectionOption<TcpDmtpClient> reconnectionOption, int attemptCount, TimeSpan currentInterval)
    {
        return reconnectionOption.Strategy switch
        {
            ReconnectionStrategy.Simple => reconnectionOption.BaseInterval,
            ReconnectionStrategy.ExponentialBackoff => TimeSpan.FromMilliseconds(Math.Min(
                reconnectionOption.BaseInterval.TotalMilliseconds * Math.Pow(reconnectionOption.BackoffMultiplier, attemptCount - 1),
                reconnectionOption.MaxInterval.TotalMilliseconds)),
            ReconnectionStrategy.LinearBackoff => TimeSpan.FromMilliseconds(Math.Min(
                reconnectionOption.BaseInterval.TotalMilliseconds + (attemptCount - 1) * reconnectionOption.BackoffMultiplier,
                reconnectionOption.MaxInterval.TotalMilliseconds)),
            _ => reconnectionOption.BaseInterval
        };
    }
    private async Task<TcpDmtpService> GetTcpDmtpService()
    {
        var tcpDmtpService = new TcpDmtpService();
        var config = new TouchSocketConfig()
               .SetListenIPHosts(_managementConfig.ServerUri)
                   .SetAdapterOption(a => a.MaxPackageSize = 1024 * 1024 * 1024)
               .SetDmtpOption(a => a.VerifyToken = _managementConfig.VerifyToken)
               .ConfigureContainer(a =>
               {
                   a.AddDmtpRouteService();//添加路由策略
                   a.AddLogger(LogMessage);
               })
               .ConfigurePlugins(a =>
               {
                   a.UseTcpSessionCheckClear();

                   a.UseDmtpRpc(a => a.ConfigureDefaultSerializationSelector(b =>
                   {
                       b.UseSystemTextJson(json =>
                       {
                           json.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
                           json.Converters.Add(new SystemTextJsonByteArrayToNumberArrayConverter());
                           json.Converters.Add(new JTokenSystemTextJsonConverter());
                           json.Converters.Add(new JValueSystemTextJsonConverter());
                           json.Converters.Add(new JObjectSystemTextJsonConverter());
                           json.Converters.Add(new JArraySystemTextJsonConverter());
                           json.TypeInfoResolver = new DefaultJsonTypeInfoResolver();
                       });
                   }));

                   a.UseDmtpFileTransfer();//必须添加文件传输插件

                   a.Add<FilePlugin>();


               });

        await tcpDmtpService.SetupAsync(config).ConfigureAwait(false);
        return tcpDmtpService;
    }

    private async Task EnsureChannelOpenAsync(CancellationToken cancellationToken)
    {
        if (_managementConfig.IsServer)
        {
            if (_tcpDmtpService.ServerState != ServerState.Running)
            {
                if (_tcpDmtpService.ServerState != ServerState.Stopped)
                    await _tcpDmtpService.StopAsync(cancellationToken).ConfigureAwait(false);

                await _tcpDmtpService.StartAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        else
        {
            if (!_tcpDmtpClient.Online)
                await _tcpDmtpClient.ConnectAsync().ConfigureAwait(false);
        }
    }


    protected override async Task DisposeAsync(bool disposing)
    {
        if (_tcpDmtpClient != null)
        {
            await _tcpDmtpClient.CloseAsync().ConfigureAwait(false);
            _tcpDmtpClient.SafeDispose();
            _tcpDmtpClient = null;
        }
        if (_tcpDmtpService != null)
        {
            await _tcpDmtpService.ClearAsync().ConfigureAwait(false);
            _tcpDmtpService.SafeDispose();
            _tcpDmtpService = null;
        }

        TextLogger?.TryDispose();
        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
}
