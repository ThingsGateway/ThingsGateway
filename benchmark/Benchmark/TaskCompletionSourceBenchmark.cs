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

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

[MemoryDiagnoser]
[ThreadingDiagnoser]
public class TaskCompletionSourceBenchmark
{
    [Benchmark]
    public async Task AsyncAutoResetEvent()
    {
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                AsyncAutoResetEvent asyncAutoResetEvent = new(true);
                Task task = Task.Run(async () =>
                {
                    {
                        await asyncAutoResetEvent.WaitOneAsync(); // Simulating completion
                    }
                });

                asyncAutoResetEvent.Set(); // Simulating completion
                await task;
            }));
        }
        await Task.WhenAll(tasks);
    }

    [Benchmark]
    public async Task EasyLock()
    {
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(async () =>
        {
            EasyLock easyLock = new(true);
            Task task = Task.Run(async () =>
            {
                {
                    await easyLock.WaitAsync(); // Simulating completion
                }
            });

            easyLock.Release(); // Simulating completion
            await task;
        }));
        }
        await Task.WhenAll(tasks);
    }
}