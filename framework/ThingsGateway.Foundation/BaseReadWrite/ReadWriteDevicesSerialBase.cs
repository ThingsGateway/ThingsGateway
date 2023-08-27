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

using ThingsGateway.Foundation.Serial;

namespace ThingsGateway.Foundation;

/// <summary>
/// TCP读写设备
/// </summary>
public abstract class ReadWriteDevicesSerialBase : ReadWriteDevicesClientBase
{
    /// <summary>
    /// WaitingClientEx
    /// </summary>
    public virtual IWaitingClient<SerialsSession> WaitingClientEx { get; }
    /// <summary>
    /// <inheritdoc cref="ReadWriteDevicesSerialBase"/>
    /// </summary>
    /// <param name="serialSession"></param>
    public ReadWriteDevicesSerialBase(SerialsSession serialSession)
    {
        SerialsSession = serialSession;
        WaitingClientEx = SerialsSession.GetWaitingClientEx(new() { BreakTrigger = true });

        SerialsSession.Connecting -= Connecting;
        SerialsSession.Connected -= Connected;
        SerialsSession.Disconnecting -= Disconnecting;
        SerialsSession.Disconnected -= Disconnected;
        SerialsSession.Connecting += Connecting;
        SerialsSession.Connected += Connected;
        SerialsSession.Disconnecting += Disconnecting;
        SerialsSession.Disconnected += Disconnected;
        Logger = SerialsSession.Logger;
    }
    /// <summary>
    /// 串口管理对象
    /// </summary>
    public SerialsSession SerialsSession { get; }

    /// <inheritdoc/>
    public override void Connect(CancellationToken token)
    {
        SerialsSession.Connect();
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken token)
    {
        return SerialsSession.ConnectAsync();
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        SerialsSession.Close();
    }

    /// <inheritdoc/>
    public override OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
    {
        try
        {
            waitingOptions ??= new WaitingOptions { ThrowBreakException = true, AdapterFilter = AdapterFilter.NoneAll };
            ResponsedData result = SerialsSession.GetWaitingClientEx(waitingOptions).SendThenResponse(data, TimeOut, token);
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
            ResponsedData result = await SerialsSession.GetWaitingClientEx(waitingOptions).SendThenResponseAsync(data, TimeOut, token);
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
        return SerialsSession.SerialProperty.ToString();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        SerialsSession.Connecting -= Connecting;
        SerialsSession.Connected -= Connected;
        SerialsSession.Disconnecting -= Disconnecting;
        SerialsSession.Disconnected -= Disconnected;
        SerialsSession.Close();
        SerialsSession.SafeDispose();
        base.Dispose(disposing);
    }
    private void Connected(ISerialSession client, ConnectedEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "连接成功");
    }

    private void Connecting(ISerialSession client, SerialConnectingEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "正在连接");
        SetDataAdapter();
    }

    private void Disconnected(ISerialSessionBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "断开连接-" + e.Message);
    }

    private void Disconnecting(ISerialSessionBase client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "正在主动断开连接-" + e.Message);
    }
}