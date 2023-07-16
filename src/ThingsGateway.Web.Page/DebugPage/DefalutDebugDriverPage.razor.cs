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

namespace ThingsGateway.Web.Page
{
    public partial class DefalutDebugDriverPage : DriverDebugUIBase
    {
        public TcpClientPage tcpClientPage;
        public SerialClientPage serialClientPage;
        public TcpServerPage tcpServerPage;
        public UdpSessionPage udpSessionPage;
        [Parameter]
        public override ThingsGateway.Foundation.IReadWriteDevice PLC { get; set; }
        /// <summary>
        /// ͨ��ѡ��1-TCPCLIENT��2-���ڣ�3-UDP��4-TCPServer
        /// </summary>
        [Parameter]
        public int Channel { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                if (tcpClientPage != null)
                    tcpClientPage.LogAction = LogOut;
                if (serialClientPage != null)
                    serialClientPage.LogAction = LogOut;
                if (tcpServerPage != null)
                    tcpServerPage.LogAction = LogOut;
                if (udpSessionPage != null)
                    udpSessionPage.LogAction = LogOut;
                //��������
                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }
        /// <summary>
        /// ģ��
        /// </summary>
        [Parameter]
        public RenderFragment<ThingsGateway.Foundation.IReadWriteDevice> PLCTemplate { get; set; }
        public override void Dispose()
        {
            PLC.SafeDispose();
            if (tcpClientPage != null)
                tcpClientPage.SafeDispose();
            if (serialClientPage != null)
                serialClientPage.SafeDispose();
            if (tcpServerPage != null)
                tcpServerPage.SafeDispose();
            if (udpSessionPage != null)
                udpSessionPage.SafeDispose();
            base.Dispose();
        }

    }
}