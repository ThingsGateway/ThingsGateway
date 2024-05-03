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
/// <inheritdoc/>
/// </summary>
internal class ModbusRtuOverUdpServerDataHandleAdapter : ReadWriteDevicesUdpDataHandleAdapter<ModbusRtuServerMessage>
{

    public override void PackCommand(ISendMessage item)
    {
        var crc = CRC16Utils.CRC16Only(item.SendBytes, item.Offset, item.Length);
        byte[] bytes = new byte[item.Length + crc.Length];
        Array.Copy(item.SendBytes, item.Offset, bytes, 0, item.Length);
        Array.Copy(crc, 0, bytes, item.Length, crc.Length);
        item.SetBytes(bytes);
    }

    /// <inheritdoc/>
    protected override byte[] UnpackResponse(ModbusRtuServerMessage request)
    {
        var response = request.ReceivedByteBlock;
        var result = ModbusHelper.CheckCrc(response);
        request.OperCode = result.OperCode;
        request.ErrorMessage = result.ErrorMessage;
        if (result.IsSuccess)
        {
            response.Pos = 0;
            var bytes = ModbusHelper.ModbusServerAnalysisAddressValue(request, response);
            return bytes;
        }
        else
        {
            return null;
        }
    }
}
