#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Diagnostics;

using ThingsGateway.Foundation.Adapter.OPCDA;
using ThingsGateway.Foundation.Adapter.OPCDA.Da;
using ThingsGateway.Foundation.Adapter.OPCDA.Discovery;
using ThingsGateway.Foundation.Extension.Json;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class OPCDATest : IDisposable
    {
        private ITestOutputHelper _output;
        private OPCDAClient _opc;
        public OPCDATest(ITestOutputHelper output)
        {
            _output = output;
        }

        public void Dispose()
        {
            _opc?.SafeDispose();
        }

        [Theory]
        [InlineData("Kepware.KEPServerEX.V6")] //kep
        [InlineData("xxxxx")] //填错
        public void OpcDiscoveryTest(string address)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var info = OpcDiscovery.GetOpcServer(address, "");
            _output.WriteLine(info?.ToJson().FormatJson());
            stopwatch.Stop();
            _output.WriteLine(address + "耗时：" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory]
        [InlineData("test1.DWord2")] //kep
        public async Task OpcSubscribeTest(string address)
        {
            _opc = new OPCDAClient(new TGEasyLogger(Log_Out));
            _opc.Init(new OPCNode() { OPCName = "Kepware.KEPServerEX.V6", CheckRate = 5000, ActiveSubscribe = true });
            _opc.SetTags(new List<string> { address });
            _opc.DataChangedHandler += Info_DataChangedHandler;
            _opc.Connect();
            Assert.True(_opc.IsConnected);
            var result = _opc.Write(address, new Random().Next(100).ToString());
            Assert.True(result.IsSuccess, result.Message);

            await Task.Delay(2000);
            _opc.Disconnect();
        }

        protected void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
        {
            _output.WriteLine(arg1.ToString() + arg2?.ToJson() + arg3?.ToJson() + arg4?.ToJson());
        }

        private void Info_DataChangedHandler(List<ItemReadResult> values)
        {
            _output.WriteLine(values?.ToJson());
        }



    }
}