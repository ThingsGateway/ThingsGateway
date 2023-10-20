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
/// UDP读写设备
/// </summary>
public abstract class ReadWriteDevicesUdpSessionBase : ReadWriteDevicesBase
{
    /// <inheritdoc cref="ReadWriteDevicesUdpSessionBase"/>
    public ReadWriteDevicesUdpSessionBase(UdpSession udpSession)
    {
        UdpSession = udpSession;
        SetDataAdapter();
        WaitingClientEx = UdpSession.CreateWaitingClient(new() { });
    }

    /// <summary>
    /// Socket管理对象
    /// </summary>
    public UdpSession UdpSession { get; }

    /// <summary>
    /// WaitingClientEx
    /// </summary>
    public virtual IWaitingClient<UdpSession> WaitingClientEx { get; }
    /// <inheritdoc/>
    public override void Connect(CancellationToken cancellationToken)
    {
        UdpSession.Start();
    }

    /// <inheritdoc/>
    public override Task ConnectAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(UdpSession.Start());
    }
    /// <inheritdoc/>
    public override void Disconnect()
    {
        if (CascadeDisposal)
            UdpSession.Stop();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        Disconnect();
        if (CascadeDisposal)
            UdpSession.SafeDispose();
    }
    /// <inheritdoc/>
    public OperResult<byte[]> SendThenResponse(byte[] data, WaitingOptions waitingOptions = null, CancellationToken cancellationToken = default)
    {
        try
        {
            waitingOptions ??= new WaitingOptions { };
            ResponsedData result = UdpSession.CreateWaitingClient(waitingOptions).SendThenResponse(data, TimeOut, cancellationToken);
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
            waitingOptions ??= new WaitingOptions { };
            ResponsedData result = await UdpSession.CreateWaitingClient(waitingOptions).SendThenResponseAsync(data, TimeOut, cancellationToken);
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