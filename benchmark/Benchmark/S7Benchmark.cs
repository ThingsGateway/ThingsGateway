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

using HslCommunication.Profinet.Siemens;

using S7.Net;

using ThingsGateway.Foundation.SiemensS7;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

[Config(typeof(Config))]
[MemoryDiagnoser]
public class S7Benchmark : IDisposable
{
    private SiemensS7Master siemensS7;

    private Plc plc;
    private SiemensS7Net siemensS7Net;

    public S7Benchmark()

    {
        {
            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
            var clientChannel = clientConfig.GetTcpClientWithIPHost("127.0.0.1:102");
            siemensS7 = new(clientChannel)
            {
                //modbus协议格式
                SiemensS7Type = SiemensTypeEnum.S1500
            };
            siemensS7.Channel.Connect();

            siemensS7Net = new(SiemensPLCS.S1500, "127.0.0.1");
            siemensS7Net.ConnectServer();

            plc = new Plc(CpuType.S7300, "127.0.0.1", 102, 0, 0);
            plc.Open();//打开plc连接
            siemensS7Net.ReadAsync("M0", 100).GetFalseAwaitResult();
            plc.ReadAsync(DataType.Memory, 1, 0, VarType.Byte, 100).GetFalseAwaitResult();
            siemensS7.ReadAsync("M1", 100).GetFalseAwaitResult();
        }
    }

    [Benchmark]
    public async Task S7netplus()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < Program.TaskNumberOfItems; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < Program.NumberOfItems; i++)
                {
                    var result = await plc.ReadAsync(DataType.Memory, 1, 0, VarType.Byte, 100);
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
                    var result = await siemensS7Net.ReadAsync("M0", 100);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.Message);
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
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
                    var result = await siemensS7.ReadAsync("M1", 100);
                    if (!result.IsSuccess)
                    {
                        throw new Exception(result.ToString());
                    }
                }
            }));
        }
        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        plc.SafeDispose();
        siemensS7Net.SafeDispose();
        siemensS7.Channel.SafeDispose();
        siemensS7.SafeDispose();
    }
}