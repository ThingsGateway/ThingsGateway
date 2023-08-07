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

using System;
using System.Threading.Tasks;

using ThingsGateway.Foundation.Adapter.OPCUA;

using TouchSocket.Core;

namespace ThingsGateway.OPCUA;

/// <summary>
/// OPCUAClientPage
/// </summary>
public partial class OPCUAClientPage
{
    /// <summary>
    /// ��־���
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;
    /// <summary>
    /// OPC
    /// </summary>
    public ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient OPC;

    private readonly OPCNode node = new();


    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public void Dispose()
    {
        OPC.SafeDispose();
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        var log = new EasyLogger(LogOut)
        {
            LogLevel = LogLevel.Trace
        };
        OPC = new ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient(log);
        base.OnInitialized();
    }

    private async Task ConnectAsync()
    {
        try
        {
            OPC.Disconnect();
            await GetOPCClient().ConnectAsync();
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
            OPC.Disconnect();
        }
        catch (Exception ex)
        {
            LogAction?.Invoke(LogLevel.Error, null, null, ex);
        }
    }
    private ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient GetOPCClient()
    {
        OPC.OPCNode = node;
        return OPC;
    }

    private void LogOut(LogLevel logLevel, object source, string message, Exception exception) => LogAction?.Invoke(logLevel, source, message, exception);

}