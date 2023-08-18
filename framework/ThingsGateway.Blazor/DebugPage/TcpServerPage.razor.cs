﻿#region copyright
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

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Blazor;

/// <inheritdoc/>
public partial class TcpServerPage
{
    /// <summary>
    /// 日志输出
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    private TouchSocketConfig config;

    private string ip = "127.0.0.1";

    private int port = 502;

    private TcpService TcpServer { get; set; } = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        TcpServer.SafeDispose();
    }
    private void Connect()
    {
        try
        {
            TcpServer.Stop();
            GetTcpServer().Start();
        }
        catch (Exception ex)
        {

            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }

    }
    private void DisConnect()
    {
        try
        {
            TcpServer.Stop();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }
    /// <summary>
    /// 获取对象
    /// </summary>
    /// <returns></returns>
    public TcpService GetTcpServer()
    {
        config ??= new TouchSocketConfig();
        var LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        config.SetListenIPHosts(new IPHost[] { new IPHost(ip + ":" + port) });
        config.SetBufferLength(300);
        //载入配置
        TcpServer.Setup(config);
        return TcpServer;
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        config?.Dispose();
        config = new TouchSocketConfig();
        var LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        config.SetListenIPHosts(new IPHost[] { new IPHost(ip + ":" + port) });
        config.SetBufferLength(300);
        TcpServer = new TcpService();
        TcpServer.Setup(config);
        base.OnInitialized();
    }

    private void LogOut(LogLevel logLevel, object source, string message, Exception exception) => LogAction?.Invoke(logLevel, source, message, exception);

}