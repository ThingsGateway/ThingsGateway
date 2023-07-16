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

using ThingsGateway.Foundation;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Web.Page
{
    public partial class TcpServerPage
    {
        public Action<string> LogAction;

        private TouchSocketConfig config;

        private string ip = "127.0.0.1";

        private int port = 502;

        private TcpService tgTcpServer { get; set; } = new();

        public void Dispose()
        {
            tgTcpServer.SafeDispose();
        }
        public TcpService GetTGTcpServer()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetListenIPHosts(new IPHost[] { new IPHost(ip + ":" + port) });
            config.SetBufferLength(300);
            //��������
            tgTcpServer.Setup(config);
            return tgTcpServer;
        }

        protected override void OnInitialized()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetListenIPHosts(new IPHost[] { new IPHost(ip + ":" + port) });
            config.SetBufferLength(300);
            tgTcpServer = config.Container.Resolve<TcpService>();
            base.OnInitialized();
        }

        private void LogOut(string str)
        {
            LogAction?.Invoke(str);
        }
    }
}