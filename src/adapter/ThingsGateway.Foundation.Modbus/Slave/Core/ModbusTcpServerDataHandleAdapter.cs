﻿
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
internal class ModbusTcpServerDataHandleAdapter : ReadWriteDevicesSingleStreamDataHandleAdapter<ModbusTcpServerMessage>
{
    /// <inheritdoc/>
    public override byte[] PackCommand(byte[] command, ModbusTcpServerMessage item)
    {
        return command;
    }

    /// <inheritdoc/>
    protected override ModbusTcpServerMessage GetInstance()
    {
        return new ModbusTcpServerMessage();
    }

    /// <inheritdoc/>
    protected override FilterResult UnpackResponse(ModbusTcpServerMessage request, byte[]? send, byte[] body, byte[] response)
    {
        var result = ModbusHelper.GetModbusWriteData(response.RemoveBegin(6));
        request.OperCode = result.OperCode;
        request.ErrorMessage = result.ErrorMessage;
        if (result.IsSuccess)
        {
            int offset = 6;
            ModbusHelper.ModbusServerAnalysisAddressValue(request, response, result, offset);
            return FilterResult.Success;
        }
        else
        {
            return result.Content2;
        }
    }
}