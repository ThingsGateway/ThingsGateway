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

using System.Collections.Generic;
using System.ComponentModel;

namespace ThingsGateway.Foundation.Adapter.Modbus;

/// <inheritdoc/>
public class ModbusRtuOverUdp : ReadWriteDevicesUdpSessionBase
{
    /// <inheritdoc/>
    public ModbusRtuOverUdp(UdpSession udpSession) : base(udpSession)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
    }

    /// <summary>
    /// Crc校验
    /// </summary>
    [Description("Crc校验")]
    public bool Crc16CheckEnable { get; set; }

    /// <summary>
    /// 站号
    /// </summary>
    [Description("站号")]
    public byte Station { get; set; } = 1;
    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        return base.GetAddressDescription() + Environment.NewLine + ModbusHelper.GetAddressDescription();
    }
    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            Connect(cancellationToken);
            var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
            return SendThenReturn(commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            await ConnectAsync(cancellationToken);
            var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
            return await SendThenReturnAsync(commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override void SetDataAdapter(object socketClient = null)
    {
        ModbusRtuOverUdpDataHandleAdapter dataHandleAdapter = new()
        {
            Crc16CheckEnable = Crc16CheckEnable,
        };
        UdpSession.Config.SetUdpDataHandlingAdapter(() =>
        {
            return dataHandleAdapter;
        });
        UdpSession.Setup(UdpSession.Config);
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack)
    {
        return PackHelper.LoadSourceRead<T, T2>(this, deviceVariables, maxPack);

    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            Connect(cancellationToken);
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            return SendThenReturn(commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            Connect(cancellationToken);
            var commandResult = ModbusHelper.GetWriteBoolModbusCommand(address, value, Station);
            return SendThenReturn(commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            await ConnectAsync(cancellationToken);
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            return await SendThenReturnAsync(commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            await ConnectAsync(cancellationToken);
            var commandResult = ModbusHelper.GetWriteBoolModbusCommand(address, value, Station);
            return await SendThenReturnAsync(commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    private OperResult<byte[]> SendThenReturn(OperResult<byte[]> commandResult, CancellationToken cancellationToken)
    {
        if (commandResult.IsSuccess)
        {
            var item = commandResult.Content;
            if (FrameTime != 0)
                Thread.Sleep(FrameTime);
            var result = WaitingClientEx.SendThenResponse(item, TimeOut, cancellationToken);
            return (MessageBase)result.RequestInfo;
        }
        else
        {
            return new OperResult<byte[]>(commandResult.Message);
        }
    }

    private async Task<OperResult<byte[]>> SendThenReturnAsync(OperResult<byte[]> commandResult, CancellationToken cancellationToken)
    {
        if (commandResult.IsSuccess)
        {
            var item = commandResult.Content;
            await Task.Delay(FrameTime, cancellationToken);
            var result = await WaitingClientEx.SendThenResponseAsync(item, TimeOut, cancellationToken);
            return (MessageBase)result.RequestInfo;
        }
        else
        {
            return new OperResult<byte[]>(commandResult.Message);
        }
    }
}
