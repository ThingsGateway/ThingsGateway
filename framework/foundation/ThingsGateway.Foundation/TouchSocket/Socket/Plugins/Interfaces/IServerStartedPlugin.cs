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

namespace ThingsGateway.Foundation.Sockets
{
    /// <summary>
    /// IServerStartedPlugin
    /// </summary>
    public interface IServerStartedPlugin<in TServer> : IPlugin where TServer : IService
    {
        /// <summary>
        /// 当服务器执行<see cref="IService.Start"/>后时。
        /// <para>
        /// 注意：此处并不表示服务器成功启动，具体状态请看<see cref="ServiceStateEventArgs.ServerState"/>
        /// </para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        Task OnServerStarted(TServer sender, ServiceStateEventArgs e);
    }

    /// <summary>
    /// IServerStartedPlugin
    /// </summary>
    public interface IServerStartedPlugin : IServerStartedPlugin<IService>
    {
    }
}