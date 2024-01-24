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
using System.Text;

using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Modbus;

/// <inheritdoc/>
public class ModbusMaster : ProtocolBase
{
    /// <inheritdoc/>
    public ModbusMaster(IChannel channel) : base(channel)
    {
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
        RegisterByteLength = 2;
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    private ModbusTypeEnum modbusType;

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
    /// 无交互时2min自动断开连接(TCP服务通道时)
    /// </summary>
    [Description("自动断开连接")]
    public bool CheckClear { get; set; } = false;

    /// <summary>
    /// 心跳检测(大写16进制字符串)
    /// </summary>
    [Description("心跳检测")]
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
                if (CheckClear)
                {
                    action = a => a.UseCheckClear()
      .SetCheckClearType(CheckClearType.All)
      .SetTick(TimeSpan.FromSeconds(60))
      .SetOnClose((c, t) =>
      {
          c.TryShutdown();
          c.SafeClose("超时清理");
      });
                }

                action += a =>
                   {
                       DtuPlugin dtuPlugin = new(this);
                       a.Add(dtuPlugin);
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
        return PackHelper.LoadSourceRead<T>(this, deviceVariables.ToList(), maxPack, defaultIntervalTime);
    }

    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = ModbusHelper.GetReadModbusCommand(address, (ushort)length, Station);

            return SendThenReturn(address, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    private OperResult<byte[]> SendThenReturn(string address, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            var mAddress = ModbusAddressHelper.ParseFrom(address, Station);
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={mAddress.SocketId}", out TgSocketClient? client))
                return SendThenReturn(new SendMessage(commandResult), cancellationToken, client);
            else
                return new OperResult<byte[]>(FoundationConst.DtuNoConnectedWaining);
        }
        else
            return SendThenReturn(new SendMessage(commandResult), cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = ModbusHelper.GetReadModbusCommand(address, (ushort)length, Station);
            return await SendThenReturnAsync(address, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    private Task<OperResult<byte[]>> SendThenReturnAsync(string address, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            var mAddress = ModbusAddressHelper.ParseFrom(address, Station);
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={mAddress.SocketId}", out TgSocketClient? client))
                return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken, client);
            else
                return Task.FromResult(new OperResult<byte[]>(FoundationConst.DtuNoConnectedWaining));
        }
        else
            return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken);
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            return SendThenReturn(address, commandResult, cancellationToken);
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
            var mAddress = ModbusAddressHelper.ParseFrom(address, Station);
            //功能码或实际长度
            if (value.Length > 1 || mAddress.WriteFunction == 15)
            {
                var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value, (ushort)value.Length);
                return SendThenReturn(address, commandResult, cancellationToken);
            }
            else
            {
                if (mAddress.BitIndex == null)
                {
                    var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value[0]);
                    return SendThenReturn(address, commandResult, cancellationToken);
                }
                else if (mAddress.BitIndex < 16)
                {
                    //比如40001.1
                    var read = ModbusHelper.GetReadModbusCommand(mAddress, 1);
                    var readData = SendThenReturn(address, read, cancellationToken);
                    if (!readData.IsSuccess) return readData;
                    var writeData = ThingsGatewayBitConverter.ToUInt16(readData.Content, 0);
                    ushort mask = (ushort)(1 << mAddress.BitIndex);
                    ushort result = (ushort)(value[0] ? (writeData | mask) : (writeData & ~mask));
                    var write = ModbusHelper.GetWriteOneModbusCommand(mAddress, ThingsGatewayBitConverter.GetBytes(result));
                    return SendThenReturn(address, read, cancellationToken);
                }
                else
                {
                    return new(string.Format(ModbusConst.ValueOverlimit, nameof(mAddress.BitIndex), 16));
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
            var commandResult = ModbusHelper.GetWriteModbusCommand(address, value, Station);
            return await SendThenReturnAsync(address, commandResult, cancellationToken);
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
            var mAddress = ModbusAddressHelper.ParseFrom(address, Station);
            //功能码或实际长度
            if (value.Length > 1 || mAddress.WriteFunction == 15)
            {
                var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value, (ushort)value.Length);
                return await SendThenReturnAsync(address, commandResult, cancellationToken);
            }
            else
            {
                if (mAddress.BitIndex == null)
                {
                    var commandResult = ModbusHelper.GetWriteBoolModbusCommand(mAddress, value[0]);
                    return await SendThenReturnAsync(address, commandResult, cancellationToken);
                }
                else if (mAddress.BitIndex < 16)
                {
                    //比如40001.1

                    var read = ModbusHelper.GetReadModbusCommand(mAddress, 1);
                    var readData = await SendThenReturnAsync(address, read, cancellationToken);
                    if (!readData.IsSuccess) return readData;
                    var writeData = ThingsGatewayBitConverter.ToUInt16(readData.Content, 0);
                    ushort mask = (ushort)(1 << mAddress.BitIndex);
                    ushort result = (ushort)(value[0] ? (writeData | mask) : (writeData & ~mask));
                    var write = ModbusHelper.GetWriteOneModbusCommand(mAddress, ThingsGatewayBitConverter.GetBytes(result));
                    return await SendThenReturnAsync(address, read, cancellationToken);
                }
                else
                {
                    return new(string.Format(ModbusConst.ValueOverlimit, nameof(mAddress.BitIndex), 16));
                }
            }
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }
}

[PluginOption(Singleton = true)]
internal class DtuPlugin : PluginBase, ITcpReceivingPlugin
{
    private ModbusMaster _modbusMaster;

    public DtuPlugin(ModbusMaster modbusMaster)
    {
        this._modbusMaster = modbusMaster;
    }

    public async Task OnTcpReceiving(ITcpClientBase client, ByteBlockEventArgs e)
    {
        if (client is ISocketClient socket)
        {
            var bytes = e.ByteBlock.ToArray();
            if (!socket.Id.StartsWith("ID="))
            {
                var id = $"ID={Encoding.UTF8.GetString(bytes)}";
                client.Logger.Info(string.Format(FoundationConst.DtuConnected, id));
                socket.ResetId(id);
            }
            if (_modbusMaster.HeartbeatHexString == bytes.ToHexString())
            {
                //回应心跳包
                socket.DefaultSend(bytes);
                socket.Logger?.Trace($"{socket.ToString()}- {FoundationConst.Send}:{bytes.ToHexString(' ')}");
            }
        }
        await e.InvokeNext();//如果本插件无法处理当前数据，请将数据转至下一个插件。
    }
}