#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

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

        #region 连接
        private void ModbusRtuClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetSerialProperty(new SerialProperty().Pase(url))
    .SetBufferLength(300);
            //载入配置
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
            //载入配置
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
            //载入配置
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
            //载入配置
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
            //载入配置
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
            ModbusRtu?.SafeDispose();
            ModbusRtuOverTcp?.SafeDispose();
            ModbusRtuOverUdp?.SafeDispose();
            ModbusTcp?.SafeDispose();
            ModbusUdp?.SafeDispose();
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
            var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, ModbusRtu.ThingsGatewayBitConverter);
            var test = await ModbusRtu.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);
        }
        [Theory(DisplayName = "127.0.0.1:502")]
        //[MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusRtuOverTcpReadTest(string address)
        {
            ModbusRtuOverTcpClient("127.0.0.1:502");
            await ModbusRtuOverTcp.ConnectAsync(CancellationToken.None);
            Stopwatch stopwatch = new Stopwatch();
            var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, ModbusRtuOverTcp.ThingsGatewayBitConverter);
            stopwatch.Start();
            var test = await ModbusRtuOverTcp.ReadAsync(address, 1);
            stopwatch.Stop();
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
        }

        [Theory(DisplayName = "127.0.0.1:512")]
        [MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusRtuOverUdpReadTest(string address)
        {
            ModbusRtuOverUdpClient("127.0.0.1:512");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, ModbusRtuOverUdp.ThingsGatewayBitConverter);
            var test = await ModbusRtuOverUdp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory(DisplayName = "127.0.0.1:513")]
        [MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusTcpReadTest1(string address)
        {
            ModbusTcpClient("127.0.0.1:513");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, ModbusTcp.ThingsGatewayBitConverter);
            var test = await ModbusTcp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory(DisplayName = "127.0.0.1:513")]
        [MemberData(nameof(RangeData), 0, 1, 10)]
        [MemberData(nameof(RangeData), 1, 1, 10)]
        public async Task ModbusTcpReadTest2(string address)
        {
            ModbusTcpClient("127.0.0.1:513");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, ModbusTcp.ThingsGatewayBitConverter);
            var test = await ModbusTcp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToBoolean(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory(DisplayName = "127.0.0.1:514")]
        [MemberData(nameof(RangeData), 3, 1, 10)]
        [MemberData(nameof(RangeData), 4, 1, 10)]
        public async Task ModbusUdpReadTest(string address)
        {
            ModbusUdpClient("127.0.0.1:514");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var byteConverter = ByteTransformHelpers.GetTransByAddress(ref address, ModbusUdp.ThingsGatewayBitConverter);
            var test = await ModbusUdp.ReadAsync(address, 1);
            Assert.True(test.IsSuccess, test.Message);
            var data = byteConverter.ToInt16(test.Content, 0);
            _output.WriteLine(data.ToJson());
            stopwatch.Stop();
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);
        }
    }
}