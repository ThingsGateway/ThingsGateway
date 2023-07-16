#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
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
                LogOut(DateTime.Now.ToDateTimeF() + " 连接失败 - " + ex.Message);
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
            //载入配置
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