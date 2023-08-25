using ThingsGateway.Foundation.Adapter.OPCUA;

using TouchSocket.Core;

namespace ModbusDemo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            OPCUAClient oPCUAClient = new(new EasyLogger(a => Console.WriteLine(a)))
            {
                OPCNode = new()
                {
                    OPCUrl = "opc.tcp://desktop-p5gb4iq:50001/StandardServer",
                    IsUseSecurity = true,
                }
            };
            await oPCUAClient.ConnectAsync();

            var testData1 = await oPCUAClient.ReadJTokenValueAsync(new[] { "ns=2;i=2897" });
            await oPCUAClient.WriteNodeAsync(new()
            {
                {"ns=2;i=2897", testData1.FirstOrDefault().Item3 }
            });

        }
    }
}