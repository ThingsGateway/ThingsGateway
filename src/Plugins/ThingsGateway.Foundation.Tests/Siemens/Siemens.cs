using System.Diagnostics;

using ThingsGateway.Foundation.Adapter.Siemens;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class Siemens
    {
        private ITestOutputHelper _output;
        private SiemensS7PLC SiemensS7PLC;
        public Siemens(ITestOutputHelper output)
        {
            _output = output;
        }
        public static IEnumerable<object[]> RangeData(string func, int start, int end)
        {
            if (func.ToUpper() == "DB")
            {
                return Enumerable.Range(start, end).Select(i => new object[] { $"{func}{1}.{i}" });
            }
            else
            {
                return Enumerable.Range(start, end).Select(i => new object[] { $"{func}{i}" });
            }
        }
        [Theory(DisplayName = "127.0.0.1:102")]
        [MemberData(nameof(RangeData), "M", 1, 10)]
        [MemberData(nameof(RangeData), "I", 1, 10)]
        [MemberData(nameof(RangeData), "Q", 1, 10)]
        [MemberData(nameof(RangeData), "DB", 1, 10)]
        [MemberData(nameof(RangeData), "C", 1, 10)]
        [MemberData(nameof(RangeData), "T", 1, 10)]
        public async Task SiemensS7PLCReadTest(string address)
        {
            SiemensS7PLCClient("127.0.0.1:102");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, SiemensS7PLC.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var test = await SiemensS7PLC.ReadAsync(address, 2);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);

        }



        private void SiemensS7PLCClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(url))
    .SetBufferLength(300);
            //‘ÿ»Î≈‰÷√
            var client = config.Container.Resolve<TcpClient>();
            client.Setup(config);
            SiemensS7PLC = new(client, SiemensEnum.S1500);
            SiemensS7PLC.ConnectTimeOut = 5000;
            SiemensS7PLC.TimeOut = 5000;
        }


    }
}