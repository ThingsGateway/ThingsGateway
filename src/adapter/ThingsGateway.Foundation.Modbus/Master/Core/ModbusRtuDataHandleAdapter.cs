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
/// Rtu适配器
/// </summary>
internal class ModbusRtuDataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<ModbusRtuMessage>
{
    /// <inheritdoc/>
    public override void PackCommand(ISendMessage item)
    {
        var crc = CRC16Utils.CRC16Only(item.SendBytes, item.Offset, item.Length);
        byte[] bytes = new byte[item.Length + crc.Length];
        Array.Copy(item.SendBytes, item.Offset, bytes, 0, item.Length);
        Array.Copy(crc, 0, bytes, item.Length, crc.Length);
        item.SetBytes(bytes);
    }

    /// <inheritdoc/>
    protected override AdapterResult UnpackResponse(ModbusRtuMessage request)
    {
        var send = request.SendBytes;
        var response = request.ReceivedByteBlock;
        //通道干扰时需剔除前缀中的多于字节，初步按站号+功能码找寻初始字节
        //int index = -1;
        //for (int i = 0; i < response.Length - 1; i++)
        //{
        //    if (response[i] == send[0] && (response[i + 1] == send[1] || response[i + 1] == (send[1] + 0x80)))
        //    {
        //        index = i;
        //        break;
        //    }
        //}

        //using var response1 = new ByteBlock(response.Len);
        //if (index > 0)
        //{
        //    response1.Write(response.Buffer, index, response.Len - index);
        //}
        //else
        //{
        //    response1.Write(response.Buffer);
        //}
        //response1.SeekToStart();

        var result = ModbusHelper.GetModbusRtuData(send, response, true);
        request.OperCode = result.OperCode;
        request.ErrorMessage = result.ErrorMessage;
        return result.Content;

    }
}
