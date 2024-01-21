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

using ThingsGateway.Foundation.Modbus;

using TouchSocket.Sockets;

using Xunit.Abstractions;

namespace ThingsGateway.Foundation;

public class ModbusTest
{
    private readonly ITestOutputHelper _outputHelper;

    public ModbusTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Theory]
    [InlineData(ChannelTypeEnum.TcpClient, ChannelTypeEnum.TcpService, ModbusTypeEnum.ModbusTcp, "127.0.0.1:502", "127.0.0.1:502")]
    [InlineData(ChannelTypeEnum.TcpClient, ChannelTypeEnum.TcpService, ModbusTypeEnum.ModbusRtu, "127.0.0.1:502", "127.0.0.1:502")]
    [InlineData(ChannelTypeEnum.UdpSession, ChannelTypeEnum.UdpSession, ModbusTypeEnum.ModbusTcp, "127.0.0.1:502", "127.0.0.1:502")]
    [InlineData(ChannelTypeEnum.UdpSession, ChannelTypeEnum.UdpSession, ModbusTypeEnum.ModbusRtu, "127.0.0.1:502", "127.0.0.1:502")]
    [InlineData(ChannelTypeEnum.SerialPortClient, ChannelTypeEnum.SerialPortClient, ModbusTypeEnum.ModbusRtu, "COM1", "COM2")]
    public async Task TcpReadOk(
        ChannelTypeEnum channelType,
        ChannelTypeEnum serviceChannelType,
        ModbusTypeEnum modbusType,
        string clienturi,
        string serviceuri
        )
    {
        //System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en");
        //创建通道，也可以通过TouchSocketConfig.GetChannel扩展获取
        var channel = Util.GetChannel(channelType, clienturi, null, clienturi);
        var serviceChannel = Util.GetChannel(channelType, null, serviceuri, serviceuri);

        //创建modbus客户端，传入通道
        using ModbusMaster modbusClient = new(channel)
        {
            //modbus协议格式
            //ModbusType = Modbus.ModbusTypeEnum.ModbusRtu,
            ModbusType = modbusType
        };

        //创建modbus服务端，传入通道
        using ModbusSlave modbusServer = new(serviceChannel)
        {
            //modbus协议格式
            //ModbusType = Modbus.ModbusTypeEnum.ModbusRtu,
            ModbusType = modbusType
        };
        //启动服务
        serviceChannel.Connect();
        var result = await modbusClient.ReadInt16Async("40001", 1);
        Assert.True(result.IsSuccess, result.ErrorMessage);
    }
}