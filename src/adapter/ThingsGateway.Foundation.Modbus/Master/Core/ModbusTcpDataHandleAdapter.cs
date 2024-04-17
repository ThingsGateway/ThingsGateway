
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------




using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// ModbusTcpDataHandleAdapter
/// </summary>
internal class ModbusTcpDataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<ModbusTcpMessage>
{
    private readonly IncrementCount _incrementCount = new(ushort.MaxValue);

    /// <inheritdoc/>
    public override byte[] PackCommand(byte[] command, ModbusTcpMessage modbusTcpMessage)
    {
        return ModbusHelper.AddModbusTcpHead(command, (ushort)modbusTcpMessage.Sign);
    }

    public override bool IsSingleThread { get; } = false;

    /// <inheritdoc/>
    protected override ModbusTcpMessage GetInstance()
    {
        return new ModbusTcpMessage();
    }

    /// <inheritdoc/>
    protected override FilterResult UnpackResponse(ModbusTcpMessage request, byte[]? send, byte[] body, byte[] response)
    {
        var result = ModbusHelper.GetModbusData(send?.RemoveBegin(6), response.RemoveBegin(6));
        request.OperCode = result.OperCode;
        request.ErrorMessage = result.ErrorMessage;
        request.Sign = TouchSocketBitConverter.BigEndian.ToUInt16(response, 0);
        if (result.IsSuccess)
        {
            request.Content = result.Content;
        }
        return result.Content2;
    }
}