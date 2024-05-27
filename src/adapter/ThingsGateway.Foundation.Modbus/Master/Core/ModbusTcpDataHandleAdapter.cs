//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation.Modbus;

/// <summary>
/// ModbusTcpDataHandleAdapter
/// </summary>
internal class ModbusTcpDataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<ModbusTcpMessage>
{
    public ModbusTcpDataHandleAdapter()
    {
        IsSendPackCommand = true;
    }

    public override byte[] PackCommand(ISendMessage item)
    {
        return ModbusHelper.AddModbusTcpHead(item.SendBytes, 0, item.SendBytes.Length, (ushort)item.Sign);
    }

    public override bool IsSingleThread { get; } = false;

    /// <inheritdoc/>
    protected override AdapterResult UnpackResponse(ModbusTcpMessage request, IByteBlock byteBlock)
    {
        byteBlock.Position = 6;
        var result = ModbusHelper.GetModbusData(null, byteBlock);
        request.OperCode = result.OperCode;
        request.ErrorMessage = result.ErrorMessage;

        byteBlock.Position = 0;
        request.Sign = byteBlock.ReadUInt16(EndianType.Big);
        return result.Content;
    }
}
