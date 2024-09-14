//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using ThingsGateway.Foundation.Json.Extension;
using ThingsGateway.Foundation.SiemensS7;

using TouchSocket.Core;

namespace ThingsGateway.Foundation;

internal class S7MasterTest
{
    public static async Task Test()
    {

        using SiemensS7Master siemensS7Master = GetMaster();
        //modbusMaster.HeartbeatHexString = "ccccdddd";//心跳
        await siemensS7Master.ConnectAsync().ConfigureAwait(false);

        var addresss = new SiemensAddress[]
        {
            SiemensAddress.ParseFrom("M200"),
            SiemensAddress.ParseFrom("M210"),
            SiemensAddress.ParseFrom("M220"),

        };
        addresss[0].Data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        addresss[0].Length = addresss[0].Data.Length;
        addresss[1].Data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        addresss[1].Length = addresss[0].Data.Length;
        addresss[2].Data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        addresss[2].Length = addresss[0].Data.Length;
        var result = await siemensS7Master.S7WriteAsync(addresss).ConfigureAwait(false);
        Console.WriteLine(result.ToJsonNetString());

        S7Variable s7Variable = new S7Variable(siemensS7Master, 200);

        Console.ReadLine();
        //Console.WriteLine("批量读取");
        //await s7Variable.MultiReadAsync().ConfigureAwait(false);
        //Console.WriteLine("写入");
        //await s7Variable.WriteData2Async(1, default).ConfigureAwait(false);
        //Console.WriteLine("批量读取");
        //await s7Variable.MultiReadAsync().ConfigureAwait(false);

        //Console.WriteLine(s7Variable.ToJsonString());
        //执行连读
        Console.ReadLine();
    }

    private static SiemensS7Master GetMaster()
    {
        ConsoleLogger.Default.LogLevel = LogLevel.Trace;
        var clientConfig = new TouchSocketConfig();
        clientConfig.ConfigureContainer(a => a.AddConsoleLogger());
        var clientChannel = clientConfig.GetTcpClientWithIPHost("127.0.0.1:102");
        //clientChannel.Logger?.LogLevel = LogLevel.Trace;
        SiemensS7Master siemensS7Master = new(clientChannel)
        {
            SiemensS7Type = SiemensTypeEnum.S1500,
        };
        return siemensS7Master;
    }
}
