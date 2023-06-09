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
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);

        }



        private void SiemensS7PLCClient(string url)
        {
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(new IPHost(url))
    .SetBufferLength(300);
            //载入配置
            var client = config.Container.Resolve<TGTcpClient>();
            client.Setup(config);
            SiemensS7PLC = new(client, SiemensEnum.S1500);
            SiemensS7PLC.ConnectTimeOut = 5000;
            SiemensS7PLC.TimeOut = 5000;
        }


    }
}