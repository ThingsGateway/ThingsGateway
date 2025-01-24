// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

using ThingsGateway.Foundation.Extension.String;
using ThingsGateway.Foundation.Modbus;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.Test;

public class ModbusTest
{

    [Theory]
    [InlineData("400045", true, "00010000002F01032C0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("300045", true, "00010000002F01042C0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000")]
    [InlineData("100045", true, "000100000009010206000000000000")]
    [InlineData("000045", true, "000100000009010106000000000000")]
    [InlineData("400045", false, "0001000000060106002C0001", "1", DataTypeEnum.UInt16)]
    [InlineData("000045", false, "0001000000060105002CFF00", "true", DataTypeEnum.Boolean)]
    public async Task ModbusTcp_ReadWrite_OK(string address, bool read, string data, string writeData = null, DataTypeEnum dataTypeEnum = DataTypeEnum.UInt16)
    {
        byte[] bytes = data.HexStringToBytes();
        var modbusChannel = new TouchSocketConfig().GetChannel(new ChannelOptions()
        {
            ChannelType = ChannelTypeEnum.Other
        }) as IClientChannel;
        var modbusMaster = new ModbusMaster() { ModbusType = ModbusTypeEnum.ModbusTcp, Timeout = 10000 };
        modbusMaster.InitChannel(modbusChannel);
        await modbusChannel.SetupAsync(modbusChannel.Config);
        var adapter = modbusChannel.ReadOnlyDataHandlingAdapter as SingleStreamDataHandlingAdapter;

        var task1 = Task.Run(async () =>
         {
             if (read)
             {
                 var result = await modbusMaster.ReadAsync(address, default).ConfigureAwait(false);
                 Assert.True(result.IsSuccess, result.ToString());
             }
             else
             {
                 var result = await modbusMaster.WriteAsync(address, JTokenUtil.GetJTokenFromString(writeData), dataTypeEnum).ConfigureAwait(false);
                 Assert.True(result.IsSuccess, result.ToString());
             }

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

    [Theory]
    [InlineData("400045", true, "01032C00000000000000000000000000000000000000000000000000000000000000000000000000000000000000007859")]
    [InlineData("300045", true, "01042C00000000000000000000000000000000000000000000000000000000000000000000000000000000000000008ADE")]
    [InlineData("100045", true, "010206000000000000E0B9")]
    [InlineData("000045", true, "010106000000000000A0AC")]
    [InlineData("400045", false, "0106002C000189C3", "1", DataTypeEnum.UInt16)]
    [InlineData("000045", false, "0105002CFF004DF3", "true", DataTypeEnum.Boolean)]
    public async Task ModbusRtu_ReadWrite_OK(string address, bool read, string data, string writeData = null, DataTypeEnum dataTypeEnum = DataTypeEnum.UInt16)
    {
        byte[] bytes = data.HexStringToBytes();
        var modbusChannel = new TouchSocketConfig().GetChannel(new ChannelOptions()
        {
            ChannelType = ChannelTypeEnum.Other
        }) as IClientChannel;
        var modbusMaster = new ModbusMaster() { ModbusType = ModbusTypeEnum.ModbusRtu, Timeout = 10000, Station = 1 };
        modbusMaster.InitChannel(modbusChannel);
        await modbusChannel.SetupAsync(modbusChannel.Config);
        var adapter = modbusChannel.ReadOnlyDataHandlingAdapter as SingleStreamDataHandlingAdapter;

        var task1 = Task.Run(async () =>
        {
            if (read)
            {
                var result = await modbusMaster.ReadAsync(address, default).ConfigureAwait(false);
                Assert.True(result.IsSuccess, result.ToString());
            }
            else
            {
                var result = await modbusMaster.WriteAsync(address, JTokenUtil.GetJTokenFromString(writeData), dataTypeEnum).ConfigureAwait(false);
                Assert.True(result.IsSuccess, result.ToString());
            }

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
