//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Collections.Concurrent;

using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// ChannelEventHandler
/// </summary>
public delegate ValueTask<OperResult> ModbusServerWriteEventHandler(ModbusAddress modbusAddress, IThingsGatewayBitConverter bitConverter, IClientChannel channel);

/// <inheritdoc/>
public class ModbusSlave : ProtocolBase, ITcpService, IDtuClient
{
    /// <summary>
    /// 继电器
    /// </summary>
    private ConcurrentDictionary<byte, ValueByteBlock> ModbusServer01ByteBlocks = new();

    /// <summary>
    /// 开关输入
    /// </summary>
    private ConcurrentDictionary<byte, ValueByteBlock> ModbusServer02ByteBlocks = new();

    /// <summary>
    /// 输入寄存器
    /// </summary>
    private ConcurrentDictionary<byte, ValueByteBlock> ModbusServer03ByteBlocks = new();

    /// <summary>
    /// 保持寄存器
    /// </summary>
    private ConcurrentDictionary<byte, ValueByteBlock> ModbusServer04ByteBlocks = new();

    /// <inheritdoc/>
    public ModbusSlave(IChannel channel) : base(channel)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
        if (channel is IClientChannel client)
            client.WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    #region 属性

    private ModbusTypeEnum modbusType;

    /// <summary>
    /// 客户端连接滑动过期时间(TCP服务通道时)
    /// </summary>
    public int CheckClearTime { get; set; } = 120;

    public string DtuId { get; set; } = "DtuId";

    public string HeartbeatHexString { get; set; } = "FFFF8080";

    public int HeartbeatTime { get; set; } = 5;

    /// <summary>
    /// 写入内存
    /// </summary>
    public bool IsWriteMemory { get; set; }

    /// <summary>
    /// 最大连接数
    /// </summary>
    public int MaxClientCount { get; set; } = 10000;

    /// <summary>
    /// Modbus类型
    /// </summary>
    public ModbusTypeEnum ModbusType
    {
        get { return modbusType; }
        set { modbusType = value; }
    }

    /// <summary>
    /// 多站点
    /// </summary>
    public bool MulStation { get; set; } = true;

    /// <summary>
    /// 默认站点
    /// </summary>
    public byte Station { get; set; } = 1;

    #endregion 属性

    /// <summary>
    /// 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
    /// </summary>
    public ModbusServerWriteEventHandler WriteData { get; set; }

