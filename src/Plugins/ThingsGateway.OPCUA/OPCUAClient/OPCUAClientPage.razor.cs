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

using Newtonsoft.Json.Linq;

using Opc.Ua;

using ThingsGateway.Core;
using ThingsGateway.Foundation.Adapter.OPCUA;

using TouchSocket.Core;

namespace ThingsGateway.OPCUA
{
    public partial class OPCUAClientPage
    {
        public async Task Reconnect()
        {
            try
            {
                OPC.Disconnect();
                await GetOPCClient().ConnectAsync();
            }
            catch (Exception ex)
            {
                LogOut(DateTime.Now.ToDateTimeF() + " ����ʧ�� - " + ex.Message);
            }
        }

        public void Dispose()
        {
            OPC.SafeDispose();
        }

        private OPCNode node = new OPCNode();
        private string username;
        private string password;
        public Action<string> LogAction;
        public ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient OPC;
        private void LogOut(string str)
        {
            LogAction?.Invoke(str);
        }

        protected override void OnInitialized()
        {
            OPC = new ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient();
            OPC.DataChangedHandler += Info_DataChangedHandler;
            OPC.OpcStatusChange += Info_OpcStatusChange;
            base.OnInitialized();
        }

        private void Info_DataChangedHandler((NodeId id, DataValue dataValue, JToken jToken) item)
        {
            LogAction?.Invoke(DateTime.Now.ToDateTimeF() + item.id + ":" + item.jToken);
        }

        private void Info_OpcStatusChange(object sender, OPCUAStatusEventArgs e)
        {
            LogAction?.Invoke(DateTime.Now.ToDateTimeF() + e.Text);
        }



        private ThingsGateway.Foundation.Adapter.OPCUA.OPCUAClient GetOPCClient()
        {
            //��������
            if (!username.IsNullOrEmpty())
            {
                OPC.UserIdentity = new UserIdentity(username, password);
            }
            else
            {
                OPC.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
            }

            OPC.OPCNode = node;
            return OPC;
        }
    }
}