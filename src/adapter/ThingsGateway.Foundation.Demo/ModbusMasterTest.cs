//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Json.Extension;
using ThingsGateway.Foundation.Modbus;

namespace ThingsGateway.Foundation;

internal class ModbusMasterTest
{
    private static ModbusMaster GetMaster()
    {
        var clientConfig = new TouchSocketConfig();
        ConsoleLogger.Default.LogLevel = LogLevel.Trace;
        clientConfig.ConfigureContainer(a => a.AddConsoleLogger());
        //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取
        //var clientChannel = clientConfig.GetTcpServiceWithBindIPHost("tcp://127.0.0.1:502");
        //var clientChannel = clientConfig.GetSerialPortWithOption("COM1");
        var clientChannel = clientConfig.GetTcpClientWithIPHost("127.0.0.1:502");
        //clientChannel.Logger.LogLevel = LogLevel.Trace;
        ModbusMaster modbusMaster = new(clientChannel)
        {
            //modbus协议格式
            ModbusType = Modbus.ModbusTypeEnum.ModbusRtu,
            //ModbusType = Modbus.ModbusTypeEnum.ModbusTcp,
        };
        return modbusMaster;
    }

    public static async Task Test()
    {
        using ModbusMaster modbusMaster = GetMaster();
        //modbusMaster.HeartbeatHexString = "ccccdddd";//心跳
        await modbusMaster.Channel.ConnectAsync();
        //Console.WriteLine("回车后读取注册包为abcd的客户端");
        Console.ReadLine();
        var data = await modbusMaster.ReadInt16Async("40001;id=abcd");//寄存器;{id=注册包}
        Console.WriteLine(data.ToJsonNetString());
        //Console.ReadLine();

        //等待读取到指定值
        var waitResult = await OperResultUtil.WaitAsync<short>(async (a) =>
           {
               return await modbusMaster.ReadInt16Async("40001;");
           }
           , 10, 10000
          );

        Console.WriteLine(waitResult.ToJsonNetString());
        //构造实体类对象，传入协议对象与连读打包的最大数量
        ModbusVariable modbusVariable = new(modbusMaster, 100);

        await Test(modbusVariable);
        Console.WriteLine(modbusVariable.ToJsonString());
        Console.ReadLine();

        static async Task Test(ModbusVariable modbusVariable)
        {
            //源生成WriteData1与WriteData2方法(Write{属性名称})
            await modbusVariable.WriteData3Async("123", default);
            await modbusVariable.WriteData2Async(1, default);
            await modbusVariable.WriteData1Async(new ushort[] { 1, 2 }, default);

            //执行连读
            await modbusVariable.MultiReadAsync();
            Console.WriteLine(modbusVariable.ToJsonString());
            //执行连读
            await modbusVariable.MultiReadAsync();
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
