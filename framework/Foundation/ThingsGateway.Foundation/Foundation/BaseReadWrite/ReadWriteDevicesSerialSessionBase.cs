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
/// TCP读写设备
/// </summary>
public abstract class ReadWriteDevicesSerialSessionBase : ReadWriteDevicesBase
{
    /// <summary>
    /// <inheritdoc cref="ReadWriteDevicesSerialSessionBase"/>
    /// </summary>
    /// <param name="serialSession"></param>
    public ReadWriteDevicesSerialSessionBase(SerialSession serialSession)
    {
        SerialSession = serialSession;
        WaitingClientEx = SerialSession.GetWaitingClient(new() { ThrowBreakException = true });
        SerialSession.Received -= Received;
        SerialSession.Connecting -= Connecting;
        SerialSession.Connected -= Connected;
        SerialSession.Disconnecting -= Disconnecting;
        SerialSession.Disconnected -= Disconnected;
        SerialSession.Connecting += Connecting;
        SerialSession.Connected += Connected;
        SerialSession.Disconnecting += Disconnecting;
        SerialSession.Disconnected += Disconnected;
        SerialSession.Received += Received;
        Logger = SerialSession.Logger;
    }
    /// <summary>
    /// 接收解析
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task Received(SerialSession client, ReceivedDataEventArgs e)
    {
        return EasyTask.CompletedTask;
    }



    /// <summary>
    /// 串口管理对象
    /// </summary>
    public SerialSession SerialSession { get; }

    /// <summary>
    /// WaitingClientEx
    /// </summary>
    public virtual IWaitingClient<SerialSession> WaitingClientEx { get; }
    /// <inheritdoc/>
    public override void Connect(CancellationToken cancellationToken)
    {
        SerialSession.Connect();
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return SerialSession.ConnectAsync();
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        if (CascadeDisposal)
            SerialSession.Close();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Disconnect();
        SerialSession.Received -= Received;
        SerialSession.Connecting -= Connecting;
        SerialSession.Connected -= Connected;
        SerialSession.Disconnecting -= Disconnecting;
        SerialSession.Disconnected -= Disconnected;
        if (CascadeDisposal)
            SerialSession.SafeDispose();
    }

    /// <inheritdoc/>
    public OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken cancellationToken = default)
    {
        try
        {
            waitingOptions ??= new WaitingOptions { ThrowBreakException = true };
            ResponsedData result = SerialSession.GetWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, cancellationToken);
            return OperResult.CreateSuccessResult(result.Data);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken cancellationToken = default)
    {
        try
        {
            waitingOptions ??= new WaitingOptions { ThrowBreakException = true };
            ResponsedData result = await SerialSession.GetWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, cancellationToken);
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
        return SerialSession.SerialProperty.ToString();
    }
    /// <summary>
    /// Connected
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task Connected(ISerialSession client, ConnectedEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "连接成功");
        SetDataAdapter();
        return EasyTask.CompletedTask;
    }

    private Task Connecting(ISerialSession client, SerialConnectingEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "正在连接");
        return EasyTask.CompletedTask;
    }

    private Task Disconnected(ISerialSessionBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "断开连接-" + e.Message);
        return EasyTask.CompletedTask;
    }

    private Task Disconnecting(ISerialSessionBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "正在主动断开连接-" + e.Message);
        return EasyTask.CompletedTask;
    }
}