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
            var channelResult = GetChannel(mAddress.SocketId);
            if (!channelResult.IsSuccess) return new OperResult<byte[]>(channelResult);
            ValueByteBlock valueByteBlock = new ValueByteBlock(1024);
            try
            {
                var waitData = channelResult.Content.WaitHandlePool.GetWaitDataAsync(out var sign);
                var commandResult = ModbusHelper.GetReadModbusCommand(ref valueByteBlock, mAddress, (ushort)length, ModbusType, (ushort)sign, 0);
                return await this.SendThenReturnAsync(new SendMessage(commandResult) { Sign = sign }, waitData, cancellationToken, channelResult.Content).ConfigureAwait(false);
            }
            finally
            {
                valueByteBlock.SafeDispose();
            }
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

            var channelResult = GetChannel(mAddress.SocketId);
            if (!channelResult.IsSuccess) return new OperResult<byte[]>(channelResult);
            ValueByteBlock valueByteBlock = new ValueByteBlock(1024);
            try
            {
                var waitData = channelResult.Content.WaitHandlePool.GetWaitDataAsync(out var sign);
                if (value.Length > 2 || mAddress.WriteFunction == 16)
                    ModbusHelper.GetWriteModbusCommand(ref valueByteBlock, mAddress, value, (ushort)(value.Length / RegisterByteLength), ModbusType, (ushort)sign, 0);
                else
                    ModbusHelper.GetWriteOneModbusCommand(ref valueByteBlock, mAddress, value, ModbusType, (ushort)sign, 0);

                return await this.SendThenReturnAsync(new SendMessage(valueByteBlock.Memory) { Sign = sign }, waitData, cancellationToken, channelResult.Content).ConfigureAwait(false);
            }
            finally
            {
                valueByteBlock.SafeDispose();
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
            var mAddress = ModbusAddress.ParseFrom(address, Station, DtuId);
            var channelResult = GetChannel(mAddress.SocketId);
            if (!channelResult.IsSuccess) return new OperResult<byte[]>(channelResult);
            ValueByteBlock valueByteBlock = new ValueByteBlock(1024);
            try
            {
                var waitData = channelResult.Content.WaitHandlePool.GetWaitDataAsync(out var sign);
                if (value.Length > 1 || mAddress.WriteFunction == 15)
                {
                    ModbusHelper.GetWriteBoolModbusCommand(ref valueByteBlock, mAddress, value, (ushort)value.Length, ModbusType, (ushort)sign, 0);
                }
                else
                {
                    if (mAddress.BitIndex == null)
                    {
                        ModbusHelper.GetWriteBoolModbusCommand(ref valueByteBlock, mAddress, value[0], ModbusType, (ushort)sign, 0);
                    }
                    else if (mAddress.BitIndex < 16)
                    {
                        ValueByteBlock readValueByteBlock = new ValueByteBlock(1024);
                        try
                        {
                            var readWaitData = channelResult.Content.WaitHandlePool.GetWaitDataAsync(out var readSign);
                            //比如40001.1
                            ModbusHelper.GetReadModbusCommand(ref readValueByteBlock, mAddress, 1, ModbusType, (ushort)readSign, 0);
                            var readData = await this.SendThenReturnAsync(new SendMessage(readValueByteBlock.Memory) { Sign = readSign }, readWaitData, cancellationToken, channelResult.Content).ConfigureAwait(false);
                            if (!readData.IsSuccess) return readData;
                            var writeData = ThingsGatewayBitConverter.ToUInt16(readData.Content, 0);
                            ushort mask = (ushort)(1 << mAddress.BitIndex);
                            ushort result = (ushort)(value[0] ? (writeData | mask) : (writeData & ~mask));
                            ModbusHelper.GetWriteOneModbusCommand(ref valueByteBlock, mAddress, ThingsGatewayBitConverter.GetBytes(result), ModbusType, (ushort)sign, 0);
                        }
                        finally
                        {
                            readValueByteBlock.SafeDispose();
                        }
                    }
                    else
                    {
                        return new OperResult(ModbusResource.Localizer["ValueOverlimit", nameof(mAddress.BitIndex), 16]);
                    }
                }

                return await this.SendThenReturnAsync(new SendMessage(valueByteBlock.Memory) { Sign = sign }, waitData, cancellationToken, channelResult.Content).ConfigureAwait(false);
            }
            finally
            {
                valueByteBlock.SafeDispose();
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }
}
