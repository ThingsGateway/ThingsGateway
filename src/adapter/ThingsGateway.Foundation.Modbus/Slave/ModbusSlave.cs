
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using System.Collections.Concurrent;

using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// ChannelEventHandler
/// </summary>
public delegate Task<OperResult> ModbusServerWriteEventHandler(ModbusAddress modbusAddress, byte[] writeValue, IThingsGatewayBitConverter bitConverter, IClientChannel channel);

/// <inheritdoc/>
public class ModbusSlave : ProtocolBase
{
    /// <inheritdoc/>
    public ModbusSlave(IChannel channel) : base(channel)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
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

    #endregion 属性

    /// <summary>
    /// 接收外部写入时，传出变量地址/写入字节组/转换规则/客户端
    /// </summary>
    public ModbusServerWriteEventHandler WriteData { get; set; }

    /// <summary>
    /// 继电器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer01ByteBlocks = new();

    /// <summary>
    /// 开关输入
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer02ByteBlocks = new();

    /// <summary>
    /// 输入寄存器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer03ByteBlocks = new();

    /// <summary>
    /// 保持寄存器
    /// </summary>
    private ConcurrentDictionary<byte, ByteBlock> ModbusServer04ByteBlocks = new();

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
                    case ChannelTypeEnum.SerialPortClient:
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
        ModbusServer01ByteBlocks.GetOrAdd(mAddress.Station, a => new ByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer02ByteBlocks.GetOrAdd(mAddress.Station, a => new ByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer03ByteBlocks.GetOrAdd(mAddress.Station, a => new ByteBlock(new byte[ushort.MaxValue * 2]));
        ModbusServer04ByteBlocks.GetOrAdd(mAddress.Station, a => new ByteBlock(new byte[ushort.MaxValue * 2]));
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
    protected override async Task Received(IClientChannel client, ReceivedDataEventArgs e)
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
                    var coreData = data.Content;
                    if (modbusServerMessage.ModbusAddress.ReadFunction == 1 || modbusServerMessage.ModbusAddress.ReadFunction == 2)
                    {
                        coreData = data.Content.Select(m => m > 0).ToArray().BoolArrayToByte().SelectMiddle(0, (int)Math.Ceiling(modbusServerMessage.Length / 8.0));
                    }
                    //rtu返回头
                    if (ModbusType == ModbusTypeEnum.ModbusRtu)
                    {
                        var sendData = DataTransUtil.SpliceArray(modbusServerMessage.ReceivedBytes.SelectMiddle(0, 2), new byte[] { (byte)coreData.Length }, coreData);
                        ReturnData(client, e, sendData);
                    }
                    else
                    {
                        var sendData = DataTransUtil.SpliceArray(modbusServerMessage.ReceivedBytes.SelectMiddle(0, 8), new byte[] { (byte)coreData.Length }, coreData);
                        sendData[5] = (byte)(sendData.Length - 6);
                        ReturnData(client, e, sendData);
                    }
                }
                else
                {
                    WriteError(this.ModbusType, client, modbusServerMessage, e);//返回错误码
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
                        if ((await this.WriteData(modbusServerMessage.ModbusAddress, modbusServerMessage.Content, this.ThingsGatewayBitConverter, client)).IsSuccess)
                        {
                            WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                            if (this.IsWriteMemory)
                            {
                                var result = this.Write(modbusServerMessage.ModbusAddress.ToString(), coreData.ByteToBoolArray(modbusServerMessage.Length));
                                if (result.IsSuccess)
                                    WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                                else
                                    WriteError(this.ModbusType, client, modbusServerMessage, e);
                            }
                            else
                                WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                        }
                        else
                        {
                            WriteError(this.ModbusType, client, modbusServerMessage, e);
                        }
                    }
                    else
                    {
                        //写入内存区
                        var result = this.Write(modbusServerMessage.ModbusAddress.ToString(), coreData.ByteToBoolArray(modbusServerMessage.Length));
                        if (result.IsSuccess)
                        {
                            WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                        }
                        else
                        {
                            WriteError(this.ModbusType, client, modbusServerMessage, e);
                        }
                    }
                }
                else
                {
                    //写入寄存器
                    if (this.WriteData != null)
                    {
                        if ((await this.WriteData(modbusServerMessage.ModbusAddress, modbusServerMessage.Content, this.ThingsGatewayBitConverter, client)).IsSuccess)
                        {
                            if (this.IsWriteMemory)
                            {
                                var result = this.Write(modbusServerMessage.ModbusAddress.ToString(), coreData);
                                if (result.IsSuccess)
                                    WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                                else
                                    WriteError(this.ModbusType, client, modbusServerMessage, e);
                            }
                            else
                                WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                        }
                        else
                        {
                            WriteError(this.ModbusType, client, modbusServerMessage, e);
                        }
                    }
                    else
                    {
                        var result = this.Write(modbusServerMessage.ModbusAddress.ToString(), coreData);
                        if (result.IsSuccess)
                        {
                            WriteSuccess(this.ModbusType, client, modbusServerMessage, e);
                        }
                        else
                        {
                            WriteError(this.ModbusType, client, modbusServerMessage, e);
                        }
                    }
                }
            }
        }
    }

    private static void ReturnData(IClientChannel client, ReceivedDataEventArgs e, byte[] sendData)
    {
        if (client is IUdpClientSender udpClientSender)
            udpClientSender.Send(((UdpReceivedDataEventArgs)e).EndPoint, sendData);
        else
            client.Send(sendData);
    }

    /// <summary>
    /// 返回错误码
    /// </summary>
    private static void WriteError(ModbusTypeEnum modbusType, IClientChannel client, IModbusServerMessage modbusServerMessage, ReceivedDataEventArgs e)
    {
        if (modbusType == ModbusTypeEnum.ModbusRtu)
        {
            var sendData = DataTransUtil
.SpliceArray(modbusServerMessage.ReceivedBytes.SelectMiddle(0, 2), new byte[] { (byte)1 });//01 lllegal function
            sendData[1] = (byte)(sendData[1] + 128);
            ReturnData(client, e, sendData);
        }
        else
        {
            var sendData = DataTransUtil
.SpliceArray(modbusServerMessage.ReceivedBytes.SelectMiddle(0, 8), new byte[] { (byte)1 });//01 lllegal function
            sendData[5] = (byte)(sendData.Length - 6);
            sendData[7] = (byte)(sendData[7] + 128);
            ReturnData(client, e, sendData);
        }
    }

    /// <summary>
    /// 返回成功
    /// </summary>
    internal static void WriteSuccess(ModbusTypeEnum modbusType, IClientChannel client, IModbusServerMessage modbusServerMessage, ReceivedDataEventArgs e)
    {
        if (modbusType == ModbusTypeEnum.ModbusRtu)
        {
            var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 6);
            ReturnData(client, e, sendData);
        }
        else
        {
            var sendData = modbusServerMessage.ReceivedBytes.SelectMiddle(0, 12);
            sendData[5] = (byte)(sendData.Length - 6);
            ReturnData(client, e, sendData);
        }
    }

    private readonly ReaderWriterLockSlim _lockSlim = new();

    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
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
                    return new OperResult<byte[]>(ModbusResource.Localizer["StationNotSame", mAddress.Station, this.Station]);
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
                        byte[] bytes0 = new byte[len];
                        ModbusServer01ByteBlock.Pos = mAddress.AddressStart;
                        ModbusServer01ByteBlock.Read(bytes0);
                        return OperResult.CreateSuccessResult(bytes0);

                    case 2:
                        byte[] bytes1 = new byte[len];
                        ModbusServer02ByteBlock.Pos = mAddress.AddressStart;
                        ModbusServer02ByteBlock.Read(bytes1);
                        return OperResult.CreateSuccessResult(bytes1);

                    case 3:

                        byte[] bytes3 = new byte[len];
                        ModbusServer03ByteBlock.Pos = mAddress.AddressStart * this.RegisterByteLength;
                        ModbusServer03ByteBlock.Read(bytes3);
                        return OperResult.CreateSuccessResult(bytes3);

                    case 4:
                        byte[] bytes4 = new byte[len];
                        ModbusServer04ByteBlock.Pos = mAddress.AddressStart * this.RegisterByteLength;
                        ModbusServer04ByteBlock.Read(bytes4);
                        return OperResult.CreateSuccessResult(bytes4);
                }
            }

            return new OperResult<byte[]>(ModbusResource.Localizer["FunctionError"]);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <inheritdoc/>
    public override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Read(address, length, cancellationToken));
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
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
                    return new OperResult<byte[]>(ModbusResource.Localizer["StationNotSame", mAddress.Station, this.Station]);
                }
                this.Init(mAddress);
            }

            using (new WriteLock(this._lockSlim))
            {
                var ModbusServer03ByteBlock = this.ModbusServer03ByteBlocks[mAddress.Station];
                var ModbusServer04ByteBlock = this.ModbusServer04ByteBlocks[mAddress.Station];
                switch (mAddress.ReadFunction)
                {
                    case 3:
                        ModbusServer03ByteBlock.Pos = mAddress.AddressStart * this.RegisterByteLength;
                        ModbusServer03ByteBlock.Write(value);
                        return new();

                    case 4:
                        ModbusServer04ByteBlock.Pos = mAddress.AddressStart * this.RegisterByteLength;
                        ModbusServer04ByteBlock.Write(value);
                        return new();
                }
            }
            return new OperResult<byte[]>(ModbusResource.Localizer["FunctionError"]);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default)
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
                    return new OperResult<byte[]>(ModbusResource.Localizer["StationNotSame", mAddress.Station, this.Station]);
                }
                this.Init(mAddress);
            }

            using (new WriteLock(this._lockSlim))
            {
                var ModbusServer01ByteBlock = this.ModbusServer01ByteBlocks[mAddress.Station];
                var ModbusServer02ByteBlock = this.ModbusServer02ByteBlocks[mAddress.Station];
                switch (mAddress.ReadFunction)
                {
                    case 1:
                        ModbusServer01ByteBlock.Pos = mAddress.AddressStart;
                        ModbusServer01ByteBlock.Write(value.BoolArrayToByte());
                        return new();

                    case 2:
                        ModbusServer02ByteBlock.Pos = mAddress.AddressStart;
                        ModbusServer02ByteBlock.Write(value.BoolArrayToByte());
                        return new();
                }
            }
            return new OperResult<byte[]>(ModbusResource.Localizer["FunctionError"]);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Write(address, value, cancellationToken));
    }

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Write(address, value, cancellationToken));
    }

    #endregion 核心
}