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

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.Extension.String;

namespace ThingsGateway.Foundation.Adapter.Siemens
{
    /// <summary>
    /// 相关命令含义源自网络资料/Shrap7/s7netplus
    /// </summary>
    public partial class SiemensS7PLC : ReadWriteDevicesTcpClientBase
    {
        private SiemensEnum siemensEnum = SiemensEnum.S1200;
        /// <summary>
        /// S7类型
        /// </summary>
        [Description("S7类型")]
        public SiemensEnum SiemensEnum
        {
            get { return siemensEnum; }
            set
            {
                siemensEnum = value;
                switch (siemensEnum)
                {
                    case SiemensEnum.S1200:
                        ISO_CR[21] = 0x00;
                        break;
                    case SiemensEnum.S300:
                        ISO_CR[21] = 0x02;
                        break;
                    case SiemensEnum.S400:
                        ISO_CR[21] = 0x03;
                        ISO_CR[17] = 0x00;
                        break;
                    case SiemensEnum.S1500:
                        ISO_CR[21] = 0x00;
                        break;
                    case SiemensEnum.S200Smart:
                        ISO_CR = SiemensHelper.ISO_CR200SMART;
                        S7_PN = SiemensHelper.S7200SMART_PN;
                        break;
                    case SiemensEnum.S200:
                        ISO_CR = SiemensHelper.ISO_CR200;
                        S7_PN = SiemensHelper.S7200_PN;
                        break;
                }
            }
        }
        private byte[] ISO_CR;
        private byte[] S7_PN;
        private int pdu_length = 100;
        private byte plc_rack = 0;
        private byte plc_slot = 0;
        /// <summary>
        /// 传入PLC类型，程序内会改变相应PLC类型的S7协议LocalTSAP， RemoteTSAP等
        /// </summary>
        /// <param name="tcpClient"></param>
        public SiemensS7PLC(TcpClient tcpClient) : base(tcpClient)
        {
            RegisterByteLength = 1;
            ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
            ISO_CR = new byte[22];
            S7_PN = new byte[25];
            Array.Copy(SiemensHelper.ISO_CR, ISO_CR, ISO_CR.Length);
            Array.Copy(SiemensHelper.S7_PN, S7_PN, S7_PN.Length);
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
            return base.GetAddressDescription() + Environment.NewLine + stringBuilder.ToString();
        }

