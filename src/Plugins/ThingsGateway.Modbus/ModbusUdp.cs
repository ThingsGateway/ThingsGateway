using Microsoft.Extensions.DependencyInjection;

using System.Net;

using ThingsGateway.Foundation;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;
using TouchSocket.Sockets;


namespace ThingsGateway.Modbus
{
    public class ModbusUdp : DriverBase
    {

        private ThingsGateway.Foundation.Adapter.Modbus.ModbusUdp _plc;

        public ModbusUdp(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }

        [DeviceProperty("连接超时时间", "")] public ushort ConnectTimeOut { get; set; } = 3000;
        [DeviceProperty("默认解析顺序", "")] public DataFormat DataFormat { get; set; }
        [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";
        [DeviceProperty("最大打包长度", "")] public ushort MaxPack { get; set; } = 100;
        [DeviceProperty("端口", "")] public int Port { get; set; } = 502;
        [DeviceProperty("默认站号", "")] public byte Station { get; set; } = 1;
        [DeviceProperty("读写超时时间", "")] public ushort TimeOut { get; set; } = 3000;
        [DeviceProperty("连接标识检测", "")] public bool MessageIdCheckEnable { get; set; }

        public override void AfterStop()
        {
            _plc.Disconnect();
        }

        public override Task BeforStart()
        {
            return _plc.ConnectAsync();
        }
        public override void Dispose()
        {
            _plc?.Disconnect();
        }
        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            if (client == null)
            {
                TouchSocketConfig.SetRemoteIPHost(new IPHost($"{IP}:{Port}"))
                    .SetBufferLength(1024);
                client = TouchSocketConfig.BuildWithUdpSession<UdpSession>();
            }
            //载入配置
            _plc = new((UdpSession)client);
            _plc.DataFormat = DataFormat;
            _plc.ConnectTimeOut = ConnectTimeOut;
            _plc.Station = Station;
            _plc.TimeOut = TimeOut;
            _plc.IsCheckMessageId = MessageIdCheckEnable;
        }

        public override bool IsSupportAddressRequest()
        {
            return true;
        }

        public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
        {
            return deviceVariables.LoadSourceRead(_logger, ThingsGatewayBitConverter, MaxPack);
        }
        protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
        {
            return await _plc.ReadAsync(address, length, cancellationToken);
        }


        public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
        {
            return await _plc.WriteAsync(deviceVariable.DataType, deviceVariable.VariableAddress, value);
        }



    }
}
