//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

namespace ThingsGateway.Foundation;

/// <summary>
/// 通道管理
/// </summary>
public interface IChannel : ISetupConfigObject, IDisposable, IClosableClient, IConnectableClient
{
    /// <summary>
    /// 接收数据事件
    /// </summary>
    public ChannelReceivedEventHandler ChannelReceived { get; }

    /// <summary>
    /// 通道配置
    /// </summary>
    public IChannelOptions ChannelOptions { get; }

    /// <summary>
    /// 通道类型
    /// </summary>
    ChannelTypeEnum ChannelType { get; }

    /// <summary>
    /// 通道下的所有设备
    /// </summary>
    public ConcurrentList<IDevice> Collects { get; }

    /// <summary>
    /// Online
    /// </summary>
    public bool Online { get; }

    /// <summary>
    /// MaxSign
    /// </summary>
    int MaxSign { get; set; }

    /// <summary>
    /// 通道启动成功后
    /// </summary>
    public ChannelEventHandler Started { get; }

    /// <summary>
    /// 通道启动即将成功
    /// </summary>
    public ChannelEventHandler Starting { get; }

    /// <summary>
    /// 通道停止
    /// </summary>
    public ChannelEventHandler Stoped { get; }

    /// <summary>
    /// 通道停止前
    /// </summary>
    public ChannelEventHandler Stoping { get; }

    /// <summary>
    /// 主动请求时的等待池
    /// </summary>
    public ConcurrentDictionary<long, Func<IClientChannel, ReceivedDataEventArgs, bool, Task>> ChannelReceivedWaitDict { get; }

}

/// <summary>
/// 接收事件回调类
/// </summary>
public class ChannelReceivedEventHandler : List<Func<IClientChannel, ReceivedDataEventArgs, bool, Task>>
{
}

/// <summary>
/// 通道事件回调类
/// </summary>
public class ChannelEventHandler : List<Func<IClientChannel, bool, ValueTask<bool>>>
{
}
