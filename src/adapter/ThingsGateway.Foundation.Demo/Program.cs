//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Diagnostics;

namespace ThingsGateway.Foundation;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("开始测试前，请先启动ModbusSlave，建议使用本项目自带的ThingsGateway.Debug.Photino软件开启，或者ModbusSim");
        Console.WriteLine("输入任意键，开始多客户端(1000),多线程(100)并发读取(10次)测试");
        Console.ReadKey();

        await ThingsGateway();
        await NModbus4();
        await TouchSocket();
        await HslCommunication();

        Console.ReadLine();
        //await ModbusMasterTest.Test();
        //S7MatserTest.Test();
    }

    private static async Task ThingsGateway()
    {
        Console.WriteLine("输入任意键，开始 ThingsGateway 测试");
        Console.ReadKey();
        Console.WriteLine(" ThingsGateway 测试已开始");
        Stopwatch stopwatch = new();
        stopwatch.Start();
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                ModbusCompare modbusBenchmarker = new();
                await modbusBenchmarker.ThingsGateway();
                modbusBenchmarker.Dispose();
            }));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        Console.WriteLine($" ThingsGateway 耗时：{stopwatch.ElapsedMilliseconds}");
    }

    private static async Task TouchSocket()
    {
        Console.WriteLine("输入任意键，开始 TouchSocket 测试");
        Console.ReadKey();
        Console.WriteLine(" TouchSocket 测试已开始");
        Stopwatch stopwatch = new();
        stopwatch.Start();
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                ModbusCompare modbusBenchmarker = new();
                await modbusBenchmarker.TouchSocket();
                modbusBenchmarker.Dispose();
            }));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        Console.WriteLine($" TouchSocket 耗时：{stopwatch.ElapsedMilliseconds}");
    }

    private static async Task NModbus4()
    {
        Console.WriteLine("输入任意键，开始 NModbus4 测试");
        Console.ReadKey();
        Console.WriteLine(" NModbus4 测试已开始");
        Stopwatch stopwatch = new();
        stopwatch.Start();
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                ModbusCompare modbusBenchmarker = new();
                await modbusBenchmarker.NModbus4();
                modbusBenchmarker.Dispose();
            }));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        Console.WriteLine($" NModbus4 耗时：{stopwatch.ElapsedMilliseconds}");
    }

    private static async Task HslCommunication()
    {
        Console.WriteLine("输入任意键，开始 HslCommunication 测试");
        Console.ReadKey();
        Console.WriteLine(" HslCommunication 测试已开始");
        Stopwatch stopwatch = new();
        stopwatch.Start();
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                ModbusCompare modbusBenchmarker = new();
                await modbusBenchmarker.HslCommunication();
                modbusBenchmarker.Dispose();
            }));
        }
        await Task.WhenAll(tasks);
        stopwatch.Stop();
        Console.WriteLine($" HslCommunication 耗时：{stopwatch.ElapsedMilliseconds}");
    }
}
