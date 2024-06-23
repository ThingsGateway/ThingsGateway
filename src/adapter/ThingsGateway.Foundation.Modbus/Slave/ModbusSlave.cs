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

using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// ChannelEventHandler
/// </summary>
public delegate ValueTask<OperResult> ModbusServerWriteEventHandler(ModbusAddress modbusAddress, byte[] writeValue, IThingsGatewayBitConverter bitConverter, IClientChannel channel);

/// <inheritdoc/>
public class ModbusSlave : ProtocolBase, ITcpService, IDtuClient
{
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

    /// <summary>
    /// 写入内存
    /// </summary>
    public bool IsWriteMemory { get; set; }

    /// <summary>
    /// 最大连接数
    /// </summary>
    public int MaxClientCount { get; set; } = 10000;

    public string DtuId { get; set; } = "TEST";
    public int HeartbeatTime { get; set; } = 5;
    public string HeartbeatHexString { get; set; } = "FFFF8080";

    #endregion 属性

    /// <summary>
    /// 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
    /// </summary>
    public ModbusServerWriteEventHandler WriteData { get; set; }

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
    public override string GetAddressDescription()
    {
        return $"{base.GetAddressDescription()}{Environment.NewLine}{ModbusHelper.GetAddressDescription()}";
    }

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
                        return new ModbusTcpServerDataHandleAdapter()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new ModbusUdpServerDataHandleAdapter()
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
                        return new ModbusRtuServerDataHandleAdapter()
                        {
                            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                        };

                    case ChannelTypeEnum.UdpSession:
                        return new ModbusRtuOverUdpServerDataHandleAdapter()
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
    private void Init(ModbusAddress mAddress)
    {
        ModbusServer01ByteBlocks.GetOrAdd(mAddress.Station, a => new ValueByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer02ByteBlocks.GetOrAdd(mAddress.Station, a => new ValueByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer03ByteBlocks.GetOrAdd(mAddress.Station, a => new ValueByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer04ByteBlocks.GetOrAdd(mAddress.Station, a => new ValueByteBlock(new byte[ushort.MaxValue * 2]));
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

    #region 核心

    /// <inheritdoc/>
    protected override Task ChannelReceived(IClientChannel client, ReceivedDataEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            var requestInfo = e.RequestInfo;
            //接收外部报文
            if (requestInfo is IModbusServerMessage modbusServerMessage)
            {
                if (modbusServerMessage.ModbusAddress == null)
                {
                    return;//无法解析直接返回
                }
                if (!modbusServerMessage.IsSuccess)
                {
                    return;//无法解析直接返回
                }

                if (modbusServerMessage.ModbusAddress.WriteFunction == null)//读取
                {
                    var data = this.Read(modbusServerMessage.ModbusAddress.ToString(), modbusServerMessage.Length);
                    if (data.IsSuccess)
                    {
                        ValueByteBlock valueByteBlock = new(1024);
                        try
                        {
                            //rtu返回头
                            if (ModbusType == ModbusTypeEnum.ModbusRtu)
                            {
                                valueByteBlock.Write(modbusServerMessage.Bytes.Slice(0, 2).Span);
                                if (modbusServerMessage.ModbusAddress.ReadFunction == 1 || modbusServerMessage.ModbusAddress.ReadFunction == 2)
                                {
                                    valueByteBlock.WriteByte((byte)(Math.Ceiling(modbusServerMessage.Length / 8.0)));
                                    valueByteBlock.Write(data.Content.ToArray().Select(m => m > 0).ToArray().BoolArrayToByte().SelectMiddle(0, (int)Math.Ceiling(modbusServerMessage.Length / 8.0)));
                                }
                                else
                                {
                                    valueByteBlock.WriteByte((byte)data.Content.Length);
                                    valueByteBlock.Write(data.Content.Span);
                                }
                                valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory));
                                await ReturnData(client, e, valueByteBlock.Memory);
                            }
                            else
                            {
                                valueByteBlock.Write(modbusServerMessage.Bytes.Slice(0, 8).Span);
                                if (modbusServerMessage.ModbusAddress.ReadFunction == 1 || modbusServerMessage.ModbusAddress.ReadFunction == 2)
                                {
                                    valueByteBlock.WriteByte((byte)(Math.Ceiling(modbusServerMessage.Length / 8.0)));
                                    valueByteBlock.Write(data.Content.ToArray().Select(m => m > 0).ToArray().BoolArrayToByte().SelectMiddle(0, (int)Math.Ceiling(modbusServerMessage.Length / 8.0)));
                                }
                                else
                                {
                                    valueByteBlock.WriteByte((byte)data.Content.Length);
                                    valueByteBlock.Write(data.Content.Span);
                                }
                                valueByteBlock[5] = (byte)(valueByteBlock.Length - 6);
                                await ReturnData(client, e, valueByteBlock.Memory);
                            }
                        }
                        finally
                        {
                            valueByteBlock.SafeDispose();
                        }
                    }
                    else
                    {
                        await WriteError(this.ModbusType, client, modbusServerMessage, e);//返回错误码
                    }
                }
                else//写入
                {
                    var coreData = modbusServerMessage.Content;
                    if (modbusServerMessage.ModbusAddress.ReadFunction == 1 || modbusServerMessage.ModbusAddress.ReadFunction == 2)
                    {
                        //写入继电器
                        if (this.WriteData != null)
                        {
                            // 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
                            if ((await this.WriteData(modbusServerMessage.ModbusAddress, modbusServerMessage.Content, this.ThingsGatewayBitConverter, client).ConfigureAwait(false)).IsSuccess)
                            {
                                await WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                                if (this.IsWriteMemory)
                                {
                                    var result = this.Write(modbusServerMessage.ModbusAddress.ToString(), coreData, modbusServerMessage.Length);
                                    if (result.IsSuccess)
                                        await WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                                    else
                                        await WriteError(this.ModbusType, client, modbusServerMessage, e);
                                }
                                else
                                    await WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                            }
                            else
                            {
                                await WriteError(this.ModbusType, client, modbusServerMessage, e);
                            }
                        }
                        else
                        {
                            //写入内存区
                            var result = this.Write(modbusServerMessage.ModbusAddress.ToString(), coreData, modbusServerMessage.Length);
                            if (result.IsSuccess)
                            {
                                await WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                            }
                            else
                            {
                                await WriteError(this.ModbusType, client, modbusServerMessage, e);
                            }
                        }
                    }
                    else
                    {
                        //写入寄存器
                        if (this.WriteData != null)
                        {
                            if ((await this.WriteData(modbusServerMessage.ModbusAddress, modbusServerMessage.Content, this.ThingsGatewayBitConverter, client).ConfigureAwait(false)).IsSuccess)
                            {
                                if (this.IsWriteMemory)
                                {
                                    var result = this.Write(modbusServerMessage.ModbusAddress.ToString(), coreData);
                                    if (result.IsSuccess)
                                        await WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                                    else
                                        await WriteError(this.ModbusType, client, modbusServerMessage, e);
                                }
                                else
                                    await WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                            }
                            else
                            {
                                await WriteError(this.ModbusType, client, modbusServerMessage, e);
                            }
                        }
                        else
                        {
                            var result = this.Write(modbusServerMessage.ModbusAddress.ToString(), coreData);
                            if (result.IsSuccess)
                            {
                                await WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                            }
                            else
                            {
                                await WriteError(this.ModbusType, client, modbusServerMessage, e);
                            }
                        }
                    }
                }
            }
        });
        return EasyTask.CompletedTask;
    }

