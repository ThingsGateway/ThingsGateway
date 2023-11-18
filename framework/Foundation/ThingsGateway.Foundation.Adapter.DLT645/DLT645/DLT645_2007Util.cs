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

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.Adapter.DLT645;
/// <summary>
/// DLT645_2007
/// </summary>
internal static class DLT645_2007Util
{
    /// <inheritdoc/>
    public static string GetAddressDescription()
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
        return Environment.NewLine + str;
    }

    /// <inheritdoc/>
    public static OperResult<byte[]> Read(IDLT645_2007 dlt645_2007, string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            dlt645_2007.Connect(cancellationToken);
            var commandResult = DLT645Helper.GetDLT645_2007Command(address, (byte)ControlCode.Read, dlt645_2007.Station);
            if (!commandResult.IsSuccess) return commandResult;
            return dlt645_2007.SendThenReturn<DLT645_2007Message>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public static async Task<OperResult<byte[]>> ReadAsync(IDLT645_2007 dlt645_2007, string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            await dlt645_2007.ConnectAsync(cancellationToken);
            var commandResult = DLT645Helper.GetDLT645_2007Command(address, (byte)ControlCode.Read, dlt645_2007.Station);
            if (!commandResult.IsSuccess) return commandResult;
            return await dlt645_2007.SendThenReturnAsync<DLT645_2007Message>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }

    }


    /// <inheritdoc/>
    public static OperResult Write(IDLT645_2007 dlt645_2007, string address, string value, CancellationToken cancellationToken = default)
    {

        try
        {
            dlt645_2007.Connect(cancellationToken);
            dlt645_2007.Password ??= string.Empty;
            dlt645_2007.OperCode ??= string.Empty;
            if (dlt645_2007.Password.Length < 8)
                dlt645_2007.Password = dlt645_2007.Password.PadLeft(8, '0');
            if (dlt645_2007.OperCode.Length < 8)
                dlt645_2007.OperCode = dlt645_2007.OperCode.PadLeft(8, '0');

            var data = DataTransUtil.SpliceArray(dlt645_2007.Password.ByHexStringToBytes(), dlt645_2007.OperCode.ByHexStringToBytes());
            string[] strArray = value.SplitStringBySemicolon();
            var commandResult = DLT645Helper.GetDLT645_2007Command(address, (byte)ControlCode.Write, dlt645_2007.Station, data, strArray);
            if (!commandResult.IsSuccess) return commandResult;
            return dlt645_2007.SendThenReturn<DLT645_2007Message>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }

    }


    /// <inheritdoc/>
    public static async Task<OperResult> WriteAsync(IDLT645_2007 dlt645_2007, string address, string value, CancellationToken cancellationToken = default)
    {

        try
        {
            await dlt645_2007.ConnectAsync(cancellationToken);
            dlt645_2007.Password ??= string.Empty;
            dlt645_2007.OperCode ??= string.Empty;
            if (dlt645_2007.Password.Length < 8)
                dlt645_2007.Password = dlt645_2007.Password.PadLeft(8, '0');
            if (dlt645_2007.OperCode.Length < 8)
                dlt645_2007.OperCode = dlt645_2007.OperCode.PadLeft(8, '0');
            var data = DataTransUtil.SpliceArray(dlt645_2007.Password.ByHexStringToBytes(), dlt645_2007.OperCode.ByHexStringToBytes());
            string[] strArray = value.SplitStringBySemicolon();
            var commandResult = DLT645Helper.GetDLT645_2007Command(address, (byte)ControlCode.Write, dlt645_2007.Station, data, strArray);
            if (!commandResult.IsSuccess) return commandResult;
            return await dlt645_2007.SendThenReturnAsync<DLT645_2007Message>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }

    }

    #region 其他方法

    /// <summary>
    /// 广播校时
    /// </summary>
    /// <param name="dlt645_2007">链路</param>
    /// <param name="dateTime">时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public static OperResult BroadcastTime(IDLT645_2007 dlt645_2007, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        try
        {
            dlt645_2007.Connect(cancellationToken);
            string str = $"{dateTime.Second:D2}{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}{dateTime.Year % 100:D2}";
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.BroadcastTime, str.ByHexStringToBytes().ToArray(), "999999999999".ByHexStringToBytes());
            if (commandResult.IsSuccess)
            {
                dlt645_2007.Send(commandResult.Content);
                return OperResult.CreateSuccessResult();
            }
            else
            {
                return new OperResult(commandResult);
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
    /// <param name="dlt645_2007">链路</param>
    /// <param name="dateTime">时间</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public static async Task<OperResult> FreezeAsync(IDLT645_2007 dlt645_2007, DateTime dateTime, CancellationToken cancellationToken = default)
    {
        try
        {
            await dlt645_2007.ConnectAsync(cancellationToken);
            string str = $"{dateTime.Minute:D2}{dateTime.Hour:D2}{dateTime.Day:D2}{dateTime.Month:D2}";
            if (dlt645_2007.Station.IsNullOrEmpty()) dlt645_2007.Station = string.Empty;
            if (dlt645_2007.Station.Length < 12) dlt645_2007.Station = dlt645_2007.Station.PadLeft(12, '0');
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.Freeze, str.ByHexStringToBytes().ToArray(), dlt645_2007.Station.ByHexStringToBytes().Reverse().ToArray());
            if (!commandResult.IsSuccess) return commandResult;
            return await dlt645_2007.SendThenReturnAsync<DLT645_2007Message>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }

    /// <summary>
    /// 读取通信地址
    /// </summary>
    /// <param name="dlt645_2007">链路</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public static async Task<OperResult<string>> ReadDeviceStationAsync(IDLT645_2007 dlt645_2007, CancellationToken cancellationToken = default)
    {
        try
        {
            await dlt645_2007.ConnectAsync(cancellationToken);
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.ReadStation, null, "AAAAAAAAAAAA".ByHexStringToBytes());
            if (!commandResult.IsSuccess) return new(commandResult);
            var result = await dlt645_2007.SendThenReturnAsync<DLT645_2007Message>(commandResult.Content, cancellationToken);
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
    /// <param name="dlt645_2007">链路</param>
    /// <param name="baudRate">波特率</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public static async Task<OperResult> WriteBaudRateAsync(IDLT645_2007 dlt645_2007, int baudRate, CancellationToken cancellationToken = default)
    {
        try
        {
            await dlt645_2007.ConnectAsync(cancellationToken);
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
            if (dlt645_2007.Station.IsNullOrEmpty()) dlt645_2007.Station = string.Empty;
            if (dlt645_2007.Station.Length < 12) dlt645_2007.Station = dlt645_2007.Station.PadLeft(12, '0');
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.WriteBaudRate, new byte[] { baudRateByte }, dlt645_2007.Station.ByHexStringToBytes().Reverse().ToArray());
            if (!commandResult.IsSuccess) return commandResult;
            return await dlt645_2007.SendThenReturnAsync<DLT645_2007Message>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }

    /// <summary>
    /// 更新通信地址
    /// </summary>
    /// <param name="dlt645_2007">链路</param>
    /// <param name="station">站号</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public static async Task<OperResult> WriteDeviceStationAsync(IDLT645_2007 dlt645_2007, string station, CancellationToken cancellationToken = default)
    {
        try
        {
            await dlt645_2007.ConnectAsync(cancellationToken);
            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.WriteStation, station.ByHexStringToBytes().Reverse().ToArray(), "AAAAAAAAAAAA".ByHexStringToBytes());
            if (!commandResult.IsSuccess) return commandResult;
            return await dlt645_2007.SendThenReturnAsync<DLT645_2007Message>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }
    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="dlt645_2007">链路</param>
    /// <param name="level">密码等级，0-8</param>
    /// <param name="oldPassword">旧密码</param>
    /// <param name="newPassword">新密码</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <returns></returns>
    public static async Task<OperResult> WritePasswordAsync(IDLT645_2007 dlt645_2007, byte level, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            await dlt645_2007.ConnectAsync(cancellationToken);

            if (dlt645_2007.Station.IsNullOrEmpty()) dlt645_2007.Station = string.Empty;
            if (dlt645_2007.Station.Length < 12) dlt645_2007.Station = dlt645_2007.Station.PadLeft(12, '0');
            string str = $"04000C{level + 1:D2}";

            var commandResult = DLT645Helper.GetDLT645_2007Command((byte)ControlCode.WritePassword,
                str.ByHexStringToBytes().Reverse().ToArray()
                .SpliceArray(oldPassword.ByHexStringToBytes().Reverse().ToArray())
                .SpliceArray(newPassword.ByHexStringToBytes().Reverse().ToArray())
                , dlt645_2007.Station.ByHexStringToBytes().Reverse().ToArray());
            if (!commandResult.IsSuccess) return commandResult;
            return await dlt645_2007.SendThenReturnAsync<DLT645_2007Message>(commandResult.Content, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult<string>(ex);
        }

    }
    #endregion

}
