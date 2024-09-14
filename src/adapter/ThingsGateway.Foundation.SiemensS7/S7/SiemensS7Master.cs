//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

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
        lock (channel)
        {
            RegisterByteLength = 1;
            ThingsGatewayBitConverter = new S7BitConverter(EndianType.Big);

            if (channel.Collects.Count(a => a.GetType() == typeof(SiemensS7Master)) > 1)
            {
                Channel.Starting.Remove(ChannelStarting);
                Channel.Stoped.Remove(ChannelStoped);
                Channel.Started.Remove(ChannelStarted);
            }
        }
    }

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
    /// S7类型
    /// </summary>
    public SiemensTypeEnum SiemensS7Type { get; set; }

    /// <summary>
    /// 槽号，需重新连接
    /// </summary>
    public byte Slot { get; set; }

    #endregion 设置

    /// <inheritdoc/>
    public override bool BitReverse(string address)
    {
        return false;
    }

    /// <inheritdoc/>
    public override string GetAddressDescription()
    {
        var str = SiemensS7Resource.Localizer["AddressDes"];

        return $"{base.GetAddressDescription()}{Environment.NewLine}{str}";
    }

    /// <inheritdoc/>
    public override int? GetBitOffset(string address)
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
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override DataHandlingAdapter GetDataAdapter()
    {
        return new ProtocolSingleStreamDataHandleAdapter<S7Message>()
        {
            CacheTimeout = TimeSpan.FromMilliseconds(CacheTimeout)
        };
    }

    /// <inheritdoc/>
    public override List<T> LoadSourceRead<T>(IEnumerable<IVariable> deviceVariables, int maxPack, int defaultIntervalTime)
    {
        try
        {
            Channel.Connect();
        }
        catch
        {
        }
        return PackHelper.LoadSourceRead<T>(this, deviceVariables, maxPack, defaultIntervalTime);
    }

    /// <summary>
    /// 此方法并不会智能分组以最大化效率，减少传输次数，因为返回值是byte[]，所以一切都按地址数组的顺序执行，最后合并数组
    /// </summary>
    public async ValueTask<OperResult<byte[]>> S7ReadAsync(SiemensAddress[] sAddresss, CancellationToken cancellationToken = default)
    {
        {
            var byteBlock = new ValueByteBlock(2048);
            try
            {
                foreach (var sAddress in sAddresss)
                {
                    int num = 0;
                    var addressLen = sAddress.Length == 0 ? 1 : sAddress.Length;
                    while (num < addressLen)
                    {
                        //pdu长度，重复生成报文，直至全部生成
                        int len = Math.Min(addressLen - num, PduLength);
                        sAddress.Length = len;

                        var result = await SendThenReturnAsync(new S7Send([sAddress], true), cancellationToken: cancellationToken).ConfigureAwait(false);
                        if (!result.IsSuccess) return result;

                        byteBlock.Write(result.Content);
                        num += len;

                        if (sAddress.DataCode == (byte)S7WordLength.Timer || sAddress.DataCode == (byte)S7WordLength.Counter)
                        {
                            sAddress.AddressStart += len / 2;
                        }
                        else
                        {
                            sAddress.AddressStart += len * 8;
                        }
                    }
                }

                return new OperResult<byte[]>() { Content = byteBlock.ToArray() };
            }
            catch (Exception ex)
            {
                return new OperResult<byte[]>(ex);
            }
            finally
            {
                byteBlock.SafeDispose();
            }
        }
    }
    /// <summary>
    /// 此方法并不会智能分组以最大化效率，减少传输次数，因为返回值是byte[]，所以一切都按地址数组的顺序执行，最后合并数组
    /// </summary>
    public async ValueTask<Dictionary<SiemensAddress, OperResult>> S7WriteAsync(SiemensAddress[] sAddresss, CancellationToken cancellationToken = default)
    {


        var dictOperResult = new Dictionary<SiemensAddress, OperResult>();

        void SetFailOperResult(OperResult operResult)
        {
            foreach (var item in sAddresss)
            {
                dictOperResult.TryAdd(item, operResult);
            }
        }

        {
            var sAddress = sAddresss[0];
            if (sAddresss.Length <= 1 && sAddress.IsBit)
            {
                //读取，再写入
                var byteBlock = new ValueByteBlock(2048);
                try
                {
                    var addressLen = sAddress.Length == 0 ? 1 : sAddress.Length;

                    if (addressLen > PduLength)
                    {
                        SetFailOperResult(new OperResult("Write length exceeds limit"));
                        return dictOperResult;
                    }

                    var result = await SendThenReturnAsync(new S7Send([sAddress], true), cancellationToken: cancellationToken).ConfigureAwait(false);

                    if (!result.IsSuccess)
                    {
                        SetFailOperResult(result);
                        return dictOperResult;
                    }

                    var value = sAddress.Data.ByteToBoolArray(sAddress.BitLength);
                    for (int i = sAddress.BitCode; i < value.Length + sAddress.BitCode; i++)
                    {
                        result.Content[i / 8] = result.Content[i / 8].SetBit((i % 8), value[i - sAddress.BitCode]);
                    }
                    sAddress.Data = result.Content;
                    var wresult = await SendThenReturnAsync(new S7Send([sAddress], false), cancellationToken: cancellationToken).ConfigureAwait(false);
                    dictOperResult.TryAdd(sAddress, wresult);
                    return dictOperResult;

                }
                catch (Exception ex)
                {
                    SetFailOperResult(new OperResult(ex));
                    return dictOperResult;
                }
                finally
                {
                    byteBlock.SafeDispose();
                }
            }
            else
            {
                //多写
                HashSet<List<SiemensAddress>> siemensAddresses = new();
                ushort dataLen = 0;
                ushort itemLen = 1;
                List<SiemensAddress> addresses = new();
                foreach (var item in sAddresss)
                {
                    siemensAddresses.Add(addresses);
                    dataLen = (ushort)(dataLen + item.Data.Length + 4);
                    ushort telegramLen = (ushort)(itemLen * 12 + 19 + dataLen);
                    if (telegramLen < PduLength)
                    {
                        addresses.Add(item);
                        itemLen++;
                    }
                    else
                    {
                        addresses = new();
                        itemLen = 1;
                        dataLen = 0;
                        dataLen = (ushort)(dataLen + item.Data.Length + 4);
                        telegramLen = (ushort)(itemLen * 12 + 19 + dataLen);
                        if (telegramLen < PduLength)
                        {
                            addresses.Add(item);
                            itemLen++;
                        }
                        else
                        {
                            SetFailOperResult(new OperResult("Write length exceeds limit"));
                            return dictOperResult;
                        }
                    }

                }

                foreach (var item in siemensAddresses)
                {

                    try
                    {
                        var result = await SendThenReturnAsync(new S7Send(item.ToArray(), false), cancellationToken: cancellationToken).ConfigureAwait(false);
                        foreach (var i1 in item)
                        {
                            dictOperResult.TryAdd(i1, result);
                        }
                    }
                    catch (Exception ex)
                    {

                        SetFailOperResult(new OperResult(ex));
                        return dictOperResult;
                    }

                }
                return dictOperResult;

            }
        }
    }
    #region 读写

    /// <inheritdoc/>
    public override ValueTask<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
    {
        try
        {
            var sAddress = SiemensAddress.ParseFrom(address, length);
            return S7ReadAsync([sAddress], cancellationToken);
        }
        catch (Exception ex)
        {
            return EasyValueTask.FromResult(new OperResult<byte[]>(ex));
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
    {
        try
        {
            var sAddress = SiemensAddress.ParseFrom(address);
            sAddress.Data = value;
            sAddress.Length = value.Length;
            return (await S7WriteAsync([sAddress], cancellationToken).ConfigureAwait(false)).FirstOrDefault().Value;
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
            var sAddress = SiemensAddress.ParseFrom(address);
            sAddress.Data = value.BoolArrayToByte();
            sAddress.Length = sAddress.Data.Length;
            sAddress.BitLength = value.Length;
            sAddress.IsBit = true;
            return (await S7WriteAsync([sAddress], cancellationToken).ConfigureAwait(false)).FirstOrDefault().Value;
        }
        catch (Exception ex)
        {
            return new OperResult(ex);
        }
    }

    #endregion 读写

    #region 初始握手

    /// <inheritdoc/>
    protected override async Task<bool> ChannelStarted(IClientChannel channel)
    {
        try
        {
            var ISO_CR = SiemensHelper.ISO_CR;

            var S7_PN = SiemensHelper.S7_PN;
            //获取正确的ISO_CR/S7_PN
            //类型
            switch (SiemensS7Type)
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
                if (SiemensS7Type == SiemensTypeEnum.S200 || SiemensS7Type == SiemensTypeEnum.S200Smart)
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
                if (SiemensS7Type != SiemensTypeEnum.S200 && SiemensS7Type != SiemensTypeEnum.S200Smart)
                {
                    ISO_CR[21] = (byte)((Rack * 0x20) + Slot);
                }
            }

            try
            {
                var result2 = await SendThenReturnAsync(new S7Send(ISO_CR)).ConfigureAwait(false);
                if (!result2.IsSuccess)
                {
                    Logger?.LogWarning(SiemensS7Resource.Localizer["HandshakeError1", channel.ToString(), result2.ErrorMessage]);
                    await channel.CloseAsync().ConfigureAwait(false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(SiemensS7Resource.Localizer["HandshakeError1", channel.ToString(), ex.Message]);
                await channel.CloseAsync().ConfigureAwait(false);
                return true;
            }
            try
            {
                var result2 = await SendThenReturnAsync(new S7Send(S7_PN)).ConfigureAwait(false);
                if (!result2.IsSuccess)
                {
                    Logger?.LogWarning(SiemensS7Resource.Localizer["HandshakeError2", channel.ToString(), result2.ErrorMessage]);
                    await channel.CloseAsync().ConfigureAwait(false);
                    return true;
                }
                PduLength = ThingsGatewayBitConverter.ToUInt16(result2.Content, 0) - 28;
                Logger?.LogInformation($"PduLength：{PduLength}");
                PduLength = PduLength < 200 ? 200 : PduLength;
            }
            catch (Exception ex)
            {
                Logger?.LogWarning(SiemensS7Resource.Localizer["HandshakeError2", channel.ToString(), ex.Message]);
                await channel.CloseAsync().ConfigureAwait(false);
                return true;
            }
        }
        catch (Exception ex)
        {
            await channel.CloseAsync().ConfigureAwait(false);
            Logger?.Exception(ex);
        }
        finally
        {
            await base.ChannelStarted(channel).ConfigureAwait(false);
        }
        return true;
    }

    #endregion 初始握手

    #region 其他方法

    /// <summary>
    /// 读取日期
    /// </summary>
    /// <returns></returns>
    public async ValueTask<OperResult<System.DateTime>> ReadDateAsync(string address, CancellationToken cancellationToken)
    {
        return (await ReadAsync(address, 2, cancellationToken).ConfigureAwait(false)).
             Then(m => OperResult.CreateSuccessResult(S7DateTime.SpecMinimumDateTime.AddDays(
                 ThingsGatewayBitConverter.ToUInt16(m, 0)))
             );
    }

    /// <summary>
    /// 读取时间
    /// </summary>
    /// <returns></returns>
    public async ValueTask<OperResult<System.DateTime>> ReadDateTimeAsync(string address, CancellationToken cancellationToken)
    {
        return OperResultExtension.GetResultFromBytes(await ReadAsync(address, 8, cancellationToken).ConfigureAwait(false), S7DateTime.FromByteArray);
    }

    /// <summary>
    /// 写入日期
    /// </summary>
    /// <returns></returns>
    public ValueTask<OperResult> WriteDateAsync(string address, System.DateTime dateTime, CancellationToken cancellationToken)
    {
        return base.WriteAsync(address, Convert.ToUInt16((dateTime - S7DateTime.SpecMinimumDateTime).TotalDays), null, cancellationToken);
    }

    /// <summary>
    /// 写入时间
    /// </summary>
    /// <returns></returns>
    public ValueTask<OperResult> WriteDateTimeAsync(string address, System.DateTime dateTime, CancellationToken cancellationToken)
    {
        return WriteAsync(address, S7DateTime.ToByteArray(dateTime), cancellationToken);
    }

    #endregion 其他方法

    #region 字符串读写

    /// <inheritdoc/>
    public override async ValueTask<OperResult<string[]>> ReadStringAsync(string address, int length, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        var siemensAddress = SiemensAddress.ParseFrom(address);
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        if (bitConverter.IsVariableStringLength)
        {
            if (length > 1)
            {
                return new OperResult<string[]>(SiemensS7Resource.Localizer["StringLengthReadError"]);
            }
            var result = await SiemensHelper.ReadStringAsync(this, address, bitConverter.Encoding, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return OperResult.CreateSuccessResult(new string[] { result.Content });
            }
            else
            {
                return new OperResult<string[]>(result);
            }
        }
        else
        {
            return await base.ReadStringAsync(address, length, bitConverter, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public override ValueTask<OperResult> WriteAsync(string address, string value, IThingsGatewayBitConverter bitConverter = null, CancellationToken cancellationToken = default)
    {
        var siemensAddress = SiemensAddress.ParseFrom(address);
        bitConverter ??= ThingsGatewayBitConverter.GetTransByAddress(ref address);
        if (bitConverter.IsVariableStringLength)
        {
            return SiemensHelper.WriteAsync(this, address, value, bitConverter.Encoding);
        }
        else
        {
            return base.WriteAsync(address, value, bitConverter, cancellationToken);
        }
    }

    #endregion 字符串读写
}
