
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation;

/// <summary>
/// 通道管理
/// </summary>
public interface IChannel : ISetupConfigObject, IDisposable ,IClosableClient,IConnectableClient
{
    /// <summary>
    /// 该通道下的所有设备
    /// </summary>
    public ConcurrentList<IProtocol> Collects { get; }

    /// <summary>
    /// Online
    /// </summary>
    public bool Online { get; }

    /// <summary>
    /// 通道启动成功后
    /// </summary>
    public ChannelEventHandler Started { get; set; }

    /// <summary>
    /// 通道停止
    /// </summary>
    public ChannelEventHandler Stoped { get; set; }

    /// <summary>
    /// 通道启动即将成功
    /// </summary>
    public ChannelEventHandler Starting { get; set; }

    /// <summary>
    /// 接收到数据
    /// </summary>
    public ChannelReceivedEventHandler ChannelReceived { get; set; }

    /// <summary>
    /// 通道类型
    /// </summary>
    ChannelTypeEnum ChannelType { get; }

    /// <summary>
    /// 关闭客户端。
    /// </summary>
    /// <param name="msg"></param>
    void Close(string msg);

    /// <summary>
    /// 启动
    /// </summary>
    /// <param name="millisecondsTimeout">最大等待时间</param>
    /// <param name="token">可取消令箭</param>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="Exception"></exception>
    void Connect(int millisecondsTimeout = 3000, CancellationToken token = default);
 

}

/// <summary>
/// ChannelEventHandler
/// </summary>
public delegate Task ChannelEventHandler(IClientChannel channel);

/// <summary>
/// 接收数据
/// </summary>
/// <param name="client"></param>
/// <param name="e"></param>
public delegate Task ChannelReceivedEventHandler(IClientChannel client, ReceivedDataEventArgs e);
