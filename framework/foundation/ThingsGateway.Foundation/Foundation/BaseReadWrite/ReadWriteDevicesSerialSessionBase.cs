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
/// 串口读写设备
/// </summary>
public abstract class ReadWriteDevicesSerialSessionBase : ReadWriteDevicesBase
{
    /// <summary>
    /// <inheritdoc cref="ReadWriteDevicesSerialSessionBase"/>
    /// </summary>
    /// <param name="serialPortClient"></param>
    public ReadWriteDevicesSerialSessionBase(SerialPortClient serialPortClient)
    {
        SerialPortClient = serialPortClient;
        WaitingClientEx = SerialPortClient.CreateWaitingClient(new() { });
        SerialPortClient.Received -= Received;
        SerialPortClient.Connecting -= Connecting;
        SerialPortClient.Connected -= Connected;
        SerialPortClient.Disconnecting -= Disconnecting;
        SerialPortClient.Disconnected -= Disconnected;
        SerialPortClient.Connecting += Connecting;
        SerialPortClient.Connected += Connected;
        SerialPortClient.Disconnecting += Disconnecting;
        SerialPortClient.Disconnected += Disconnected;
        SerialPortClient.Received += Received;

        Logger = SerialPortClient.Logger;
    }
    /// <summary>
    /// 接收解析
    /// </summary>
    /// <param name="client"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task Received(SerialPortClient client, ReceivedDataEventArgs e)
    {
        return EasyTask.CompletedTask;
    }

    /// <inheritdoc/>
    public override ChannelEnum ChannelEnum => ChannelEnum.SerialPortClient;

    /// <summary>
    /// 串口管理对象
    /// </summary>
    public SerialPortClient SerialPortClient { get; }

    /// <summary>
    /// 默认WaitingClientEx
    /// </summary>
    public virtual IWaitingClient<SerialPortClient> WaitingClientEx { get; }
    /// <inheritdoc/>
    public override bool IsConnected()
    {
        return SerialPortClient?.CanSend == true;
    }
    /// <inheritdoc/>
    public override void Connect(CancellationToken cancellationToken)
    {
        SerialPortClient.Connect();
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return SerialPortClient.ConnectAsync();
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        if (CascadeDisposal && IsConnected())
            SerialPortClient.Close();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Disconnect();
        SerialPortClient.Received -= Received;
        SerialPortClient.Connecting -= Connecting;
        SerialPortClient.Connected -= Connected;
        SerialPortClient.Disconnecting -= Disconnecting;
        SerialPortClient.Disconnected -= Disconnected;
        if (CascadeDisposal && !SerialPortClient.DisposedValue)
            SerialPortClient.SafeDispose();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return SerialPortClient.ToString();
    }

    private async Task Connected(ISerialPortClient client, ConnectedEventArgs e)
    {
        Logger?.Debug(client.ToString() + "连接成功");
        SetDataAdapter();
        await EasyTask.CompletedTask;
    }

    private async Task Connecting(ISerialPortClient client, SerialConnectingEventArgs e)
    {
        Logger?.Debug(client.ToString() + "正在连接");
        await EasyTask.CompletedTask;
    }

    private async Task Disconnected(ISerialPortClient client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.ToString() + "断开连接-" + e.Message);
        await EasyTask.CompletedTask;
    }

    private async Task Disconnecting(ISerialPortClient client, DisconnectEventArgs e)
    {
        Logger?.Debug(client.ToString() + "正在主动断开连接-" + e.Message);
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
        SerialPortClient.Send(command);
    }
}