namespace ThingsGateway.Siemens
{
    public class S7_200SMART : S7
    {
        public S7_200SMART(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }
        public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            if (client == null)
            {
                TouchSocketConfig.SetRemoteIPHost(new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}"))
                    .SetBufferLength(1024);
                client = TouchSocketConfig.Container.Resolve<TGTcpClient>();
                ((TGTcpClient)client).Setup(TouchSocketConfig);
            }
            //载入配置
            _plc = new((TGTcpClient)client, SiemensEnum.S200Smart);
            _plc.DataFormat = driverPropertys.DataFormat;
            _plc.ConnectTimeOut = driverPropertys.ConnectTimeOut;
            _plc.TimeOut = driverPropertys.TimeOut;
            if (driverPropertys.LocalTSAP != 0)
            {
                _plc.LocalTSAP = driverPropertys.LocalTSAP;
            }
            if (driverPropertys.DestTSAP != 0)
            {
                _plc.DestTSAP = driverPropertys.DestTSAP;
            }
        }

    }
}