    private async Task ReturnData(IClientChannel client, ReceivedDataEventArgs e, ReadOnlyMemory<byte> sendData)
    {
        if (SendDelayTime > 0)
            await Task.Delay(SendDelayTime).ConfigureAwait(false);
        if (client is IUdpClientSender udpClientSender)
            if (ModbusType == ModbusTypeEnum.ModbusRtu)
                await udpClientSender.SendAsync(((UdpReceivedDataEventArgs)e).EndPoint, new SendMessage(sendData));
            else
                await udpClientSender.SendAsync(((UdpReceivedDataEventArgs)e).EndPoint, sendData);
        else
            if (ModbusType == ModbusTypeEnum.ModbusRtu)
            await client.SendAsync(new SendMessage(sendData));
        else
            await client.SendAsync(sendData);
    }

    /// <summary>
    /// 返回错误码
    /// </summary>
    private async Task WriteError(ModbusTypeEnum modbusType, IClientChannel client, IModbusServerMessage modbusServerMessage, ReceivedDataEventArgs e)
    {
        ValueByteBlock valueByteBlock = new(20);
        try
        {
            if (modbusType == ModbusTypeEnum.ModbusRtu)
            {
                valueByteBlock.Write(modbusServerMessage.Bytes.Slice(0, 2).Span);
                valueByteBlock.WriteByte((byte)1);
                valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory));
                valueByteBlock[1] = (byte)(valueByteBlock[1] + 128);
                await ReturnData(client, e, valueByteBlock.Memory);
            }
            else
            {
                valueByteBlock.Write(modbusServerMessage.Bytes.Slice(0, 8).Span);
                valueByteBlock.WriteByte((byte)1);
                valueByteBlock[5] = (byte)(valueByteBlock.Length - 6);
                valueByteBlock[7] = (byte)(valueByteBlock[7] + 128);
                await ReturnData(client, e, valueByteBlock.Memory);
            }
        }
        finally
        {
            valueByteBlock.SafeDispose();
        }
    }

    /// <summary>
    /// 返回成功
    /// </summary>
    internal async Task WriteSuccess(ModbusTypeEnum modbusType, IClientChannel client, IModbusServerMessage modbusServerMessage, ReceivedDataEventArgs e)
    {
        ValueByteBlock valueByteBlock = new(20);
        if (modbusType == ModbusTypeEnum.ModbusRtu)
        {
            valueByteBlock.Write(modbusServerMessage.Bytes.Slice(0, 6).Span);
            valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory));
            await ReturnData(client, e, valueByteBlock.Memory);
        }
        else
        {
            valueByteBlock.Write(modbusServerMessage.Bytes.Slice(0, 12).Span);
            valueByteBlock[5] = (byte)(valueByteBlock.Length - 6);
            await ReturnData(client, e, valueByteBlock.Memory);
        }
    }

    private readonly ReaderWriterLockSlim _lockSlim = new();

    /// <inheritdoc/>
    public OperResult<Memory<byte>> Read(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            ModbusAddress mAddress = ModbusAddress.ParseFrom(address, this.Station);

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

            using (new ReadLock(this._lockSlim))
            {
                var ModbusServer01ByteBlock = this.ModbusServer01ByteBlocks[mAddress.Station];
                var ModbusServer02ByteBlock = this.ModbusServer02ByteBlocks[mAddress.Station];
                var ModbusServer03ByteBlock = this.ModbusServer03ByteBlocks[mAddress.Station];
                var ModbusServer04ByteBlock = this.ModbusServer04ByteBlocks[mAddress.Station];
                int len = mAddress.ReadFunction == 2 || mAddress.ReadFunction == 1 ? length : length * this.RegisterByteLength;

                switch (mAddress.ReadFunction)
                {
                    case 1:
                        return OperResult.CreateSuccessResult(ModbusServer01ByteBlock.ToMemory(mAddress.AddressStart, len));

                    case 2:
                        return OperResult.CreateSuccessResult(ModbusServer02ByteBlock.ToMemory(mAddress.AddressStart, len));

                    case 3:

                        return OperResult.CreateSuccessResult(ModbusServer03ByteBlock.ToMemory(mAddress.AddressStart * this.RegisterByteLength, len));

                    case 4:
                        return OperResult.CreateSuccessResult(ModbusServer04ByteBlock.ToMemory(mAddress.AddressStart * this.RegisterByteLength, len));
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
        var result = Read(address, length, cancellationToken);
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
    public OperResult Write(string address, byte[] value, int? length = null, CancellationToken cancellationToken = default)
    {
        try
        {
            length ??= value.Length;
            ModbusAddress mAddress = ModbusAddress.ParseFrom(address, this.Station);

            if (this.MulStation)
            {
                this.Init(mAddress);
            }
            else
            {
                if (this.Station != mAddress.Station)
                {
                    return new OperResult<byte[]>(ModbusResource.Localizer["StationNotSame", mAddress.Station, this.Station]);
                }
                this.Init(mAddress);
            }

            using (new WriteLock(this._lockSlim))
            {
                var ModbusServer01ByteBlock = this.ModbusServer01ByteBlocks[mAddress.Station];
                var ModbusServer02ByteBlock = this.ModbusServer02ByteBlocks[mAddress.Station];
                var ModbusServer03ByteBlock = this.ModbusServer03ByteBlocks[mAddress.Station];
                var ModbusServer04ByteBlock = this.ModbusServer04ByteBlocks[mAddress.Station];
                switch (mAddress.ReadFunction)
                {
                    case 1:
                        ModbusServer01ByteBlock.Position = mAddress.AddressStart;
                        ModbusServer01ByteBlock.Write(value.ByteToBoolArray(length.Value).BoolArrayToByte());
                        return OperResult.Success;

                    case 2:
                        ModbusServer02ByteBlock.Position = mAddress.AddressStart;
                        ModbusServer02ByteBlock.Write(value);
                        return OperResult.Success;

                    case 3:
                        ModbusServer03ByteBlock.Position = mAddress.AddressStart * this.RegisterByteLength;
                        ModbusServer03ByteBlock.Write(value);
                        return OperResult.Success;

                    case 4:
                        ModbusServer04ByteBlock.Position = mAddress.AddressStart * this.RegisterByteLength;
                        ModbusServer04ByteBlock.Write(value);
                        return OperResult.Success;
                }
            }
            return new OperResult<byte[]>(ModbusResource.Localizer["FunctionError"]);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        return EasyValueTask.FromResult(Write(address, value, cancellationToken: cancellationToken));
    }

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        return EasyValueTask.FromResult(Write(address, value.BoolArrayToByte(), cancellationToken: cancellationToken));
    }

    #endregion 核心
}
