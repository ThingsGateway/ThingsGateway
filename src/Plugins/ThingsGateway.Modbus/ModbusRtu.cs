using Microsoft.Extensions.DependencyInjection;

using System.IO.Ports;

using ThingsGateway.Foundation;
using ThingsGateway.Foundation.Serial;
using ThingsGateway.Web.Foundation;

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ThingsGateway.Modbus
{
    public class ModbusRtu : DriverBase, IDisposable
    {

        private ThingsGateway.Foundation.Adapter.Modbus.ModbusRtu _plc;

        public ModbusRtu(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }
        [DeviceProperty("连接超时时间", "")] public ushort ConnectTimeOut { get; set; } = 3000;
        [DeviceProperty("默认解析顺序", "")] public DataFormat DataFormat { get; set; }
        [DeviceProperty("最大打包长度", "")] public ushort MaxPack { get; set; } = 100;
        [DeviceProperty("默认站号", "")] public byte Station { get; set; } = 1;
        [DeviceProperty("读写超时时间", "")] public ushort TimeOut { get; set; } = 3000;
        [DeviceProperty("CRC检测", "")] public bool Crc16CheckEnable { get; set; } = true;
        [DeviceProperty("COM口", "示例：COM1")] public string PortName { get; set; } = "COM1";
        [DeviceProperty("波特率", "通常为：38400/19200/9600/4800")] public int BaudRate { get; set; } = 9600;
        [DeviceProperty("数据位", "通常为：8/7/6")] public byte DataBits { get; set; } = 8;
        [DeviceProperty("校验位", "示例：None/Odd/Even/Mark/Space")] public Parity Parity { get; set; } = Parity.None;
        [DeviceProperty("停止位", "示例：None/One/Two/OnePointFive")] public StopBits StopBits { get; set; } = StopBits.One;

        public override void AfterStop()
        {
            _plc?.Close();
        }

        public override async Task BeforStart()
        {
            await _plc?.OpenAsync();
        }

        public override void Dispose()
        {
            _plc.Close();
            _plc.Dispose();
        }

        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            if (client == null)
            {
                TouchSocketConfig.SetSerialProperty(new()
                {
                    PortName = PortName,
                    BaudRate = BaudRate,
                    DataBits = DataBits,
                    Parity = Parity,
                    StopBits = StopBits,
                })
                    .SetBufferLength(1024);
                client = TouchSocketConfig.Container.Resolve<SerialClient>();
                ((SerialClient)client).Setup(TouchSocketConfig);
            }
            //载入配置
            _plc = new((SerialClient)client);
            _plc.Crc16CheckEnable = Crc16CheckEnable;
            _plc.DataFormat = DataFormat;
            _plc.Station = Station;
            _plc.TimeOut = TimeOut;
        }

        public override bool IsSupportAddressRequest()
        {
            return true;
        }

        public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
        {
            return deviceVariables.LoadSourceRead(_logger, ThingsGatewayBitConverter, MaxPack);
        }


        public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
        {
            return await _plc.WriteAsync(deviceVariable.DataType, deviceVariable.VariableAddress, value);
        }

        protected override async Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
        {
            return await _plc.ReadAsync(address, length);
        }

    }
}
