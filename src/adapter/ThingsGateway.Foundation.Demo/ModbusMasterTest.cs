//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

internal class ModbusMasterTest
{
    public static async Task Test()
    {

        ModbusMaster modbusMaster = GetMaster();

        //查看modbus驱动地址说明
        Console.WriteLine(modbusMaster.GetAddressDescription());

        //读取并解析寄存器数据
        var data = await modbusMaster.ReadInt16Async("40001", 1, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        var data1 = await modbusMaster.ReadSingleAsync("40001").ConfigureAwait(false);
        var data2 = await modbusMaster.ReadUInt32Async("40001").ConfigureAwait(false);
        var data3 = await modbusMaster.WriteAsync("40001", (ushort)1).ConfigureAwait(false);

        //读取字节数组
        var data4 = await modbusMaster.ReadAsync("40001", 10).ConfigureAwait(false);
        if (data4.IsSuccess)
        {
            ValueByteBlock valueByteBlock = new(data4.Content);
            var float1 = valueByteBlock.ReadFloat(EndianType.LittleSwap);
        }
        //自定义数据格式
        var data5 = await modbusMaster.ReadSingleAsync("40001;data=ABCD").ConfigureAwait(false);

        //自带打包方法
        //1、实现IVariable接口
        //2、实现IVariableSource接口
        //3、调用LoadSourceRead，返回打包封装类
        //4、调用ReadAsync读取数据，并调用 PraseStructContent 解析数据
        //5、解析数据后，可以通过IVariable获取解析后的数据
        List<VariableClass> variableClasses = new()
        {
            new VariableClass()
            {
                DataType=DataTypeEnum.Int16,
                RegisterAddress="40001",
                IntervalTime=1000,
            },
                new VariableClass()
            {
                DataType=DataTypeEnum.Int32,
                RegisterAddress="40011",
                IntervalTime=1000,
            },
        };
        var deviceVariableSourceReads = modbusMaster.LoadSourceRead<VariableSourceClass>(variableClasses, 100, 1000);
        foreach (var item in deviceVariableSourceReads)
        {
            var result = await modbusMaster.ReadAsync(item.RegisterAddress, item.Length).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                try
                {
                    var result1 = item.VariableRunTimes.PraseStructContent(modbusMaster, result.Content, exWhenAny: true);
                    if (!result1.IsSuccess)
                    {
                        item.LastErrorMessage = result1.ErrorMessage;
                        var time = DateTime.Now;
                        item.VariableRunTimes.ForEach(a => a.SetValue(null, time, isOnline: false));
                        modbusMaster.Logger?.Warning(result1.ToString());
                    }
                }
                catch (Exception ex)
                {
                    modbusMaster.Logger?.Exception(ex);
                }
            }
            else
            {
                item.LastErrorMessage = result.ErrorMessage;
                var time = DateTime.Now;
                item.VariableRunTimes.ForEach(a => a.SetValue(null, time, isOnline: false));
                modbusMaster.Logger?.Warning(result.ToString());
            }
        }

        //自带实体通讯类，包含连读打包与写入
        //1、实现VariableObject虚类，添加业务属性，比如添加 ushort AlarmLevel{get;set;}，业务属性需添加VariableRuntime特性，指定寄存器地址、特定数据类型(不填默认为C#属性类型)、读写表达式(原始值为raw，可填：raw*100+10，做一些数学转换))
        //2、实例化业务实体类，需传入协议对象与连读打包的最大数量
        //3、可以看到源代码生成器已经自动生成了写入方法，可直接调用WriteAlarmLevelAsync方法写入数据
        //4、调用MultiReadAsync方法执行连读，结果会自动解析到业务实体类的属性中

        //构造实体类对象，传入协议对象与连读打包的最大数量
        ModbusVariable modbusVariable = new(modbusMaster, 100);

        await Test(modbusVariable).ConfigureAwait(false);
        Console.WriteLine(modbusVariable.ToJsonString());
        Console.ReadLine();

        static async Task Test(ModbusVariable modbusVariable)
        {
            //源生成WriteData1与WriteData2方法(Write{属性名称})
            await modbusVariable.WriteData3Async("123", default).ConfigureAwait(false);
            await modbusVariable.WriteData2Async(1, default).ConfigureAwait(false);
            await modbusVariable.WriteData1Async([1, 2], default).ConfigureAwait(false);

            //执行连读
            await modbusVariable.MultiReadAsync().ConfigureAwait(false);
            Console.WriteLine(modbusVariable.ToJsonString());
            //执行连读
            await modbusVariable.MultiReadAsync().ConfigureAwait(false);
            Console.WriteLine(modbusVariable.ToJsonString());
        }
    }

    private static ModbusMaster GetMaster()
    {
        ConsoleLogger.Default.LogLevel = LogLevel.Trace;

        var clientConfig = new TouchSocketConfig();
        clientConfig.ConfigureContainer(a => a.AddConsoleLogger()); //日志

        //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取

        //tcp服务
        //var clientChannel = clientConfig.GetTcpServiceWithBindIPHost("tcp://127.0.0.1:502");
        //串口
        //var clientChannel = clientConfig.GetSerialPortWithOption("COM1");
        //udp
        //var clientChannel = clientConfig.GetUdpSessionWithIPHost("127.0.0.1:502",null);

        //tcp客户端
        var clientChannel = clientConfig.GetTcpClientWithIPHost("127.0.0.1:502");

        //modbus主站，构造函数传入通道
        ModbusMaster modbusMaster = new(clientChannel)
        {
            //modbus协议格式
            ModbusType = Modbus.ModbusTypeEnum.ModbusTcp,
            //ModbusType = Modbus.ModbusTypeEnum.ModbusTcp,

            //默认站号
            Station = 1,
            //默认数据格式
            DataFormat = DataFormatEnum.ABCD,
            //读写超时
            Timeout = 3000,
        };
        return modbusMaster;
    }
}
