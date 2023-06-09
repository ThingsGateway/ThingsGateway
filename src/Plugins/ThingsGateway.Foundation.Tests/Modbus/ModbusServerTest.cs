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

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class ModbusServerTest : IDisposable
    {
        private ModbusServer ModbusServer;
        private ITestOutputHelper _output;
        public ModbusServerTest(ITestOutputHelper output)
        {
            _output = output;
        }
        public void Dispose()
        {
            ModbusServer?.SafeDispose();
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
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);
        }


        private void ModbusServerClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(url) })
    .SetBufferLength(300);
            //载入配置
            var service = config.Container.Resolve<TcpService>();
            service.Setup(config);
            ModbusServer = new(service);
            ModbusServer.ConnectTimeOut = 5000;
            ModbusServer.Station = 1;
            ModbusServer.TimeOut = 5000;
            ModbusServer.MulStation = true;
            ModbusServer.Connect(CancellationToken.None);
        }
    }
}