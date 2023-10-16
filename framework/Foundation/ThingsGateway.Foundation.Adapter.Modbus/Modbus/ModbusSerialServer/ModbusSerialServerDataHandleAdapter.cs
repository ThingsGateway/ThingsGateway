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

/// <inheritdoc/>
public class ModbusSerialServerDataHandleAdapter : ReadWriteDevicesTcpDataHandleAdapter<ModbusSerialServerMessage>
{
    private readonly ThingsGatewayBitConverter ThingsGatewayBitConverter = new(EndianType.Big);

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public override byte[] PackCommand(byte[] command)
    {
        return ModbusHelper.AddCrc(command);
    }

    /// <summary>
    /// 获取modbus写入数据区内容
    /// </summary>
    /// <param name="response">返回数据</param>
    /// <returns></returns>
    internal OperResult<byte[]> GetModbusData(byte[] response)
    {
        try
        {
            var func = ThingsGatewayBitConverter.ToByte(response, 1);
            if (func == 1 || func == 2 || func == 3 || func == 4 || func == 5 || func == 6)
            {
                if (response.Length == 6)
                    return OperResult.CreateSuccessResult(response);
            }
            else if (func == 15 || func == 16)
            {
                var length = ThingsGatewayBitConverter.ToByte(response, 6);
                if (response.Length == 7 + length)
                {
                    return OperResult.CreateSuccessResult(response);
                }
            }

            return new OperResult<byte[]>() { Message = $"数据长度{response.Length}错误" };
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>(ex);
        }
    }

    /// <inheritdoc/>
    protected override ModbusSerialServerMessage GetInstance()
    {
        return new ModbusSerialServerMessage();
    }

    /// <inheritdoc/>
    protected override FilterResult UnpackResponse(ModbusSerialServerMessage request, byte[] send, byte[] body, byte[] response)
    {
        var result1 = ModbusHelper.GetModbusRtuData(new byte[0], response, true);
        if (result1.IsSuccess)
        {
            var result = GetModbusData(response.RemoveLast(2));
            if (result.IsSuccess)
            {
                //解析01 03 00 00 00 0A
                var station = ThingsGatewayBitConverter.ToByte(response, 0);
                var function = ThingsGatewayBitConverter.ToByte(response, 1);
                int addressStart = ThingsGatewayBitConverter.ToInt16(response, 2);
                if (addressStart == -1)
                {
                    addressStart = 65535;
                }
                if (function > 4)
                {
                    if (function > 6)
                    {
                        request.CurModbusAddress = new ModbusAddress()
                        {
                            Station = station,
                            Address = addressStart.ToString(),
                            WriteFunction = function,
                            ReadFunction = (byte)(function == 16 ? 3 : function == 15 ? 1 : 3),
                        };
                        request.Length = ThingsGatewayBitConverter.ToByte(response, 5);
                        request.Content = result.Content.RemoveBegin(7);
                    }
                    else
                    {
                        request.CurModbusAddress = new ModbusAddress()
                        {
                            Station = station,
                            Address = addressStart.ToString(),
                            WriteFunction = function,
                            ReadFunction = (byte)(function == 6 ? 3 : function == 5 ? 1 : 3),
                        };
                        request.Length = 1;
                        request.Content = result.Content.RemoveBegin(4);
                    }
                }
                else
                {
                    request.CurModbusAddress = new ModbusAddress()
                    {
                        Station = station,
                        Address = addressStart.ToString(),
                        ReadFunction = function,
                    };
                    request.Length = ThingsGatewayBitConverter.ToByte(response, 5);
                }
                request.ErrorCode = result.ErrorCode;
                request.Message = result.Message;
                return FilterResult.Success;
            }
            else
            {
                request.ErrorCode = result.ErrorCode;
                request.Message = result.Message;
                return FilterResult.Cache;
            }
        }
        else
        {
            return result1.Content2;
        }

    }
}
