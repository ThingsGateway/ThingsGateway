//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

namespace ThingsGateway.Foundation;

/// <summary>
/// 常用转换
/// </summary>
public class DataTransUtil
{
    /// <summary>
    /// 字节数据转化成16进制表示的字符串
    /// </summary>
    /// <param name="InBytes">字节数组</param>
    /// <param name="segment">分割符</param>
    /// <returns>返回的字符串</returns>
    public static string ByteToHexString(byte[] InBytes, char segment = default) => ByteToHexString(InBytes, segment, 0);

    /// <summary>
    /// 将字节数组转换为十六进制表示的字符串
    /// </summary>
    /// <param name="inBytes">输入的字节数组</param>
    /// <param name="offset">offset</param>
    /// <param name="length">length</param>
    /// <param name="segment">用于分隔每个字节的字符</param>
    /// <param name="newLineCount">指定在何处换行，设为0则不换行</param>
    /// <returns>转换后的十六进制字符串</returns>
    public static string ByteToHexString(byte[] inBytes, int offset, int length, char segment = default, int newLineCount = 0)
    {
        if (inBytes == null || inBytes.Length == 0)
            return string.Empty;

        StringBuilder stringBuilder = new();
        var len = length + offset;
        for (int i = offset; i < len; i++)
        {
            // 将字节转换为两位十六进制数并追加到字符串构建器中
            stringBuilder.Append(inBytes[i].ToString("X2"));

            // 如果设置了分隔符并且不是最后一个字节，则添加分隔符
            if (segment != char.MinValue && i < len - 1)
                stringBuilder.Append(segment);

            // 如果设置了换行数且当前位置满足换行条件，则换行
            if (newLineCount > 0 && (i + 1) % newLineCount == 0)
                stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 将字节数组转换为十六进制表示的字符串
    /// </summary>
    /// <param name="inBytes">输入的字节数组</param>
    /// <param name="segment">用于分隔每个字节的字符</param>
    /// <param name="newLineCount">指定在何处换行，设为0则不换行</param>
    /// <returns>转换后的十六进制字符串</returns>
    public static string ByteToHexString(ReadOnlySpan<byte> inBytes, char segment = default, int newLineCount = 0)
    {
        if (inBytes.Length == 0)
            return string.Empty;

        StringBuilder stringBuilder = new();
        var len = inBytes.Length;
        for (int i = 0; i < len; i++)
        {
            // 将字节转换为两位十六进制数并追加到字符串构建器中
            stringBuilder.Append(inBytes[i].ToString("X2"));

            // 如果设置了分隔符并且不是最后一个字节，则添加分隔符
            if (segment != char.MinValue && i < len - 1)
                stringBuilder.Append(segment);

            // 如果设置了换行数且当前位置满足换行条件，则换行
            if (newLineCount > 0 && (i + 1) % newLineCount == 0)
                stringBuilder.AppendLine();
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取Bcd值
    /// </summary>
    public static byte GetBcdCodeFromChar(char value, BcdFormatEnum format)
    {
        return format switch
        {
            BcdFormatEnum.C8421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C5421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 8,
                '6' => 9,
                '7' => 10,
                '8' => 11,
                '9' => 12,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C2421 => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 11,
                '6' => 12,
                '7' => 13,
                '8' => 14,
                '9' => 15,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.C3 => value switch
            {
                '0' => 3,
                '1' => 4,
                '2' => 5,
                '3' => 6,
                '4' => 7,
                '5' => 8,
                '6' => 9,
                '7' => 10,
                '8' => 11,
                '9' => 12,
                _ => byte.MaxValue,
            },
            BcdFormatEnum.Gray => value switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 3,
                '3' => 2,
                '4' => 6,
                '5' => 7,
                '6' => 5,
                '7' => 4,
                '8' => 12,
                '9' => 8,
                _ => byte.MaxValue,
            },
            _ => byte.MaxValue,
        };
    }

    /// <summary>
    /// 获取Bcd值
    /// </summary>
    public static string GetBcdFromByte(int value, BcdFormatEnum format)
    {
        return format switch
        {
            BcdFormatEnum.C8421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                5 => "5",
                6 => "6",
                7 => "7",
                8 => "8",
                9 => "9",
                _ => "*",
            },
            BcdFormatEnum.C5421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                8 => "5",
                9 => "6",
                10 => "7",
                11 => "8",
                12 => "9",
                _ => "*",
            },
            BcdFormatEnum.C2421 => value switch
            {
                0 => "0",
                1 => "1",
                2 => "2",
                3 => "3",
                4 => "4",
                11 => "5",
                12 => "6",
                13 => "7",
                14 => "8",
                15 => "9",
                _ => "*",
            },
            BcdFormatEnum.C3 => value switch
            {
                3 => "0",
                4 => "1",
                5 => "2",
                6 => "3",
                7 => "4",
                8 => "5",
                9 => "6",
                10 => "7",
                11 => "8",
                12 => "9",
                _ => "*",
            },
            BcdFormatEnum.Gray => value switch
            {
                0 => "0",
                1 => "1",
                2 => "3",
                3 => "2",
                4 => "7",
                5 => "6",
                6 => "4",
                7 => "5",
                8 => "9",
                12 => "8",
                _ => "*",
            },
            _ => "*",
        };
    }

    /// <summary>
    /// 根据指定的字节数组和Bcd格式返回对应的Bcd值
    /// </summary>
    /// <param name="buffer">输入的字节数组</param>
    /// <param name="format">Bcd格式枚举</param>
    /// <returns>转换后的Bcd值字符串</returns>
    public static string GetBcdValue(ReadOnlySpan<byte> buffer, BcdFormatEnum format)
    {
        // 用于存储最终的Bcd值的字符串构建器
        StringBuilder stringBuilder = new();

        // 遍历字节数组进行Bcd值计算
        for (int index = 0; index < buffer.Length; ++index)
        {
            // 获取当前字节的低四位和高四位
            int num1 = buffer[index] & 15;
            int num2 = buffer[index] >> 4;

            // 根据指定的Bcd格式将每个字节转换为Bcd并追加到字符串构建器中
            stringBuilder.Append(GetBcdFromByte(num2, format));
            stringBuilder.Append(GetBcdFromByte(num1, format));
        }

        // 返回最终的Bcd值字符串
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 根据给定的Bcd值字符串和Bcd格式返回对应的字节数组
    /// </summary>
    /// <param name="value">Bcd值字符串</param>
    /// <param name="format">Bcd格式枚举</param>
    /// <returns>转换后的字节数组</returns>
    public static byte[] GetBytesFromBCD(string value, BcdFormatEnum format)
    {
        // 如果输入字符串为空，则返回空字节数组
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<byte>();
        }

        // 计算输出字节数组的长度
        int length = value.Length % 2 == 1 ? (value.Length / 2) + 1 : value.Length / 2;
        byte[] bytesFromBcd = new byte[length];

        // 遍历输入的Bcd值字符串进行转换
        for (int index = 0; index < length; ++index)
        {
            // 将每两个字符转换为一个字节
            bytesFromBcd[index] = (byte)(bytesFromBcd[index] | ((uint)GetBcdCodeFromChar(value[2 * index], format) << 4));

            // 如果还有下一个字符，则将其转换为字节的低四位
            if ((2 * index) + 1 < value.Length)
            {
                bytesFromBcd[index] = (byte)(bytesFromBcd[index] | (uint)GetBcdCodeFromChar(value[(2 * index) + 1], format));
            }
        }

        // 返回转换后的字节数组
        return bytesFromBcd;
    }

    /// <summary>
    /// 返回bit代表的数据
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static byte GetDataByBitIndex(int offset)
    {
        return offset switch
        {
            0 => 1,
            1 => 2,
            2 => 4,
            3 => 8,
            4 => 16,
            5 => 32,
            6 => 64,
            7 => 128,
            _ => 0,
        };
    }

    /// <summary>
    /// 16进制Char转int
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    public static int GetIntByHexChar(char ch)
    {
        return ch switch
        {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            '3' => 3,
            '4' => 4,
            '5' => 5,
            '6' => 6,
            '7' => 7,
            '8' => 8,
            '9' => 9,
            'A' or 'a' => 10,
            'B' or 'b' => 11,
            'C' or 'c' => 12,
            'D' or 'd' => 13,
            'E' or 'e' => 14,
            'F' or 'f' => 15,
            _ => -1,
        };
    }

    /// <summary>
    /// 将十六进制字符串转换为对应的字节数组
    /// </summary>
    /// <param name="hex">输入的十六进制字符串</param>
    /// <returns>转换后的字节数组</returns>
    public static byte[] HexStringToBytes(string hex)
    {
        // 创建内存流用于存储转换后的字节数据
        using var bytes = new MemoryStream();

        // 遍历十六进制字符串进行转换
        for (int index = 0; index < hex.Length; ++index)
        {
            // 检查当前字符和下一个字符是否都在合法的十六进制范围内
            if (index + 1 < hex.Length && GetHexCharIndex(hex[index]) <= 0x0F && GetHexCharIndex(hex[index + 1]) <= 0x0F)
            {
                // 计算两个十六进制字符对应的字节值并写入内存流
                bytes.WriteByte((byte)(GetHexCharIndex(hex[index]) * 16 + GetHexCharIndex(hex[index + 1])));
                ++index; // 跳过下一个字符，因为已经处理过了
            }
        }

        return bytes.ToArray();
    }

    /// <summary>
    /// 拼接任意个泛型数组为一个总的泛型数组对象。
    /// </summary>
    /// <typeparam name="T">数组的类型信息</typeparam>
    /// <param name="arrays">任意个长度的数组</param>
    /// <returns>拼接之后的最终的结果对象</returns>
    public static T[] SpliceArray<T>(params T[][] arrays)
    {
        // 初始化一个动态数组用于存储拼接后的结果
        List<T> resultList = new List<T>();

        // 遍历输入的数组
        foreach (T[] array in arrays)
        {
            // 如果数组不为空且长度不为0，则将其元素添加到结果数组中
            if (array != null && array.Length != 0)
            {
                resultList.AddRange(array);
            }
        }

        // 将动态数组转换为静态数组并返回
        return resultList.ToArray();
    }

    /// <summary>
    /// 将整数进行有效的拆分成数组，指定每个元素的最大值
    /// </summary>
    /// <param name="integer">整数信息</param>
    /// <param name="everyLength">单个的数组长度</param>
    /// <returns>拆分后的数组长度</returns>
    public static int[] SplitIntegerToArray(int integer, int everyLength)
    {
        int[] array = new int[(integer / everyLength) + (integer % everyLength == 0 ? 0 : 1)];
        for (int index = 0; index < array.Length; ++index)
            array[index] = index != array.Length - 1 ? everyLength : (integer % everyLength == 0 ? everyLength : integer % everyLength);
        return array;
    }

    private static byte GetHexCharIndex(char ch)
    {
        return ch switch
        {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            '3' => 3,
            '4' => 4,
            '5' => 5,
            '6' => 6,
            '7' => 7,
            '8' => 8,
            '9' => 9,
            'A' or 'a' => 0x0a,
            'B' or 'b' => 0x0b,
            'C' or 'c' => 0x0c,
            'D' or 'd' => 0x0d,
            'E' or 'e' => 0x0e,
            'F' or 'f' => 0x0f,
            _ => 0x10,
        };
    }
}
