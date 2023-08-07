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

namespace ThingsGateway.Foundation;

/// <summary>
/// UDP读写设备
/// </summary>
public abstract class ReadWriteDevicesUdpBase : ReadWriteDevicesClientBase
{
    /// <summary>
    /// WaitingClientEx
    /// </summary>
    public virtual IWaitingClient<UdpSession> WaitingClientEx { get; }
    /// <inheritdoc cref="ReadWriteDevicesUdpBase"/>
    public ReadWriteDevicesUdpBase(UdpSession udpSession)
    {

        UdpSession = udpSession;
        WaitingClientEx = UdpSession.GetWaitingClientEx(new());

        SetDataAdapter();
    }
    /// <summary>
    /// Socket管理对象
    /// </summary>
    public UdpSession UdpSession { get; }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken token)
    {
        return Task.FromResult(UdpSession.Start());
    }
    /// <inheritdoc/>
    public override void Connect(CancellationToken token)
    {
        UdpSession.Start();
    }
    /// <inheritdoc/>
    public override void Disconnect()
    {
        UdpSession.Stop();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        UdpSession.Stop();
        UdpSession.SafeDispose();
        base.Dispose(disposing);
    }
    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> SendThenResponseAsync(byte[] data, WaitingOptions waitingOptions = null, CancellationToken token = default)
    {
        try
        {
            waitingOptions ??= new WaitingOptions { ThrowBreakException = true, AdapterFilter = AdapterFilter.NoneAll };
            ResponsedData result = await UdpSession.GetWaitingClientEx(waitingOptions).SendThenResponseAsync(data, TimeOut, CancellationToken.None);
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
            waitingOptions ??= new WaitingOptions { ThrowBreakException = true, AdapterFilter = AdapterFilter.NoneAll };
            ResponsedData result = UdpSession.GetWaitingClientEx(waitingOptions).SendThenResponse(data, TimeOut, CancellationToken.None);
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
        return UdpSession.RemoteIPHost.ToString();
    }
}