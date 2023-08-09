#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway-docs/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Blazor;

/// <inheritdoc/>
public partial class TcpServerPage
{
    /// <summary>
    /// ��־���
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
    /// ��ȡ����
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
        //��������
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