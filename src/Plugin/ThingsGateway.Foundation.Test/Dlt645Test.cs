// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Foundation.Dlt645;
using ThingsGateway.Foundation.Extension.String;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Test;

public class Dlt645Test
{

    [Theory]
    [InlineData("02010100", "FE FE FE FE 68 11 11 11 11 11 11 68 91 07 33 34 34 35 33 59 36 60 16 ")]
    public async Task Dlt645_Read_OK(string address, string data)
    {
        byte[] bytes = data.HexStringToBytes();
        var dltChannel = new TouchSocketConfig().GetChannel(new ChannelOptions()
        {
            ChannelType = ChannelTypeEnum.Other
        }) as IClientChannel;
        var dltMaster = new Dlt645_2007Master() { Timeout = 10000, Station = "111111111111" };
        dltMaster.InitChannel(dltChannel);
        await dltChannel.SetupAsync(dltChannel.Config);
        var adapter = dltChannel.ReadOnlyDataHandlingAdapter as SingleStreamDataHandlingAdapter;

        var task1 = Task.Run(async () =>
         {

             var result = await dltMaster.ReadAsync(address, default).ConfigureAwait(false);
             Assert.True(result.IsSuccess, result.ToString());

         });
        await Task.Delay(50);
        var task2 = Task.Run(async () =>
        {
            foreach (var item in bytes)
            {
                var data = new ByteBlock([item]);
                await adapter.ReceivedInputAsync(data).ConfigureAwait(false);
            }
        });
        await Task.WhenAll(task1, task2);
    }


}
