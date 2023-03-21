using System.Text;

using ThingsGateway.Foundation;

namespace ThingsGateway.Siemens
{
    public abstract class S7 : DriverBase
    {

        protected SiemensS7PLC _plc;

        protected S7(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        [DeviceProperty("连接超时时间", "")] public ushort ConnectTimeOut { get; set; } = 3000;
        [DeviceProperty("默认解析顺序", "")] public DataFormat DataFormat { get; set; }
        [DeviceProperty("IP", "")] public string IP { get; set; } = "127.0.0.1";
        [DeviceProperty("端口", "")] public int Port { get; set; } = 102;
        public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }
        [DeviceProperty("读写超时时间", "")] public ushort TimeOut { get; set; } = 3000;
        [DeviceProperty("LocalTSAP", "为0时不写入")] public int LocalTSAP { get; set; } = 0;
        [DeviceProperty("DestTSAP", "为0时不写入")] public int DestTSAP { get; set; } = 0;


        [DeviceMethod("ReadDate", "")]
        public Task<OperResult<System.DateTime>> ReadDate(string address)
        {
            return _plc?.ReadDate(address);
        }
        [DeviceMethod("ReadDateTime", "")]
        public Task<OperResult<System.DateTime>> ReadDateTime(string address)
        {
            return _plc?.ReadDateTime(address);
        }

        [DeviceMethod("ReadString", "")]
        public Task<OperResult<string>> ReadString(string address, Encoding encoding)
        {
            return _plc?.ReadString(address, encoding);
        }

        [DeviceMethod("WriteDate", "")]
        public Task<OperResult> WriteDate(string address, System.DateTime dateTime)
        {
            return _plc?.WriteDate(address, dateTime);
        }
        [DeviceMethod("WriteDateTime", "")]
        public Task<OperResult> WriteDateTime(string address, System.DateTime dateTime)
        {
            return _plc?.WriteDateTime(address, dateTime);
        }


        public override void AfterStop()
        {
            _plc?.Disconnect();
        }

        public override async Task BeforStart()
        {
            await _plc.ConnectAsync();
        }

        public override void Dispose()
        {
            _plc.Disconnect();
        }

        public override bool IsSupportAddressRequest()
        {
            return true;
        }
        public override  OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
        {
            Init(null);
            _plc.Connect();
            var data= deviceVariables.LoadSourceRead(_logger, ThingsGatewayBitConverter, _plc);
            _plc?.Disconnect();
            return data;
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
