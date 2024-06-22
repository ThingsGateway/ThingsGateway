//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

using BenchmarkDotNet.Running;

using System.Diagnostics;

using ThingsGateway.Foundation;

namespace BenchmarkConsoleApp
{
    internal class Program
    {
        private static int ClientCount = 10;
        public static int TaskNumberOfItems = 10;
        public static int NumberOfItems = 1000;

        private static async Task Main(string[] args)
        {
            Console.WriteLine("开始测试前，请先启动ModbusSlave，建议使用本项目自带的ThingsGateway.Debug.Photino软件开启");
            //Console.WriteLine("输入客户端数量");
            //ClientCount = Console.ReadLine().ToInt(1);
            //Console.WriteLine("输入并发任务数量");
            //TaskNumberOfItems = Console.ReadLine().ToInt(1);
            //Console.WriteLine("输入每个任务执行读取次数");
            //NumberOfItems = Console.ReadLine().ToInt(1);
            Console.WriteLine($"多客户端({ClientCount}),多线程({TaskNumberOfItems})并发读取({NumberOfItems})测试，共{ClientCount * TaskNumberOfItems * NumberOfItems}次");
            var consoleAction = new ConsoleAction();

            consoleAction.Add("0", "基准测试", () =>
            {
                BenchmarkRunner.Run(typeof(Program).Assembly);
            });
            consoleAction.Add("1", "单独测试 ThingsGateway", async () => { await ThingsGateway(); });
            consoleAction.Add("2", "单独测试 NModbus4", async () => { await NModbus4(); });
            consoleAction.Add("3", "单独测试 TouchSocket", async () => { await TouchSocket(); });

            consoleAction.ShowAll();

            while (true)
            {
                await consoleAction.RunAsync(Console.ReadLine());
            }
        }

        private static async Task ThingsGateway()
        {
            try
            {
                Console.WriteLine(" ThingsGateway 测试已开始");
                Stopwatch stopwatch = new();
                stopwatch.Start();
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < ClientCount; i++)
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
            catch (Exception ex)
            {
                Console.WriteLine($" ThingsGateway 发生错误：{ex.Message}");
            }
        }

        private static async Task TouchSocket()
        {
            try
            {
                Console.WriteLine(" TouchSocket 测试已开始");
                Stopwatch stopwatch = new();
                stopwatch.Start();
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < ClientCount; i++)
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
            catch (Exception ex)
            {
                Console.WriteLine($" TouchSocket 发生错误：{ex.Message}");
            }
        }

        private static async Task NModbus4()
        {
            try
            {
                Console.WriteLine(" NModbus4 测试已开始");
                Stopwatch stopwatch = new();
                stopwatch.Start();
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < ClientCount; i++)
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
            catch (Exception ex)
            {
                Console.WriteLine($" NModbus4 发生错误：{ex.Message}");
            }
        }
    }
}