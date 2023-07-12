#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
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
            //载入配置
            OPC.Init(node);
            return OPC;
        }
    }
}