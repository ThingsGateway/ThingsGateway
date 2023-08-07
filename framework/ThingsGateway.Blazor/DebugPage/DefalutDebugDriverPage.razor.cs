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

using ThingsGateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Blazor;

/// <summary>
/// <inheritdoc/>
/// </summary>
public enum ChannelEnum
{
    /// <inheritdoc/>
    None = 0,
    /// <inheritdoc/>
    TcpClientEx = 1,
    /// <inheritdoc/>
    SerialPort = 2,
    /// <inheritdoc/>
    UdpSession = 3,
    /// <inheritdoc/>
    TcpServer = 4,
}
/// <inheritdoc/>
public partial class DefalutDebugDriverPage : DriverDebugUIBase
{
    /// <summary>
    /// SerialClientPage
    /// </summary>
    public SerialClientPage SerialClientPage;
    /// <summary>
    /// TcpClientPage
    /// </summary>
    public TcpClientPage TcpClientPage;
    /// <summary>
    /// TcpServerPage
    /// </summary>
    public TcpServerPage TcpServerPage;
    /// <summary>
    /// UdpSessionPage
    /// </summary>
    public UdpSessionPage UdpSessionPage;
    /// <summary>
    /// ѡ��1-TCPCLIENT��2-���ڣ�3-UDP��4-TCPServer
    /// </summary>
    [Parameter]
    public ChannelEnum Channel { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ThingsGateway.Foundation.IReadWriteDevice Plc { get; set; }

    /// <summary>
    /// ģ��
    /// </summary>
    [Parameter]
    public RenderFragment ChildContent { get; set; }
    /// <summary>
    /// �Զ���ģ��
    /// </summary>
    [Parameter]
    public RenderFragment OtherContent { get; set; }
    /// <inheritdoc/>
    public override void Dispose()
    {
        Plc?.SafeDispose();
        TcpClientPage?.SafeDispose();
        SerialClientPage?.SafeDispose();
        TcpServerPage?.SafeDispose();
        UdpSessionPage?.SafeDispose();
        base.Dispose();
    }
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="firstRender"></param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (TcpClientPage != null)
                TcpClientPage.LogAction = LogOut;
            if (SerialClientPage != null)
                SerialClientPage.LogAction = LogOut;
            if (TcpServerPage != null)
                TcpServerPage.LogAction = LogOut;
            if (UdpSessionPage != null)
                UdpSessionPage.LogAction = LogOut;
            //��������
            StateHasChanged();
        }

        base.OnAfterRender(firstRender);
    }
}