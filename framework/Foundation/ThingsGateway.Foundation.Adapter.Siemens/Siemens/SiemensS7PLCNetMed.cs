﻿#region copyright
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

namespace ThingsGateway.Foundation.Adapter.Siemens;

public partial class SiemensS7PLC : ReadWriteDevicesTcpClientBase
{
    private static OperResult<byte[]> GetWriteBitCommand(string address, bool data)
    {
        try
        {
            var result = SiemensAddress.ParseFrom(address);
            return SiemensHelper.GetWriteBitCommand(result, data);

        }
        catch (Exception ex)
        {
            return new(ex);
        }

    }


    private OperResult<List<byte[]>> GetReadByteCommand(string address, int length)
    {
        try
        {
            var from = SiemensAddress.ParseFrom(address, length);
            ushort num1 = 0;
            var listBytes = new List<byte[]>();
            while (num1 < length)
            {
                //pdu长度，重复生成报文，直至全部生成
                ushort num2 = (ushort)Math.Min(length - num1, pdu_length);
                from.Length = num2;
                var result = GetReadByteCommand(new SiemensAddress[1] { from });
                if (!result.IsSuccess) return new(result);
                listBytes.AddRange(result.Content);
                num1 += num2;
                if (from.DataCode == (byte)S7WordLength.Timer || from.DataCode == (byte)S7WordLength.Counter)
                {
                    from.Address += num2 / 2;
                }
                else
                {
                    from.Address += num2 * 8;
                }
            }
            return OperResult.CreateSuccessResult(listBytes);
        }
        catch (Exception ex)
        {
            return new OperResult<List<byte[]>>(ex);
        }

    }

    private OperResult<List<byte[]>> GetReadByteCommand(SiemensAddress[] siemensAddress)
    {
        if (siemensAddress.Length <= 19)
        {
            return ByteTransformUtil.GetResultFromBytes(SiemensHelper.GetReadCommand(siemensAddress), m => new List<byte[]>() { m });
        }

        List<byte[]> byteList = new();
        List<SiemensAddress[]> s7AddressDataArrayList = siemensAddress.ArraySplitByLength(19);
        for (int index = 0; index < s7AddressDataArrayList.Count; ++index)
        {
            var result = GetReadByteCommand(s7AddressDataArrayList[index]);
            if (!result.IsSuccess)
            {
                return result;
            }
            byteList.AddRange(result.Content);
        }
        return OperResult.CreateSuccessResult(byteList);
    }

    private OperResult<List<byte[]>> GetWriteByteCommand(string address, byte[] value)
    {
        try
        {
            var s_Address = SiemensAddress.ParseFrom(address);

            return GetWriteByteCommand(s_Address, value);
        }
        catch (Exception ex)
        {
            return new(ex);
        }
    }
    /// <summary>
    /// DefalutConverter
    /// </summary>
    public static ThingsGatewayBitConverter DefalutConverter = new(BitConverter.IsLittleEndian ? EndianType.Little : EndianType.Big);
    private OperResult<List<byte[]>> GetWriteByteCommand(SiemensAddress address, byte[] value)
    {
        int length1 = value.Length;
        ushort index = 0;
        List<byte[]> bytes = new();
        while (index < length1)
        {
            //pdu长度，重复生成报文，直至全部生成
            ushort length2 = (ushort)Math.Min(length1 - index, pdu_length);
            byte[] data = DefalutConverter.ToByte(value, index, length2);
            OperResult<byte[]> result1 = SiemensHelper.GetWriteByteCommand(address, data);
            if (!result1.IsSuccess)
            {
                return new(result1);
            }
            bytes.Add(result1.Content);
            index += length2;
            address.Address += length2 * 8;
        }
        return OperResult.CreateSuccessResult(bytes);
    }

}
