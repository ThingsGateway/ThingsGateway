using System.Diagnostics;

using ThingsGateway.Foundation.Adapter.Modbus;

using Xunit;

namespace ThingsGateway.Foundation.Tests
{
    public class ModbusServerTest
    {
        private TouchSocketConfig config;
        TcpService service;
        private ModbusServer TcpService;

        [Fact]
        public async Task TcpReadTest()
        {
            ModbusServer();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            TcpService.MulStation = true;
            for (int i = 0; i < 10; i++)
            {
                string address = $"0{i + 1};";
                await TcpService.WriteAsync(address, true);
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, TcpService.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var test = await TcpService.ReadAsync(address, 1);
                var data = byteConverter.ToBoolean(test.Content, 0);
                Console.WriteLine(data.ToJson());
            }
            for (int i = 10; i < 20; i++)
            {
                string address = $"4{i + 1};";
                await TcpService.WriteAsync(address, 88);
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, TcpService.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var test = await TcpService.ReadAsync(address, 2);
                var data = byteConverter.ToInt32(test.Content, 0);
                Console.WriteLine(data.ToJson());
            }
            stopwatch.Stop();
            Console.WriteLine("×ÜºÄÊ±£º" + stopwatch.Elapsed.TotalSeconds);
            while (true)
            {
                await Task.Delay(10000);
            }
        }

        private void ModbusServer()
        {
            config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost("127.0.0.1:5023") })
    .SetBufferLength(300);
            //ÔØÈëÅäÖÃ
            service = config.Container.Resolve<TcpService>();
            service.Setup(config);
            TcpService = new(service);
            TcpService.ConnectTimeOut = 5000;
            TcpService.Station = 1;
            TcpService.TimeOut = 5000;
            TcpService.Start();
        }
    }
}