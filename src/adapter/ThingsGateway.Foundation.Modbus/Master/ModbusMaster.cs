//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

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
        IsBoolReverseByteWord = true;
        RegisterByteLength = 2;
        if (channel is IClientChannel client)
            client.WaitHandlePool.MaxSign = ushort.MaxValue;
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
    /// 默认Dtu注册包,utf-8字符串
    /// </summary>
    public string DtuId { get; set; } = "TEST";

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
                return PluginUtil.GetDtuPlugin(this);
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
                    case ChannelTypeEnum.SerialPort:
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
                    case ChannelTypeEnum.SerialPort:
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
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime, Station, DtuId);
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = ModbusAddress.ParseFrom(address, Station, DtuId);
            var commandResult = ModbusHelper.GetReadModbusCommand(mAddress, (ushort)length);
            return await SendThenReturnAsync(mAddress.SocketId, commandResult, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = ModbusAddress.ParseFrom(address, Station, DtuId);
            value = value.ArrayExpandToLengthEven();
            byte[]? commandResult = null;
            //功能码或实际长度
            if (value.Length > 2 || mAddress.WriteFunction == 16)
                commandResult = ModbusHelper.GetWriteModbusCommand(mAddress, value);
            else
                commandResult = ModbusHelper.GetWriteOneModbusCommand(mAddress, value);
            return await SendThenReturnAsync(mAddress.SocketId, commandResult, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = ModbusAddress.ParseFrom(address, Station, DtuId);
            //功能码或实际长度
            if (value.Length > 1 || mAddress.WriteFunction == 15)
            {
                var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value, (ushort)value.Length);
                return await SendThenReturnAsync(mAddress.SocketId, commandResult, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (mAddress.BitIndex == null)
                {
                    var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value[0]);
                    return await SendThenReturnAsync(mAddress.SocketId, commandResult, cancellationToken).ConfigureAwait(false);
                }
                else if (mAddress.BitIndex < 16)
                {
                    //比如40001.1

                    var read = ModbusHelper.GetReadModbusCommand(mAddress, 1);
                    var readData = await SendThenReturnAsync(mAddress.SocketId, read, cancellationToken).ConfigureAwait(false);
                    if (!readData.IsSuccess) return readData;
                    var writeData = ThingsGatewayBitConverter.ToUInt16(readData.Content, 0);
                    ushort mask = (ushort)(1 << mAddress.BitIndex);
                    ushort result = (ushort)(value[0] ? (writeData | mask) : (writeData & ~mask));
                    var write = ModbusHelper.GetWriteOneModbusCommand(mAddress, ThingsGatewayBitConverter.GetBytes(result));
                    return await SendThenReturnAsync(mAddress.SocketId, write, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return new OperResult(ModbusResource.Localizer["ValueOverlimit", nameof(mAddress.BitIndex), 16]);
                }
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }
}
