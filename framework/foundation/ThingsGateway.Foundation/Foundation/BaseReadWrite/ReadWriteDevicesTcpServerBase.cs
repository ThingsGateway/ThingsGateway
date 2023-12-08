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
/// TCP服务设备
/// </summary>
public abstract class ReadWriteDevicesTcpServerBase : ReadWriteDevicesBase
{
    /// <inheritdoc cref="ReadWriteDevicesTcpServerBase"/>
    public ReadWriteDevicesTcpServerBase(TcpService tcpService)
    {
        TcpService = tcpService;
        TcpService.Received -= Received;
        TcpService.Connecting -= Connecting;
        TcpService.Connected -= Connected;
        TcpService.Disconnecting -= Disconnecting;
        TcpService.Disconnected -= Disconnected;
        TcpService.Received += Received;
        TcpService.Connecting += Connecting;
        TcpService.Connected += Connected;
        TcpService.Disconnecting += Disconnecting;
        TcpService.Disconnected += Disconnected;
        Logger = TcpService.Logger;
    }

    /// <inheritdoc/>
    public override ChannelEnum ChannelEnum => ChannelEnum.TcpServer;

    /// <summary>
    /// 服务管理对象
    /// </summary>
    public TcpService TcpService { get; }

    /// <inheritdoc/>
    public override void Connect(CancellationToken cancellationToken)
    {
        TcpService.Start();
    }

    /// <inheritdoc/>
    public override bool IsConnected()
    {
        return TcpService?.ServerState == ServerState.Running;
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return TcpService.StartAsync();
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        if (CascadeDisposal && IsConnected())
            TcpService.Stop();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Disconnect();
        TcpService.Received -= Received;
        TcpService.Connecting -= Connecting;
        TcpService.Connected -= Connected;
        TcpService.Disconnecting -= Disconnecting;
        TcpService.Disconnected -= Disconnected;
        if (CascadeDisposal && !TcpService.DisposedValue)
            TcpService.SafeDispose();
    }

    /// <summary>
    /// 接收解析
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task Received(SocketClient client, ReceivedDataEventArgs e)
    {
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var list = TcpService.Monitors.Select(a => a.Option.IpHost.ToString());
        string result = list.Aggregate("[", (current, next) => current + next + ",");
        result = result.Remove(result.Length - 1) + "]";
        return result;
    }

    private async Task Connected(SocketClient client, ConnectedEventArgs e)
    {
        Logger?.Debug($"{client.IP}:{client.Port}连接成功");
        await EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    private async Task Connecting(SocketClient client, ConnectingEventArgs e)
    {
        Logger?.Debug($"{client.IP}:{client.Port}正在连接");
        SetDataAdapter(client);
        await EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    private async Task Disconnected(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug($"{client.IP}:{client.Port}断开连接-{e.Message}");
        await EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    private async Task Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug($"{client.IP}:{client.Port}正在主动断开连接-{e.Message}");
        await EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    public override void Send(byte[] command, string id = default)
    {
        TcpService.Send(id, command);
    }

    /// <inheritdoc/>
    public override async Task<ResponsedData> GetResponsedDataAsync(byte[] item, int timeout, CancellationToken cancellationToken, ISenderClient senderClient = default)
    {
        if (senderClient == default)
            return new ResponsedData();
        else
            return await senderClient.CreateWaitingClient(new()).SendThenResponseAsync(item, TimeOut, cancellationToken);
    }

    /// <inheritdoc/>
    public override ResponsedData GetResponsedData(byte[] item, int timeout, CancellationToken cancellationToken, ISenderClient senderClient = default)
    {
        if (senderClient == default)
            return new ResponsedData();
        else
            return senderClient.CreateWaitingClient(new()).SendThenResponse(item, TimeOut, cancellationToken);
    }
}