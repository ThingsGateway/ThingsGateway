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

/// <inheritdoc/>
internal class ModbusRtuServerDataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<ModbusRtuServerMessage>
{
    /// <inheritdoc/>
    public override byte[] PackCommand(byte[] command, ModbusRtuServerMessage item)
    {
        return ModbusHelper.AddCrc(command);
    }

    /// <inheritdoc/>
    protected override ModbusRtuServerMessage GetInstance()
    {
        return new ModbusRtuServerMessage();
    }

    /// <inheritdoc/>
    protected override FilterResult UnpackResponse(ModbusRtuServerMessage request, byte[]? send, byte[] body, byte[] response)
    {
        var result1 = ModbusHelper.GetModbusRtuData(new byte[0], response, true);

        if (result1.IsSuccess)
        {
            var result = ModbusHelper.GetModbusWriteData(response.RemoveLast(2));
            request.OperCode = result.OperCode;
            request.ErrorMessage = result.ErrorMessage;
            if (result.IsSuccess)
            {
                int offset = 0;
                ModbusHelper.ModbusServerAnalysisAddressValue(request, response, result, offset);
                return FilterResult.Success;
            }
            else
            {
                return result.Content2;
            }
        }
        else
        {
            return result1.Content2;
        }
    }
}