    /// <inheritdoc/>
    public override Action<IPluginManager> ConfigurePlugins()
    {
        Channel.Config.SetMaxCount(MaxClientCount);
        switch (Channel.ChannelType)
        {
            case ChannelTypeEnum.TcpService:
                return PluginUtil.GetTcpServicePlugin(this);

            case ChannelTypeEnum.TcpClient:
                return PluginUtil.GetDtuClientPlugin(this);
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
                        return new ProtocolSingleStreamDataHandleAdapter<ModbusTcpSlaveMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new ProtocolUdpDataHandleAdapter<ModbusTcpSlaveMessage>()
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
                        return new ProtocolSingleStreamDataHandleAdapter<ModbusRtuSlaveMessage>()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new ProtocolUdpDataHandleAdapter<ModbusRtuSlaveMessage>()
                        {
                        };
                }
                break;
        }
        return new ProtocolSingleStreamDataHandleAdapter<ModbusTcpSlaveMessage>()
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
    protected override void Dispose(bool disposing)
    {
        foreach (var item in ModbusServer01ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer02ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer03ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        foreach (var item in ModbusServer04ByteBlocks)
        {
            item.Value.SafeDispose();
        }
        ModbusServer01ByteBlocks.Clear();
        ModbusServer02ByteBlocks.Clear();
        ModbusServer03ByteBlocks.Clear();
        ModbusServer04ByteBlocks.Clear();
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    private void Init(ModbusRequest mAddress)
    {
        ModbusServer01ByteBlocks.GetOrAdd(mAddress.Station, a => new ValueByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer02ByteBlocks.GetOrAdd(mAddress.Station, a => new ValueByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer03ByteBlocks.GetOrAdd(mAddress.Station, a => new ValueByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer04ByteBlocks.GetOrAdd(mAddress.Station, a => new ValueByteBlock(new byte[ushort.MaxValue * 2]));
    }

    #region 核心

    private readonly ReaderWriterLockSlim _lockSlim = new();

    /// <inheritdoc/>
    public OperResult<ReadOnlyMemory<byte>> ModbusRequest(ModbusRequest mAddress, bool read, CancellationToken cancellationToken = default)
    {
        try
        {
            if (this.MulStation)
            {
                this.Init(mAddress);
            }
            else
            {
                if (this.Station != mAddress.Station)
                {
                    return new(ModbusResource.Localizer["StationNotSame", mAddress.Station, this.Station]);
                }
                this.Init(mAddress);
            }
            var ModbusServer01ByteBlock = this.ModbusServer01ByteBlocks[mAddress.Station];
            var ModbusServer02ByteBlock = this.ModbusServer02ByteBlocks[mAddress.Station];
            var ModbusServer03ByteBlock = this.ModbusServer03ByteBlocks[mAddress.Station];
            var ModbusServer04ByteBlock = this.ModbusServer04ByteBlocks[mAddress.Station];
            if (read)
            {
                using (new ReadLock(this._lockSlim))
                {
                    int len = mAddress.Length;

                    switch (mAddress.FunctionCode)
                    {
                        case 1:
                            return OperResult.CreateSuccessResult(ModbusServer01ByteBlock.Memory.Slice(mAddress.StartAddress, len));

                        case 2:
                            return OperResult.CreateSuccessResult(ModbusServer02ByteBlock.Memory.Slice(mAddress.StartAddress, len));

                        case 3:

                            return OperResult.CreateSuccessResult(ModbusServer03ByteBlock.Memory.Slice(mAddress.StartAddress * this.RegisterByteLength, len));

                        case 4:
                            return OperResult.CreateSuccessResult(ModbusServer04ByteBlock.Memory.Slice(mAddress.StartAddress * this.RegisterByteLength, len));
                    }
                }
            }
            else
            {
                if (mAddress.FunctionCode == 0x05)
                {
                    var data = ModbusServer01ByteBlock.Memory.Slice(mAddress.StartAddress, 1);
                    mAddress.Data = new ReadOnlyMemory<byte>([data.Span[0].SetBit(mAddress.StartAddress % 8, mAddress.Data.Span[0] == 0xff)]);
                }
                using (new WriteLock(this._lockSlim))
                {
                    switch (mAddress.FunctionCode)
                    {
                        case 2:
                            ModbusServer02ByteBlock.Position = mAddress.StartAddress;
                            ModbusServer02ByteBlock.Write(mAddress.Data.Span);
                            return new();

                        case 1:
                        case 5:
                        case 15:
                            ModbusServer01ByteBlock.Position = mAddress.StartAddress;
                            ModbusServer01ByteBlock.Write(mAddress.Data.Span);
                            return new();

                        case 4:
                            ModbusServer04ByteBlock.Position = mAddress.StartAddress;
                            ModbusServer04ByteBlock.Write(mAddress.Data.Span);
                            return new();

                        case 3:
                        case 6:
                        case 16:
                            ModbusServer03ByteBlock.Position = mAddress.StartAddress * this.RegisterByteLength;
                            ModbusServer03ByteBlock.Write(mAddress.Data.Span);
                            return new();
                    }
                }
            }

            return new(ModbusResource.Localizer["FunctionError"]);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <inheritdoc/>
    public override ValueTask<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        ModbusAddress mAddress = ModbusAddress.ParseFrom(address, this.Station);
        mAddress.Length = (ushort)(length * RegisterByteLength);
        var result = ModbusRequest(mAddress, true, cancellationToken);
        if (result.IsSuccess)
        {
            return EasyValueTask.FromResult(new OperResult<byte[]>() { Content = result.Content.ToArray() });
        }
        else
        {
            return EasyValueTask.FromResult(new OperResult<byte[]>(result));
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            await EasyValueTask.CompletedTask;
            var mAddress = ModbusAddress.ParseFrom(address, Station, DtuId);
            mAddress.Data = value;
            return ModbusRequest(mAddress, false, cancellationToken);
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
            await EasyValueTask.CompletedTask;
            var mAddress = ModbusAddress.ParseFrom(address, Station, DtuId);
            if (value.Length > 1 && mAddress.FunctionCode == 1)
            {
                mAddress.WriteFunctionCode = 15;
                mAddress.Data = value.BoolArrayToByte();
                ModbusRequest(mAddress, false, cancellationToken);
                return OperResult.Success;
            }
            else if (mAddress.BitIndex == null)
            {
                mAddress.Data = value[0] ? new byte[2] { 255, 0 } : [0, 0];
                ModbusRequest(mAddress, false, cancellationToken);
                return OperResult.Success;
            }
            else
            {
                if (mAddress.BitIndex < 16)
                {
                    mAddress.Length = 2;
                    var readData = ModbusRequest(mAddress, true, cancellationToken);
                    if (!readData.IsSuccess) return readData;
                    var writeData = TouchSocketBitConverter.BigEndian.To<ushort>(readData.Content.Span);
                    for (int i = 0; i < value.Length; i++)
                    {
                        writeData = writeData.SetBit(mAddress.BitIndex.Value + i, value[i]);
                    }
                    mAddress.Data = ThingsGatewayBitConverter.GetBytes(writeData);
                    ModbusRequest(mAddress, false, cancellationToken);
                    return OperResult.Success;
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

    /// <inheritdoc/>
    protected override async Task ChannelReceived(IClientChannel client, ReceivedDataEventArgs e)
    {
        var requestInfo = e.RequestInfo;
        bool modbusRtu = false;
        ModbusRequest modbusRequest = default;
        ReadOnlyMemory<byte> Bytes = default;
        //接收外部报文
        if (requestInfo is ModbusRtuSlaveMessage modbusRtuSlaveMessage)
        {
            if (!modbusRtuSlaveMessage.IsSuccess)
            {
                return;
            }
            modbusRequest = modbusRtuSlaveMessage.Request;
            Bytes = modbusRtuSlaveMessage.Bytes;
            modbusRtu = true;
        }
        else if (requestInfo is ModbusTcpSlaveMessage modbusTcpSlaveMessage)
        {
            if (!modbusTcpSlaveMessage.IsSuccess)
            {
                return;
            }
            modbusRequest = modbusTcpSlaveMessage.Request;
            Bytes = modbusTcpSlaveMessage.Bytes;
            modbusRtu = false;
        }

        if (modbusRequest.FunctionCode <= 4)
        {
            var data = this.ModbusRequest(modbusRequest, true);
            if (data.IsSuccess)
            {
                ValueByteBlock valueByteBlock = new(1024);
                try
                {
                    if (modbusRtu)
                    {
                        valueByteBlock.Write(Bytes.Slice(0, 2).Span);
                        if (modbusRequest.FunctionCode == 1 || modbusRequest.FunctionCode == 2)
                        {
                            valueByteBlock.WriteByte((byte)modbusRequest.Length);
                            valueByteBlock.Write(data.Content.Span);
                        }
                        else
                        {
                            valueByteBlock.WriteByte((byte)data.Content.Length);
                            valueByteBlock.Write(data.Content.Span);
                        }
                        valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory.Span));
                        await ReturnData(client, valueByteBlock.Memory, e);
                    }
                    else
                    {
                        valueByteBlock.Write(Bytes.Slice(0, 8).Span);
                        if (modbusRequest.FunctionCode == 1 || modbusRequest.FunctionCode == 2)
                        {
                            valueByteBlock.WriteByte((byte)modbusRequest.Length);
                            valueByteBlock.Write(data.Content.Span);
                        }
                        else
                        {
                            valueByteBlock.WriteByte((byte)data.Content.Length);
                            valueByteBlock.Write(data.Content.Span);
                        }
                        valueByteBlock[5] = (byte)(valueByteBlock.Length - 6);
                        await ReturnData(client, valueByteBlock.Memory, e);
                    }
                }
                finally
                {
                    valueByteBlock.SafeDispose();
                }
            }
            else
            {
                await WriteError(modbusRtu, client, Bytes, e);//返回错误码
            }
        }
        else//写入
        {
            if (modbusRequest.FunctionCode == 5)
            {
                //写入继电器
                if (this.WriteData != null)
                {
                    var modbusAddress = new ModbusAddress(modbusRequest) { WriteFunctionCode = modbusRequest.FunctionCode, FunctionCode = 1 };
                    // 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
                    if ((await this.WriteData(modbusAddress, this.ThingsGatewayBitConverter, client).ConfigureAwait(false)).IsSuccess)
                    {
                        await WriteSuccess(modbusRtu, client, Bytes, e);
                        if (this.IsWriteMemory)
                        {
                            var result = this.ModbusRequest(modbusRequest, false);
                            if (result.IsSuccess)
                                await WriteSuccess(modbusRtu, client, Bytes, e);
                            else
                                await WriteError(modbusRtu, client, Bytes, e);
                        }
                        else
                            await WriteSuccess(modbusRtu, client, Bytes, e);
                    }
                    else
                    {
                        await WriteError(modbusRtu, client, Bytes, e);
                    }
                }
                else
                {
                    //写入内存区
                    var result = this.ModbusRequest(modbusRequest, false);
                    if (result.IsSuccess)
                    {
                        await WriteSuccess(modbusRtu, client, Bytes, e);
                    }
                    else
                    {
                        await WriteError(modbusRtu, client, Bytes, e);
                    }
                }
            }
            else
            {
                //写入寄存器
                if (this.WriteData != null)
                {
                    var modbusAddress = new ModbusAddress(modbusRequest) { WriteFunctionCode = modbusRequest.FunctionCode, FunctionCode = 3 };
                    if ((await this.WriteData(modbusAddress, this.ThingsGatewayBitConverter, client).ConfigureAwait(false)).IsSuccess)
                    {
                        if (this.IsWriteMemory)
                        {
                            var result = this.ModbusRequest(modbusRequest, false);
                            if (result.IsSuccess)
                                await WriteSuccess(modbusRtu, client, Bytes, e);
                            else
                                await WriteError(modbusRtu, client, Bytes, e);
                        }
                        else
                            await WriteSuccess(modbusRtu, client, Bytes, e);
                    }
                    else
                    {
                        await WriteError(modbusRtu, client, Bytes, e);
                    }
                }
                else
                {
                    var result = this.ModbusRequest(modbusRequest, false);
                    if (result.IsSuccess)
                    {
                        await WriteSuccess(modbusRtu, client, Bytes, e);
                    }
                    else
                    {
                        await WriteError(modbusRtu, client, Bytes, e);
                    }
                }
            }
        }
    }

    private async Task ReturnData(IClientChannel client, ReadOnlyMemory<byte> sendData, ReceivedDataEventArgs e)
    {
        if (SendDelayTime > 0)
            await Task.Delay(SendDelayTime).ConfigureAwait(false);
        if (client is IUdpClientSender udpClientSender)
            await udpClientSender.SendAsync(((UdpReceivedDataEventArgs)e).EndPoint, sendData);
        else
            await client.SendAsync(sendData);
    }

    private async Task WriteError(bool modbusRtu, IClientChannel client, ReadOnlyMemory<byte> bytes, ReceivedDataEventArgs e)
    {
        ValueByteBlock valueByteBlock = new(20);
        try
        {
            if (modbusRtu)
            {
                valueByteBlock.Write(bytes.Slice(0, 2).Span);
                valueByteBlock.WriteByte((byte)1);
                valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Span));
                valueByteBlock[1] = (byte)(valueByteBlock[1] + 128);
            }
            else
            {
                valueByteBlock.Write(bytes.Slice(0, 8).Span);
                valueByteBlock.WriteByte((byte)1);
                valueByteBlock[5] = (byte)(valueByteBlock.Length - 6);
                valueByteBlock[7] = (byte)(valueByteBlock[7] + 128);
            }
            await ReturnData(client, valueByteBlock.Memory, e);
        }
        finally
        {
            valueByteBlock.SafeDispose();
        }
    }

    private async Task WriteSuccess(bool modbusRtu, IClientChannel client, ReadOnlyMemory<byte> bytes, ReceivedDataEventArgs e)
    {
        ValueByteBlock valueByteBlock = new(20);
        try
        {
            if (modbusRtu)
            {
                valueByteBlock.Write(bytes.Slice(0, 6).Span);
                valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Span));
            }
            else
            {
                valueByteBlock.Write(bytes.Slice(0, 12).Span);
                valueByteBlock[5] = (byte)(valueByteBlock.Length - 6);
            }
            await ReturnData(client, valueByteBlock.Memory, e);
        }
        finally
        {
            valueByteBlock.SafeDispose();
        }
    }

    #endregion 核心
}
