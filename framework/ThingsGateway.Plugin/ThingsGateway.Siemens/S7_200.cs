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

using System;

using ThingsGateway.Application;
using ThingsGateway.Foundation;

namespace ThingsGateway.Siemens
{
    /// <inheritdoc/>
    public class S7_200 : Siemens
    {

        /// <inheritdoc/>
        public override Type DriverDebugUIType => typeof(S7_200DebugDriverPage);
        /// <inheritdoc/>
        public override CollectDriverPropertyBase DriverPropertys => driverPropertys;

        /// <inheritdoc/>
        protected override void Init(CollectDeviceRunTime device, object client = null)
        {
            if (client == null)
            {
                FoundataionConfig.SetRemoteIPHost(new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}"))
                    ;
                client = new TcpClientEx();
                ((TcpClientEx)client).Setup(FoundataionConfig);
            }
            //载入配置
            _plc = new((TcpClientEx)client, SiemensEnum.S200)
            {
                DataFormat = driverPropertys.DataFormat,
                ConnectTimeOut = driverPropertys.ConnectTimeOut,
                TimeOut = driverPropertys.TimeOut
            };
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
