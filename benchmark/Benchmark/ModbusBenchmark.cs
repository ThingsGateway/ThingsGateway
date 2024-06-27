//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BenchmarkConsoleApp;

using BenchmarkDotNet.Attributes;

using HslCommunication.ModBus;

using NModbus.Device;
using NModbus.Serial;

using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;
using TouchSocket.SerialPorts;

using IModbusMaster = NModbus.IModbusMaster;
using ModbusMaster = ThingsGateway.Foundation.Modbus.ModbusMaster;
using TcpClient = System.Net.Sockets.TcpClient;

namespace ThingsGateway.Foundation;

[Config(typeof(Config))]
[MemoryDiagnoser]
public class ModbusBenchmark : IDisposable
{
    private ModbusMaster thingsgatewaymodbus;
    private IModbusMaster nmodbus;
    private ModbusRtu modbusTcpNet;

    public ModbusBenchmark()
    {
        {
            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var clientChannel = clientConfig.GetSerialPortWithOption(new SerialPortOption() { PortName = "COM1" });
            thingsgatewaymodbus = new(clientChannel)
            {
                //modbus协议格式
                ModbusType = ModbusTypeEnum.ModbusRtu,
            };
            thingsgatewaymodbus.Channel.Connect();
        }

        var factory = new NModbus.ModbusFactory();
        var s = new System.IO.Ports.SerialPort("COM7");
        s.Open();
        nmodbus = factory.CreateRtuMaster(s);
        modbusTcpNet = new();
        modbusTcpNet.SerialPortInni("COM5");
        modbusTcpNet.Open();
        thingsgatewaymodbus.ReadAsync("40001", 100).GetFalseAwaitResult();
        nmodbus.ReadHoldingRegistersAsync(1, 0, 100).GetFalseAwaitResult();
        modbusTcpNet.ReadAsync("0", 100).GetFalseAwaitResult();
    }

    [Benchmark]
    public async Task ThingsGateway()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < Program.TaskNumberOfItems; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < Program.NumberOfItems; i++)
                {
                    var result = await thingsgatewaymodbus.ReadAsync("40001", 100);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.ToString());
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task NModbus4()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < Program.TaskNumberOfItems; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < Program.NumberOfItems; i++)
                {
                    var result = await nmodbus.ReadHoldingRegistersAsync(1, 0, 100);
                }
            }));
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task HslCommunication()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < Program.TaskNumberOfItems; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < Program.NumberOfItems; i++)
                {
                    var result = await modbusTcpNet.ReadAsync("0", 100);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.Message);
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        thingsgatewaymodbus?.Channel.SafeDispose();
        thingsgatewaymodbus?.SafeDispose();
        nmodbus?.SafeDispose();
        modbusTcpNet?.SafeDispose();
    }
}