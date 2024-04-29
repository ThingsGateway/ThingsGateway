//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

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
            var clientChannel = clientConfig.GetTcpClientWithIPHost("tcp://127.0.0.1:502");
            //var clientChannel = clientConfig.GetTcpServiceWithBindIPHost("tcp://127.0.0.1:502");
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

        private static ModbusMaster GetDtuMaster()
        {
            var clientConfig = new TouchSocketConfig();
            ConsoleLogger.Default.LogLevel = LogLevel.Trace;
            clientConfig.ConfigureContainer(a => a.AddConsoleLogger());
            //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取
            //var clientChannel = clientConfig.GetTcpClientWithIPHost("tcp://127.0.0.1:502");
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

        private static ModbusVariable GetVariable()
        {
            var modbusMaster = GetMaster();
            //构造实体类对象，传入协议对象与连读打包的最大数量
            ModbusVariable modbusVariable = new(modbusMaster, 100);
            return modbusVariable;
        }

        /// <summary>
        /// 测试实体类读写
        /// </summary>
        public static void Test1()
        {
            ModbusVariable modbusVariable = GetVariable();
            //源生成WriteData1与WriteData2方法(Write{属性名称})
            modbusVariable.WriteData3("123", default);
            modbusVariable.WriteData2(1, default);
            modbusVariable.WriteData1(new ushort[] { 1, 2 }, default);

            //执行连读，如果带有读写表达式，初次读写会进行动态编译，耗时较长
            var result = modbusVariable.MulRead();
            if (!result.IsSuccess) Console.WriteLine(result.ToJsonString());
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + modbusVariable.ToJsonString());
            //执行连读
            result = modbusVariable.MulRead();
            if (!result.IsSuccess) Console.WriteLine(result.ToJsonString());
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff") + modbusVariable.ToJsonString());
            Console.ReadLine();
        }
        /// <summary>
        /// 测试直接读写
        /// </summary>
        public static void Test2()
        {
            using ModbusMaster modbusMaster = GetMaster();

            modbusMaster.Channel.Connect(3000, CancellationToken.None);
            var data = modbusMaster.ReadInt16("40001;");//寄存器;
            Console.WriteLine(data.ToJsonNetString());
            Console.ReadLine();
        }
        /// <summary>
        /// 测试DTU读写
        /// </summary>
        public static void Test3()
        {
            using ModbusMaster modbusMaster = GetDtuMaster();

            modbusMaster.HeartbeatHexString = "ccccdddd";//心跳
            modbusMaster.Channel.Connect(3000, CancellationToken.None);
            Console.WriteLine("回车后读取注册包为abcd的客户端");
            Console.ReadLine();
            //构造实体类对象，传入协议对象与连读打包的最大数量
            var data = modbusMaster.ReadInt16("40001;id=abcd");//寄存器;{id=注册包}
            Console.WriteLine(data.ToJsonNetString());
            Console.ReadLine();
        }
    }


    [GeneratorVariable]
    public partial class ModbusVariable : VariableObject
    {
        [VariableRuntime(RegisterAddress = "400001;arraylen=2")]
        public ushort[] Data1 { get; set; }

        [VariableRuntime(RegisterAddress = "400051", ReadExpressions = "raw*10-5+500")]
        public ushort Data2 { get; set; }

        [VariableRuntime(RegisterAddress = "400061;len=10")]
        public string Data3 { get; set; }

        public ModbusVariable(IProtocol protocol, int maxPack) : base(protocol, maxPack)
        {
        }
    }

}
