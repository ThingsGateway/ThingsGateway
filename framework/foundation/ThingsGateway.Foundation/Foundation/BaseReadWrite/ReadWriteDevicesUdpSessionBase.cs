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

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// UDP读写设备
/// </summary>
public abstract class ReadWriteDevicesUdpSessionBase : ReadWriteDevicesBase
{
    /// <inheritdoc cref="ReadWriteDevicesUdpSessionBase"/>
    public ReadWriteDevicesUdpSessionBase(UdpSession udpSession)
    {
        UdpSession = udpSession;
        SetDataAdapter();
        WaitingClientEx = UdpSession.CreateWaitingClient(new() { });
    }
    /// <inheritdoc/>
    public override ChannelEnum ChannelEnum => ChannelEnum.UdpSession;
    /// <summary>
    /// Socket管理对象
    /// </summary>
    public UdpSession UdpSession { get; }
    /// <inheritdoc/>
    public override bool IsConnected()
    {
        return UdpSession?.CanSend == true;
    }
    /// <summary>
    /// WaitingClientEx
    /// </summary>
    public virtual IWaitingClient<UdpSession> WaitingClientEx { get; }
    /// <inheritdoc/>
    public override void Connect(CancellationToken cancellationToken)
    {
        UdpSession.Start();
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return UdpSession.StartAsync();
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        if (CascadeDisposal && IsConnected())
            UdpSession.Stop();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Disconnect();
        if (CascadeDisposal && !UdpSession.DisposedValue)
            UdpSession.SafeDispose();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return UdpSession.RemoteIPHost?.ToString();
    }

    /// <inheritdoc/>
    public override Task<ResponsedData> GetResponsedDataAsync(byte[] item, int timeout, CancellationToken cancellationToken, ISenderClient senderClient = default)
    {
        if (senderClient == default)
            return WaitingClientEx.SendThenResponseAsync(item, TimeOut, cancellationToken);
        else
            return senderClient.CreateWaitingClient(new()).SendThenResponseAsync(item, TimeOut, cancellationToken);
    }

    /// <inheritdoc/>
    public override ResponsedData GetResponsedData(byte[] item, int timeout, CancellationToken cancellationToken, ISenderClient senderClient = default)
    {
        if (senderClient == default)
            return WaitingClientEx.SendThenResponse(item, TimeOut, cancellationToken);
        else
            return senderClient.CreateWaitingClient(new()).SendThenResponse(item, TimeOut, cancellationToken);
    }
    /// <inheritdoc/>
    public override void Send(byte[] command, string id = default)
    {
        UdpSession.Send(command);
    }
}