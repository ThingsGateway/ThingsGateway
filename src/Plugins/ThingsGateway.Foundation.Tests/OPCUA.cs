using Opc.Ua;
using Opc.Ua.Client;

using ThingsGateway.Foundation.Adapter.OPCUA;
using ThingsGateway.Foundation.Extension.Json;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class OPCUA
    {
        private ITestOutputHelper _output;
        public OPCUA(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task OpcUaTest()
        {
            var info = new OPCUAClient();
            info.UserIdentity = new UserIdentity("Administrator", "111111");
            info.OPCNode = new() { OPCUrl = "opc.tcp://127.0.0.1:49320" };
            var MonitorNodeTags = new string[]
              {
        "ns=2;s=通道 4.设备 1.40001",
        "ns=2;s=通道 4.设备 1.40002",
       "ns=2;s=通道 4.设备 1.40003"
              };
            info.SetTags(MonitorNodeTags.ToList());
            info.OpcStatusChange += Info_OpcStatusChange;
            info.DataChangedHandler = DataReceived;
            await info.ConnectServer();

            var value = await info.ReadNodeAsync(new NodeId("ns=2;s=通道 4.设备 1.40001"));
            // 多个节点的订阅

            Thread.Sleep(2000);
            info.Disconnect();
        }

        private void DataReceived(List<(MonitoredItem, MonitoredItemNotification)> obj)
        {
            _output.WriteLine(obj?.ToJson().FormatJson());
        }

        private void Info_OpcStatusChange(object sender, OPCUAStatusEventArgs e)
        {
            if (e.Error)
                _output.WriteLine(e?.ToJson());
        }


    }
}