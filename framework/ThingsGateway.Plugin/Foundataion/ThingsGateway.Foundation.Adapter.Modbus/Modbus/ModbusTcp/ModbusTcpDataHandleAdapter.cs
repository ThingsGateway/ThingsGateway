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

using ThingsGateway.Foundation.Extension;
using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Adapter.Modbus;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class ModbusTcpDataHandleAdapter : ReadWriteDevicesTcpDataHandleAdapter<ModbusTcpMessage>
{
    private readonly EasyIncrementCount easyIncrementCount = new(ushort.MaxValue);

    /// <summary>
    /// 检测事务标识符
    /// </summary>
    public bool IsCheckMessageId
    {
        get
        {
            return Request?.IsCheckMessageId ?? false;
        }
        set
        {
            Request.IsCheckMessageId = value;
        }
    }

    /// <inheritdoc/>
    public override byte[] PackCommand(byte[] command)
    {
        return ModbusHelper.AddModbusTcpHead(command, (ushort)easyIncrementCount.GetCurrentValue());
    }

    /// <inheritdoc/>
    protected override ModbusTcpMessage GetInstance()
    {
        return new ModbusTcpMessage();
    }

    /// <inheritdoc/>
    protected override FilterResult UnpackResponse(ModbusTcpMessage request, byte[] send, byte[] body, byte[] response)
    {
        //理想状态检测
        var result = ModbusHelper.GetModbusData(send.RemoveBegin(6), response.RemoveBegin(6));
        if (result.IsSuccess)
        {
            request.ResultCode = result.ResultCode;
            request.Message = result.Message;
            request.Content = result.Content;
            return FilterResult.Success;
        }
        else
        {
            //如果返回错误，具体分析
            var op = result.Copy<byte[], FilterResult>();
            if (response.Length == 9)
            {
                if (response[7] >= 0x80)//错误码
                {
                    request.ResultCode = result.ResultCode;
                    request.Message = result.Message;
                    request.Content = result.Content;
                    return FilterResult.Success;
                }
            }
            if (response.Length < 10)
            {
                request.ResultCode = result.ResultCode;
                request.Message = result.Message;
                request.Content = result.Content;
                return FilterResult.Cache;
                //如果长度不足，返回缓存
            }
            if ((response.Length > response[8] + 9))
            {
                request.ResultCode = result.ResultCode;
                request.Message = result.Message;
                request.Content = result.Content;
                return FilterResult.Success;
                //如果长度已经超了，说明这段报文已经不能继续解析了，直接返回放弃
            }
            else
            {
                request.ResultCode = result.ResultCode;
                request.Message = result.Message;
                request.Content = result.Content;
                return FilterResult.Cache;
                //否则返回缓存
            }
        }

    }
}
