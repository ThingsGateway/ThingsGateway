#region copyright
//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://diego2098.gitee.io/thingsgateway/
//  QQȺ��605534569
//------------------------------------------------------------------------------
#endregion

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;

using TouchSocket.Core;

namespace ThingsGateway.OPCDA
{
    public partial class OPCDAClientPage
    {
        public void Dispose()
        {
            OPC.SafeDispose();
        }

        private OPCNode node = new OPCNode();
        public Action<string> LogAction;
        public Action<List<ItemReadResult>> ValueAction;
        public ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient OPC;
        private void LogOut(string str)
        {
            LogAction?.Invoke(str);
        }

        protected override void OnInitialized()
        {
            OPC = new ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient(new TGEasyLogger(LogOut));
            OPC.DataChangedHandler += Info_DataChangedHandler;
            OPC.Init(node);
            base.OnInitialized();
        }

        private void Info_DataChangedHandler(List<ItemReadResult> values)
        {
            ValueAction?.Invoke(values);
        }

        private ThingsGateway.Foundation.Adapter.OPCDA.OPCDAClient GetOPCClient()
        {
            //��������
            OPC.Init(node);
            return OPC;
        }
    }
}