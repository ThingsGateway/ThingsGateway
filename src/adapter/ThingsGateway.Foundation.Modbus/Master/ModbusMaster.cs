//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

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
        if (channel is IClientChannel client)
            client.WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <summary>
    /// 客户端连接滑动过期时间(TCP服务通道时)
    /// </summary>
    public int CheckClearTime { get; set; } = 120;

    /// <summary>
    /// 默认Dtu注册包,utf-8字符串
    /// </summary>
    public string DtuId { get; set; } = "DtuId";

    /// <summary>
    /// 心跳检测(大写16进制字符串)
    /// </summary>
    public string HeartbeatHexString { get; set; } = "FFFF8080";

    /// <inheritdoc/>
    public override bool IsSingleThread
    {
        get
        {
            switch (ModbusType)
            {
                default: return true;
            }
        }
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

    /// <summary>
    /// 站号
    /// </summary>
    public byte Station { get; set; } = 1;

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
    public override string GetAddressDescription()
    {
        return $"{base.GetAddressDescription()}{Environment.NewLine}{ModbusHelper.GetAddressDescription()}";
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
                        return new ProtocolSingleStreamDataHandleAdapter<ModbusTcpMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new ProtocolUdpDataHandleAdapter<ModbusTcpMessage>()
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
                        return new ProtocolSingleStreamDataHandleAdapter<ModbusRtuMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new ProtocolUdpDataHandleAdapter<ModbusRtuMessage>()
                        {
                        };
                }
                break;
        }
        return new ProtocolSingleStreamDataHandleAdapter<ModbusTcpMessage>()
        {
            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime, Station, DtuId);
    }

    public async ValueTask<OperResult<byte[]>> ModbusRequestAsync(ModbusAddress mAddress, bool read, CancellationToken cancellationToken = default)
    {
        try
        {
            var channelResult = await GetChannelAsync(mAddress.SocketId).ConfigureAwait(false);
            if (!channelResult.IsSuccess) return new OperResult<byte[]>(channelResult);
            var waitData = channelResult.Content.WaitHandlePool.GetWaitDataAsync(out var sign);
            return await SendThenReturnAsync(GetSendMessage(mAddress, (ushort)sign, read),
              channelResult.Content, waitData, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var mAddress = ModbusAddress.ParseFrom(address, Station, DtuId);
            mAddress.Length = (ushort)length;
            return await ModbusRequestAsync(mAddress, true, cancellationToken).ConfigureAwait(false);
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
            mAddress.Data = value;
            return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
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
            if (value.Length > 1 && mAddress.FunctionCode == 1)
            {
                mAddress.WriteFunctionCode = 15;
                mAddress.Data = value.BoolArrayToByte();
                return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
            }
            else if (mAddress.BitIndex == null)
            {
                mAddress.Data = value[0] ? new byte[2] { 255, 0 } : [0, 0];
                return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                if (mAddress.BitIndex < 16)
                {
                    mAddress.Length = 1; //请求寄存器数量
                    var readData = await ModbusRequestAsync(mAddress, true, cancellationToken).ConfigureAwait(false);
                    if (!readData.IsSuccess) return readData;
                    var writeData = ThingsGatewayBitConverter.ToUInt16(readData.Content, 0);
                    for (int i = 0; i < value.Length; i++)
                    {
                        writeData = writeData.SetBit(mAddress.BitIndex.Value + i, value[i]);
                    }
                    mAddress.Data = ThingsGatewayBitConverter.GetBytes(writeData);
                    return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
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

    private ISendMessage GetSendMessage(ModbusAddress modbusAddress, ushort sign, bool read)
    {
        if (ModbusType == ModbusTypeEnum.ModbusRtu)
        {
            return new ModbusRtuSend(modbusAddress, sign, read);
        }
        else
        {
            return new ModbusTcpSend(modbusAddress, sign, read);
        }
    }
}