        /// <inheritdoc/>
        public override int GetBitOffset(string address)
        {
            if (address.IndexOf('.') > 0)
            {
                string[] addressSplits = address.SplitStringByDelimiter();
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

        #region 设置

        /// <summary>
        /// 本地TSAP
        /// </summary>
        public int LocalTSAP
        {
            get
            {
                return
                    siemensEnum == SiemensEnum.S200 || siemensEnum == SiemensEnum.S200Smart ?
                    (ISO_CR[13] * 256) + ISO_CR[14] :
                    (ISO_CR[16] * 256) + ISO_CR[17];
            }
            set
            {
                if (siemensEnum == SiemensEnum.S200 || siemensEnum == SiemensEnum.S200Smart)
                {
                    ISO_CR[13] = BitConverter.GetBytes(value)[1];
                    ISO_CR[14] = BitConverter.GetBytes(value)[0];
                }
                else
                {
                    ISO_CR[16] = BitConverter.GetBytes(value)[1];
                    ISO_CR[17] = BitConverter.GetBytes(value)[0];
                }
            }
        }
        /// <summary>
        /// PDULength
        /// </summary>
        public int PDULength => pdu_length;

        /// <summary>
        /// 机架号，需重新连接
        /// </summary>
        public byte Rack
        {
            get => plc_rack;
            set
            {
                plc_rack = value;
                if (siemensEnum == SiemensEnum.S200 || siemensEnum == SiemensEnum.S200Smart)
                {
                    return;
                }

                ISO_CR[21] = (byte)((plc_rack * 0x20) + plc_slot);
            }
        }

        /// <summary>
        /// 槽号，需重新连接
        /// </summary>
        public byte Slot
        {
            get => plc_slot;
            set
            {
                plc_slot = value;
                if (siemensEnum == SiemensEnum.S200 || siemensEnum == SiemensEnum.S200Smart)
                {
                    return;
                }
                ISO_CR[21] = (byte)((plc_rack * 0x20) + plc_slot);
            }
        }

        #endregion

        /// <inheritdoc/>
        public override List<T> LoadSourceRead<T, T2>(List<T2> deviceVariables, int maxPack, int defaultIntervalTime)
        {
            return PackHelper.LoadSourceRead<T, T2>(this, deviceVariables, maxPack, defaultIntervalTime);
        }

        /// <inheritdoc/>
        public override OperResult<byte[]> Read(string address, int length, CancellationToken cancellationToken = default)
        {
            try
            {
                Connect(cancellationToken);
                var commandResult = GetReadByteCommand(address, length);
                List<byte> bytes = new();
                foreach (var item in commandResult)
                {
                    var result = SendThenReturn<SiemensMessage>(item, cancellationToken);
                    if (result.IsSuccess)
                        bytes.AddRange(result.Content);
                    else
                        return result;
                }
                return OperResult.CreateSuccessResult(bytes.ToArray());
            }
            catch (Exception ex)
            {
                return new(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken = default)
        {
            try
            {
                await ConnectAsync(cancellationToken);
                var commandResult = GetReadByteCommand(address, length);
                List<byte> bytes = new();
                foreach (var item in commandResult)
                {
                    var result = await SendThenReturnAsync<SiemensMessage>(item, cancellationToken);
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
        public override void SetDataAdapter(ISocketClient socketClient = default)
        {
            SiemensS7PLCDataHandleAdapter dataHandleAdapter = new();
            TcpClient.SetDataHandlingAdapter(dataHandleAdapter);
        }


        /// <inheritdoc/>
        public override OperResult Write(string address, byte[] value, CancellationToken cancellationToken = default)
        {
            try
            {
                Connect(cancellationToken);
                var commandResult = GetWriteByteCommand(address, value);
                foreach (var item in commandResult)
                {
                    var result = SendThenReturn<SiemensMessage>(item, cancellationToken);
                    if (!result.IsSuccess)
                        return result;
                }
                return OperResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return new OperResult<bool[]>(ex);
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
                Connect(cancellationToken);

                var commandResult = GetWriteBitCommand(address, value[0]);
                return SendThenReturn<SiemensMessage>(commandResult, cancellationToken);
            }
            catch (Exception ex)
            {
                return new OperResult<bool[]>(ex);
            }
            finally
            {
            }
        }

        /// <inheritdoc/>
        public override Task<OperResult> WriteAsync(string address, string value, CancellationToken cancellationToken = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteTransformUtil.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return SiemensHelper.WriteAsync(this, address, value, transformParameter.Encoding);
        }

        /// <inheritdoc/>
        public override async Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken cancellationToken = default)
        {
            try
            {
                await ConnectAsync(cancellationToken);
                var commandResult = GetWriteByteCommand(address, value);
                foreach (var item in commandResult)
                {
                    var result = await SendThenReturnAsync<SiemensMessage>(item, cancellationToken);
                    if (!result.IsSuccess)
                        return result;
                }
                return OperResult.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return new OperResult<bool[]>(ex);
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
                await ConnectAsync(cancellationToken);

                var commandResult = GetWriteBitCommand(address, value[0]);
                return await SendThenReturnAsync<SiemensMessage>(commandResult, cancellationToken);
            }
            catch (Exception ex)
            {
                return new OperResult<bool[]>(ex);
            }
            finally
            {
            }
        }


        #region 其他方法
        /// <summary>
        /// 读取日期
        /// </summary>
        /// <returns></returns>
        public async Task<OperResult<System.DateTime>> ReadDateAsync(string address, CancellationToken cancellationToken)
        {
            return (await this.ReadAsync(address, 2, cancellationToken)).
                 Then(m => OperResult.CreateSuccessResult(ThingsGateway.Foundation.Adapter.Siemens.DateTime.SpecMinimumDateTime.AddDays(
                     ThingsGatewayBitConverter.ToUInt16(m, 0)))
                 );
        }

        /// <summary>
        /// 读取时间
        /// </summary>
        /// <returns></returns>
        public async Task<OperResult<System.DateTime>> ReadDateTimeAsync(string address, CancellationToken cancellationToken)
        {
            return ByteTransformUtil.GetResultFromBytes(await ReadAsync(address, 8, cancellationToken), ThingsGateway.Foundation.Adapter.Siemens.DateTime.FromByteArray);
        }

        /// <summary>
        /// 读取变长字符串
        /// </summary>
        public async Task<OperResult<string>> ReadStringAsync(string address, Encoding encoding, CancellationToken cancellationToken)
        {
            return await SiemensHelper.ReadStringAsync(this, address, encoding, cancellationToken);
        }

        /// <summary>
        /// 写入日期
        /// </summary>
        /// <returns></returns>
        public async Task<OperResult> WriteDateAsync(string address, System.DateTime dateTime, CancellationToken cancellationToken)
        {
            return await base.WriteAsync(address, Convert.ToUInt16((dateTime - ThingsGateway.Foundation.Adapter.Siemens.DateTime.SpecMinimumDateTime).TotalDays), cancellationToken);
        }
        /// <summary>
        /// 写入时间
        /// </summary>
        /// <returns></returns>
        public async Task<OperResult> WriteDateTimeAsync(string address, System.DateTime dateTime, CancellationToken cancellationToken)
        {
            return await WriteAsync(address, ThingsGateway.Foundation.Adapter.Siemens.DateTime.ToByteArray(dateTime), cancellationToken);
        }
        #endregion
        /// <inheritdoc/>
        public override void Dispose()
        {
            TcpClient.Connected -= Connected;
            base.Dispose();
        }
        /// <inheritdoc/>
        protected override async Task Connected(ITcpClient client, ConnectedEventArgs e)
        {
            try
            {
                NormalDataHandlingAdapter dataHandleAdapter = new();
                TcpClient.SetDataHandlingAdapter(dataHandleAdapter);
                try
                {
                    await GetResponsedDataAsync(ISO_CR, TimeOut, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Logger?.Warning($"{client.IP} : {client.Port}：ISO_TP握手失败-{ex.Message}");
                    TcpClient.Close();
                    return;
                }
                try
                {
                    var result2 = await GetResponsedDataAsync(S7_PN, TimeOut, CancellationToken.None);
                    pdu_length = ThingsGatewayBitConverter.ToUInt16(result2.Data.SelectLast(2), 0);
                    pdu_length = pdu_length < 200 ? 200 : pdu_length;
                }
                catch (Exception ex)
                {
                    Logger?.LogInformation($"{client.IP} : {client.Port}：PDU初始化失败-{ex.Message}");
                    return;
                }

            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }
            finally
            {
                SetDataAdapter();
                await base.Connected(client, e);
            }
        }
    }
}