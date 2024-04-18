
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
using ThingsGateway.Foundation.Extension.String;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Dlt645;

/// <inheritdoc/>
public class Dlt645_2007Master : ProtocolBase
{
    /// <inheritdoc/>
    public Dlt645_2007Master(IChannel channel) : base(channel)
    {
        ThingsGatewayBitConverter = new Dlt645_2007BitConverter(EndianType.Big);
        RegisterByteLength = 2;
        WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <inheritdoc/>
    public bool EnableFEHead { get; set; }

    /// <inheritdoc/>
    public string OperCode { get; set; }

    /// <inheritdoc/>
    public string Password { get; set; }

    /// <inheritdoc/>
    public string Station { get; set; }

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
        return $"{base.GetAddressDescription()}{Environment.NewLine}{DltResource.Localizer["AddressDes"]}";
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
                    a.Add<DtuPlugin>();
                };
                return action;
        }
        return base.ConfigurePlugins();
    }

    /// <inheritdoc/>
    public override DataHandlingAdapter GetDataAdapter()
    {
        switch (Channel.ChannelType)
        {
            case ChannelTypeEnum.TcpClient:
            case ChannelTypeEnum.TcpService:
            case ChannelTypeEnum.SerialPortClient:
                return new Dlt645_2007DataHandleAdapter
                {
                    EnableFEHead = EnableFEHead,
                    CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                };

            case ChannelTypeEnum.UdpSession:
                return new Dlt645_2007UdpDataHandleAdapter()
                {
                    EnableFEHead = EnableFEHead,
                };
        }

        return new Dlt645_2007DataHandleAdapter
        {
            EnableFEHead = EnableFEHead,
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
            var commandResult = Dlt645Helper.GetDlt645_2007Command(address, (byte)ControlCode.Read, Station);

            return SendThenReturn(address, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    private OperResult Send(string address, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            var mAddress = Dlt645_2007Address.ParseFrom(address);
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={mAddress.SocketId}", out TgSocketClient? client))
            {
                Send(commandResult, client);
                return new();
            }
            else
                return new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]);
        }
        else
        {
            Send(commandResult);
            return new();
        }
    }

    private OperResult<byte[]> SendThenReturn(string address, byte[] commandResult, CancellationToken cancellationToken)
    {
        if (Channel.ChannelType == ChannelTypeEnum.TcpService)
        {
            var mAddress = Dlt645_2007Address.ParseFrom(address);
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={mAddress.SocketId}", out TgSocketClient? client))
                return SendThenReturn(new SendMessage(commandResult), cancellationToken, client);
            else
                return new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]);
        }
        else
            return SendThenReturn(new SendMessage(commandResult), cancellationToken);
    }

    /// <inheritdoc/>
    public override OperResult<string[]> ReadString(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = Read(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => new[] { bitConverter.ToString(result.Content, 0, length) });
    }

    /// <inheritdoc/>
    public override async Task<OperResult<string[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken);
        return result.OperResultFrom(() => new[] { bitConverter.ToString(result.Content, 0, length) });
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = Dlt645Helper.GetDlt645_2007Command(address, (byte)ControlCode.Read, Station);
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
            var mAddress = Dlt645_2007Address.ParseFrom(address);
            if (((TcpServiceBase)Channel).SocketClients.TryGetSocketClient($"ID={mAddress.SocketId}", out TgSocketClient? client))
                return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken, client);
            else
                return Task.FromResult(new OperResult<byte[]>(DefaultResource.Localizer["DtuNoConnectedWaining"]));
        }
        else
            return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken);
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Password ??= string.Empty;
            OperCode ??= string.Empty;
            if (Password.Length < 8)
                Password = Password.PadLeft(8, '0');
            if (OperCode.Length < 8)
                OperCode = OperCode.PadLeft(8, '0');

            var data = DataTransUtil.SpliceArray(Password.ByHexStringToBytes(), OperCode.ByHexStringToBytes());
            string[] strArray = value.SplitStringBySemicolon();
            var commandResult = Dlt645Helper.GetDlt645_2007Command(address, (byte)ControlCode.Write, Station, data, strArray);
            return SendThenReturn(string.Empty, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            Password ??= string.Empty;
            OperCode ??= string.Empty;
            if (Password.Length < 8)
                Password = Password.PadLeft(8, '0');
            if (OperCode.Length < 8)
                OperCode = OperCode.PadLeft(8, '0');

            var data = DataTransUtil.SpliceArray(Password.ByHexStringToBytes(), OperCode.ByHexStringToBytes());
            string[] strArray = value.SplitStringBySemicolon();
            var commandResult = Dlt645Helper.GetDlt645_2007Command(address, (byte)ControlCode.Write, Station, data, strArray);
            return await SendThenReturnAsync(string.Empty, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    #region

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default) => Write(address, value.ToString(), null, cancellationToken);

    /// <inheritdoc/>
    public override OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default) => Write(address, value.ToString(), null, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default) => Task.FromResult(new OperResult());

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default) => Task.FromResult(new OperResult());

    #endregion

    #region 其他方法

    /// <summary>
    /// 广播校时
    /// </summary>
    /// <param name="dateTime">时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public OperResult BroadcastTime(DateTime dateTime, CancellationToken cancellationToken = default)
    {
        try
        {
            string str = $"{dateTime.Second:D2}{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}{dateTime.Year % 100:D2}";
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.BroadcastTime, str.ByHexStringToBytes().ToArray(), "999999999999".ByHexStringToBytes());
            Send(string.Empty, commandResult, cancellationToken);
            return new();
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    /// <summary>
    /// 冻结
    /// </summary>
    /// <param name="dateTime">时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async Task<OperResult> FreezeAsync(DateTime dateTime, CancellationToken cancellationToken = default)
    {
        try
        {
            string str = $"{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}";
            if (Station.IsNullOrEmpty()) Station = string.Empty;
            if (Station.Length < 12) Station = Station.PadLeft(12, '0');
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.Freeze, str.ByHexStringToBytes().ToArray(), Station.ByHexStringToBytes().Reverse().ToArray());
            return await SendThenReturnAsync(string.Empty, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    /// <summary>
    /// 读取通信地址
    /// </summary>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async Task<OperResult<string>> ReadDeviceStationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.ReadStation, null, "AAAAAAAAAAAA".ByHexStringToBytes());
            var result = await SendThenReturnAsync(string.Empty, commandResult, cancellationToken);
            if (result.IsSuccess)
            {
                var buffer = result.Content.SelectMiddle(0, 6).BytesAdd(-0x33);
                return OperResult.CreateSuccessResult(buffer.Reverse().ToArray().ToHexString());
            }
            else
            {
                return new(result);
            }
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }

    /// <summary>
    /// 修改波特率
    /// </summary>
    /// <param name="baudRate">波特率</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async Task<OperResult> WriteBaudRateAsync(int baudRate, CancellationToken cancellationToken = default)
    {
        try
        {
            byte baudRateByte;
            switch (baudRate)
            {
                case 600: baudRateByte = 0x02; break;
                case 1200: baudRateByte = 0x04; break;
                case 2400: baudRateByte = 0x08; break;
                case 4800: baudRateByte = 0x10; break;
                case 9600: baudRateByte = 0x20; break;
                case 19200: baudRateByte = 0x40; break;
                default: return new OperResult<string>(DltResource.Localizer["BaudRateError", baudRate]);
            }
            if (Station.IsNullOrEmpty()) Station = string.Empty;
            if (Station.Length < 12) Station = Station.PadLeft(12, '0');
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.WriteBaudRate, new byte[] { baudRateByte }, Station.ByHexStringToBytes().Reverse().ToArray());
            return await SendThenReturnAsync(string.Empty, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    /// <summary>
    /// 更新通信地址
    /// </summary>
    /// <param name="station">站号</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async Task<OperResult> WriteDeviceStationAsync(string station, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.WriteStation, station.ByHexStringToBytes().Reverse().ToArray(), "AAAAAAAAAAAA".ByHexStringToBytes());
            return await SendThenReturnAsync(string.Empty, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="level">密码等级，0-8</param>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async Task<OperResult> WritePasswordAsync(byte level, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            if (Station.IsNullOrEmpty()) Station = string.Empty;
            if (Station.Length < 12) Station = Station.PadLeft(12, '0');
            string str = $"04000C{level + 1:D2}";

            var bytes = DataTransUtil.SpliceArray(str.ByHexStringToBytes().Reverse().ToArray()
                , (oldPassword.ByHexStringToBytes().Reverse().ToArray())
                , (newPassword.ByHexStringToBytes().Reverse().ToArray()));

            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.WritePassword,
                bytes
                , Station.ByHexStringToBytes().Reverse().ToArray());
            return await SendThenReturnAsync(string.Empty, commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    #endregion 其他方法
}

[PluginOption(Singleton = true)]
internal class DtuPlugin : PluginBase, ITcpReceivingPlugin
{
    private Dlt645_2007Master _dlt645_2007Master;

    public DtuPlugin(Dlt645_2007Master dlt645_2007Master)
    {
        _dlt645_2007Master = dlt645_2007Master;
    }

    public async Task OnTcpReceiving(ITcpClientBase client, ByteBlockEventArgs e)
    {
        if (client is ISocketClient socket)
        {
            var bytes = e.ByteBlock.ToArray();
            if (!socket.Id.StartsWith("ID="))
            {
                var id = $"ID={Encoding.UTF8.GetString(bytes)}";
                client.Logger.Info(DefaultResource.Localizer["DtuConnected", id]);
                socket.ResetId(id);
            }
            if (_dlt645_2007Master.HeartbeatHexString == bytes.ToHexString())
            {
                //回应心跳包
                socket.DefaultSend(bytes);
                socket.Logger?.Trace($"{socket.ToString()}- Send:{bytes.ToHexString(' ')}");
            }
        }
        await e.InvokeNext();//如果本插件无法处理当前数据，请将数据转至下一个插件。
    }
}
