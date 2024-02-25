//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;

namespace ThingsGateway.Foundation
{
    internal class ModbusMasterTest
    {
        private static ModbusMaster GetMaster()
        {
            var clientConfig = new TouchSocketConfig();
            clientConfig.ConfigureContainer(a => a.AddConsoleLogger());
            //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取
            var clientChannel = clientConfig.GetTcpClientWithIPHost("tcp://127.0.0.1:502");
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
            //构造实体类对象，传入协议对象与连读打包的最大数量
            ModbusVariable modbusVariable = new(modbusMaster, 100);

            Test(modbusVariable, new ushort[] { 1, 2 });
            Console.WriteLine(modbusVariable.ToJsonString());
            Console.ReadLine();

            static void Test(ModbusVariable modbusVariable, object value)
            {
                //源生成WriteData1与WriteData2方法(Write{属性名称})
                modbusVariable.WriteData3("123", default);
                modbusVariable.WriteData2(1, default);
                modbusVariable.WriteData1(value, default);

                //执行连读
                modbusVariable.MulRead();
                Console.WriteLine(modbusVariable.ToJsonString());
                //执行连读
                modbusVariable.MulRead();
                Console.WriteLine(modbusVariable.ToJsonString());
            }
        }
    }

    [GeneratorVariable]
    public partial class ModbusVariable : VariableObject
    {
        [VariableRuntime(RegisterAddress = "400001;arraylen=2")]
        public ushort[] Data1 { get; set; }

        [VariableRuntime(RegisterAddress = "400051")]
        public ushort Data2 { get; set; }

        [VariableRuntime(RegisterAddress = "400061;len=10")]
        public string Data3 { get; set; }

        public ModbusVariable(IProtocol protocol, int maxPack) : base(protocol, maxPack)
        {
        }
    }
}