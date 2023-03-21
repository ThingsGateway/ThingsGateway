using System.Diagnostics;

using ThingsGateway.Foundation.Adapter.OPCDA;
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
            _opc?.Dispose();
        }

        [Theory]
        [InlineData("Kepware.KEPServerEX.V6")] //kep
        [InlineData("xxxxx")] //Ìî´í
        public void OpcDiscoveryTest(string address)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var info = OpcDaClient.Discovery.OpcDiscovery.GetOpcServer(address, "");
            _output.WriteLine(info?.ToJson().FormatJson());
            stopwatch.Stop();
            _output.WriteLine(address + "ºÄÊ±£º" + stopwatch.Elapsed.TotalSeconds);
        }

        [Theory]
        [InlineData("test1.DWord2")] //kep
        public async Task OpcSubscribeTest(string address)
        {
            _opc = new OPCDAClient(new EasyLogger(Log_Out));
            _opc.Init(new OPCNode() { OPCName = "Kepware.KEPServerEX.V6", CheckRate = 5000, ActiveSubscribe = true });
            _opc.SetTags(new List<string> { address });
            _opc.DataChangedHandler += Info_DataChangedHandler;
            _opc.Connect();
            Assert.True(_opc.IsConnected);
            var result = _opc.Write(address, new Random().Next(100));
            Assert.True(result.IsSuccess, result.Message);

            await Task.Delay(2000);
            _opc.Disconnect();
        }

        protected void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
        {
            _output.WriteLine(arg1.ToString() + arg2?.ToJson() + arg3?.ToJson() + arg4?.ToJson());
        }

        private void Info_DataChangedHandler(List<OpcDaClient.Da.ItemReadResult> values)
        {
            _output.WriteLine(values?.ToJson());
        }



    }
}