
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation
{
    /// <summary>
    /// ISerialSendingPlugin
    /// </summary>
    public interface ISerialSendingPlugin<in TClient> : IPlugin where TClient : ISerialPortClient
    {
        /// <summary>
        /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnSerialSending(TClient client, SendingEventArgs e);
    }

    /// <summary>
    /// ISerialSendingPlugin
    /// </summary>
    public interface ISerialSendingPlugin : ISerialSendingPlugin<ISerialPortClient>
    {
    }
}