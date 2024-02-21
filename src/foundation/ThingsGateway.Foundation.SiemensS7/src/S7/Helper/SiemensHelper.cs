//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

using ThingsGateway.Foundation.Extension.Generic;

using TouchSocket.Core;

namespace ThingsGateway.Foundation.SiemensS7;

internal partial class SiemensHelper
{
    #region 验证

    //internal static OperResult<byte[]> AnalysisReadBit(byte[] content)
    //{
    //    int length = 1;
    //    if (content.Length < 21 || content[20] != 1)
    //        return new OperResult<byte[]>("数据块长度校验失败");
    //    byte[] numArray = new byte[length];
    //    if (content[21] == byte.MaxValue && content[22] == 3)//Bit:3;Byte:4;Counter或者Timer:9
    //    {
    //        numArray[0] = content[25];//+4
    //    }
    //    else
    //    {
    //        return new OperResult<byte[]>((int)content[21] + GetCpuError(content[21]));
    //    }
    //    return OperResult.CreateSuccessResult<byte[]>(numArray);
    //}

    internal static OperResult<byte[], FilterResult> AnalysisReadByte(byte[] sends, byte[] content)
    {
        int length = 0;
        int itemLen = (sends.Length - 19) / 12;

        //添加错误代码校验
        if (content[17] + content[18] > 0)
        {
            return new($"PLC返回错误，错误类型{content[17].ToString("X2")}错误代码：{content[18].ToString("X2")}")
            {
                Content2 = FilterResult.Success
            };
        }
        if (content.Length < 21)
        {
            return new($"长度不足")
            {
                Content2 = FilterResult.Cache
            };
        }
        if (content.Length < 25 + content[20])
        {
            return new($"长度不足")
            {
                Content2 = FilterResult.Cache
            };
        }
        //添加返回代码校验
        if (content[21] != 0xff)
        {
            return new($"PLC返回错误，返回代码{content[21].ToString("X2")}")
            {
                Content2 = FilterResult.Success
            };
        }

        for (int index = 0; index < itemLen; index++)
        {
            if (sends[22 + (index * 12)] >= (byte)S7WordLength.Word)
            {
                length += ((sends[23 + (index * 12)] * 256) + sends[24 + (index * 12)]) * 2;
            }
            else
            {
                length += (sends[23 + (index * 12)] * 256) + sends[24 + (index * 12)];
            }
        }

        if (content[20] != itemLen)
        {
            return new("数据块长度校验失败");
        }

        byte[] dataArray = new byte[length];
        int index1 = 0;
        int dataIndex = 0;
        for (int index2 = 21; index2 < content.Length; index2++)
        {
            if (index2 + 1 < content.Length)
            {
                int s7len;
                if (sends[22 + (index1 * 12)] >= (byte)S7WordLength.Word)
                {
                    s7len = ((sends[23 + (index1 * 12)] * 256) + sends[24 + (index1 * 12)]) * 2;
                }
                else
                {
                    s7len = (sends[23 + (index1 * 12)] * 256) + sends[24 + (index1 * 12)];
                }
                if (content[index2] == byte.MaxValue && content[index2 + 1] == 4)//Bit:3;Byte:4;Counter或者Timer:9
                {
                    Array.Copy(content, index2 + 4, dataArray, dataIndex, s7len);
                    index2 += s7len + 3;
                    dataIndex += s7len;
                    index1++;
                }
                else if (content[index2] == byte.MaxValue && content[index2 + 1] == 9)//Counter或者Timer:9
                {
                    int num = (content[index2 + 2] * 256) + content[index2 + 3];
                    if (num % 3 == 0)
                    {
                        for (int index3 = 0; index3 < num / 3; index3++)
                        {
                            Array.Copy(content, index2 + 5 + (3 * index3), dataArray, dataIndex, 2);
                            dataIndex += 2;
                        }
                    }
                    else
                    {
                        for (int index4 = 0; index4 < num / 5; index4++)
                        {
                            Array.Copy(content, index2 + 7 + (5 * index4), dataArray, dataIndex, 2);
                            dataIndex += 2;
                        }
                    }
                    index2 += num + 4;
                    index1++;
                }
                else
                {
                    return new((int)content[index2] + GetCpuError(content[index2]))
                    {
                        Content2 = FilterResult.Success
                    };
                }
            }
        }
        return new OperResult<byte[], FilterResult>() { Content = dataArray, Content2 = FilterResult.Success };
    }

