#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/dotnetchina/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using Newtonsoft.Json.Linq;

using Opc.Ua;

using ThingsGateway.Foundation.Adapter.OPCUA;
using ThingsGateway.Foundation.Extension.Json;

using Xunit;
using Xunit.Abstractions;

namespace ThingsGateway.Foundation.Tests
{
    public class OPCUATest : IDisposable
    {
        private ITestOutputHelper _output;
        private OPCUAClient _opc;
        public OPCUATest(ITestOutputHelper output)
        {
            _output = output;
        }
        public void Dispose()
        {
            _opc?.SafeDispose();
        }
        [Theory]
        [InlineData("ns=2;s=数据类型示例.8 位设备.K 寄存器.DWord2")] //kep
        public async Task OpcSubscribeTest(string address)
        {
            _opc = new OPCUAClient();
            _opc.UserIdentity = new UserIdentity("Administrator", "111111");
            //_opc.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
            _opc.OPCNode = new() { OPCURL = "opc.tcp://127.0.0.1:49320" };

            var MonitorNodeTags = new string[] { address };
            _opc.Variables.AddRange(MonitorNodeTags.ToList());
            _opc.OpcStatusChange += Info_OpcStatusChange;
            _opc.DataChangedHandler += DataReceived;
            await _opc.ConnectAsync();
            Assert.True(_opc.Connected);
            var result = await _opc.WriteNodeAsync(address, JToken.Parse(new Random().Next(100).ToString()));
            Assert.True(result.IsSuccess);

            await Task.Delay(2000);
            _opc.Disconnect();
        }

        private void DataReceived((NodeId id, DataValue dataValue, JToken jToken) item)
        {
            _output.WriteLine(new { item.id, item.jToken }?.ToJson().FormatJson());
        }



        private void Info_OpcStatusChange(object sender, OPCUAStatusEventArgs e)
        {
            if (e.Error)
                _output.WriteLine(e?.ToJson());
        }



    }
}