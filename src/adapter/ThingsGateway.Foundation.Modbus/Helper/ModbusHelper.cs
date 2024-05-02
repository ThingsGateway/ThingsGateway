
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

internal class ModbusHelper
{

    #region modbusServer

    internal static ByteBlock ModbusServerAnalysisAddressValue(IModbusServerMessage request, ByteBlock response, ByteBlock writeSource, int offset)
    {
        //解析01 03 00 00 00 0A
        var station = response[0 + offset];
        var function = response[1 + offset];
        int addressStart = TouchSocketBitConverter.BigEndian.ToUInt16(response.Buffer, 2 + offset);

        if (function > 4)
        {
            if (function > 6)
            {
                request.ModbusAddress = new ModbusAddress()
                {
                    Station = station,
                    Address = addressStart.ToString(),
                    WriteFunction = function,
                    ReadFunction = (byte)(function == 16 ? 3 : function == 15 ? 1 : 3),
                };
                request.Length = response[5 + offset];
                return writeSource.RemoveBegin(7);
            }
            else
            {
                request.ModbusAddress = new ModbusAddress()
                {
                    Station = station,
                    Address = addressStart.ToString(),
                    WriteFunction = function,
                    ReadFunction = (byte)(function == 6 ? 3 : function == 5 ? 1 : 3),
                };
                request.Length = 1;
                return writeSource.RemoveBegin(4);
            }
        }
        else
        {
            request.ModbusAddress = new ModbusAddress()
            {
                Station = station,
                Address = addressStart.ToString(),
                ReadFunction = function,
            };
            request.Length = response[5 + offset];
            return new ByteBlock(0);
        }
    }

    /// <summary>
    /// 获取modbus写入数据区内容
    /// </summary>
    /// <param name="response">返回数据</param>
    /// <returns></returns>
    internal static OperResult<AdapterResult> GetModbusWriteData(ByteBlock response)
    {
        try
        {
            var func = response[1];
            if (func == 1 || func == 2 || func == 3 || func == 4 || func == 5 || func == 6)
            {
                if (response.Length == 6)
                    return new OperResult<AdapterResult>()
                    {
                        Content = new AdapterResult()
                        {
                            ByteBlock = response,
                            FilterResult = FilterResult.Success
                        }
                    };
            }
            else if (func == 15 || func == 16)
            {
                var length = response[6];
                if (response.Length == 7 + length)
                {
                    return new OperResult<AdapterResult>()
                    {
                        Content = new AdapterResult()
                        {
                            ByteBlock = response,
                            FilterResult = FilterResult.Success
                        }
                    };
                }
                if (response.Length > 7 + length)
                {
                    return new(DefaultResource.Localizer["DataLengthError", response.Length])
                    {
                        Content = new AdapterResult()
                        {
                            FilterResult = FilterResult.Success
                        }
                    };
                }
                else
                {
                    return new(DefaultResource.Localizer["DataLengthError", response.Length])
                    {
                        Content = new AdapterResult()
                        {
                            FilterResult = FilterResult.Cache
                        }
                    };
                }
            }

            return new(DefaultResource.Localizer["DataLengthError", response.Length]) { Content = new AdapterResult() { FilterResult = FilterResult.Success } };
        }
        catch (Exception ex)
        {
            return new(ex) { Content = new AdapterResult() { FilterResult = FilterResult.Success } };
        }
    }

    #endregion modbusServer

    #region 解析

    /// <summary>
    /// modbus地址格式说明
    /// </summary>
    /// <returns></returns>
    internal static string GetAddressDescription()
    {
        return ModbusResource.Localizer["AddressDes"];
    }

    /// <summary>
    /// 通过错误码来获取到对应的文本消息
    /// </summary>
    internal static string GetDescriptionByErrorCode(byte code)
    {
        return code switch
        {
            1 => ModbusResource.Localizer["ModbusError1"],
            2 => ModbusResource.Localizer["ModbusError2"],
            3 => ModbusResource.Localizer["ModbusError3"],
            4 => ModbusResource.Localizer["ModbusError4"],
            5 => ModbusResource.Localizer["ModbusError5"],
            6 => ModbusResource.Localizer["ModbusError6"],
            8 => ModbusResource.Localizer["ModbusError8"],
            10 => ModbusResource.Localizer["ModbusError10"],
            11 => ModbusResource.Localizer["ModbusError11"],
            _ => DefaultResource.Localizer["UnknownError", code],
        };
    }

    /// <summary>
    /// 获取modbus数据区内容，response需去除Crc和tcp报文头，例如：01 03 02 00 01
    /// </summary>
    /// <param name="send">发送数据</param>
    /// <param name="response">返回数据</param>
    /// <returns></returns>
    internal static OperResult<AdapterResult> GetModbusData(ByteBlock send, ByteBlock response)
    {
        try
        {
            if (response[1] >= 0x80)//错误码
                return new OperResult<AdapterResult>(GetDescriptionByErrorCode(response[2])) { Content = new AdapterResult() { FilterResult = FilterResult.Success } };

            if (send == null || send.Length == 0)
            {
                return new OperResult<AdapterResult>()
                {
                    Content = new()
                    {
                        ByteBlock = response.RemoveBegin(3),
                        FilterResult = FilterResult.Success
                    }
                };
            }
            //站号验证
            if (send[0] != response[0])
                return new OperResult<AdapterResult>(ModbusResource.Localizer["StationNotSame", send[0], response[0]])
                {
                    Content = new()
                    {
                        FilterResult = FilterResult.Success
                    }
                };
            //功能码验证
            if (send[1] != response[1])
                return new OperResult<AdapterResult>(ModbusResource.Localizer["FunctionNotSame", send[1], response[1]])
                {
                    Content = new()
                    {
                        FilterResult = FilterResult.Success
                    }
                };
            return new OperResult<AdapterResult>()
            {
                Content = new()
                {
                    ByteBlock = response.RemoveBegin(3),
                    FilterResult = FilterResult.Success
                }
            };

        }
        catch (Exception ex)
        {
            return new OperResult<AdapterResult>(ex)
            {
                Content = new()
                {
                    FilterResult = FilterResult.Success
                }
            };
        }
    }