    internal static OperResult<byte[], FilterResult> AnalysisWrite(byte[] content)
    {
        if (content.Length < 22)
        {
            return new("长度不足")
            {
                Content2 = FilterResult.Success
            };
        }

        byte err = content[21];
        if (err != byte.MaxValue)
        {
            return new($"错误代码：{(int)content[21]}描述：{GetCpuError(content[21])}")
            {
                Content2 = FilterResult.Success
            };
        }
        else
        {
            return new OperResult<byte[], FilterResult> { Content = content, Content2 = FilterResult.Success };
        }
    }

    internal static byte[] GetReadCommand(SiemensAddress[] siemensAddress)
    {
        int len = siemensAddress.Length;
        int telegramLen = len * 12 + 19;
        int parameterLen = len * 12 + 2;

        byte[] numArray = new byte[telegramLen];

        Array.Copy(S7_MULRW_HEADER, 0, numArray, 0, S7_MULRW_HEADER.Length);
        numArray[2] = (byte)(telegramLen / 256);
        numArray[3] = (byte)(telegramLen % 256);
        numArray[13] = (byte)(parameterLen / 256);
        numArray[14] = (byte)(parameterLen % 256);
        numArray[18] = (byte)len;

        for (int index = 0; index < len; index++)
        {
            Array.Copy(S7_MULRD_ITEM, 0, numArray, 19 + (index * 12), S7_MULRD_ITEM.Length);
            if (siemensAddress[index].DataCode == (byte)S7WordLength.Counter || siemensAddress[index].DataCode == (byte)S7WordLength.Timer)
            {
                numArray[22 + (index * 12)] = siemensAddress[index].DataCode;
                numArray[23 + (index * 12)] = (byte)(siemensAddress[index].Length / 256);
                numArray[24 + (index * 12)] = (byte)(siemensAddress[index].Length % 256);
            }
            else
            {
                numArray[22 + (index * 12)] = (byte)S7WordLength.Byte;
                numArray[23 + (index * 12)] = (byte)(siemensAddress[index].Length / 256);
                numArray[24 + (index * 12)] = (byte)(siemensAddress[index].Length % 256);
            }
            numArray[25 + (index * 12)] = (byte)(siemensAddress[index].DbBlock / 256U);
            numArray[26 + (index * 12)] = (byte)(siemensAddress[index].DbBlock % 256U);
            numArray[27 + (index * 12)] = siemensAddress[index].DataCode;
            numArray[28 + (index * 12)] = (byte)(siemensAddress[index].AddressStart / 256 / 256 % 256);
            numArray[29 + (index * 12)] = (byte)(siemensAddress[index].AddressStart / 256 % 256);
            numArray[30 + (index * 12)] = (byte)(siemensAddress[index].AddressStart % 256);
        }
        return numArray;
    }

