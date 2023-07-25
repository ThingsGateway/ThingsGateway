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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Resources;

namespace ThingsGateway.Foundation.Adapter.Siemens
{
    /// <summary>
    /// 相关命令含义源自网络资料/Shrap7/s7netplus
    /// </summary>
    public partial class SiemensS7PLC : ReadWriteDevicesTcpClientBase
    {

        public SiemensS7PLCDataHandleAdapter DataHandleAdapter = new();
        private SiemensEnum _currentPlc = SiemensEnum.S1200;
        private byte[] ISO_CR;
        private int pdu_length = 0;
        private byte plc_rack = 0;
        private byte plc_slot = 0;
        private byte[] S7_PN;
        private IWaitingClient<TGTcpClient> waitingClient;
        /// <summary>
        /// 传入PLC类型，程序内会改变相应PLC类型的S7协议LocalTSAP， RemoteTSAP等
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="siemensPLCEnum"></param>
        public SiemensS7PLC(TGTcpClient tcpClient, SiemensEnum siemensPLCEnum) : base(tcpClient)
        {
            _currentPlc = siemensPLCEnum;
            RegisterByteLength = 1;
            ThingsGatewayBitConverter = new ThingsGatewayBitConverter(EndianType.Big);
            ISO_CR = new byte[22];
            S7_PN = new byte[25];
            Array.Copy(SiemensHelper.ISO_CR, ISO_CR, ISO_CR.Length);
            Array.Copy(SiemensHelper.S7_PN, S7_PN, S7_PN.Length);
            switch (siemensPLCEnum)
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
            tcpClient.Connected += Connected;

            waitingClient = TGTcpClient.GetTGWaitingClient(new());
        }

