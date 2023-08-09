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

using Microsoft.AspNetCore.Components;

using ThingsGateway.Foundation;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Blazor;

/// <inheritdoc/>
public partial class TcpClientPage
{
    /// <summary>
    /// ��־���
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    private TouchSocketConfig config;
    /// <summary>
    /// IP
    /// </summary>
    private string IP = "127.0.0.1";
    /// <summary>
    /// �˿�
    /// </summary>
    [Parameter]
    public int Port { get; set; } = 502;

    private TcpClientEx TcpClientEx { get; set; } = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        TcpClientEx.SafeDispose();
    }

    private async Task ConnectAsync()
    {
        try
        {
            TcpClientEx.Close();
            await GetTcpClient().ConnectAsync();
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
            TcpClientEx.Close();
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
    public TcpClientEx GetTcpClient()
    {
        config ??= new TouchSocketConfig();
        var LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        config.SetRemoteIPHost(new IPHost(IP + ":" + Port)).SetBufferLength(300);
        //��������
        TcpClientEx.Setup(config);
        return TcpClientEx;
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        config?.Dispose();
        config = new TouchSocketConfig();
        var LogMessage = new TouchSocket.Core.LoggerGroup() { LogLevel = TouchSocket.Core.LogLevel.Trace };
        LogMessage.AddLogger(new EasyLogger(LogOut) { LogLevel = TouchSocket.Core.LogLevel.Trace });
        config.ConfigureContainer(a => a.RegisterSingleton<ILog>(LogMessage));
        config.SetRemoteIPHost(new IPHost(IP + ":" + Port)).SetBufferLength(300);
        TcpClientEx = new TcpClientEx();
        TcpClientEx.Setup(config);
        base.OnInitialized();
    }

    private void LogOut(LogLevel logLevel, object source, string message, Exception exception) => LogAction?.Invoke(logLevel, source, message, exception);

}