    internal static byte[] GetWriteBitCommand(SiemensAddress address, bool data)
    {
        int len = 1;
        int telegramLen = 16 + 19 + len;//最后的1是写入值的byte数量
        int parameterLen = 12 + 2;

        byte[] numArray = new byte[telegramLen];

        Array.Copy(S7_MULRW_HEADER, 0, numArray, 0, S7_MULRW_HEADER.Length);
        numArray[2] = (byte)(telegramLen / 256);
        numArray[3] = (byte)(telegramLen % 256);
        numArray[13] = (byte)(parameterLen / 256);
        numArray[14] = (byte)(parameterLen % 256);
        numArray[15] = (byte)((4 + len) / 256);
        numArray[16] = (byte)((4 + len) % 256);
        numArray[17] = 5;
        numArray[18] = (byte)1;
        //写入Item与读取大致相同
        numArray[19] = (byte)18;
        numArray[20] = (byte)10;
        numArray[21] = (byte)16;
        numArray[22] = (byte)S7WordLength.Bit;
        numArray[23] = (byte)(len / 256);
        numArray[24] = (byte)(len % 256);
        numArray[25] = (byte)(address.DbBlock / 256U);
        numArray[26] = (byte)(address.DbBlock % 256U);
        numArray[27] = (byte)address.DataCode;
        numArray[28] = (byte)((address.AddressStart + address.BitCode) / 256 / 256);
        numArray[29] = (byte)((address.AddressStart + address.BitCode) / 256);
        numArray[30] = (byte)((address.AddressStart + address.BitCode) % 256);
        //后面跟的是写入的数据信息
        numArray[31] = 0;
        numArray[32] = 3;//Bit:3;Byte:4;Counter或者Timer:9
        numArray[33] = (byte)(len / 256);
        numArray[34] = (byte)(len % 256);
        numArray[35] = (byte)(data ? 1 : 0);

        return numArray;
    }

    internal static byte[] GetWriteByteCommand(SiemensAddress address, byte[] data)
    {
        int len = data.Length;
        int telegramLen = 16 + 19 + len;//最后的1是写入值的byte数量
        int parameterLen = 12 + 2;

        byte[] numArray = new byte[telegramLen];

        Array.Copy(S7_MULRW_HEADER, 0, numArray, 0, S7_MULRW_HEADER.Length);
        numArray[2] = (byte)(telegramLen / 256);
        numArray[3] = (byte)(telegramLen % 256);
        numArray[13] = (byte)(parameterLen / 256);
        numArray[14] = (byte)(parameterLen % 256);
        numArray[15] = (byte)((4 + len) / 256);
        numArray[16] = (byte)((4 + len) % 256);
        numArray[17] = 5;
        numArray[18] = (byte)1;
        //写入Item与读取大致相同
        numArray[19] = (byte)18;
        numArray[20] = (byte)10;
        numArray[21] = (byte)16;
        numArray[22] = (byte)S7WordLength.Byte;
        numArray[23] = (byte)(len / 256);
        numArray[24] = (byte)(len % 256);

        numArray[25] = (byte)(address.DbBlock / 256U);
        numArray[26] = (byte)(address.DbBlock % 256U);
        numArray[27] = (byte)address.DataCode;
        numArray[28] = (byte)(address.AddressStart / 256 / 256);
        numArray[29] = (byte)(address.AddressStart / 256);
        numArray[30] = (byte)(address.AddressStart % 256);
        //后面跟的是写入的数据信息
        numArray[31] = 0;
        numArray[32] = 4;//Bit:3;Byte:4;Counter或者Timer:9
        numArray[33] = (byte)(len * 8 / 256);
        numArray[34] = (byte)(len * 8 % 256);
        data.CopyTo(numArray, 35);

        return numArray;
    }

