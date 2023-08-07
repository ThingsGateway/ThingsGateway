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
/// TCP读写设备
/// </summary>
public abstract class ReadWriteDevicesTcpClientBase : ReadWriteDevicesClientBase
{
    /// <summary>
    /// WaitingClientEx
    /// </summary>
    public virtual IWaitingClient<TcpClientEx> WaitingClientEx { get; }

    /// <inheritdoc cref="ReadWriteDevicesTcpClientBase"/>
    public ReadWriteDevicesTcpClientBase(TcpClientEx tcpClient)
    {
        TcpClientEx = tcpClient;
        WaitingClientEx = TcpClientEx.GetWaitingClientEx(new());
        TcpClientEx.Connecting -= Connecting;
        TcpClientEx.Connected -= Connected;
        TcpClientEx.Disconnecting -= Disconnecting;
        TcpClientEx.Disconnected -= Disconnected;
        TcpClientEx.Connecting += Connecting;
        TcpClientEx.Connected += Connected;
        TcpClientEx.Disconnecting += Disconnecting;
        TcpClientEx.Disconnected += Disconnected;
        Logger = TcpClientEx.Logger;
    }
    /// <summary>
    /// Socket管理对象
    /// </summary>
    public TcpClientEx TcpClientEx { get; }

    /// <inheritdoc/>
    public override void Connect(CancellationToken token)
    {
        TcpClientEx.Connect(ConnectTimeOut);
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken token)
    {
        return TcpClientEx.ConnectAsync(ConnectTimeOut);
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        TcpClientEx.Close();
    }

    /// <inheritdoc/>
    public override OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
    {
        try
        {
            waitingOptions ??= new WaitingOptions { ThrowBreakException = true, AdapterFilter = AdapterFilter.NoneAll };
            ResponsedData result = TcpClientEx.GetWaitingClientEx(waitingOptions).SendThenResponse(data, TimeOut, token);
            return OperResult.CreateSuccessResult(result.Data);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
    {
        try
        {
            waitingOptions ??= new WaitingOptions { ThrowBreakException = true, AdapterFilter = AdapterFilter.NoneAll };
            ResponsedData result = await TcpClientEx.GetWaitingClientEx(waitingOptions).SendThenResponseAsync(data, TimeOut, token);
            return OperResult.CreateSuccessResult(result.Data);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return TcpClientEx.RemoteIPHost.ToString();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        TcpClientEx.Connecting -= Connecting;
        TcpClientEx.Connected -= Connected;
        TcpClientEx.Disconnecting -= Disconnecting;
        TcpClientEx.Disconnected -= Disconnected;
        TcpClientEx.Close();
        TcpClientEx.SafeDispose();
        base.Dispose(disposing);
    }
    private void Connected(ITcpClient client, ConnectedEventArgs e)
    {
        Logger?.Debug(client.RemoteIPHost.ToString() + "连接成功");
    }

    private void Connecting(ITcpClient client, ConnectingEventArgs e)
    {
        Logger?.Debug(client.RemoteIPHost.ToString() + "正在连接");
        SetDataAdapter();
    }

    private void Disconnected(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "断开连接-" + e.Message);
    }

    private void Disconnecting(ITcpClientBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.IP + ":" + client.Port + "正在主动断开连接-" + e.Message);
    }
}