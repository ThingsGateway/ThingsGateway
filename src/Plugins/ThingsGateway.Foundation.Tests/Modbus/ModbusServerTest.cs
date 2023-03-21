using System.Diagnostics;

using ThingsGateway.Foundation.Adapter.Modbus;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class ModbusServerTest:IDisposable
    {
        private ModbusServer ModbusServer;
        private ITestOutputHelper _output;
        public ModbusServerTest(ITestOutputHelper output)
        {
            _output = output;
        }
        public void Dispose()
        {
            ModbusServer?.Dispose();
        }
        public static IEnumerable<object[]> RangeData(int func, int start, int end)
        {
            return Enumerable.Range(start, end).Select(i => new object[] { func + i.ToString().PadLeft(5, '0') });
        }

        [Theory(DisplayName = "127.0.0.1:5023")]
        [MemberData(nameof(ModbusTest.RangeData), 3, 1, 10)]
        [MemberData(nameof(ModbusTest.RangeData), 4, 1, 10)]
        public async Task ModbusServerReadTest(string address)
        {
            ModbusServerClient("127.0.0.1:5023");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, ModbusServer.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var test = await ModbusServer.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);
        }


        private void ModbusServerClient(string url)
        {
           var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(url) })
    .SetBufferLength(300);
            //‘ÿ»Î≈‰÷√
           var service = config.Container.Resolve<TcpService>();
            service.Setup(config);
            ModbusServer = new(service);
            ModbusServer.ConnectTimeOut = 5000;
            ModbusServer.Station = 1;
            ModbusServer.TimeOut = 5000;
            ModbusServer.MulStation = true;
            ModbusServer.Start();
        }
    }
}