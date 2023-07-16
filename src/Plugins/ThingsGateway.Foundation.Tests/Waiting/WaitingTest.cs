#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    /// <summary>
    /// 测试超时等待
    /// </summary>
    public class WaitingTest
    {
        private ITestOutputHelper _output;
        /// <summary>
        /// 测试超时等待
        /// </summary>
        /// <param name="output"></param>
        public WaitingTest(ITestOutputHelper output)
        {
            _output = output;
        }
        [Theory(DisplayName = "WaitingTest500")]
        [InlineData(500)] //填错
        async Task Main(int num)
        {
            ThreadPool.SetMaxThreads(100000, 100000);
            for (int i = 0; i < num; i++)
            {
                await Task.Factory.StartNew(async () =>
                {
                    await NewMethod();
                }
               );
            }
            var data = DateTime.UtcNow;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < num; i++)
            {
                tasks.Add(TestWaiting());
            }
            _output.WriteLine(data.ToString("yyyy-MM-dd hh:mm:ss fff"));
            _output.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss fff"));
            while (true)
            {

            }
        }
        int dd = 0;
        private Task TestWaiting()
        {
            ////然后使用SendThenReturn。
            return Task.Factory.StartNew(async () =>
               {
                   try
                   {
                       int i = Interlocked.Increment(ref dd);
                       IWaitingClient<TGTcpClient> waitClient = list[i];
                       var returnData = await waitClient.SendThenResponseAsync(new byte[] { 1, 2, 3, 4 }, 0, 4, 5000);
                       _output.WriteLine(DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss fff") + "----" + i);
                   }
                   catch (Exception ex)
                   {
                       _output.WriteLine(ex.Message);

                   }
               }
               );

        }

        List<IWaitingClient<TGTcpClient>> list = new();
        private async Task NewMethod()
        {
            TGTcpClient m_tcpClient = new TGTcpClient();
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:503"));

            //载入配置
            m_tcpClient.Setup(config);

            //调用GetWaitingClient获取到IWaitingClient的对象。
            IWaitingClient<TGTcpClient> waitClient = m_tcpClient.GetTGWaitingClient(new WaitingOptions()
            {
                AdapterFilter = AdapterFilter.AllAdapter,//表示发送和接收的数据都会经过适配器
                BreakTrigger = true,//表示当连接断开时，会立即触发
                ThrowBreakException = true//表示当连接断开时，是否触发异常
            });
            try
            {
                await m_tcpClient.ConnectAsync(5000);

            }
            catch (Exception ex)
            {
                _output.WriteLine($"{ex.Message}");
            }
            list.Add(waitClient);
        }

    }
}