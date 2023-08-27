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

using ThingsGateway.Foundation.Extension.Byte;
using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Serial;

namespace ThingsGateway.Foundation.Adapter.DLT645;
/// <summary>
/// DLT645_2007
/// </summary>
public class DLT645_2007 : ReadWriteDevicesSerialBase
{

    /// <summary>
    /// DLT645_2007
    /// </summary>
    /// <param name="serialSession"></param>
    public DLT645_2007(SerialsSession serialSession) : base(serialSession)
    {
        ThingsGatewayBitConverter = new DLT645_2007BitConverter(EndianType.Big);
        RegisterByteLength = 2;
    }



    /// <summary>
    /// 组包缓存时间/ms
    /// </summary>
    [Description("组包缓存时间ms")]
    public int CacheTimeout { get; set; } = 1000;

    /// <summary>
    /// 增加FE FE FE FE的报文头部
    /// </summary>
    [Description("前导符报文头")]
    public bool EnableFEHead { get; set; }

    /// <summary>
    /// 帧前时间ms
    /// </summary>
    [Description("帧前时间ms")]
    public int FrameTime { get; set; }
    /// <summary>
    /// 写入需操作员代码
    /// </summary>
    [Description("操作员代码")]
    public string OperCode { get; set; }

    /// <summary>
    /// 写入密码
    /// </summary>
    [Description("写入密码")]
    public string Password { get; set; }

