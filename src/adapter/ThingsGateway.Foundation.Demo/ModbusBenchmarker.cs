//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using BenchmarkDotNet.Attributes;

using HslCommunication.ModBus;

using NModbus;

using System.Net.Sockets;

using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;
using TouchSocket.Modbus;
using TouchSocket.Sockets;

using IModbusMaster = NModbus.IModbusMaster;
using TcpClient = System.Net.Sockets.TcpClient;

namespace ThingsGateway.Foundation;

[MemoryDiagnoser, RankColumn]
public class ModbusBenchmarker
{
    private int NumberOfItems = 1000;
    private ModbusMaster thingsgatewaymodbus;
    private TouchSocket.Modbus.ModbusTcpMaster touchsocketmodbus;
    private IModbusMaster nmodbus;
    private ModbusTcpNet hslmodbus;

    public ModbusBenchmarker()
    {
        {
            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var clientChannel = clientConfig.GetTcpClientWithIPHost("127.0.0.1:502");
            thingsgatewaymodbus = new(clientChannel)
            {
                //modbus协议格式
                ModbusType = Modbus.ModbusTypeEnum.ModbusTcp,
            };
            thingsgatewaymodbus.Channel.Connect();
        }
        {
            var clientConfig = new TouchSocket.Core.TouchSocketConfig().SetRemoteIPHost("127.0.0.1:502");
            touchsocketmodbus = new()
            {
            };
            touchsocketmodbus.Setup(clientConfig);
            touchsocketmodbus.Connect();
        }

        TcpClient client = new TcpClient("127.0.0.1", 502);
        var factory = new ModbusFactory();
        nmodbus = factory.CreateMaster(client);
        hslmodbus = new("127.0.0.1");
        hslmodbus.ConnectServer();
    }

    [Benchmark]
    public async Task ThingsGateway()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < NumberOfItems; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = await thingsgatewaymodbus.ReadAsync("40001", 100);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result);
                }
            }));
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task Touchsocket()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < NumberOfItems; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = await touchsocketmodbus.ReadHoldingRegistersAsync(0, 100);
                if (result.ErrorCode != ModbusErrorCode.Success)
                {
                    Console.WriteLine(result);
                }
            }));
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task NModbus4()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < NumberOfItems; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var result = await nmodbus.ReadHoldingRegistersAsync(1, 0, 100);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }));
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task HslCommunication()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < NumberOfItems; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = await hslmodbus.ReadAsync("0", 100);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result);
                }
            }));
        }
        await Task.WhenAll(tasks);
    }
}
