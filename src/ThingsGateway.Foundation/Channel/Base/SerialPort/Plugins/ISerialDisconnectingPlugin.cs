
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 即将断开连接(仅主动断开时有效)。
    /// </summary>
    public interface ISerialDisconnectingPlugin<in TClient> : IPlugin where TClient : ISerialPortClient
    {
        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnSerialDisconnecting(TClient client, ClosingEventArgs e);
    }

    /// <summary>
    /// ISerialDisconnectingPlugin
    /// </summary>
    public interface ISerialDisconnectingPlugin : ISerialDisconnectingPlugin<ISerialPortClient>
    {
    }
}
