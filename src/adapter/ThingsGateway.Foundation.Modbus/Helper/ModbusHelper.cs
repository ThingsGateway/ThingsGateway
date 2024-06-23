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
    internal static OperResult<AdapterResult> GetModbusData(ReadOnlySpan<byte> send, IByteBlock response)
    {
        try
        {
            if (response[response.Position + 1] >= 0x80)//错误码
                return new OperResult<AdapterResult>(GetDescriptionByErrorCode(response[response.Position + 2])) { Content = new AdapterResult() { FilterResult = FilterResult.Success } };

            if (send.Length == 0)
            {
                return new OperResult<AdapterResult>()
                {
                    Content = new()
                    {
                        Content = response.ToArray(response.Position + 3, response[response.Position + 2]),
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
                    Content = response.ToArray(response.Position + 3, response[response.Position + 2]),
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
        var crc = CRC16Utils.Crc16Only(new ReadOnlyMemory<byte>(response.AsSegment().Array, 0, dataLen - 2));
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
    internal static OperResult<AdapterResult> GetModbusRtuData(ReadOnlySpan<byte> send, IByteBlock response)
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

    /// <summary>
    /// 获取读取报文
    /// </summary>
    internal static ReadOnlyMemory<byte> GetReadModbusCommand(ref ValueByteBlock valueByteBlock, ModbusAddress mAddress, ushort length, ModbusTypeEnum modbusType, ushort sign, ushort protocolId)
    {
        if (modbusType == ModbusTypeEnum.ModbusTcp)
        {
            valueByteBlock.WriteUInt16(sign, EndianType.Big);
            valueByteBlock.WriteUInt16(protocolId, EndianType.Big);
            valueByteBlock.WriteUInt16(6, EndianType.Big);
        }
        valueByteBlock.WriteByte(mAddress.Station);
        valueByteBlock.WriteByte(mAddress.ReadFunction);
        valueByteBlock.WriteUInt16(mAddress.AddressStart, EndianType.Big);
        valueByteBlock.WriteUInt16(length, EndianType.Big);

        if (modbusType == ModbusTypeEnum.ModbusRtu)
        {
            valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory));
        }
        return valueByteBlock.Memory;
    }

    /// <summary>
    /// 获取05写入布尔量报文
    /// </summary>
    internal static ReadOnlyMemory<byte> GetWriteBoolModbusCommand(ref ValueByteBlock valueByteBlock, ModbusAddress mAddress, bool value, ModbusTypeEnum modbusType, ushort sign, ushort protocolId)
    {
        if (modbusType == ModbusTypeEnum.ModbusTcp)
        {
            valueByteBlock.WriteUInt16(sign, EndianType.Big);
            valueByteBlock.WriteUInt16(protocolId, EndianType.Big);
            valueByteBlock.WriteUInt16(6, EndianType.Big);
        }
        valueByteBlock.WriteByte(mAddress.Station);
        valueByteBlock.WriteByte(5);
        valueByteBlock.WriteUInt16(mAddress.AddressStart, EndianType.Big);
        valueByteBlock.Write(value ? new byte[] { 0xFF, 0 } : new byte[] { 0, 0 });

        if (modbusType == ModbusTypeEnum.ModbusRtu)
        {
            valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory));
        }

        return valueByteBlock.Memory;
    }

    /// <summary>
    /// 获取15写入布尔量报文
    /// </summary>
    internal static ReadOnlyMemory<byte> GetWriteBoolModbusCommand(ref ValueByteBlock valueByteBlock, ModbusAddress mAddress, bool[] values, ushort length, ModbusTypeEnum modbusType, ushort sign, ushort protocolId)
    {
        if (modbusType == ModbusTypeEnum.ModbusTcp)
        {
            valueByteBlock.WriteUInt16(sign, EndianType.Big);
            valueByteBlock.WriteUInt16(protocolId, EndianType.Big);
            valueByteBlock.WriteUInt16(6, EndianType.Big);
        }
        valueByteBlock.WriteByte(mAddress.Station);
        valueByteBlock.WriteByte(15);
        valueByteBlock.WriteUInt16(mAddress.AddressStart, EndianType.Big);
        valueByteBlock.WriteUInt16(length, EndianType.Big);
        byte[] data = values.BoolArrayToByte();
        valueByteBlock.WriteByte((byte)data.Length);
        valueByteBlock.Write(values.BoolArrayToByte());

        if (modbusType == ModbusTypeEnum.ModbusRtu)
        {
            valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory));
        }

        return valueByteBlock.Memory;
    }

    /// <summary>
    /// 获取16写入字报文
    /// </summary>
    internal static ReadOnlyMemory<byte> GetWriteModbusCommand(ref ValueByteBlock valueByteBlock, ModbusAddress mAddress, byte[] values, ushort length, ModbusTypeEnum modbusType, ushort sign, ushort protocolId)
    {
        if (modbusType == ModbusTypeEnum.ModbusTcp)
        {
            valueByteBlock.WriteUInt16(sign, EndianType.Big);
            valueByteBlock.WriteUInt16(protocolId, EndianType.Big);
            valueByteBlock.WriteUInt16(6, EndianType.Big);
        }
        valueByteBlock.WriteByte(mAddress.Station);
        valueByteBlock.WriteByte(16);
        valueByteBlock.WriteUInt16(mAddress.AddressStart, EndianType.Big);
        valueByteBlock.WriteUInt16(length, EndianType.Big);
        valueByteBlock.WriteByte((byte)values.Length);
        valueByteBlock.Write(values);

        if (modbusType == ModbusTypeEnum.ModbusRtu)
        {
            valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory));
        }

        return valueByteBlock.Memory;
    }

    /// <summary>
    /// 获取6写入字报文
    /// </summary>
    internal static ReadOnlyMemory<byte> GetWriteOneModbusCommand(ref ValueByteBlock valueByteBlock, ModbusAddress mAddress, byte[] values, ModbusTypeEnum modbusType, ushort sign, ushort protocolId)
    {
        if (modbusType == ModbusTypeEnum.ModbusTcp)
        {
            valueByteBlock.WriteUInt16(sign, EndianType.Big);
            valueByteBlock.WriteUInt16(protocolId, EndianType.Big);
            valueByteBlock.WriteUInt16(6, EndianType.Big);
        }
        valueByteBlock.WriteByte(mAddress.Station);
        valueByteBlock.WriteByte(6);
        valueByteBlock.WriteUInt16(mAddress.AddressStart, EndianType.Big);
        valueByteBlock.Write(values);

        if (modbusType == ModbusTypeEnum.ModbusRtu)
        {
            valueByteBlock.Write(CRC16Utils.Crc16Only(valueByteBlock.Memory));
        }

        return valueByteBlock.Memory;
    }

    #endregion 报文构建
}
