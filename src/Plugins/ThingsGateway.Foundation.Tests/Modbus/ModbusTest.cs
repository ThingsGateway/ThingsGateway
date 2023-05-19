using System.Diagnostics;

using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Serial;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{



    public class ModbusTest : IDisposable
    {

        private ITestOutputHelper _output;
        private ModbusRtu ModbusRtu;
        private ModbusRtuOverTcp ModbusRtuOverTcp;
        private ModbusRtuOverUdp ModbusRtuOverUdp;
        private ModbusTcp ModbusTcp;
        private ModbusUdp ModbusUdp;

        #region ¡¨Ω”
        private void ModbusRtuClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetSerialProperty(new SerialProperty().Pase(url))
    .SetBufferLength(300);
            //‘ÿ»Î≈‰÷√
            var serialClient = config.Container.Resolve<SerialClient>();
            serialClient.Setup(config);
            ModbusRtu = new(serialClient);
            ModbusRtu.Station = 1;
            ModbusRtu.TimeOut = 5000;
        }
        private void ModbusRtuOverTcpClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(url))
    .SetBufferLength(300);
            //‘ÿ»Î≈‰÷√
            var client = config.Container.Resolve<TGTcpClient>();
            client.Setup(config);
            ModbusRtuOverTcp = new(client);
            ModbusRtuOverTcp.ConnectTimeOut = 5000;
            ModbusRtuOverTcp.Station = 1;
            ModbusRtuOverTcp.TimeOut = 5000;
        }
        private void ModbusRtuOverUdpClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(url)).SetBindIPHost(new IPHost(0))
                .SetBufferLength(3000);
            //‘ÿ»Î≈‰÷√
            var client1 = config.BuildWithUdpSession<TGUdpSession>();
            ModbusRtuOverUdp = new(client1);
            ModbusRtuOverUdp.ConnectTimeOut = 5000;
            ModbusRtuOverUdp.Station = 1;
            ModbusRtuOverUdp.TimeOut = 5000;
        }
        private void ModbusTcpClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(url))
    .SetBufferLength(300);
            //‘ÿ»Î≈‰÷√
            var client = config.Container.Resolve<TGTcpClient>();
            client.Setup(config);
            ModbusTcp = new(client);
            ModbusTcp.ConnectTimeOut = 5000;
            ModbusTcp.Station = 1;
            ModbusTcp.TimeOut = 5000;
        }
        private void ModbusUdpClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(url)).SetBindIPHost(new IPHost(0))
                .SetBufferLength(3000);
            //‘ÿ»Î≈‰÷√
            var client1 = config.BuildWithUdpSession<TGUdpSession>();
            ModbusUdp = new(client1);
            ModbusUdp.ConnectTimeOut = 5000;
            ModbusUdp.Station = 1;
            ModbusUdp.TimeOut = 5000;
        }

        #endregion


        public ModbusTest(ITestOutputHelper output)
        {
            _output = output;
        }
        public void Dispose()
        {
            ModbusRtu?.Dispose();
            ModbusRtuOverTcp?.Dispose();
            ModbusRtuOverUdp?.Dispose();
            ModbusTcp?.Dispose();
            ModbusUdp?.Dispose();
        }


        public static IEnumerable<object[]> RangeData(int func, int start, int end)
        {
            return Enumerable.Range(start, end).Select(i => new object[] { func + i.ToString().PadLeft(5, '0') });
        }

        [Theory(DisplayName = "COM1-9600-8-0-1")]
        [MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusRtuReadTest(string address)
        {
            ModbusRtuClient("COM1-9600-8-0-1");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, ModbusRtu.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var test = await ModbusRtu.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);
        }
        [Theory(DisplayName = "127.0.0.1:503")]
        //[MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusRtuOverTcpReadTest(string address)
        {
            ModbusRtuOverTcpClient("127.0.0.1:503");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, ModbusRtuOverTcp.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var test = await ModbusRtuOverTcp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory(DisplayName = "127.0.0.1:512")]
        [MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusRtuOverUdpReadTest(string address)
        {
            ModbusRtuOverUdpClient("127.0.0.1:512");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, ModbusRtuOverUdp.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var test = await ModbusRtuOverUdp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory(DisplayName = "127.0.0.1:513")]
        [MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusTcpReadTest1(string address)
        {
            ModbusTcpClient("127.0.0.1:513");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, ModbusTcp.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var test = await ModbusTcp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory(DisplayName = "127.0.0.1:513")]
        [MemberData(nameof(RangeData), 0, 1, 10)]
        [MemberData(nameof(RangeData), 1, 1, 10)]
        public async Task ModbusTcpReadTest2(string address)
        {
            ModbusTcpClient("127.0.0.1:513");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, ModbusTcp.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var test = await ModbusTcp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToBoolean(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory(DisplayName = "127.0.0.1:514")]
        [MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusUdpReadTest(string address)
        {
            ModbusUdpClient("127.0.0.1:514");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, ModbusUdp.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
            var test = await ModbusUdp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);
        }
    }
}