
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
internal class ModbusUdpDataHandleAdapter : ReadWriteDevicesUdpDataHandleAdapter<ModbusTcpMessage>
{
    private readonly IncrementCount _incrementCount = new(ushort.MaxValue);


    public override void PackCommand(ISendMessage item)
    {
        ModbusHelper.AddModbusTcpHead(item);
    }


    /// <inheritdoc/>
    protected override ByteBlock UnpackResponse(ModbusTcpMessage request)
    {
        using var send = request.SendByteBlock?.RemoveBegin(6);
        using var response = request.ReceivedByteBlock.RemoveBegin(6);
        var result = ModbusHelper.GetModbusData(send, response);
        request.OperCode = result.OperCode;
        request.ErrorMessage = result.ErrorMessage;
        request.Sign = TouchSocketBitConverter.BigEndian.ToUInt16(request.ReceivedByteBlock.Buffer, 0);
        return result.Content.ByteBlock;
    }
}
