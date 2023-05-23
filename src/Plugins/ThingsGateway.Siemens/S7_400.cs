#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

namespace ThingsGateway.Siemens
{
    public class S7_400 : S7
    {
        public S7_400(IServiceScopeFactory scopeFactory) : base(scopeFactory)
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
            _plc = new((TGTcpClient)client, SiemensEnum.S400);
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
