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

namespace ThingsGateway.Foundation.Serial;

/// <summary>
/// 串口插件接口
/// </summary>
public interface ISerialPlugin : IPlugin, IOpeningPlugin, IOpenedPlugin, IClosingPlugin, IClosedPlguin
{
    /// <summary>
    /// 在收到数据时触发
    /// </summary>
    /// <param name="client">客户端</param>
    /// <param name="e">参数</param>
    [AsyncRaiser]
    void OnReceivedData(ISerialClientBase client, ReceivedDataEventArgs e);

    /// <summary>
    /// 在收到数据时触发
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    Task OnReceivedDataAsync(ISerialClientBase client, ReceivedDataEventArgs e);

    /// <summary>
    /// 在刚收到数据时触发，即在适配器之前。
    /// </summary>
    /// <param name="client">客户端</param>
    /// <param name="e">参数</param>
    [AsyncRaiser]
    void OnReceivingData(ISerialClientBase client, ByteBlockEventArgs e);

    /// <summary>
    /// 在刚收到数据时触发，即在适配器之前。
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    Task OnReceivingDataAsync(ISerialClientBase client, ByteBlockEventArgs e);

    /// <summary>
    /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
    /// </summary>
    /// <param name="client">客户端</param>
    /// <param name="e">参数</param>
    [AsyncRaiser]
    void OnSendingData(ISerialClientBase client, SendingEventArgs e);

    /// <summary>
    /// 当即将发送数据时，调用该方法在适配器之后，接下来即会发送数据。
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    Task OnSendingDataAsync(ISerialClientBase client, SendingEventArgs e);
}