using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ThingsGateway.Foundation.Extension;

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
        private int pdu_length = 0;
        private byte plc_rack = 0;
        private byte plc_slot = 0;
        private byte[] ISO_CR;
        private byte[] S7_PN;
        public SiemensEnum CurrentPlc => _currentPlc;
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
                await ConnectAsync();
                var commandResult = GetReadByteCommand(address, length);
                if (commandResult.IsSuccess)
                {
                    List<byte> bytes = new();
                    foreach (var item in commandResult.Content)
                    {
                        var result = TGTcpClient.GetTGWaitingClient(new()).SendThenResponse(item, TimeOut, token);
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

        /// <summary>
        /// 读取变长字符串
        /// </summary>
        public async Task<OperResult<string>> ReadString(string address, Encoding encoding)
        {
            return await SiemensHelper.ReadString(this, address, encoding);
        }

        public async Task<OperResult<System.DateTime>> ReadDateTime(string address)
        {
            return ByteTransformHelper.GetResultFromBytes(await ReadAsync(address, 8), ThingsGateway.Foundation.Adapter.Siemens.DateTime.FromByteArray);
        }

        public async Task<OperResult<System.DateTime>> ReadDate(string address)
        {
            return (await this.ReadAsync(address, 2)).
                 Then(m => OperResult.CreateSuccessResult(ThingsGateway.Foundation.Adapter.Siemens.DateTime.SpecMinimumDateTime.AddDays(
                     ThingsGatewayBitConverter.ToUInt16(m, 0)))
                 );
        }

        public override void SetDataAdapter()
        {
            DataHandleAdapter = new();
            TGTcpClient.SetDataHandlingAdapter(DataHandleAdapter);
        }


        public override Task<OperResult> WriteAsync(string address, string value, Encoding encoding, CancellationToken token = default)
        {
            return SiemensHelper.Write(this, address, value, encoding);
        }

        public override async Task<OperResult> WriteAsync(string address, byte[] value, CancellationToken token = default)
        {
            try
            {
                await ConnectAsync();
                var commandResult = GetWriteByteCommand(address, value);
                if (commandResult.IsSuccess)
                {
                    List<ResponsedData> bytes = new();
                    foreach (var item in commandResult.Content)
                    {
                        ResponsedData result = TGTcpClient.GetTGWaitingClient(new()).SendThenResponse(item, TimeOut, token);
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
                await ConnectAsync();

                var commandResult = GetWriteBitCommand(address, value[0]);
                if (commandResult.IsSuccess)
                {
                    var result = TGTcpClient.GetTGWaitingClient(new()).SendThenResponse(commandResult.Content, TimeOut, token);
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

        private void Connected(ITcpClient client, MsgEventArgs e)
        {
            try
            {
                var result1 = SendThenResponse(ISO_CR);
                if (!result1.IsSuccess)
                {
                    Logger?.Error(client.GetIPPort() + "ISO初始化失败");
                    return;
                }
                var result2 = SendThenResponse(S7_PN);
                if (!result2.IsSuccess)
                {
                    Logger?.Error(client.GetIPPort() + "初始化失败");
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

        public async Task<OperResult> WriteDateTime(string address, System.DateTime dateTime)
        {
            return await WriteAsync(address, ThingsGateway.Foundation.Adapter.Siemens.DateTime.ToByteArray(dateTime));
        }

        public async Task<OperResult> WriteDate(string address, System.DateTime dateTime)
        {
            return await base.WriteAsync(address, Convert.ToUInt16((dateTime - ThingsGateway.Foundation.Adapter.Siemens.DateTime.SpecMinimumDateTime).TotalDays));
        }


    }
}