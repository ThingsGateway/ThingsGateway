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

using ThingsGateway.Foundation.Resources;

namespace ThingsGateway.Foundation.Dmtp.Rpc
{
    /// <summary>
    /// DmtpRpcActorExtension
    /// </summary>
    public static class DmtpRpcActorExtension
    {
        #region DependencyProperty

        /// <summary>
        /// DmtpRpcActor
        /// </summary>
        public static readonly DependencyProperty<IDmtpRpcActor> DmtpRpcActorProperty =
            DependencyProperty<IDmtpRpcActor>.Register("DmtpRpcActor", default);

        #endregion DependencyProperty

        /// <summary>
        /// 新创建一个直接向目标地址请求的<see cref="IRpcClient"/>客户端。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="targetId"></param>
        public static IRpcClient CreateTargetDmtpRpcActor(this IDmtpActorObject client, string targetId)
        {
            return new TargetDmtpRpcActor(targetId, client.GetDmtpRpcActor());
        }

        /// <summary>
        /// 从<see cref="DmtpActor"/>中获取<see cref="IDmtpRpcActor"/>
        /// </summary>
        /// <param name="dmtpActor"></param>
        /// <returns></returns>
        public static IDmtpRpcActor GetDmtpRpcActor(this IDmtpActor dmtpActor)
        {
            return dmtpActor.GetValue(DmtpRpcActorProperty);
        }

        /// <summary>
        /// 从<see cref="IDmtpActorObject"/>中获取<see cref="IDmtpRpcActor"/>，以实现Rpc调用功能。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IDmtpRpcActor GetDmtpRpcActor(this IDmtpActorObject client)
        {
            var dmtpRpcActor = client.DmtpActor.GetDmtpRpcActor();
            if (dmtpRpcActor is null)
            {
                throw new ArgumentNullException(nameof(dmtpRpcActor), TouchSocketDmtpResource.DmtpRpcActorArgumentNull.GetDescription());
            }
            return dmtpRpcActor;
        }

        /// <summary>
        /// 从<see cref="IDmtpActorObject"/>中获取继承实现<see cref="IDmtpRpcActor"/>的功能件，以实现Rpc调用功能。
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TDmtpRpcActor GetDmtpRpcActor<TDmtpRpcActor>(this IDmtpActorObject client) where TDmtpRpcActor : IDmtpRpcActor
        {
            var dmtpRpcActor = client.DmtpActor.GetDmtpRpcActor();
            if (dmtpRpcActor is null)
            {
                throw new ArgumentNullException(nameof(dmtpRpcActor), TouchSocketDmtpResource.DmtpRpcActorArgumentNull.GetDescription());
            }
            return (TDmtpRpcActor)dmtpRpcActor;
        }

        /// <summary>
        /// 向<see cref="DmtpActor"/>中设置<see cref="IDmtpRpcActor"/>
        /// </summary>
        /// <param name="dmtpActor"></param>
        /// <param name="dmtpRpcActor"></param>
        internal static void SetDmtpRpcActor(this IDmtpActor dmtpActor, IDmtpRpcActor dmtpRpcActor)
        {
            dmtpActor.SetValue(DmtpRpcActorProperty, dmtpRpcActor);
        }

        #region 插件扩展

        /// <summary>
        /// 使用DmtpRpc插件
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public static DmtpRpcFeature UseDmtpRpc(this IPluginManager pluginManager)
        {
            return pluginManager.Add<DmtpRpcFeature>();
        }

        /// <summary>
        /// 使用自定义的DmtpRpc插件。
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public static DmtpRpcFeature UseDmtpRpc<TDmtpRpcFeature>(this IPluginManager pluginManager) where TDmtpRpcFeature : DmtpRpcFeature
        {
            return pluginManager.Add<TDmtpRpcFeature>();
        }

        #endregion 插件扩展
    }
}