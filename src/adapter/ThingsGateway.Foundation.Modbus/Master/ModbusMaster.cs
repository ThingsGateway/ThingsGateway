
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Text;

using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Modbus;

/// <inheritdoc/>
public partial class ModbusMaster : ProtocolBase, IDtu
{
    private ModbusTypeEnum modbusType;

    /// <inheritdoc/>
    public ModbusMaster(IChannel channel) : base(channel)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <summary>
    /// Modbus类型
    /// </summary>
    public ModbusTypeEnum ModbusType
    {
        get { return modbusType; }
        set
        {
            if (modbusType != value)
            {
                if (Channel is IClientChannel clientChannel)
                {
                    modbusType = value;
                    clientChannel.SetDataHandlingAdapter(GetDataAdapter());
                }
            }
            modbusType = value;
        }
    }

    /// <inheritdoc/>
    public override bool IsSingleThread
    {
        get
        {
            switch (ModbusType)
            {
                case ModbusTypeEnum.ModbusTcp: return false;
                default: return true;
            }
        }
    }

    /// <summary>
    /// 站号
    /// </summary>
    public byte Station { get; set; } = 1;

    /// <summary>
    /// 客户端连接滑动过期时间(TCP服务通道时)
    /// </summary>
    public int CheckClearTime { get; set; } = 120;

