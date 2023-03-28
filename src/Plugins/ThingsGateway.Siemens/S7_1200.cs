namespace ThingsGateway.Siemens
{
    public class S7_1200 : S7
    {
        public S7_1200(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            if (client == null)
            {
                TouchSocketConfig.SetRemoteIPHost(new IPHost(IPAddress.Parse(IP), Port))
                    .SetBufferLength(1024);
                client = TouchSocketConfig.Container.Resolve<TGTcpClient>();
                ((TGTcpClient)client).Setup(TouchSocketConfig);
            }
            //载入配置
            _plc = new((TGTcpClient)client, SiemensEnum.S1200);
            _plc.DataFormat = DataFormat;
            _plc.ConnectTimeOut = ConnectTimeOut;
            _plc.TimeOut = TimeOut;
            if (LocalTSAP != 0)
            {
                _plc.LocalTSAP = LocalTSAP;
            }
            if (DestTSAP != 0)
            {
                _plc.DestTSAP = DestTSAP;
            }
        }


    }
}
