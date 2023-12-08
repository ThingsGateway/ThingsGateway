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

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.Modbus;

/// <summary>
/// Rtu适配器
/// </summary>
internal class ModbusRtuDataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<ModbusRtuMessage>
{
    /// <summary>
    /// 检测CRC
    /// </summary>
    public bool Crc16CheckEnable { get; set; } = true;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public override byte[] PackCommand(byte[] command)
    {
        return ModbusHelper.AddCrc(command);
    }

    /// <inheritdoc/>
    protected override ModbusRtuMessage GetInstance()
    {
        return new ModbusRtuMessage();
    }

    /// <inheritdoc/>
    protected override FilterResult UnpackResponse(ModbusRtuMessage request, byte[] send, byte[] body, byte[] response)
    {
        //链路干扰时需剔除前缀中的多于字节，初步按站号+功能码找寻初始字节
        if (send?.Length > 0)
        {
            int index = -1;
            for (int i = 0; i < response.Length - 1; i++)
            {
                if (response[i] == send[0] && (response[i + 1] == send[1] || response[i + 1] == (send[1] + 0x80)))
                {
                    index = i;
                    break;
                }
            }
            if (index >= 0)
            {
                response = response.RemoveBegin(index);
            }

            //理想状态检测
            var result = ModbusHelper.GetModbusRtuData(send, response, Crc16CheckEnable);
            if (result.IsSuccess)
            {
                request.ErrorCode = result.ErrorCode;
                request.Message = result.Message;
                request.Content = result.Content;
            }
            else
            {
                request.ErrorCode = result.ErrorCode;
                request.Message = result.Message;
            }
            return result.Content2;
        }
        else
        {
            return FilterResult.Success;
        }
    }
}