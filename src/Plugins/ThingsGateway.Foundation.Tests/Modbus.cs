using System.Diagnostics;

using ThingsGateway.Foundation.Adapter.Modbus;
using ThingsGateway.Foundation.Serial;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class Modbus
    {
        TcpClient client;
        SerialClient serialClient;
        private TouchSocketConfig config;
        private ModbusRtuOverTcp RtuTcpClient;
        private ModbusRtu RtuClient;
        private ModbusRtuOverUdp RtuUdpClient;
        private ModbusTcp TcpClient;
        private ModbusUdp UdpClient;
        private ITestOutputHelper _output;
        public Modbus(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public async Task RtuTcpReadTest()
        {
            ModbusRtuTcpClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 10; i++)
            {
                string address = $"4{i * 2 + 1};DATA=ABCD;";
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, RtuTcpClient.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var test = await RtuTcpClient.ReadAsync(address, 1);
                var data = byteConverter.ToInt16(test.Content, 0);
                _output.WriteLine(data.ToJson());
            }

            for (int i = 100; i < 110; i++)
            {
                string address = $"4{i * 2 + 1};TEXT=UTF8;LEN=4";
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, RtuTcpClient.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var test = await RtuTcpClient.ReadAsync(address, length / RtuTcpClient.RegisterByteLength);
                var data = byteConverter.ToString(test.Content, 0, length);
                _output.WriteLine(data.ToJson());
            }

            stopwatch.Stop();
            _output.WriteLine("◊‹∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);

        }
        [Fact]
        public async Task RtuReadTest()
        {
            ModbusRtuClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 10; i++)
            {
                string address = $"4{i * 2 + 1};DATA=ABCD;";
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, RtuClient.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var test = await RtuClient.ReadAsync(address, 1);
                var data = byteConverter.ToInt16(test.Content, 0);
                _output.WriteLine(data.ToJson());
            }

            for (int i = 100; i < 110; i++)
            {
                string address = $"4{i * 2 + 1};TEXT=UTF8;LEN=4";
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, RtuClient.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var test = await RtuClient.ReadAsync(address, length / RtuClient.RegisterByteLength);
                var data = byteConverter.ToString(test.Content, 0, length);
                _output.WriteLine(data.ToJson());
            }

            stopwatch.Stop();
            _output.WriteLine("◊‹∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);

        }
        [Fact]
        public async Task TcpReadTest()
        {
            ModbusTcpClient();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < 10; i++)
            {
                string address = $"0{i + 1};";
                var byteConverter = ByteConverterHelper.GetTransByAddress(ref address, TcpClient.ThingsGatewayBitConverter, out int length, out BcdFormat bcdFormat);
                var test = await TcpClient.ReadAsync(address, 1);
                var data = byteConverter.ToBoolean(test.Content, 0);
                _output.WriteLine(data.ToJson());
            }

            stopwatch.Stop();
            _output.WriteLine("◊‹∫ƒ ±£∫" + stopwatch.Elapsed.TotalSeconds);

        }

        private void ModbusRtuTcpClient()
        {
            config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:5021"))
    .SetBufferLength(300);
            //‘ÿ»Î≈‰÷√
            client = config.Container.Resolve<TcpClient>();
            client.Setup(config);
            RtuTcpClient = new(client);
            RtuTcpClient.ConnectTimeOut = 5000;
            RtuTcpClient.Station = 1;
            RtuTcpClient.TimeOut = 5000;
        }

        private void ModbusRtuUdpClient()
        {
            config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:5022")).SetBindIPHost(new IPHost(0))
                .SetBufferLength(3000);
            //‘ÿ»Î≈‰÷√
            var client1 = config.BuildWithUdpSession<UdpSession>();
            RtuUdpClient = new(client1);
            RtuUdpClient.ConnectTimeOut = 5000;
            RtuUdpClient.Station = 1;
            RtuUdpClient.TimeOut = 5000;
        }

        private void ModbusTcpClient()
        {
            config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:5023"))
    .SetBufferLength(300);
            //‘ÿ»Î≈‰÷√
            client = config.Container.Resolve<TcpClient>();
            client.Setup(config);
            TcpClient = new(client);
            TcpClient.ConnectTimeOut = 5000;
            TcpClient.Station = 1;
            TcpClient.TimeOut = 5000;
        }
        private void ModbusUdpClient()
        {
            config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost("127.0.0.1:5024")).SetBindIPHost(new IPHost(0))
                .SetBufferLength(3000);
            //‘ÿ»Î≈‰÷√
            var client1 = config.BuildWithUdpSession<UdpSession>();
            UdpClient = new(client1);
            UdpClient.ConnectTimeOut = 5000;
            UdpClient.Station = 1;
            UdpClient.TimeOut = 5000;
        }

        private void ModbusRtuClient()
        {
            config = new TouchSocketConfig();
            config.SetSerialProperty(new() { PortName="COM6"})
    .SetBufferLength(300);
            //‘ÿ»Î≈‰÷√
            serialClient = config.Container.Resolve<SerialClient>();
            serialClient.Setup(config);
            RtuClient = new(serialClient);
            RtuClient.Station = 1;
            RtuClient.TimeOut = 5000;
        }

    }
}