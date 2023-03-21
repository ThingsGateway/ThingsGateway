using System.Diagnostics;

using ThingsGateway.Foundation.Adapter.Siemens;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class Siemens
    {
        private ITestOutputHelper _output;
        TcpClient client;
        private TouchSocketConfig config;
        private SiemensS7PLC TcpClient;
        public Siemens(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public async Task TcpReadTest()
        {
            S7TcpClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 20; i++)
            {
                string address = $"M1.{i + 1};";
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, TcpClient.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var test = await TcpClient.ReadAsync(address, 2);
                if (test .IsSuccess)
                {
                    var data = byteConverter.ToBoolean(test.Content, 0);
                    _output.WriteLine(data.ToJson());
                }
                else
                {
                    _output.WriteLine(test.ToJson());
                }
            }

            stopwatch.Stop();
            _output.WriteLine("×ÜºÄÊ±£º" + stopwatch.Elapsed.TotalSeconds);

        }



        private void S7TcpClient()
        {
            config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:102"))
    .SetBufferLength(300);
            //ÔØÈëÅäÖÃ
            client = config.Container.Resolve<TcpClient>();
            client.Setup(config);
            TcpClient = new(client,SiemensEnum.S1500);
            TcpClient.ConnectTimeOut = 5000;
            TcpClient.TimeOut = 5000;
        }


    }
}