    /// <summary>
    /// 心跳检测(大写16进制字符串)
    /// </summary>
    public string HeartbeatHexString { get; set; } = "FFFF8080";

    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        return $"{base.GetAddressDescription()}{Environment.NewLine}{ModbusHelper.GetAddressDescription()}";
    }

    /// <inheritdoc/>
    public override Action<IPluginManager> ConfigurePlugins()
    {
        switch (Channel.ChannelType)
        {
            case ChannelTypeEnum.TcpService:
                Action<IPluginManager> action = a => { };
                {
                    action = a => a.UseCheckClear()
      .SetCheckClearType(CheckClearType.All)
      .SetTick(TimeSpan.FromSeconds(CheckClearTime))
      .SetOnClose((c, t) =>
      {
          c.TryShutdown();
          c.SafeClose($"{CheckClearTime}s Timeout");
      });
                }

                action += a =>
                   {
                       a.Add(new DtuPlugin(this));
                   };
                return action;
        }
        return base.ConfigurePlugins();
    }

    /// <inheritdoc/>
    public override DataHandlingAdapter GetDataAdapter()
    {
        switch (ModbusType)
        {
            case ModbusTypeEnum.ModbusTcp:
                switch (Channel.ChannelType)
                {
                    case ChannelTypeEnum.TcpClient:
                    case ChannelTypeEnum.TcpService:
                    case ChannelTypeEnum.SerialPortClient:
                        return new ModbusTcpDataHandleAdapter()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new ModbusUdpDataHandleAdapter()
                        {
                        };
                }
                break;

            case ModbusTypeEnum.ModbusRtu:
                switch (Channel.ChannelType)
                {
                    case ChannelTypeEnum.TcpClient:
                    case ChannelTypeEnum.TcpService:
                    case ChannelTypeEnum.SerialPortClient:
                        return new ModbusRtuDataHandleAdapter()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new ModbusRtuOverUdpDataHandleAdapter()
                        {
                        };
                }
                break;
        }
        return new ModbusTcpDataHandleAdapter()
        {
            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime);
    }

    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetReadModbusCommand(mAddress, (ushort)length);
            return SendThenReturn(mAddress, commandResult, cancellationToken);
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            var commandResult = ModbusHelper.GetReadModbusCommand(mAddress, (ushort)length);
            return await SendThenReturnAsync(mAddress, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            value = value.ArrayExpandToLengthEven();
            byte[]? commandResult = null;
            //功能码或实际长度
            if (value.Length > 2 || mAddress.WriteFunction == 16)
                commandResult = ModbusHelper.GetWriteModbusCommand(mAddress, value);
            else
                commandResult = ModbusHelper.GetWriteOneModbusCommand(mAddress, value);
            return SendThenReturn(mAddress, commandResult, cancellationToken);
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            //功能码或实际长度
            if (value.Length > 1 || mAddress.WriteFunction == 15)
            {
                var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value, (ushort)value.Length);
                return SendThenReturn(mAddress, commandResult, cancellationToken);
            }
            else
            {
                if (mAddress.BitIndex == null)
                {
                    var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value[0]);
                    return SendThenReturn(mAddress, commandResult, cancellationToken);
                }
                else if (mAddress.BitIndex < 16)
                {
                    //比如40001.1
                    var read = ModbusHelper.GetReadModbusCommand(mAddress, 1);
                    var readData = SendThenReturn(mAddress, read, cancellationToken);
                    if (!readData.IsSuccess) return readData;
                    var writeData = ThingsGatewayBitConverter.ToUInt16(readData.Content, 0);
                    ushort mask = (ushort)(1 << mAddress.BitIndex);
                    ushort result = (ushort)(value[0] ? (writeData | mask) : (writeData & ~mask));
                    var write = ModbusHelper.GetWriteOneModbusCommand(mAddress, ThingsGatewayBitConverter.GetBytes(result));
                    return SendThenReturn(mAddress, write, cancellationToken);
                }
                else
                {
                    return new(ModbusResource.Localizer["ValueOverlimit", nameof(mAddress.BitIndex), 16]);
                }
            }
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            value = value.ArrayExpandToLengthEven();
            byte[]? commandResult = null;
            //功能码或实际长度
            if (value.Length > 2 || mAddress.WriteFunction == 16)
                commandResult = ModbusHelper.GetWriteModbusCommand(mAddress, value);
            else
                commandResult = ModbusHelper.GetWriteOneModbusCommand(mAddress, value);
            return await SendThenReturnAsync(mAddress, commandResult, cancellationToken);
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            //功能码或实际长度
            if (value.Length > 1 || mAddress.WriteFunction == 15)
            {
                var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value, (ushort)value.Length);
                return await SendThenReturnAsync(mAddress, commandResult, cancellationToken);
            }
            else
            {
                if (mAddress.BitIndex == null)
                {
                    var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value[0]);
                    return await SendThenReturnAsync(mAddress, commandResult, cancellationToken);
                }
                else if (mAddress.BitIndex < 16)
                {
                    //比如40001.1

                    var read = ModbusHelper.GetReadModbusCommand(mAddress, 1);
                    var readData = await SendThenReturnAsync(mAddress, read, cancellationToken);
                    if (!readData.IsSuccess) return readData;
                    var writeData = ThingsGatewayBitConverter.ToUInt16(readData.Content, 0);
                    ushort mask = (ushort)(1 << mAddress.BitIndex);
                    ushort result = (ushort)(value[0] ? (writeData | mask) : (writeData & ~mask));
                    var write = ModbusHelper.GetWriteOneModbusCommand(mAddress, ThingsGatewayBitConverter.GetBytes(result));
                    return await SendThenReturnAsync(mAddress, write, cancellationToken);
                }
                else
                {
                    return new(ModbusResource.Localizer["ValueOverlimit", nameof(mAddress.BitIndex), 16]);
                }
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    private OperResult<byte[]> SendThenReturn(ModbusAddress mAddress, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={mAddress.SocketId}", out TgSocketClient? client))
                return SendThenReturn(new SendMessage(commandResult), cancellationToken, client);
            else
                return new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]);
        }
        else
            return SendThenReturn(new SendMessage(commandResult), cancellationToken);
    }

    private Task<OperResult<byte[]>> SendThenReturnAsync(ModbusAddress mAddress, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={mAddress.SocketId}", out TgSocketClient? client))
                return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken, client);
            else
                return Task.FromResult(new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]));
        }
        else
            return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken);
    }
}
