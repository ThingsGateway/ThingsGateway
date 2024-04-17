
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------



using Mapster;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using System.Runtime.InteropServices;

using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.FileTransfer;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Rpc;
using TouchSocket.Sockets;

namespace ThingsGateway.Gateway.Application;

public class ManagementOptions
{
    /// <summary>
    /// 获取或设置远程 URI，用于通信。
    /// </summary>
    public string RemoteUri { get; set; }

    /// <summary>
    /// 获取或设置服务器的备用 URI。
    /// </summary>
    public string ServerStandbyUri { get; set; }

    /// <summary>
    /// 获取或设置通信端口号。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 获取或设置用于验证的令牌。
    /// </summary>
    public string VerifyToken { get; set; }

    /// <summary>
    /// 获取或设置心跳间隔。
    /// </summary>
    public int HeartbeatInterval { get; set; }

    /// <summary>
    /// 获取或设置允许的最大错误计数。
    /// </summary>
    public int MaxErrorCount { get; set; }

    /// <summary>
    /// 获取或设置冗余选项。
    /// </summary>
    public Redundancy Redundancy { get; set; }
}

public class Redundancy
{
    /// <summary>
    /// 获取或设置是否启用冗余。
    /// </summary>
    public bool Enable { get; set; }

    /// <summary>
    /// 获取或设置是否为主设备。
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 获取或设置是否为启动业务的设备。
    /// </summary>
    public bool IsStartBusinessDevice { get; set; }
}

