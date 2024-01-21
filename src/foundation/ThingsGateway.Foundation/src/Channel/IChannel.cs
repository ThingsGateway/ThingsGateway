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

namespace ThingsGateway.Foundation;

/// <summary>
/// 通道管理
/// </summary>
public interface IChannel : IConnectObject, ICloseObject, ISetupConfigObject, IDisposable
{
    /// <summary>
    /// 该通道下的所有设备
    /// </summary>
    public List<IProtocol> Collects { get; }

    /// <summary>
    /// 通道启动成功后
    /// </summary>
    public bool CanSend { get; }

    /// <summary>
    /// 通道启动成功后
    /// </summary>
    public ChannelEventHandler Started { get; set; }

    /// <summary>
    /// 通道启动即将成功
    /// </summary>
    public ChannelEventHandler Starting { get; set; }

    /// <summary>
    /// 接收到数据
    /// </summary>
    public TgReceivedEventHandler Received { get; set; }

    /// <summary>
    /// 通道类型
    /// </summary>
    ChannelTypeEnum ChannelType { get; }
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
public delegate Task TgReceivedEventHandler(IClientChannel client, ReceivedDataEventArgs e);