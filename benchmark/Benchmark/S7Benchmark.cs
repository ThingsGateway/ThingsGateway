////------------------------------------------------------------------------------
////  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
////  此代码版权（除特别声明外的代码）归作者本人Diego所有
////  源代码使用协议遵循本仓库的开源协议及附加协议
////  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
////  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
////  使用文档：https://kimdiego2098.github.io/
////  QQ群：605534569
////------------------------------------------------------------------------------

//using BenchmarkConsoleApp;

//using BenchmarkDotNet.Attributes;

//using ThingsGateway.Foundation.Modbus;
//using ThingsGateway.Foundation.SiemensS7;

//using TouchSocket.Core;
//using TouchSocket.Modbus;
//using TouchSocket.Sockets;

//namespace ThingsGateway.Foundation;

//[MemoryDiagnoser]
//[ThreadingDiagnoser]
//public class S7Benchmark : IDisposable
//{
//    private SiemensS7Master siemensS7;

//    public S7Benchmark()
//    {
//        {
//            var clientConfig = new TouchSocket.Core.TouchSocketConfig();
//            var clientChannel = clientConfig.GetTcpClientWithIPHost("127.0.0.1:102");
//            siemensS7 = new(clientChannel)
//            {
//                //modbus协议格式
//                SiemensS7Type = SiemensTypeEnum.S1500
//            };
//            siemensS7.Channel.Connect();
//        }
//    }

//    [Benchmark]
//    public async Task ThingsGateway()
//    {
//        List<Task> tasks = new List<Task>();
//        for (int i = 0; i < Program.TaskNumberOfItems; i++)
//        {
//            tasks.Add(Task.Run(async () =>
//            {
//                for (int i = 0; i < Program.NumberOfItems; i++)
//                {
//                    var result = await siemensS7.ReadAsync("M1", 1000);
//                    if (!result.IsSuccess)
//                    {
//                        throw new Exception(result.ToString());
//                    }
//                }
//            }));
//        }
//        await Task.WhenAll(tasks);
//    }

//    public void Dispose()
//    {
//        siemensS7.Channel.SafeDispose();
//        siemensS7.SafeDispose();
//    }
//}