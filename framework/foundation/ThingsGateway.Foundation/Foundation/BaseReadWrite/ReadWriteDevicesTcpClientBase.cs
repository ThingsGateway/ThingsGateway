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

using System.ComponentModel;

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// TCP读写设备
/// </summary>
public abstract class ReadWriteDevicesTcpClientBase : ReadWriteDevicesBase
{
    /// <inheritdoc cref="ReadWriteDevicesTcpClientBase"/>
    public ReadWriteDevicesTcpClientBase(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        WaitingClientEx = TcpClient.CreateWaitingClient(new() { });
        TcpClient.Connecting -= Connecting;
        TcpClient.Connected -= Connected;
        TcpClient.Disconnecting -= Disconnecting;
        TcpClient.Disconnected -= Disconnected;
        TcpClient.Connecting += Connecting;
        TcpClient.Connected += Connected;
        TcpClient.Disconnecting += Disconnecting;
        TcpClient.Disconnected += Disconnected;
        Logger = TcpClient.Logger;
    }

    /// <inheritdoc/>
    public override ChannelEnum ChannelEnum => ChannelEnum.TcpClient;

    /// <summary>
    /// 连接超时时间
    /// </summary>
    [Description("连接超时时间")]
    public ushort ConnectTimeOut { get; set; } = 3000;

    /// <summary>
    /// Socket管理对象
    /// </summary>
    public TcpClient TcpClient { get; }

    /// <summary>
    /// WaitingClientEx
    /// </summary>
    public virtual IWaitingClient<TcpClient> WaitingClientEx { get; }

    /// <inheritdoc/>
    public override bool IsConnected()
    {
        return TcpClient?.CanSend == true;
    }

    /// <inheritdoc/>
    public override void Connect(CancellationToken cancellationToken)
    {
        TcpClient.Connect(ConnectTimeOut);
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return TcpClient.ConnectAsync(ConnectTimeOut, cancellationToken);
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        if (CascadeDisposal && IsConnected())
            TcpClient.Close();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Disconnect();
        TcpClient.Connecting -= Connecting;
        TcpClient.Connected -= Connected;
        TcpClient.Disconnecting -= Disconnecting;
        TcpClient.Disconnected -= Disconnected;
        if (CascadeDisposal && !TcpClient.DisposedValue)
            TcpClient.SafeDispose();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return TcpClient.RemoteIPHost.ToString();
    }

    /// <summary>
    /// 连接成功
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task Connected(ITcpClient client, ConnectedEventArgs e)
    {
        Logger?.Debug(client.RemoteIPHost.ToString() + "连接成功");
        return EasyTask.CompletedTask;
    }

    private async Task Connecting(ITcpClient client, ConnectingEventArgs e)
    {
        Logger?.Debug(client.RemoteIPHost.ToString() + "正在连接");
        SetDataAdapter();
        await EasyTask.CompletedTask;
    }

    private async Task Disconnected(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug($"{client.IP}:{client.Port}断开连接-{e.Message}");
        await EasyTask.CompletedTask;
    }

    private async Task Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug($"{client.IP}:{client.Port}正在主动断开连接-{e.Message}");
        await EasyTask.CompletedTask;
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
        TcpClient.Send(command);
    }
}