public class ManagementHostedService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IStringLocalizer<ManagementHostedService> Localizer;

    /// <inheritdoc cref="ManagementHostedService"/>
    public ManagementHostedService(ILogger<ManagementHostedService> logger, IStringLocalizer<ManagementHostedService> localizer)
    {
        _logger = logger;
        Localizer = localizer;
    }

    #region worker服务

    /// <summary>
    /// 是否启动采集
    /// </summary>
    internal bool StartCollectDeviceEnable
    {
        get
        {
            return isStart;
        }
        set
        {
            if (isStart != value)
            {
                isStart = value;
                if (StartCollectDeviceEnable)
                {
                    //启动采集
                    _ = HostedServiceUtil.CollectDeviceHostedService.StartAsync();
                }
                else
                {
                    //停止采集
                    _ = HostedServiceUtil.CollectDeviceHostedService.StopAsync(!StartBusinessDeviceEnable);
                }
            }
        }
    }

    private volatile bool isStart = false;

    /// <summary>
    /// 是否启动业务设备
    /// </summary>
    internal volatile bool StartBusinessDeviceEnable = true;

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation(Localizer["Start"]);
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation(Localizer["Stop"]);
        return base.StopAsync(cancellationToken);
    }

    internal ManagementOptions Options;

    /// <summary>
    /// 启动锁
    /// </summary>
    internal EasyLock StartLock = new(true);

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        await Task.Delay(1000);
        Options = App.Configuration.GetSection(nameof(ManagementOptions)).Get<ManagementOptions?>() ?? new();
        StartBusinessDeviceEnable = Options?.Redundancy?.Enable == true ? Options?.Redundancy?.IsStartBusinessDevice == true : true;
        if (Options?.Redundancy?.Enable == true)
        {
            var udpDmtp = GetUdpDmtp(Options);
            await udpDmtp.StartAsync();//启动

            if (Options.Redundancy.IsPrimary)
            {
                //初始化时，主站直接启动
                StartCollectDeviceEnable = true;
                StartLock.Release();
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool online = false;
                    var waitInvoke = new DmtpInvokeOption(millisecondsTimeout: 5000)
                    {
                        FeedbackType = FeedbackType.WaitInvoke,
                        Token = stoppingToken,
                        Timeout = 3000,
                        SerializationType = SerializationType.Json
                    };
                    try
                    {
                        // 声明一个可空的 GatewayState 变量，初始化为 null
                        GatewayState? gatewayState = null;

                        // 发送 Ping 请求以检查设备是否在线，超时时间为 3000 毫秒
                        online = await udpDmtp.PingAsync(3000);

                        // 如果设备在线
                        if (online)
                        {
                            // 初始化读取错误计数器
                            var readErrorCount = 0;

                            // 当读取错误次数小于最大错误计数时循环执行
                            while (readErrorCount < Options.MaxErrorCount)
                            {
                                try
                                {
                                    // 尝试调用反向回调服务器的 GetGatewayStateAsync 方法获取网关状态
                                    gatewayState = await udpDmtp.GetDmtpRpcActor().InvokeTAsync<GatewayState>(nameof(ReverseCallbackServer.GetGatewayStateAsync), waitInvoke, StartCollectDeviceEnable);

                                    // 如果成功获取网关状态，则跳出循环
                                    break;
                                }
                                catch
                                {
                                    // 捕获异常，增加读取错误计数器
                                    readErrorCount++;
                                }
                            }
                        }

                        // 检查 gatewayState 是否为 null
                        if (gatewayState == null)
                        {
                            // 无法获取状态，启动本机

                            // 如果 IsStart 为 false，则表示当前设备不是启动业务的设备
                            if (!StartCollectDeviceEnable)
                            {
                                // 输出日志，指示无法连接冗余站点，本机将切换到正常状态
                                _logger.LogInformation(Localizer["SwitchPrimaryState"]);

                                // 将 IsStart 设置为 true，表示当前设备为启动业务的设备
                                StartCollectDeviceEnable = true;
                            }
                        }
                        // 如果 gatewayState 表示主站已经启动
                        else if (gatewayState.IsPrimary)
                        {
                            // 主站已经启动

                            // 如果主站已经启动
                            if (gatewayState.IsStart)
                            {
                                // 如果当前设备是从站（IsStart 为 true），则表示主站已经恢复，从站将切换到备用状态
                                if (StartCollectDeviceEnable)
                                {
                                    // 输出日志，指示主站已恢复，从站将切换到备用状态
                                    _logger.LogInformation(Localizer["SwitchStandbyState"]);

                                    // 将 IsStart 设置为 false，表示当前设备为从站，切换到备用状态
                                    StartCollectDeviceEnable = false;
                                }
                            }
                            else
                            {
                                // 主站未启动，等待主站切换到正常后再停止从站
                            }
                        }
                        // 如果 gatewayState 表示从站已经启动
                        else
                        {
                            // 从站已经启动

                            // 如果从站已经启动
                            if (gatewayState.IsStart)
                            {
                                // 等待从站切换到备用后，再启动主站
                            }
                            else
                            {
                                // 如果当前设备是主站且未启动（IsStart 为 false），则表示从站已经切换到备用状态，主站将切换到正常状态
                                if (!StartCollectDeviceEnable)
                                {
                                    // 输出日志，指示本机(主站)将切换到正常状态
                                    _logger.LogInformation(Localizer["SwitchNormalState"]);

                                    // 将 IsStart 设置为 true，表示当前设备为主站，切换到正常状态
                                    StartCollectDeviceEnable = true;
                                }
                            }
                        }
                    }
                    finally
                    {
                        StartLock.Release();
                    }
                    // 如果 Options.Redundancy 表示当前设备为主站
                    if (Options.Redundancy.IsPrimary)
                    {
                        try
                        {
                            // 如果 online 为 true，表示设备在线
                            if (online)
                            {
                                // 调用 udpDmtp.GetDmtpRpcActor() 的 ReverseCallbackServer.UpdateGatewayDataAsync 方法，
                                // 将 GlobalData.CollectDevices 和 GlobalData.Variables 同步到从站
                                await udpDmtp.GetDmtpRpcActor().InvokeAsync(nameof(ReverseCallbackServer.UpdateGatewayDataAsync), waitInvoke, GlobalData.CollectDevices.Adapt<Dictionary<string, DeviceDataWithValue>>(), GlobalData.Variables.Adapt<Dictionary<string, VariableDataWithValue>>());
                            }
                        }
                        catch (Exception ex)
                        {
                            // 输出警告日志，指示同步数据到从站时发生错误
                            _logger.LogWarning(ex, Localizer["ErrorSynchronizingData"]);
                        }
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Execute");
                }
            }
        }
        else
        {
            //直接启动
            StartCollectDeviceEnable = true;
            //无冗余，直接启动采集服务
            _logger.LogInformation(Localizer["RedundancyDisable"]);
            StartLock.Release();
        }
    }

    #endregion worker服务

    #region

    private void LogOut(TouchSocket.Core.LogLevel logLevel, object source, string message, Exception exception)
    {
        _logger?.Log_Out(logLevel, source, message, exception);
    }

    private UdpDmtp GetUdpDmtp(ManagementOptions options)
    {
        var udpDmtp = new UdpDmtp();
        var config = new TouchSocketConfig()
               .SetRemoteIPHost(options.RemoteUri)
               .SetBindIPHost(options.Port)
               .SetDmtpOption(
            new DmtpOption() { VerifyToken = options.VerifyToken })
               .ConfigureContainer(a =>
               {
                   a.AddEasyLogger(LogOut);
                   a.AddRpcStore(store =>
                   {
                       store.RegisterServer<ReverseCallbackServer>();
                   });
               })
               .ConfigurePlugins(a =>
               {
                   a.UseDmtpFileTransfer();//必须添加文件传输插件
                                           //a.Add<FilePlugin>();
                   a.UseDmtpHeartbeat()//使用Dmtp心跳
                   .SetTick(TimeSpan.FromMilliseconds(options.HeartbeatInterval))
                   .SetMaxFailCount(options.MaxErrorCount);
                   a.UseDmtpRpc();
               });
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            config.UseUdpConnReset();
        }
        udpDmtp.Setup(config);
        return udpDmtp;
    }

    #endregion
}

internal class GatewayState
{
    /// <summary>
    /// 是否启动
    /// </summary>
    public bool IsStart { get; set; }

    /// <summary>
    /// 是否主站
    /// </summary>
    public bool IsPrimary { get; set; }
}