    internal static async Task<OperResult<string>> ReadStringAsync(SiemensS7Master plc, string address, Encoding encoding, CancellationToken cancellationToken)
    {
        //先读取一次获取长度，再读取实际值
        if (plc.SiemensType != SiemensTypeEnum.S200Smart)
        {
            var result1 = await plc.ReadAsync(address, 2, cancellationToken);
            if (!result1.IsSuccess)
            {
                return new(result1);
            }
            if (result1.Content[0] == (byte)0 || result1.Content[0] == byte.MaxValue)
            {
                return new OperResult<string>("在PLC中不是字符串类型");
            }
            var result2 = await plc.ReadAsync(address, 2 + result1.Content[1], cancellationToken);
            if (!result2.IsSuccess)
            {
                return new(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content, 2, result2.Content.Length - 2));
            }
        }
        else
        {
            var result1 = await plc.ReadAsync(address, 1, cancellationToken);
            if (!result1.IsSuccess)
                return new(result1);
            var result2 = await plc.ReadAsync(address, 1 + result1.Content[0], cancellationToken);
            if (!result2.IsSuccess)
            {
                return new(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content, 1, result2.Content.Length - 1));
            }
        }
    }

    internal static OperResult<string> ReadString(SiemensS7Master plc, string address, Encoding encoding, CancellationToken cancellationToken)
    {
        //先读取一次获取长度，再读取实际值
        if (plc.SiemensType != SiemensTypeEnum.S200Smart)
        {
            var result1 = plc.Read(address, 2, cancellationToken);
            if (!result1.IsSuccess)
            {
                return new(result1);
            }
            if (result1.Content[0] == (byte)0 || result1.Content[0] == byte.MaxValue)
            {
                return new OperResult<string>("在PLC中不是字符串类型");
            }
            var result2 = plc.Read(address, 2 + result1.Content[1], cancellationToken);
            if (!result2.IsSuccess)
            {
                return new(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content, 2, result2.Content.Length - 2));
            }
        }
        else
        {
            var result1 = plc.Read(address, 1, cancellationToken);
            if (!result1.IsSuccess)
                return new(result1);
            var result2 = plc.Read(address, 1 + result1.Content[0], cancellationToken);
            if (!result2.IsSuccess)
            {
                return new(result2);
            }
            else
            {
                return OperResult.CreateSuccessResult(encoding.GetString(result2.Content, 1, result2.Content.Length - 1));
            }
        }
    }

    internal static async Task<OperResult> WriteAsync(SiemensS7Master plc, string address, string value, Encoding encoding)
    {
        value ??= string.Empty;
        byte[] inBytes = encoding.GetBytes(value);
        //if (encoding == Encoding.Unicode)
        //    inBytes = inBytes.BytesReverseByWord();
        if (plc.SiemensType != SiemensTypeEnum.S200Smart)
        {
            OperResult<byte[]> result = await plc.ReadAsync(address, 2);
            if (!result.IsSuccess) return result;
            if (result.Content[0] == byte.MaxValue) return new OperResult<string>("在PLC中不是字符串类型");
            if (result.Content[0] == 0) result.Content[0] = 254;
            if (value.Length > result.Content[0]) return new OperResult<string>("写入值长度超限");
            return await plc.WriteAsync(
                address,
                GenericExtension.SpliceArray(new byte[2] { result.Content[0], (byte)value.Length },
                inBytes
                ));
        }
        return await plc.WriteAsync(address, GenericExtension.SpliceArray<byte>(new byte[1]
        {
    (byte) value.Length
        }, inBytes));
    }

    internal static OperResult Write(SiemensS7Master plc, string address, string value, Encoding encoding)
    {
        value ??= string.Empty;
        byte[] inBytes = encoding.GetBytes(value);
        //if (encoding == Encoding.Unicode)
        //    inBytes = inBytes.BytesReverseByWord();
        if (plc.SiemensType != SiemensTypeEnum.S200Smart)
        {
            OperResult<byte[]> result = plc.Read(address, 2);
            if (!result.IsSuccess) return result;
            if (result.Content[0] == byte.MaxValue) return new OperResult<string>("在PLC中不是字符串类型");
            if (result.Content[0] == 0) result.Content[0] = 254;
            if (value.Length > result.Content[0]) return new OperResult<string>("写入值长度超限");
            return plc.Write(
                address,
                GenericExtension.SpliceArray(new byte[2] { result.Content[0], (byte)value.Length },
                inBytes
                ));
        }
        return plc.Write(address, GenericExtension.SpliceArray<byte>(new byte[1]
        {
    (byte) value.Length
        }, inBytes));
    }

    private static string GetCpuError(ushort Error)
    {
        return Error switch
        {
            0x05 => "地址超限",
            0x06 => "返回长度无效",
            0x07 => "数据大小不匹配",
            0x0a or 0xd209 => "数据块不存在",
            0x8500 => "超出PDU大小",
            0xdc01 => "无效的值",
            0x8104 => "功能不可用",
            0xd241 => "需要密码",
            0xd602 => "无效密码",
            0xd604 or 0xd605 => "没有设置密码或已清除",
            _ => "未知错误",
        };
        ;
    }

    #endregion 验证
}