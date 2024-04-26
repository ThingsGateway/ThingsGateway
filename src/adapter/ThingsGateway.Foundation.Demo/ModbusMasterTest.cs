//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Json.Extension;
using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;

namespace ThingsGateway.Foundation
{
    internal class ModbusMasterTest
    {
        private static ModbusMaster GetMaster()
        {
            var clientConfig = new TouchSocketConfig();
            ConsoleLogger.Default.LogLevel = LogLevel.Trace;
            clientConfig.ConfigureContainer(a => a.AddConsoleLogger());
            //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取
            var clientChannel = clientConfig.GetTcpServiceWithBindIPHost("tcp://127.0.0.1:502");
            //var clientChannel = clientConfig.GetSerialPortWithOption("COM1");
            //clientChannel.Logger.LogLevel = LogLevel.Trace;
            ModbusMaster modbusMaster = new(clientChannel)
            {
                //modbus协议格式
                ModbusType = Modbus.ModbusTypeEnum.ModbusRtu,
                //ModbusType = Modbus.ModbusTypeEnum.ModbusTcp,
            };
            return modbusMaster;
        }

        public static void Test()
        {
            using ModbusMaster modbusMaster = GetMaster();

            modbusMaster.HeartbeatHexString = "ccccdddd";//心跳
            modbusMaster.Channel.Connect(3000,CancellationToken.None);
            Console.WriteLine("回车后读取注册包为abcd的客户端");
            Console.ReadLine();
            //构造实体类对象，传入协议对象与连读打包的最大数量
            var data = modbusMaster.ReadInt16("40001;id=abcd");//寄存器;{id=注册包}
            Console.WriteLine(data.ToJsonNetString());
            Console.ReadLine();
        }
    }


}