    /// <summary>
    /// 去除Crc，返回modbus数据区
    /// </summary>
    /// <param name="send"></param>
    /// <param name="response"></param>
    /// <param name="crcCheck"></param>
    /// <returns></returns>
    internal static OperResult<AdapterResult> GetModbusRtuData(ByteBlock send, ByteBlock response, bool crcCheck = true)
    {
        if (response[1] >= 0x80)//错误码
            return new OperResult<AdapterResult>(GetDescriptionByErrorCode(response[2]))
            {
                Content = new() { FilterResult = FilterResult.Success }
            };

        //crc校验
        using var data = response.SelectMiddle(0, response[1] <= 0x04 ? response[2] != 0 ? response[2] + 5 : 8 : 8);
        if (crcCheck && !CRC16Utils.CheckCRC16(data))
            return new OperResult<AdapterResult>($"{ModbusResource.Localizer["CrcError"]} {DataTransUtil.ByteToHexString(data, ' ')}")
            {
                Content = new() { FilterResult = FilterResult.Success }
            };
        return GetModbusData(send, data.RemoveLast(2));
    }

    #endregion 解析

    #region 报文构建



    /// <summary>
    /// 添加ModbusTcp报文头
    /// </summary>
    internal static void AddModbusTcpHead(ISendMessage item)
    {
        ByteBlock bytes = new ByteBlock(item.SendByteBlock.Len + 6);
        var addressByte = TouchSocketBitConverter.BigEndian.GetBytes((ushort)item.Sign);
        bytes.Write(addressByte);
        bytes.Write((ushort)0);
        var lenByte = TouchSocketBitConverter.BigEndian.GetBytes((ushort)item.SendByteBlock.Len);
        bytes.Write(lenByte);
        bytes.Write(item.SendByteBlock.Buffer, 0, item.SendByteBlock.Len);
        item.SendByteBlock = bytes;
    }

    /// <summary>
    /// 获取读取报文
    /// </summary>
    internal static ByteBlock GetReadModbusCommand(ModbusAddress mAddress, ushort length)
    {
        ByteBlock bytes = new ByteBlock(6);
        bytes.Write(mAddress.Station);
        bytes.Write(mAddress.ReadFunction);
        var addressByte = TouchSocketBitConverter.BigEndian.GetBytes(mAddress.AddressStart);
        bytes.Write(addressByte);
        var lenBytes = TouchSocketBitConverter.BigEndian.GetBytes(length);
        bytes.Write(lenBytes);
        return bytes;
    }

    /// <summary>
    /// 获取05写入布尔量报文
    /// </summary>
    internal static ByteBlock GetWriteBoolModbusCommand(ModbusAddress mAddress, bool value)
    {
        ByteBlock bytes = new ByteBlock(6);
        bytes.Write(mAddress.Station);
        bytes.Write((byte)5);
        var addressByte = TouchSocketBitConverter.BigEndian.GetBytes(mAddress.AddressStart);
        bytes.Write(addressByte);
        if (value)
        {
            bytes.Write((byte)0xFF);
            bytes.Write((byte)0x00);
        }
        else
        {
            bytes.Write((byte)0x00);
            bytes.Write((byte)0x00);
        }
        return bytes;

    }

    /// <summary>
    /// 获取15写入布尔量报文
    /// </summary>
    internal static ByteBlock GetWriteBoolModbusCommand(ModbusAddress mAddress, bool[] values, ushort length)
    {
        byte[] numArray1 = values.BoolArrayToByte();
        ByteBlock bytes = new ByteBlock(7 + numArray1.Length);
        bytes.Write(mAddress.Station);
        bytes.Write((byte)15);
        var addressByte = TouchSocketBitConverter.BigEndian.GetBytes(mAddress.AddressStart);
        bytes.Write(addressByte);

        bytes.Write((byte)(length / 256));
        bytes.Write((byte)(length % 256));
        bytes.Write((byte)numArray1.Length);
        bytes.Write(numArray1);
        return bytes;
    }

    /// <summary>
    /// 获取16写入字报文
    /// </summary>
    internal static ByteBlock GetWriteModbusCommand(ModbusAddress mAddress, byte[] values)
    {
        ByteBlock bytes = new ByteBlock(7 + values.Length);
        bytes.Write(mAddress.Station);
        bytes.Write((byte)16);
        var addressByte = TouchSocketBitConverter.BigEndian.GetBytes(mAddress.AddressStart);
        bytes.Write(addressByte);
        bytes.Write((byte)(values.Length / 2 / 256));
        bytes.Write((byte)(values.Length / 2 % 256));
        bytes.Write((byte)values.Length);
        bytes.Write(values);

        return bytes;
    }

    /// <summary>
    /// 获取6写入字报文
    /// </summary>
    internal static ByteBlock GetWriteOneModbusCommand(ModbusAddress mAddress, byte[] values)
    {
        ByteBlock bytes = new ByteBlock(4 + values.Length);
        bytes.Write(mAddress.Station);
        bytes.Write((byte)6);
        var addressByte = TouchSocketBitConverter.BigEndian.GetBytes(mAddress.AddressStart);
        bytes.Write(addressByte);
        bytes.Write(values);
        return bytes;
    }

    #endregion 报文构建
}
