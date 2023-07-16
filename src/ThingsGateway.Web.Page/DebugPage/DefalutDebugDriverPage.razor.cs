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
        /// 通道选择，1-TCPCLIENT，2-串口，3-UDP，4-TCPServer
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
                //载入配置
                StateHasChanged();
            }

            base.OnAfterRender(firstRender);
        }
        /// <summary>
        /// 模板
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