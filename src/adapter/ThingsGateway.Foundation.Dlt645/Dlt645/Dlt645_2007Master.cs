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
using ThingsGateway.Foundation.Extension.String;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.Dlt645;

/// <inheritdoc/>
public class Dlt645_2007Master : ProtocolBase, IDtu
{
    /// <inheritdoc/>
    public Dlt645_2007Master(IChannel channel) : base(channel)
    {
        ThingsGatewayBitConverter = new Dlt645_2007BitConverter();
        ThingsGatewayBitConverter.EndianType = EndianType.Big;
        RegisterByteLength = 2;
        if (channel is IClientChannel client)
            client.WaitHandlePool.MaxSign = ushort.MaxValue;
    }

    /// <inheritdoc/>
    public string FEHead { get; set; } = "FEFEFEFE";

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
                return PluginUtil.GetDtuPlugin(this);
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
            case ChannelTypeEnum.SerialPort:
                return new Dlt645_2007DataHandleAdapter
                {
                    FEHead = FEHead,
                    CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
                };

            case ChannelTypeEnum.UdpSession:
                return new Dlt645_2007UdpDataHandleAdapter()
                {
                    FEHead = FEHead,
                };
        }

        return new Dlt645_2007DataHandleAdapter
        {
            FEHead = FEHead,
            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime);
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult<string[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        var result = await ReadAsync(address, GetLength(address, length, 8), cancellationToken).ConfigureAwait(false);
        return result.OperResultFrom(() => new[] { bitConverter.ToString(result.Content, 0, length) });
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var dAddress = Dlt645_2007Address.ParseFrom(address);
            var commandResult = Dlt645Helper.GetDlt645_2007Command(dAddress, (byte)ControlCode.Read, Station);
            return await SendThenReturnAsync(dAddress.SocketId, commandResult, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
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
            var dAddress = Dlt645_2007Address.ParseFrom(address);
            var commandResult = Dlt645Helper.GetDlt645_2007Command(dAddress, (byte)ControlCode.Write, Station, data, strArray);
            return await SendThenReturnAsync(dAddress.SocketId, commandResult, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    #region

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, uint value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, double value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, float value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, long value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, ulong value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, ushort value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, short value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, int value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default) => WriteAsync(address, value.ToString(), bitConverter, cancellationToken);

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    #endregion

    #region 其他方法

    /// <summary>
    /// 广播校时
    /// </summary>
    /// <param name="dateTime">时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async ValueTask<OperResult> BroadcastTimeAsync(DateTime dateTime, CancellationToken cancellationToken = default)
    {
        try
        {
            string str = $"{dateTime.Second:D2}{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}{dateTime.Year % 100:D2}";
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.BroadcastTime, str.ByHexStringToBytes().ToArray(), "999999999999".ByHexStringToBytes());
            await SendAsync(new SendMessage(commandResult), (IClientChannel)Channel, cancellationToken);
            return OperResult.Success;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <summary>
    /// 冻结
    /// </summary>
    /// <param name="socketId">socketId</param>
    /// <param name="dateTime">时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async ValueTask<OperResult> FreezeAsync(string socketId, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        try
        {
            string str = $"{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}";
            if (Station.IsNullOrEmpty()) Station = string.Empty;
            if (Station.Length < 12) Station = Station.PadLeft(12, '0');
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.Freeze, str.ByHexStringToBytes().ToArray(), Station.ByHexStringToBytes().Reverse().ToArray());
            return await SendThenReturnAsync(socketId, commandResult, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    /// <summary>
    /// 读取通信地址
    /// </summary>
    /// <param name="socketId">socketId</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async ValueTask<OperResult<string>> ReadDeviceStationAsync(string socketId, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.ReadStation, null, "AAAAAAAAAAAA".ByHexStringToBytes());
            var result = await SendThenReturnAsync(socketId, commandResult, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                var buffer = result.Content.SelectMiddle(0, 6).BytesAdd(-0x33);
                return OperResult.CreateSuccessResult(buffer.Reverse().ToArray().ToHexString());
            }
            else
            {
                return new OperResult<string>(result);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    /// <summary>
    /// 修改波特率
    /// </summary>
    /// <param name="socketId">socketId</param>
    /// <param name="baudRate">波特率</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async ValueTask<OperResult> WriteBaudRateAsync(string socketId, int baudRate, CancellationToken cancellationToken = default)
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
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.WriteBaudRate, new byte[1] { baudRateByte }, Station.ByHexStringToBytes().Reverse().ToArray());
            return await SendThenReturnAsync(socketId, commandResult, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    /// <summary>
    /// 更新通信地址
    /// </summary>
    /// <param name="socketId">socketId</param>
    /// <param name="station">站号</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async ValueTask<OperResult> WriteDeviceStationAsync(string socketId, string station, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = Dlt645Helper.GetDlt645_2007Command((byte)ControlCode.WriteStation, station.ByHexStringToBytes().Reverse().ToArray(), "AAAAAAAAAAAA".ByHexStringToBytes());
            return await SendThenReturnAsync(socketId, commandResult, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="socketId">socketId</param>
    /// <param name="level">密码等级，0-8</param>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public async ValueTask<OperResult> WritePasswordAsync(string socketId, byte level, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
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
            return await SendThenReturnAsync(socketId, commandResult, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }
    }

    #endregion 其他方法
}
