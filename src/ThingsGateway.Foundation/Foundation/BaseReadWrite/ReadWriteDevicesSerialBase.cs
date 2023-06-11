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
    /// <inheritdoc cref="ReadWriteDevicesSerialBase"/>
    /// </summary>
    /// <param name="serialClient"></param>
    public ReadWriteDevicesSerialBase(SerialClient serialClient)
    {
        SerialClient = serialClient;
        SerialClient.Opening += Opening;
        SerialClient.Opened += Opened;
        SerialClient.Closing += Closing;
        SerialClient.Closed += Closed;
        Logger = SerialClient.Logger;
    }
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        SerialClient.Opening -= Opening;
        SerialClient.Opened -= Opened;
        SerialClient.Closing -= Closing;
        SerialClient.Closed -= Closed;
        SerialClient.Close();
        SerialClient.SafeDispose();
        base.Dispose(disposing);
    }
    /// <summary>
    /// 串口管理对象
    /// </summary>
    public SerialClient SerialClient { get; }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return SerialClient.ConnectAsync();
    }
    /// <inheritdoc/>
    public override void Connect(CancellationToken cancellationToken)
    {
        SerialClient.Connect();
    }
    /// <inheritdoc/>
    public override void Disconnect()
    {
        SerialClient.Close();
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
    {
        try
        {
            if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
            await ConnectAsync(token);
            ResponsedData result = await SerialClient.GetTGWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, token);
            return OperResult.CreateSuccessResult(result.Data);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
    {
        try
        {
            if (waitingOptions == null) { waitingOptions = new WaitingOptions(); waitingOptions.ThrowBreakException = true; waitingOptions.AdapterFilter = AdapterFilter.NoneAll; }
            Connect(token);
            ResponsedData result = SerialClient.GetTGWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, token);
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
        return SerialClient.SerialProperty.ToString();
    }

    private void Opened(ISerialClient client, MsgEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "连接成功");
    }

    private void Opening(ISerialClient client, OpeningEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "正在连接");
        SetDataAdapter();
    }

    private void Closed(ISerialClientBase client, CloseEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "断开连接-" + e.Message);
    }

    private void Closing(ISerialClientBase client, CloseEventArgs e)
    {
        Logger?.Debug(client.SerialProperty.ToString() + "正在主动断开连接-" + e.Message);
    }
}