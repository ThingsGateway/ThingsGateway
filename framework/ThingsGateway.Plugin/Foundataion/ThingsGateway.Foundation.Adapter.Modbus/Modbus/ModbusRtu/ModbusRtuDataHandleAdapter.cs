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

namespace ThingsGateway.Foundation.Adapter.Modbus;

/// <summary>
/// Rtu适配器
/// </summary>
public class ModbusRtuDataHandleAdapter : ReadWriteDevicesTcpDataHandleAdapter<ModbusRtuMessage>
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
    protected override OperResult<byte[], FilterResult> UnpackResponse(ModbusRtuMessage request, byte[] send, byte[] body, byte[] response)
    {
        //理想状态检测
        var result = ModbusHelper.GetModbusRtuData(send, response, Crc16CheckEnable);
        if (result.IsSuccess)
        {
            return OperResult.CreateSuccessResult(result.Content, FilterResult.Success);
        }
        else
        {
            //如果返回错误，具体分析
            var op = result.Copy<byte[], FilterResult>();
            if (response.Length <= 1)
            {
                //如果长度不足，返回缓存
                op.Content2 = FilterResult.Cache;
                return op;
            }
            if (!(response[1] <= 0x10))
            {
                //功能码不对，返回放弃
                op.Content2 = FilterResult.Success;
                return op;
            }
            else
            {
                if ((response.Length > response[2] + 4))
                {
                    //如果长度已经超了，说明这段报文已经不能继续解析了，直接返回放弃
                    op.Content2 = FilterResult.Success;
                    return op;
                }
                else
                {
                    //否则返回缓存
                    op.Content2 = FilterResult.Cache;
                    return op;
                }
            }
        }
    }

}
