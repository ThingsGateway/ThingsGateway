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
public partial class ModbusMaster : DeviceBase, IDtu
{

    public override void InitChannel(IChannel channel, ILog? deviceLog = null)
    {
        base.InitChannel(channel, deviceLog);

        RegisterByteLength = 2;
        channel.MaxSign = ushort.MaxValue;
    }
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get; protected set; } = new ThingsGatewayBitConverter(EndianType.Big) {  };

    /// <summary>
    /// 客户端连接滑动过期时间(TCP服务通道时)
    /// </summary>
    public int CheckClearTime { get; set; } = 120000;

    /// <summary>
    /// 默认Dtu注册包(utf-8)
    /// </summary>
    public string DtuId { get; set; } = "DtuId";

    /// <summary>
    /// 心跳检测(utf-8)
    /// </summary>
    public string Heartbeat { get; set; } = "Heartbeat";

    /// <summary>
    /// Modbus类型，在initChannelAsync之前设置
    /// </summary>
    public ModbusTypeEnum ModbusType { get; set; }

    /// <summary>
    /// 站号
    /// </summary>
    public byte Station { get; set; } = 1;

    /// <inheritdoc/>
    public override Action<IPluginManager> ConfigurePlugins(TouchSocketConfig config)
    {
        switch (Channel.ChannelType)
        {
            case ChannelTypeEnum.TcpService:
                return PluginUtil.GetDtuPlugin(this);
        }
        return base.ConfigurePlugins(config);
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
                        return new DeviceSingleStreamDataHandleAdapter<ModbusTcpMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new DeviceUdpDataHandleAdapter<ModbusTcpMessage>()
                        {
                        };
                }
                return new DeviceSingleStreamDataHandleAdapter<ModbusTcpMessage>()
                {
                    CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                };

            case ModbusTypeEnum.ModbusRtu:
                switch (Channel.ChannelType)
                {
                    case ChannelTypeEnum.TcpClient:
                    case ChannelTypeEnum.TcpService:
                    case ChannelTypeEnum.SerialPort:
                        return new DeviceSingleStreamDataHandleAdapter<ModbusRtuMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new DeviceUdpDataHandleAdapter<ModbusRtuMessage>()
                        {
                        };
                }
                return new DeviceSingleStreamDataHandleAdapter<ModbusRtuMessage>()
                {
                    CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
                };
        }
        return new DeviceSingleStreamDataHandleAdapter<ModbusTcpMessage>()
        {
            CacheTimeout = TimeSpan.FromMilliseconds(Channel.ChannelOptions.CacheTimeout),
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, string defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime, Station);
    }

    public async ValueTask<OperResult<byte[]>> ModbusRequestAsync(ModbusAddress mAddress, bool read, CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendThenReturnAsync(GetSendMessage(mAddress, read),
             cancellationToken).ConfigureAwait(false);
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            mAddress.Length = (ushort)length;
            return await ModbusRequestAsync(mAddress, true, cancellationToken).ConfigureAwait(false);

            //if (mAddress.BitIndex == null || mAddress.FunctionCode <= 2)
            //{
            //    return await ModbusRequestAsync(mAddress, true, cancellationToken).ConfigureAwait(false);
            //}
            //if (mAddress.BitIndex < 2)
            //{
            //    mAddress.Length = 1; //请求寄存器数量
            //    var readData = await ModbusRequestAsync(mAddress, true, cancellationToken).ConfigureAwait(false);
            //    if (!readData.IsSuccess) return readData;
            //    var data = readData.Content;
            //    if (mAddress.BitIndex == 0)
            //        readData.Content = new byte[] { data[1] };
            //    else
            //        readData.Content = new byte[] { data[0] };
            //    return readData;
            //}
            //else
            //{
            //    return new(ModbusResource.Localizer["ValueOverlimit", nameof(mAddress.BitIndex), 2]);
            //}

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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
            mAddress.Data = value;

            if (mAddress.BitIndex == null)
            {
                return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
            }
            if (mAddress.BitIndex < 2)
            {
                mAddress.Length = 1; //请求寄存器数量
                var readData = await ModbusRequestAsync(mAddress, true, cancellationToken).ConfigureAwait(false);
                if (!readData.IsSuccess) return readData;
                if (mAddress.BitIndex == 0)
                    readData.Content[1] = value[0];
                else
                    readData.Content[0] = value[0];

                mAddress.Data = readData.Content;
                return await ModbusRequestAsync(mAddress, false, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return new OperResult(ModbusResource.Localizer["ValueOverlimit", nameof(mAddress.BitIndex), 2]);
            }
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
            var mAddress = ModbusAddress.ParseFrom(address, Station);
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

    private ISendMessage GetSendMessage(ModbusAddress modbusAddress, bool read)
    {
        if (ModbusType == ModbusTypeEnum.ModbusRtu)
        {
            return new ModbusRtuSend(modbusAddress, read);
        }
        else
        {
            return new ModbusTcpSend(modbusAddress, read);
        }
    }
}
