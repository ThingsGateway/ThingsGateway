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

/// <inheritdoc/>
internal class ModbusTcpServerDataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<ModbusTcpServerMessage>
{
    public ModbusTcpServerDataHandleAdapter()
    {
        this.MaxPackageSize = 1024 * 1024 * 1024;
    }

    /// <inheritdoc/>
    protected override AdapterResult UnpackResponse(ModbusTcpServerMessage request, IByteBlock byteBlock)
    {
        var pos = byteBlock.Position;
        byteBlock.Position = byteBlock.Position + 6;
        var bytes = ModbusHelper.ModbusServerAnalysisAddressValue(request, byteBlock);
        request.Bytes = byteBlock.AsSegment(pos, request.HeadBytesLength + request.BodyLength);
        return new AdapterResult() { FilterResult = FilterResult.Success, Content = bytes };
    }
}