        public SiemensEnum CurrentPlc => _currentPlc;
        public override string GetAddressDescription()
        {
            StringBuilder stringBuilder = new StringBuilder();
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

        public override int GetBitOffset(string address)
        {
            int bitIndex = 0;
            string[] addressSplits = new string[] { address };
            if (address.IndexOf('.') > 0)
            {
                addressSplits = address.SplitDot();
                try
                {
                    if ((addressSplits.Length == 2 && !address.ToUpper().Contains("DB")) || (addressSplits.Length >= 3 && address.ToUpper().Contains("DB")))
                        bitIndex = Convert.ToInt32(addressSplits.Last());

                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }
        #region 设置

        /// <summary>
        /// 远程TSAP，需重新连接
        /// </summary>
        public int DestTSAP
        {
            get
            {
                return
                    _currentPlc == SiemensEnum.S200 || _currentPlc == SiemensEnum.S200Smart ?
                    (ISO_CR[17] * 256) + ISO_CR[18] :
                    (ISO_CR[20] * 256) + ISO_CR[21];
            }
            set
            {
                if (_currentPlc == SiemensEnum.S200 || _currentPlc == SiemensEnum.S200Smart)
                {
                    ISO_CR[17] = BitConverter.GetBytes(value)[1];
                    ISO_CR[18] = BitConverter.GetBytes(value)[0];
                }
                else
                {
                    ISO_CR[20] = BitConverter.GetBytes(value)[1];
                    ISO_CR[21] = BitConverter.GetBytes(value)[0];
                }
            }
        }

        /// <summary>
        /// 本地TSAP，需重新连接
        /// </summary>
        public int LocalTSAP
        {
            get
            {
                return
                    _currentPlc == SiemensEnum.S200 || _currentPlc == SiemensEnum.S200Smart ?
                    (ISO_CR[13] * 256) + ISO_CR[14] :
                    (ISO_CR[16] * 256) + ISO_CR[17];
            }
            set
            {
                if (_currentPlc == SiemensEnum.S200 || _currentPlc == SiemensEnum.S200Smart)
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
                if (_currentPlc == SiemensEnum.S200 || _currentPlc == SiemensEnum.S200Smart)
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
                if (_currentPlc == SiemensEnum.S200 || _currentPlc == SiemensEnum.S200Smart)
                {
                    return;
                }
                ISO_CR[21] = (byte)((plc_rack * 0x20) + plc_slot);
            }
        }

        #endregion

        public override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken token = default)
        {
            try
            {
                await ConnectAsync(token);
                var commandResult = GetReadByteCommand(address, length);
                if (commandResult.IsSuccess)
                {
                    List<byte> bytes = new();
                    foreach (var item in commandResult.Content)
                    {
                        var result = await waitingClient.SendThenResponseAsync(item, TimeOut, token);
                        if (result.RequestInfo is MessageBase collectMessage)
                        {
                            if (collectMessage.IsSuccess)
                                bytes.AddRange(collectMessage.Content);
                            else
                                return OperResult.CreateFailedResult<byte[]>(collectMessage);
                        }
                    }
                    return bytes.Count > 0 ? OperResult.CreateSuccessResult(bytes.ToArray()) : new OperResult<byte[]>(TouchSocketStatus.UnknownError.GetDescription());
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
            finally
            {
            }

        }

        public async Task<OperResult<System.DateTime>> ReadDateAsync(string address)
        {
            return (await this.ReadAsync(address, 2)).
                 Then(m => OperResult.CreateSuccessResult(ThingsGateway.Foundation.Adapter.Siemens.DateTime.SpecMinimumDateTime.AddDays(
                     ThingsGatewayBitConverter.ToUInt16(m, 0)))
                 );
        }

        public async Task<OperResult<System.DateTime>> ReadDateTimeAsync(string address)
        {
            return ByteTransformHelpers.GetResultFromBytes(await ReadAsync(address, 8), ThingsGateway.Foundation.Adapter.Siemens.DateTime.FromByteArray);
        }

        /// <summary>
        /// 读取变长字符串
        /// </summary>
        public async Task<OperResult<string>> ReadStringAsync(string address, Encoding encoding)
        {
            return await SiemensHelper.ReadStringAsync(this, address, encoding);
        }
        public override void SetDataAdapter()
        {
            DataHandleAdapter = new();
            TGTcpClient.SetDataHandlingAdapter(DataHandleAdapter);
        }


        public override Task<OperResult> WriteAsync(string address, string value, CancellationToken token = default)
        {
            IThingsGatewayBitConverter transformParameter = ByteTransformHelpers.GetTransByAddress(ref address, ThingsGatewayBitConverter);
            return SiemensHelper.WriteAsync(this, address, value, transformParameter.Encoding);
        }

        public override async Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default)
        {
            try
            {
                await ConnectAsync(token);
                var commandResult = GetWriteByteCommand(address, value);
                if (commandResult.IsSuccess)
                {
                    List<ResponsedData> bytes = new();
                    foreach (var item in commandResult.Content)
                    {
                        ResponsedData result = await waitingClient.SendThenResponseAsync(item, TimeOut, token);
                        bytes.Add(result);
                    }
                    return OperResult.CreateSuccessResult(bytes.ToArray());
                }
                else
                {
                    return OperResult.CreateFailedResult<bool[]>(commandResult);
                }
            }
            catch (Exception ex)
            {
                return new OperResult<bool[]>(ex);
            }
            finally
            {
            }
        }

        public override async Task<OperResult> WriteAsync(string address, bool[] value, CancellationToken token = default)
        {
            if (value.Length > 1)
            {
                return new OperResult("不支持多写");
            }
            try
            {
                await ConnectAsync(token);

                var commandResult = GetWriteBitCommand(address, value[0]);
                if (commandResult.IsSuccess)
                {
                    var result = await waitingClient.SendThenResponseAsync(commandResult.Content, TimeOut, token);
                    return OperResult.CreateSuccessResult(result);
                }
                else
                {
                    return OperResult.CreateFailedResult<bool[]>(commandResult);
                }
            }
            catch (Exception ex)
            {
                return new OperResult<bool[]>(ex);
            }
            finally
            {
            }
        }

        public async Task<OperResult> WriteDateAsync(string address, System.DateTime dateTime)
        {
            return await base.WriteAsync(address, Convert.ToUInt16((dateTime - ThingsGateway.Foundation.Adapter.Siemens.DateTime.SpecMinimumDateTime).TotalDays));
        }

        public async Task<OperResult> WriteDateTimeAsync(string address, System.DateTime dateTime)
        {
            return await WriteAsync(address, ThingsGateway.Foundation.Adapter.Siemens.DateTime.ToByteArray(dateTime));
        }

        private void Connected(ITcpClient client, MsgEventArgs e)
        {
            try
            {
                var result1 = SendThenResponse(ISO_CR);
                if (!result1.IsSuccess)
                {
                    Logger?.Warning(client.IP + ":" + client.Port + "-ISO初始化失败：" + result1.Message);
                    return;
                }
                var result2 = SendThenResponse(S7_PN);
                if (!result2.IsSuccess)
                {
                    Logger?.Warning(client.IP + ":" + client.Port + "-初始化失败");
                    return;
                }
                pdu_length = ThingsGatewayBitConverter.ToUInt16(result2.Content.SelectLast(2), 0);
                pdu_length = pdu_length < 200 ? 200 : pdu_length;
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
            }

        }
    }
}