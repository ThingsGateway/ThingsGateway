﻿#region copyright
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

namespace ThingsGateway.Foundation;

/// <summary>
/// TCP读写设备
/// </summary>
public abstract class ReadWriteDevicesTcpClientBase : ReadWriteDevicesBase
{
    /// <inheritdoc cref="ReadWriteDevicesTcpClientBase"/>
    public ReadWriteDevicesTcpClientBase(TcpClient tcpClient)
    {
        TcpClient = tcpClient;
        WaitingClientEx = TcpClient.GetWaitingClientEx(new() { BreakTrigger = true });
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
    public override void Connect(CancellationToken cancellationToken)
    {
        TcpClient.Connect(ConnectTimeOut);
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return TcpClient.ConnectAsync(ConnectTimeOut);
    }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        if (CascadeDisposal)
            TcpClient.Close();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        TcpClient.Connecting -= Connecting;
        TcpClient.Connected -= Connected;
        TcpClient.Disconnecting -= Disconnecting;
        TcpClient.Disconnected -= Disconnected;
        Disconnect();
        if (CascadeDisposal)
            TcpClient.SafeDispose();
    }

    /// <inheritdoc/>
    public OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken cancellationToken = default)
    {
        try
        {
            waitingOptions ??= new WaitingOptions { BreakTrigger = true, ThrowBreakException = true, AdapterFilter = AdapterFilter.NoneAll };
            ResponsedData result = TcpClient.GetWaitingClientEx(waitingOptions).SendThenResponse(data, TimeOut, cancellationToken);
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
            waitingOptions ??= new WaitingOptions { ThrowBreakException = true, AdapterFilter = AdapterFilter.NoneAll };
            ResponsedData result = await TcpClient.GetWaitingClientEx(waitingOptions).SendThenResponseAsync(data, TimeOut, cancellationToken);
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
        return TcpClient.RemoteIPHost.ToString();
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