    /// <summary>
    /// 通讯地址BCD码，一般应该是12个字符
    /// </summary>
    [Description("通讯地址")]
    public string Station { get; set; }
    /// <inheritdoc/>
    public override string GetAddressDescription()
    {

        var str = """
            查看附带文档或者相关资料，下面列举一下常见的数据标识地址 
            
            地址                       说明                    
            -----------------------------------------
            02010100    A相电压
            02020100    A相电流
            02030000    瞬时总有功功率
            00000000    (当前)组合有功总电能
            00010000    (当前)正向有功总电能
            
            """;
        return base.GetAddressDescription() + Environment.NewLine + str;
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);
            var commandResult = DLT645Helper.GetDLT645_2007Command(address, length, (byte)ControlCode.Read, Station);
            if (commandResult.IsSuccess)
            {
                var result = await WaitingClientEx.SendThenResponseAsync(commandResult.Content, TimeOut, token);
                return (MessageBase)result.RequestInfo;
            }
            else
            {
                return OperResult.CreateFailedResult<byte[]>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }

    }

    /// <inheritdoc/>
    public override void SetDataAdapter()
    {
        var dataHandleAdapter = new DLT645_2007DataHandleAdapter
        {
            EnableFEHead = EnableFEHead
        };
        SerialsSession.SetDataHandlingAdapter(dataHandleAdapter);
    }
    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, string value, CancellationToken token = default)
    {

        try
        {
            await ConnectAsync(token);
            Password ??= string.Empty;
            OperCode ??= string.Empty;
            if (Password.Length < 8)
                Password = Password.PadLeft(8, '0');
            if (OperCode.Length < 8)
                OperCode = OperCode.PadLeft(8, '0');
            var data = DataTransUtil.SpliceArray(Password.ByHexStringToBytes(), OperCode.ByHexStringToBytes());
            string[] strArray = value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var commandResult = DLT645Helper.GetDLT645_2007Command(address, 1, (byte)ControlCode.Write, Station, data, strArray);
            if (commandResult.IsSuccess)
            {
                var result = await WaitingClientEx.SendThenResponseAsync(commandResult.Content, TimeOut, token);
                return (MessageBase)result.RequestInfo;
            }
            else
            {
                return OperResult.CreateFailedResult<byte[]>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }


    }

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, uint value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, byte value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, double value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, float value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, long value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, ulong value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, ushort value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, short value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);
    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, int value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);


    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken token = default) => WriteAsync(address, value.ToString(), token);


    #region 其他方法

    /// <summary>
    /// 广播校时
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public OperResult BroadcastTime(DateTime dateTime, CancellationToken token = default)
    {
        try
        {
            Connect(token);
            string str = $"{dateTime.Second:D2}{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}{dateTime.Year % 100:D2}";
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.BroadcastTime, str.ByHexStringToBytes().Reverse().ToArray(), "999999999999".ByHexStringToBytes());
            if (commandResult.IsSuccess)
            {
                SerialsSession.Send(commandResult.Content);
                return OperResult.CreateSuccessResult();
            }
            else
            {
                return OperResult.CreateFailedResult<string>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }

    /// <summary>
    /// 冻结
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<OperResult> FreezeAsync(DateTime dateTime, CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);
            string str = $"{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}";
            if (Station.IsNullOrEmpty()) Station = string.Empty;
            if (Station.Length < 12) Station = Station.PadLeft(12, '0');
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.Freeze, str.ByHexStringToBytes().Reverse().ToArray(), Station.ByHexStringToBytes().Reverse().ToArray());
            if (commandResult.IsSuccess)
            {
                var result = await WaitingClientEx.SendThenResponseAsync(commandResult.Content, TimeOut, token);
                var result1 = ((MessageBase)result.RequestInfo);
                if (result1.IsSuccess)
                {
                    return OperResult.CreateSuccessResult();

                }
                else
                {
                    return OperResult.CreateFailedResult<string>(result1);
                }
            }
            else
            {
                return OperResult.CreateFailedResult<string>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }

    /// <summary>
    /// 读取通信地址
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<OperResult<string>> ReadDeviceStationAsync(CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.ReadStation, null, "AAAAAAAAAAAA".ByHexStringToBytes());
            if (commandResult.IsSuccess)
            {
                var result = await WaitingClientEx.SendThenResponseAsync(commandResult.Content, TimeOut, token);
                var result1 = ((MessageBase)result.RequestInfo);
                if (result1.IsSuccess)
                {
                    var buffer = result1.Content.SelectMiddle(0, 6).BytesAdd(-0x33);
                    return OperResult.CreateSuccessResult(buffer.Reverse().ToArray().ToHexString());
                }
                else
                {
                    return OperResult.CreateFailedResult<string>(result1);
                }
            }
            else
            {
                return OperResult.CreateFailedResult<string>(commandResult);
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
    /// <param name="baudRate"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<OperResult> WriteBaudRateAsync(int baudRate, CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);
            byte baudRateByte;
            switch (baudRate)
            {
                case 600: baudRateByte = 0x02; break;
                case 1200: baudRateByte = 0x04; break;
                case 2400: baudRateByte = 0x08; break;
                case 4800: baudRateByte = 0x10; break;
                case 9600: baudRateByte = 0x20; break;
                case 19200: baudRateByte = 0x40; break;
                default: return new OperResult<string>($"不支持此波特率:{baudRate}");
            }
            if (Station.IsNullOrEmpty()) Station = string.Empty;
            if (Station.Length < 12) Station = Station.PadLeft(12, '0');
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.WriteBaudRate, new byte[] { baudRateByte }, Station.ByHexStringToBytes().Reverse().ToArray());
            if (commandResult.IsSuccess)
            {
                var result = await WaitingClientEx.SendThenResponseAsync(commandResult.Content, TimeOut, token);
                var result1 = ((MessageBase)result.RequestInfo);
                if (result1.IsSuccess)
                {
                    return OperResult.CreateSuccessResult();
                }
                else
                {
                    return OperResult.CreateFailedResult<string>(result1);
                }
            }
            else
            {
                return OperResult.CreateFailedResult<string>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }

    /// <summary>
    /// 更新通信地址
    /// </summary>
    /// <param name="station"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<OperResult> WriteDeviceStationAsync(string station, CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.WriteStation, station.ByHexStringToBytes().Reverse().ToArray(), "AAAAAAAAAAAA".ByHexStringToBytes());
            if (commandResult.IsSuccess)
            {
                var result = await WaitingClientEx.SendThenResponseAsync(commandResult.Content, TimeOut, token);
                var result1 = ((MessageBase)result.RequestInfo);
                if (result1.IsSuccess)
                {
                    return OperResult.CreateSuccessResult();

                }
                else
                {
                    return OperResult.CreateFailedResult<string>(result1);
                }
            }
            else
            {
                return OperResult.CreateFailedResult<string>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }
    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="level"></param>
    /// <param name="oldPassword"></param>
    /// <param name="newPassword"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<OperResult> WritePasswordAsync(byte level, string oldPassword, string newPassword, CancellationToken token = default)
    {
        try
        {
            await ConnectAsync(token);

            if (Station.IsNullOrEmpty()) Station = string.Empty;
            if (Station.Length < 12) Station = Station.PadLeft(12, '0');
            string str = $"04000C{level:D2}";

            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.WritePassword,
                str.ByHexStringToBytes().Reverse().ToArray()
                .SpliceArray(oldPassword.ByHexStringToBytes().Reverse().ToArray())
                .SpliceArray(newPassword.ByHexStringToBytes().Reverse().ToArray())
                , Station.ByHexStringToBytes().Reverse().ToArray());
            if (commandResult.IsSuccess)
            {
                var result = await WaitingClientEx.SendThenResponseAsync(commandResult.Content, TimeOut, token);
                var result1 = ((MessageBase)result.RequestInfo);
                if (result1.IsSuccess)
                {
                    return OperResult.CreateSuccessResult();
                }
                else
                {
                    return OperResult.CreateFailedResult<string>(result1);
                }
            }
            else
            {
                return OperResult.CreateFailedResult<string>(commandResult);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }


    #endregion

}
/// <summary>
/// 控制码
/// </summary>
public enum ControlCode : byte
{
    /// <summary>
    /// 读数据
    /// </summary>
    Read = 0x11,
    /// <summary>
    /// 读后续数据
    /// </summary>
    ReadSub = 0x12,
    /// <summary>
    /// 读站号
    /// </summary>
    ReadStation = 0x13,
    /// <summary>
    /// 写数据
    /// </summary>
    Write = 0x14,
    /// <summary>
    /// 写站号
    /// </summary>
    WriteStation = 0x15,
    /// <summary>
    /// 广播校时
    /// </summary>
    BroadcastTime = 0x08,
    /// <summary>
    /// 冻结
    /// </summary>
    Freeze = 0x16,
    /// <summary>
    /// 更新波特率
    /// </summary>
    WriteBaudRate = 0x17,
    /// <summary>
    /// 更新密码
    /// </summary>
    WritePassword = 0x18,
}