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

using System.Linq;

namespace ThingsGateway.Foundation;

/// <summary>
/// 服务设备
/// </summary>
public abstract class ReadWriteDevicesTcpServerBase : ReadWriteDevicesClientBase
{
    /// <inheritdoc cref="ReadWriteDevicesTcpServerBase"/>
    public ReadWriteDevicesTcpServerBase(TcpService tcpService)
    {
        TcpService = tcpService;
        TcpService.Connecting += Connecting;
        TcpService.Connected += Connected;
        TcpService.Received += Received;
        TcpService.Disconnecting += Disconnecting;
        TcpService.Disconnected += Disconnected;
        Logger = TcpService.Logger;
    }
    /// <summary>
    /// 服务管理对象
    /// </summary>
    public TcpService TcpService { get; }

    /// <inheritdoc/>
    public override Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public override OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
    /// <inheritdoc/>
    public override void SetDataAdapter()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() => TcpService.Start());
    }
    /// <inheritdoc/>
    public override void Connect(CancellationToken cancellationToken)
    {
        TcpService.Start();
    }
    /// <inheritdoc/>
    public override void Disconnect()
    {
        TcpService.Stop();
    }



    /// <summary>
    /// 设置适配器
    /// </summary>
    /// <param name="client">客户端</param>
    public abstract void SetDataAdapter(SocketClient client);

    /// <inheritdoc/>
    public override string ToString()
    {
        return TcpService.Monitors.Select(a => a.IPHost.ToString() + Environment.NewLine).ToJson();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        TcpService.Connecting -= Connecting;
        TcpService.Connected -= Connected;
        TcpService.Disconnecting -= Disconnecting;
        TcpService.Disconnected -= Disconnected;
        TcpService.Stop();
        TcpService.SafeDispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// 接收解析
    /// </summary>
    protected abstract Task ReceivedAsync(SocketClient client, IRequestInfo requestInfo);

    private void Connected(SocketClient client, TouchSocketEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "连接成功");
    }

    private void Connecting(SocketClient client, OperationEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "正在连接");
        SetDataAdapter(client);
    }

    private void Disconnected(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "断开连接-" + e.Message);
    }

    private void Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "正在主动断开连接-" + e.Message);
    }

    private async void Received(SocketClient client, ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        try
        {
            await ReceivedAsync(client, requestInfo);
        }
        catch (Exception ex)
        {
            Logger.Exception(this, ex);
        }
    }
}