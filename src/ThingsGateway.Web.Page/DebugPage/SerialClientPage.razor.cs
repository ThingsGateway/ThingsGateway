#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/dotnetchina/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using System;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Serial;

using TouchSocket.Core;

namespace ThingsGateway.Web.Page
{
    public partial class SerialClientPage
    {
        public Action<string> LogAction;

        private TouchSocketConfig config;

        private SerialProperty serialProperty = new SerialProperty();

        private SerialClient serialClient { get; set; } = new();

        public void Dispose()
        {
            serialClient.SafeDispose();
        }
        public SerialClient GetSerialClient()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            config.SetSerialProperty(serialProperty);
            //��������
            serialClient.Setup(config);
            return serialClient;
        }

        protected override void OnInitialized()
        {
            config = new TouchSocketConfig();
            var logMessage = new TouchSocket.Core.LoggerGroup();
            logMessage.AddLogger(new TGEasyLogger(LogOut));
            config.ConfigureContainer(a => a.RegisterSingleton<ILog>(logMessage));
            serialClient = config.Container.Resolve<SerialClient>();
            base.OnInitialized();
        }

        private void LogOut(string str)
        {
            LogAction?.Invoke(str);
        }
    }
}