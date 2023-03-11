using ThingsGateway.Foundation.Adapter.OPCDA;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class OPCDA
    {
        private ITestOutputHelper _output;
        public OPCDA(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void OpcDiscoveryTest()
        {
            var info = OpcDaClient.Discovery.OpcDiscovery.GetOpcServer("Kepware.KEPServerEX.V6", "");
            Assert.True(info.IsSuccess);
        }
        [Fact]
        public void OpcTest()
        {

            var info = new OPCDAClient(new EasyLogger(Log_Out));
            info.Init(new OPCNode() { OPCName = "Kepware.KEPServerEX.V6", CheckRate = 50 });
            info.SetTags(new List<string> { "test.40001" });
            info.DataChangedHandler += Info_DataChangedHandler;
            info.Connect();
            var data = info.GetBrowse();

            Thread.Sleep(15000);
            info.Disconnect();

            Assert.True(!info.IsConnected);
        }

        protected void Log_Out(LogType arg1, object arg2, string arg3, Exception arg4)
        {
            _output.WriteLine(arg1.ToJson() + arg2?.ToJson() + arg3?.ToJson() + arg4?.ToJson());
        }

        private void Info_DataChangedHandler(List<OpcDaClient.Da.ItemReadResult> values)
        {
            _output.WriteLine(values?.ToJson());
        }
    }
}