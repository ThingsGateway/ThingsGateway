#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/dotnetchina/ThingsGateway
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
    public partial class UdpSessionPage
    {
        public Action<string> LogAction;

        private TouchSocketConfig config;

        private string ip = "127.0.0.1";

        private int port = 502;

        private TGUdpSession tgUdpSession { get; set; } = new();

        public void Dispose()
        {
            tgUdpSession.SafeDispose();
        }
        public TGUdpSession GetTGUdpSession()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetRemoteIPHost(new IPHost(ip + ":" + port)).SetBufferLength(300);
            config.SetBindIPHost(new IPHost(0));
            //��������
            tgUdpSession.Setup(config);
            return tgUdpSession;
        }

        protected override void OnInitialized()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetRemoteIPHost(new IPHost(ip + ":" + port)).SetBufferLength(300);
            config.SetBindIPHost(new IPHost(0));
            tgUdpSession = config.Container.Resolve<TGUdpSession>();
            base.OnInitialized();
        }

        private void LogOut(string str)
        {
            LogAction?.Invoke(str);
        }
    }
}