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
/// 服务设备
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

    /// <summary>
    /// 连接超时时间
    /// </summary>
    [Description("连接超时时间")]
    public ushort ConnectTimeOut { get; set; } = 3000;
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
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => TcpService.Start());
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        if (CascadeDisposal)
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
        if (CascadeDisposal)
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
        return TcpService.ServerName;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    protected virtual Task Connected(SocketClient client, ConnectedEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "连接成功");
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    protected virtual Task Connecting(SocketClient client, ConnectingEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "正在连接");
        SetDataAdapter(client);
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    protected virtual Task Disconnected(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "断开连接-" + e.Message);
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    protected virtual Task Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "正在主动断开连接-" + e.Message);
        return EasyTask.CompletedTask;
    }

}