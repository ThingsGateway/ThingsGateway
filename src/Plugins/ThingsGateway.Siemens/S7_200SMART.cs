namespace ThingsGateway.Siemens
{
    public class S7_200SMART : S7
    {
        public S7_200SMART(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }

        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            if (client == null)
            {
                TouchSocketConfig.SetRemoteIPHost(new IPHost(IPAddress.Parse(IP), Port))
                    .SetBufferLength(1024);
                client = TouchSocketConfig.Container.Resolve<TcpClient>();
                ((TcpClient)client).Setup(TouchSocketConfig);
            }
            //载入配置
            _plc = new((TcpClient)client, SiemensEnum.S200Smart);
            _plc.DataFormat = DataFormat;
            _plc.ConnectTimeOut = ConnectTimeOut;
            _plc.TimeOut = TimeOut;

        }

        //[Method("HotStart", "热启动")]
        //public OperResult HotStart()
        //{
        //    return _plc?.HotStart();
        //}
        //[Method("ColdStart", "冷启动")]
        //public OperResult ColdStart()
        //{
        //    return _plc?.ColdStart();
        //}
        //[Method("Stop", "停止")]
        //public OperResult Stop()
        //{
        //    return _plc?.Stop();
        //}
    }
}
