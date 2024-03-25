//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Text;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Foundation.SiemensS7;

/// <inheritdoc/>
public partial class SiemensS7Master : ProtocolBase
{
    /// <inheritdoc/>
    public SiemensS7Master(IChannel channel) : base(channel)
    {
        RegisterByteLength = 1;
        ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
    }

    /// <summary>
    /// S7类型
    /// </summary>
    [Description("S7类型")]
    public SiemensTypeEnum SiemensType { get; set; }

    /// <summary>
    /// PduLength
    /// </summary>
    public int PduLength { get; private set; } = 100;

    #region 设置

    /// <summary>
    /// 本地TSAP，需重新连接
    /// </summary>
    public int LocalTSAP { get; set; }

    /// <summary>
    /// 机架号，需重新连接
    /// </summary>
    public byte Rack { get; set; }

    /// <summary>
    /// 槽号，需重新连接
    /// </summary>
    public byte Slot { get; set; }

    #endregion 设置

    /// <inheritdoc/>
    public override bool IsSingleThread
    {
        get
        {
            return true;
        }
    }

    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        StringBuilder stringBuilder = new();
        stringBuilder.AppendLine("S7协议寄存器地址格式");
        stringBuilder.AppendLine("Txxxxx	Timer寄存器，例如T100/T100.1");
        stringBuilder.AppendLine("Cxxxxx，Counter寄存器，例如C100/C100.1");
        stringBuilder.AppendLine("AIxxxxx，AI寄存器，例如AI100/AI100.1");
        stringBuilder.AppendLine("AQxxxxx，AQ寄存器，例如AQ100/AQ100.1");
        stringBuilder.AppendLine("Ixxxxx，I寄存器，例如I100/I100.1");
        stringBuilder.AppendLine("Qxxxxx，Q寄存器，例如Q100/Q100.1");
        stringBuilder.AppendLine("Mxxxxx，M寄存器，例如M100/M100.1");
        stringBuilder.AppendLine("DBxxxxx，DB寄存器，例如DB100.1/DB100.1.1");
        stringBuilder.AppendLine("");
        return $"{base.GetAddressDescription()}{Environment.NewLine}{stringBuilder.ToString()}";
    }

    /// <inheritdoc/>
    public override int GetBitOffset(string address)
    {
        if (address.IndexOf('.') > 0)
        {
            var addressSplits1 = address.SplitStringBySemicolon().Where(a => !a.Contains("=")).FirstOrDefault();
            string[] addressSplits = addressSplits1.SplitStringByDelimiter();
            try
            {
                int bitIndex = 0;
                if ((addressSplits.Length == 2 && !address.ToUpper().Contains("DB")) || (addressSplits.Length >= 3 && address.ToUpper().Contains("DB")))
                    bitIndex = Convert.ToInt32(addressSplits.Last());
                return bitIndex;
            }
            catch
            {
                return 0;
            }
        }
        return 0;
    }

    /// <inheritdoc/>
    public override bool BitReverse(string address)
    {
        return false;
    }

    /// <inheritdoc/>
    public override DataHandlingAdapter GetDataAdapter()
    {
        return new SiemensS7DataHandleAdapter()
        {
            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime)
    {
        return PackHelper.LoadSourceRead<T>(this, deviceVariables.ToList(), maxPack, defaultIntervalTime);
    }

    #region 读写

    /// <inheritdoc/>
    public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = GetReadByteCommand(address, length);
            List<byte> bytes = new();
            foreach (var item in commandResult)
            {
                var result = SendThenReturn(item, cancellationToken);
                if (result.IsSuccess)
                    bytes.AddRange(result.Content);
                else
                    return result;
            }
            return OperResult.CreateSuccessResult(bytes.ToArray());
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = GetReadByteCommand(address, length);
            List<byte> bytes = new();
            foreach (var item in commandResult)
            {
                var result = await SendThenReturnAsync(item, cancellationToken);
                if (result.IsSuccess)
                    bytes.AddRange(result.Content);
                else
                    return result;
            }
            return OperResult.CreateSuccessResult(bytes.ToArray());
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            var commandResult = GetWriteByteCommand(address, value);
            foreach (var item in commandResult)
            {
                var result = SendThenReturn(item, cancellationToken);
                if (!result.IsSuccess)
                    return result;
            }
            return new();
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        if (value.Length > 1)
        {
            return new OperResult("不支持多写");
        }
        try
        {
            var commandResult = GetWriteBitCommand(address, value[0]);
            return SendThenReturn(commandResult, cancellationToken);
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
            var commandResult = GetWriteByteCommand(address, value);
            foreach (var item in commandResult)
            {
                var result = await SendThenReturnAsync(item, cancellationToken);
                if (!result.IsSuccess)
                    return result;
            }
            return new();
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken cancellationToken = default)
    {
        if (value.Length > 1)
        {
            return new OperResult("不支持多写");
        }
        try
        {
            var commandResult = GetWriteBitCommand(address, value[0]);
            return await SendThenReturnAsync(commandResult, cancellationToken);
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    private OperResult<byte[]> SendThenReturn(byte[] commandResult, CancellationToken cancellationToken)
    {
        return SendThenReturn(new SendMessage(commandResult), cancellationToken);
    }

    private Task<OperResult<byte[]>> SendThenReturnAsync(byte[] commandResult, CancellationToken cancellationToken)
    {
        return SendThenReturnAsync(new SendMessage(commandResult), cancellationToken);
    }

    #endregion 读写

    #region 初始握手

    /// <inheritdoc/>
    protected override async Task ChannelStarted(IClientChannel channel)
    {
        try
        {
            var ISO_CR = SiemensHelper.ISO_CR;

            var S7_PN = SiemensHelper.S7_PN;
            //获取正确的ISO_CR/S7_PN
            //类型
            switch (SiemensType)
            {
                case SiemensTypeEnum.S1200:
                    ISO_CR[21] = 0x00;
                    break;

                case SiemensTypeEnum.S300:
                    ISO_CR[21] = 0x02;
                    break;

                case SiemensTypeEnum.S400:
                    ISO_CR[21] = 0x03;
                    ISO_CR[17] = 0x00;
                    break;

                case SiemensTypeEnum.S1500:
                    ISO_CR[21] = 0x00;
                    break;

                case SiemensTypeEnum.S200Smart:
                    ISO_CR = SiemensHelper.ISO_CR200SMART;
                    S7_PN = SiemensHelper.S7200SMART_PN;
                    break;

                case SiemensTypeEnum.S200:
                    ISO_CR = SiemensHelper.ISO_CR200;
                    S7_PN = SiemensHelper.S7200_PN;
                    break;
            }

            if (LocalTSAP > 0)
            {
                //本地TSAP
                if (SiemensType == SiemensTypeEnum.S200 || SiemensType == SiemensTypeEnum.S200Smart)
                {
                    ISO_CR[13] = BitConverter.GetBytes(LocalTSAP)[1];
                    ISO_CR[14] = BitConverter.GetBytes(LocalTSAP)[0];
                }
                else
                {
                    ISO_CR[16] = BitConverter.GetBytes(LocalTSAP)[1];
                    ISO_CR[17] = BitConverter.GetBytes(LocalTSAP)[0];
                }
            }
            if (Rack > 0 || Slot > 0)
            {
                //槽号/机架号
                if (SiemensType != SiemensTypeEnum.S200 && SiemensType != SiemensTypeEnum.S200Smart)
                {
                    ISO_CR[21] = (byte)((Rack * 0x20) + Slot);
                }
            }

            //NormalDataHandlingAdapter dataHandleAdapter = new();
            //channel.SetDataHandlingAdapter(dataHandleAdapter);
            try
            {
                var result2 = await GetResponsedDataAsync(new SendMessage(ISO_CR), Timeout, channel, CancellationToken.None);
                if (!result2.IsSuccess)
                {
                    Logger?.LogWarning($"{channel.ToString()}：ISO_TP握手失败-{result2.ErrorMessage}，请检查机架号/槽号是否正确");
                    channel.Close();
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger?.Warning($"{channel.ToString()}：ISO_TP握手失败-{ex.Message}，请检查机架号/槽号是否正确");
                channel.Close();
                return;
            }
            try
            {
                var result2 = await GetResponsedDataAsync(new SendMessage(S7_PN), Timeout, channel, CancellationToken.None);
                if (!result2.IsSuccess)
                {
                    Logger?.LogWarning($"{channel.ToString()}：PDU初始化失败-{result2.ErrorMessage}，请检查机架号/槽号是否正确");
                    channel.Close();
                    return;
                }
                PduLength = ThingsGatewayBitConverter.ToUInt16(result2.Content.SelectLast(2), 0);
                PduLength = PduLength < 200 ? 200 : PduLength;
            }
            catch (Exception ex)
            {
                Logger?.LogWarning($"{channel.ToString()}：PDU初始化失败-{ex.Message}，请检查机架号/槽号是否正确");
                channel.Close();
                return;
            }
        }
        catch (Exception ex)
        {
            channel.Close();
            Logger.Exception(ex);
        }
        finally
        {
            channel.SetDataHandlingAdapter(GetDataAdapter());
            await base.ChannelStarted(channel);
        }
    }

    #endregion 初始握手

    #region 其他方法

    /// <summary>
    /// 读取日期
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<System.DateTime>> ReadDateAsync(string address, CancellationToken cancellationToken)
    {
        return (await this.ReadAsync(address, 2, cancellationToken)).
             Then(m => OperResult.CreateSuccessResult(S7DateTime.SpecMinimumDateTime.AddDays(
                 ThingsGatewayBitConverter.ToUInt16(m, 0)))
             );
    }

    /// <summary>
    /// 读取时间
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult<System.DateTime>> ReadDateTimeAsync(string address, CancellationToken cancellationToken)
    {
        return ByteTransUtil.GetResultFromBytes(await ReadAsync(address, 8, cancellationToken), S7DateTime.FromByteArray);
    }

    /// <summary>
    /// 写入日期
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult> WriteDateAsync(string address, System.DateTime dateTime, CancellationToken cancellationToken)
    {
        return await base.WriteAsync(address, Convert.ToUInt16((dateTime - S7DateTime.SpecMinimumDateTime).TotalDays), cancellationToken);
    }

    /// <summary>
    /// 写入时间
    /// </summary>
    /// <returns></returns>
    public async Task<OperResult> WriteDateTimeAsync(string address, System.DateTime dateTime, CancellationToken cancellationToken)
    {
        return await WriteAsync(address, S7DateTime.ToByteArray(dateTime), cancellationToken);
    }

    #endregion 其他方法

    #region 字符串读写

    /// <inheritdoc/>
    public override OperResult<string[]> ReadString(string address, int length, CancellationToken cancellationToken = default)
    {
        var siemensAddress = SiemensAddressHelper.ParseFrom(address);
        if (siemensAddress.IsWString)
        {
            IThingsGatewayBitConverter transformParameter = ByteTransUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            if (length > 1)
            {
                return new("不支持变成字符串的连读");
            }
            var result = SiemensHelper.ReadString(this, address, transformParameter.Encoding, cancellationToken);
            if (result.IsSuccess)
            {
                return new() { Content = new string[] { result.Content } };
            }
            else
            {
                return new(result);
            }
        }
        else
        {
            return base.ReadString(address, length, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<OperResult<string[]>> ReadStringAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        var siemensAddress = SiemensAddressHelper.ParseFrom(address);
        if (siemensAddress.IsWString)
        {
            IThingsGatewayBitConverter transformParameter = ByteTransUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            if (length > 1)
            {
                return new("不支持变成字符串的连读");
            }
            var result = await SiemensHelper.ReadStringAsync(this, address, transformParameter.Encoding, cancellationToken);
            if (result.IsSuccess)
            {
                return new() { Content = new string[] { result.Content } };
            }
            else
            {
                return new(result);
            }
        }
        else
        {
            return await base.ReadStringAsync(address, length, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override OperResult Write(string address, string value, CancellationToken cancellationToken = default)
    {
        var siemensAddress = SiemensAddressHelper.ParseFrom(address);
        if (siemensAddress.IsWString)
        {
            IThingsGatewayBitConverter transformParameter = ByteTransUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return SiemensHelper.Write(this, address, value, transformParameter.Encoding);
        }
        else
        {
            return base.Write(address, value, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override Task<OperResult> WriteAsync(string address, string value, CancellationToken cancellationToken = default)
    {
        var siemensAddress = SiemensAddressHelper.ParseFrom(address);
        if (siemensAddress.IsWString)
        {
            IThingsGatewayBitConverter transformParameter = ByteTransUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return SiemensHelper.WriteAsync(this, address, value, transformParameter.Encoding);
        }
        else
        {
            return base.WriteAsync(address, value, cancellationToken);
        }
    }

    #endregion 字符串读写
}