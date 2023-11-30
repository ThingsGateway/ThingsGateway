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
public class ModbusDtu : ReadWriteDevicesTcpServerBase
{
    /// <inheritdoc/>
    public ModbusDtu(TcpService tcpService) : base(tcpService)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
        ModbusDtuPlugin modbusTcpSalvePlugin = new ModbusDtuPlugin();
        tcpService.Config.ConfigurePlugins(a =>
         {
             a.Add(modbusTcpSalvePlugin);
         });
        tcpService.Setup(tcpService.Config);
    }

    /// <summary>
    /// 检测事务标识符
    /// </summary>
    [Description("检测事务标识符")]
    public bool IsCheckMessageId { get; set; }

    /// <summary>
    /// 是否Rtu格式
    /// </summary>
    [Description("是否Rtu格式")]
    public bool IsRtu { get; set; }

    /// <summary>
    /// Crc校验
    /// </summary>
    [Description("Crc校验")]
    public bool Crc16CheckEnable { get; set; } = true;

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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
            if (!commandResult.IsSuccess) return commandResult;
            return SendThenReturn(mAddress.SocketId, commandResult.Content, cancellationToken);
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetReadModbusCommand(address, length, Station);
            if (!commandResult.IsSuccess) return commandResult;
            return await SendThenReturnAsync(mAddress.SocketId, commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override void SetDataAdapter(ISocketClient socketClient = default)
    {
        if (socketClient != default)
        {
            if (!IsRtu)
            {
                ModbusTcpDataHandleAdapter dataHandleAdapter = new()
                {
                    IsCheckMessageId = IsCheckMessageId,
                    CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                };
                socketClient.SetDataHandlingAdapter(dataHandleAdapter);
            }
            else
            {
                ModbusRtuDataHandleAdapter dataHandleAdapter = new()
                {
                    Crc16CheckEnable = Crc16CheckEnable,
                    CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                };
                socketClient.SetDataHandlingAdapter(dataHandleAdapter);
            }
        }
        else
        {
            //只适配第一个
            var item = TcpService.GetClients().FirstOrDefault();
            if (item != null)
            {
                if (!IsRtu)
                {
                    ModbusTcpDataHandleAdapter dataHandleAdapter = new()
                    {
                        IsCheckMessageId = IsCheckMessageId,
                        CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                    };
                    item.SetDataHandlingAdapter(dataHandleAdapter);
                }
                else
                {
                    ModbusRtuDataHandleAdapter dataHandleAdapter = new()
                    {
                        Crc16CheckEnable = Crc16CheckEnable,
                        CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                    };
                    item.SetDataHandlingAdapter(dataHandleAdapter);
                }
            }
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            Connect(cancellationToken);
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            if (!commandResult.IsSuccess) return commandResult;
            return SendThenReturn(mAddress.SocketId, commandResult.Content, cancellationToken);
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetWriteBoolModbusCommand(address, value, Station);
            if (!commandResult.IsSuccess) return commandResult;
            return SendThenReturn(mAddress.SocketId, commandResult.Content, cancellationToken);
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            if (!commandResult.IsSuccess) return commandResult;
            return await SendThenReturnAsync(mAddress.SocketId, commandResult.Content, cancellationToken);
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetWriteBoolModbusCommand(address, value, Station);
            if (!commandResult.IsSuccess) return commandResult;
            return await SendThenReturnAsync(mAddress.SocketId, commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    private OperResult<byte[]> SendThenReturn(string id, byte[] command, CancellationToken cancellationToken)
    {

        if (TcpService.TryGetSocketClient($"ID={id}", out var client))
        {
            SetDataAdapter(client);
            return SendThenReturn<MessageBase>(command, cancellationToken, client);
        }
        else
        {
            return new OperResult<byte[]>("客户端未连接");
        }
    }

    private async Task<OperResult<byte[]>> SendThenReturnAsync(string id, byte[] command, CancellationToken cancellationToken)
    {
        if (TcpService.TryGetSocketClient($"ID={id}", out var client))
        {
            SetDataAdapter(client);
            return await SendThenReturnAsync<MessageBase>(command, cancellationToken, client);
        }
        else if (TcpService.SocketClients.Count == 1)
        {
            var client1 = TcpService.SocketClients.GetClients().FirstOrDefault();
            if (client1 != null)
            {
                SetDataAdapter(client1);
                return await SendThenReturnAsync<MessageBase>(command, cancellationToken, client1);
            }
        }
        return new OperResult<byte[]>("客户端未连接");
    }

    internal class ModbusDtuPlugin : PluginBase, ITcpReceivingPlugin
    {
        public async Task OnTcpReceiving(ITcpClientBase client, ByteBlockEventArgs e)
        {
            if (client is ISocketClient socket)
            {
                if (!socket.Id.StartsWith("ID="))
                {
                    ByteBlock byteBlock = e.ByteBlock;
                    var id = $"ID={byteBlock.ToArray().ToHexString()}";
                    socket.ResetId(id);
                }
            }
            await e.InvokeNext();//如果本插件无法处理当前数据，请将数据转至下一个插件。
        }
    }
}
