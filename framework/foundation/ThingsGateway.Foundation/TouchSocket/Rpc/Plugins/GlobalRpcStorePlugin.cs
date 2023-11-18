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

namespace ThingsGateway.Foundation.Rpc
{
    /// <summary>
    /// 全局Rpc仓库配置插件。
    /// </summary>
    public class GlobalRpcStorePlugin : PluginBase
    {
        private readonly RpcStore m_rpcStore;

        /// <summary>
        /// 全局Rpc仓库配置插件。
        /// </summary>
        /// <param name="container"></param>
        public GlobalRpcStorePlugin(IContainer container)
        {
            if (container.IsRegistered(typeof(RpcStore)))
            {
                this.m_rpcStore = container.Resolve<RpcStore>();
            }
            else
            {
                this.m_rpcStore = new RpcStore(container);
                container.RegisterSingleton<RpcStore>(this.m_rpcStore);
            }
        }

        /// <summary>
        /// 全局配置Rpc服务
        /// </summary>
        /// <param name="action"></param>
        public void ConfigureRpcStore(Action<RpcStore> action)
        {
            action?.Invoke(this.m_rpcStore);
        }
    }
}