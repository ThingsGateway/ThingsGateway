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
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            //Dlt645MasterTest dlt645MasterTest = new Dlt645MasterTest();
            //var channel = dlt645MasterTest.GetChannel();
            //var protocol = dlt645MasterTest.GetProtocol(channel);
            //var data = await protocol.ReadDoubleAsync("02010100"); //读取A相电压
            //Console.WriteLine(data.ToJsonString());

            var clientConfig = new TouchSocketConfig();
            clientConfig.ConfigureContainer(a => a.AddConsoleLogger());
            //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取
            //var clientChannel = clientConfig.GetTcpClientWithIPHost("tcp://127.0.0.1:502");
            var clientChannel = clientConfig.GetSerialPortWithOption("COM1");
            clientChannel.Logger.LogLevel = LogLevel.Trace;
            //创建modbus客户端，传入通道
            using ModbusMaster modbusMaster = new(clientChannel)
            {
                //modbus协议格式
                ModbusType = Modbus.ModbusTypeEnum.ModbusRtu,
                //ModbusType = Modbus.ModbusTypeEnum.ModbusTcp,
            };

            ////构造实体类对象，传入协议对象与连读打包的最大数量
            //ModbusVariable modbusVariable = new(modbusMaster, 100);

            ////手动执行连读，操作会使Data1属性更新值
            //modbusVariable.MulRead();
            //Console.WriteLine(modbusVariable.ToJsonString());
            //源代码生成了WriteData1与WriteData1Async方法    (Write{属性名称})
            //直接调用就可以写入PLC
            //var data1 = modbusVariable.WriteData1(11, CancellationToken.None);
            //var data3 = await modbusVariable.WriteData1Async(11, CancellationToken.None);
            //Console.WriteLine(data1.ToJsonString());
            //Console.WriteLine(data3.ToJsonString());

            modbusMaster.Read("40001", (ushort)11);
            modbusMaster.Write("40001", (ushort)11);
            await modbusMaster.ReadAsync("40001", (ushort)11);
            await modbusMaster.WriteAsync("40001", (ushort)11);
            modbusMaster.Read("40001", (ushort)11);
            modbusMaster.Write("40001", (ushort)11);

            Console.ReadLine();
        }
    }

    //[GeneratorVariable]
    //public partial class ModbusVariable : VariableObject
    //{
    //    public ModbusVariable(IProtocol protocol, int maxPack) : base(protocol, maxPack)
    //    {
    //    }

    //    [VariableRuntime(RegisterAddress = "400001")]
    //    public ushort Data1 { get; set; }

    //    [VariableRuntime(RegisterAddress = "400051")]
    //    public ushort Data2 { get; set; }
    //}
}