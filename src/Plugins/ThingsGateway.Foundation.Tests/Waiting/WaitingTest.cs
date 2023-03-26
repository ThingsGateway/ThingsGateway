using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class WaitingTest
    {
        private ITestOutputHelper _output;
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
            var data = DateTime.Now;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < num; i++)
            {
                tasks.Add(TestWaiting());
            }
            _output.WriteLine(data.ToString("yyyy-MM-dd hh:mm:ss fff"));
            _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff"));
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
                       IWaitingClient<TcpClient> waitClient = list[i];
                       var returnData = await waitClient.SendThenResponseAsync(new byte[] { 1, 2, 3, 4 }, 0, 4, 5000);
                       _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "----" + i);
                   }
                   catch (Exception ex)
                   {
                       _output.WriteLine(ex.Message);

                   }
               }
               );

        }

        List<IWaitingClient<TcpClient>> list = new();
        private async Task NewMethod()
        {
            TcpClient m_tcpClient = new TcpClient();
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:503"));

            //载入配置
            m_tcpClient.Setup(config);

            //调用GetWaitingClient获取到IWaitingClient的对象。
            IWaitingClient<TcpClient> waitClient = m_tcpClient.GetTGWaitingClient(new WaitingOptions()
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