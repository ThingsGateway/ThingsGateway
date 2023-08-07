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
using System.Collections.Generic;

using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;

using TouchSocket.Core;

namespace ThingsGateway.OPCDA;
/// <summary>
/// OPC
/// </summary>
public partial class OPCDAClientPage
{
    /// <summary>
    /// ��־���
    /// </summary>
    public Action<LogLevel, object, string, Exception> LogAction;

    /// <summary>
    /// OPC
    /// </summary>
    public ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient OPC;

    /// <summary>
    /// ��־���
    /// </summary>
    public Action<List<ItemReadResult>> ValueAction;

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
        OPC = new ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient(log);
        OPC.DataChangedHandler += Info_DataChangedHandler;
        OPC.Init(node);
        base.OnInitialized();
    }
    private void Connect()
    {
        try
        {
            OPC.Disconnect();
            GetOPCClient().Connect();
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
    private ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient GetOPCClient()
    {
        //��������
        OPC.Init(node);
        return OPC;
    }

    private void Info_DataChangedHandler(List<ItemReadResult> values)
    {
        ValueAction?.Invoke(values);
    }

    private void LogOut(LogLevel logLevel, object source, string message, Exception exception) => LogAction?.Invoke(logLevel, source, message, exception);
}