
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

/// <inheritdoc/>
internal class ModbusRtuServerDataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<ModbusRtuServerMessage>
{
    /// <inheritdoc/>
    public override void PackCommand(ISendMessage item)
    {
        var crc = CRC16Utils.CRC16Only(item.SendByteBlock.Buffer, 0, item.SendByteBlock.Len);
        item.SendByteBlock.SeekToEnd();
        item.SendByteBlock.Write(crc);
    }


    /// <inheritdoc/>
    protected override AdapterResult UnpackResponse(ModbusRtuServerMessage request)
    {
        var send = request.SendByteBlock;
        var response = request.ReceivedByteBlock;
        var result1 = ModbusHelper.GetModbusRtuData(send, response, true);

        if (result1.IsSuccess)
        {
            var result = ModbusHelper.GetModbusWriteData(response.RemoveLast(2));
            request.OperCode = result.OperCode;
            request.ErrorMessage = result.ErrorMessage;
            if (result.IsSuccess)
            {
                int offset = 0;
                var bytes = ModbusHelper.ModbusServerAnalysisAddressValue(request, response, result.Content.ByteBlock, offset);
                return new AdapterResult() { FilterResult = FilterResult.Success, ByteBlock = bytes };
            }
            else
            {
                return result.Content;
            }
        }
        else
        {
            return result1.Content;
        }
    }
}
