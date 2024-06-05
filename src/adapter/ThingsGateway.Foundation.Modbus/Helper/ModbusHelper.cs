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

internal class ModbusHelper
{
    #region modbusServer

    internal static byte[] ModbusServerAnalysisAddressValue(IModbusServerMessage request, IByteBlock response)
    {
        var offset = response.Position;
        //解析01 03 00 00 00 0A
        var station = response[0 + offset];
        var function = response[1 + offset];
        response.Position = 2 + offset;
        int addressStart = response.ReadUInt16(EndianType.Big);
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
                return response.ToArray(7 + offset);
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
                return response.ToArray(4 + offset);
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
            return Array.Empty<byte>();
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
    internal static OperResult<AdapterResult> GetModbusData(byte[] send, IByteBlock response)
    {
        try
        {
            if (response[response.Position + 1] >= 0x80)//错误码
                return new OperResult<AdapterResult>(GetDescriptionByErrorCode(response[response.Position + 2])) { Content = new AdapterResult() { FilterResult = FilterResult.Success } };

            if (send == null || send.Length == 0)
            {
                return new OperResult<AdapterResult>()
                {
                    Content = new()
                    {
                        Content = response.ToArray(response.Position + 3),
                        FilterResult = FilterResult.Success
                    }
                };
            }
            //站号验证
            if (send[response.Position + 0] != response[response.Position + 0])
                return new OperResult<AdapterResult>(ModbusResource.Localizer["StationNotSame", send[response.Position + 0], response[response.Position + 0]])
                {
                    Content = new()
                    {
                        FilterResult = FilterResult.Success
                    }
                };
            //功能码验证
            if (send[response.Position + 1] != response[response.Position + 1])
                return new OperResult<AdapterResult>(ModbusResource.Localizer["FunctionNotSame", send[response.Position + 1], response[response.Position + 1]])
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
                    Content = response.ToArray(response.Position + 3),
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
    /// 检测crc
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    internal static OperResult CheckCrc(IByteBlock response)
    {
        //crc校验
        var dataLen = response.Length;
        var crc = CRC16Utils.CRC16Only(response.AsSegment().Array, 0, dataLen - 2);
        if ((response[dataLen - 2] != crc[0] || response[dataLen - 1] != crc[1]))
            return new OperResult($"{ModbusResource.Localizer["CrcError"]} {DataTransUtil.ByteToHexString(response.Span, ' ')}")
            {
            };
        response.SetLength(dataLen - 2);
        return OperResult.Success;
    }

    /// <summary>
    /// 去除Crc，返回modbus数据区
    /// </summary>
    /// <param name="send"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    internal static OperResult<AdapterResult> GetModbusRtuData(byte[] send, IByteBlock response)
    {
        var result = CheckCrc(response);
        if (!result.IsSuccess)
        {
            return new OperResult<AdapterResult>(result)
            {
                Content = new() { FilterResult = FilterResult.Success }
            };
        }

        return GetModbusData(send, response);
    }

    #endregion 解析

    #region 报文构建

    public static byte[] AddCrc(ISendMessage item)
    {
        var crc = CRC16Utils.CRC16(item.SendBytes);
        return crc;
    }

    /// <summary>
    /// 添加ModbusTcp报文头
    /// </summary>
    internal static byte[] AddModbusTcpHead(byte[] modbus, int offset, int length, ushort id)
    {
        byte[] tcp = new byte[length + 6];
        var ids = BitConverter.GetBytes(id);
        var lens = BitConverter.GetBytes(length);
        tcp[0] = ids[1];
        tcp[1] = ids[0];
        tcp[4] = lens[1];
        tcp[5] = lens[0];
        Array.Copy(modbus, offset, tcp, 6, length);
        return tcp;
    }

    /// <summary>
    /// 获取读取报文
    /// </summary>
    internal static byte[] GetReadModbusCommand(ModbusAddress mAddress, ushort length)
    {
        var addresss = BitConverter.GetBytes(mAddress.AddressStart);
        var lens = BitConverter.GetBytes(length);
        byte[] array = new byte[6]
        {
        (byte) mAddress.Station,
        (byte) mAddress.ReadFunction,
        addresss[1],
        addresss[0],
        lens[1],
        lens[0]
        };
        return array;
    }

    /// <summary>
    /// 获取05写入布尔量报文
    /// </summary>
    internal static byte[] GetWriteBoolModbusCommand(ModbusAddress mAddress, bool value)
    {
        var addresss = BitConverter.GetBytes(mAddress.AddressStart);
        byte[] array = new byte[6]
        {
    (byte) mAddress.Station,
    (byte)5,
    addresss[1],
    addresss[0],
     0,
     0
        };
        if (value)
        {
            array[4] = 0xFF;
            array[5] = 0;
        }
        else
        {
            array[4] = 0;
            array[5] = 0;
        }
        return array;
    }

    /// <summary>
    /// 获取15写入布尔量报文
    /// </summary>
    internal static byte[] GetWriteBoolModbusCommand(ModbusAddress mAddress, bool[] values, ushort length)
    {
        var addresss = BitConverter.GetBytes(mAddress.AddressStart);
        byte[] numArray1 = values.BoolArrayToByte();
        byte[] numArray2 = new byte[7 + numArray1.Length];
        numArray2[0] = (byte)mAddress.Station;
        numArray2[1] = (byte)15;
        numArray2[2] = addresss[1];
        numArray2[3] = addresss[0];
        numArray2[4] = (byte)(length / 256);
        numArray2[5] = (byte)(length % 256);
        numArray2[6] = (byte)numArray1.Length;
        numArray1.CopyTo(numArray2, 7);
        return numArray2;
    }

    /// <summary>
    /// 获取16写入字报文
    /// </summary>
    internal static byte[] GetWriteModbusCommand(ModbusAddress mAddress, byte[] values)
    {
        var addresss = BitConverter.GetBytes(mAddress.AddressStart);
        byte[] numArray = new byte[7 + values.Length];
        numArray[0] = (byte)mAddress.Station;
        numArray[1] = (byte)16;
        numArray[2] = addresss[1];
        numArray[3] = addresss[0];
        numArray[4] = (byte)(values.Length / 2 / 256);
        numArray[5] = (byte)(values.Length / 2 % 256);
        numArray[6] = (byte)values.Length;
        values.CopyTo(numArray, 7);
        return numArray;
    }

    /// <summary>
    /// 获取6写入字报文
    /// </summary>
    internal static byte[] GetWriteOneModbusCommand(ModbusAddress mAddress, byte[] values)
    {
        var addresss = BitConverter.GetBytes(mAddress.AddressStart);
        byte[] numArray = new byte[4 + values.Length];
        numArray[0] = (byte)mAddress.Station;
        numArray[1] = (byte)6;
        numArray[2] = addresss[1];
        numArray[3] = addresss[0];
        values.CopyTo(numArray, 4);
        return numArray;
    }

    #endregion 报文构建
}
