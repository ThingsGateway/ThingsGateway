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

using System.ComponentModel;

namespace ThingsGateway.Foundation.Adapter.Modbus;

/// <inheritdoc/>
public class ModbusTcp : ReadWriteDevicesTcpClientBase
{
    /// <inheritdoc/>
    public ModbusTcp(TcpClient tcpClient) : base(tcpClient)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
    }

    /// <summary>
    /// 检测事务标识符
    /// </summary>
    [Description("检测事务标识符")]
    public bool IsCheckMessageId { get; set; }

    /// <summary>
    /// 站号
    /// </summary>
    [Description("站号")]
    public byte Station { get; set; } = 1;

    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        return $"{base.GetAddressDescription()}{Environment.NewLine}{ModbusHelper.GetAddressDescription()}";
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack, int defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T, T2>(this, deviceVariables, maxPack, defaultIntervalTime);
    }

    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            Connect(cancellationToken);
            var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
            if (!commandResult.IsSuccess) return commandResult;
            return SendThenReturn<ModbusTcpMessage>(commandResult.Content, cancellationToken);
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
            if (!commandResult.IsSuccess) return commandResult;
            return await SendThenReturnAsync<ModbusTcpMessage>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override void SetDataAdapter(ISocketClient socketClient = default)
    {
        ModbusTcpDataHandleAdapter dataHandleAdapter = new()
        {
            IsCheckMessageId = IsCheckMessageId,
            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
        };
        TcpClient.SetDataHandlingAdapter(dataHandleAdapter);
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            Connect(cancellationToken);
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            if (!commandResult.IsSuccess) return commandResult;
            return SendThenReturn<ModbusTcpMessage>(commandResult.Content, cancellationToken);
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
            if (!commandResult.IsSuccess) return commandResult;
            return SendThenReturn<ModbusTcpMessage>(commandResult.Content, cancellationToken);
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
            if (!commandResult.IsSuccess) return commandResult;
            return await SendThenReturnAsync<ModbusTcpMessage>(commandResult.Content, cancellationToken);
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
            if (!commandResult.IsSuccess) return commandResult;
            return await SendThenReturnAsync<ModbusTcpMessage>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }
}