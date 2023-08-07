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
    /// <returns>返回的字符串</returns>
    public static string ByteToHexString(byte[] InBytes) => DataTransUtil.ByteToHexString(InBytes, char.MinValue);

    /// <summary>
    /// 字节数据转化成16进制表示的字符串
    /// </summary>
    /// <param name="InBytes">字节数组</param>
    /// <param name="segment">分割符</param>
    /// <returns>返回的字符串</returns>
    public static string ByteToHexString(byte[] InBytes, char segment) => DataTransUtil.ByteToHexString(InBytes, segment, 0);

    /// <summary>
    /// 字节数据转化成16进制表示的字符串
    /// </summary>
    /// <param name="InBytes">字节数组</param>
    /// <param name="segment">分割符</param>
    /// <param name="newLineCount">每隔指定数量的时候进行换行</param>
    /// <returns>返回的字符串</returns>
    public static string ByteToHexString(byte[] InBytes, char segment, int newLineCount)
    {
        if (InBytes == null)
            return string.Empty;
        StringBuilder stringBuilder = new();
        long num = 0;
        foreach (byte inByte in InBytes)
        {
            if (segment == char.MinValue)
                stringBuilder.Append(string.Format("{0:X2}", inByte));
            else
                stringBuilder.Append(string.Format("{0:X2}{1}", inByte, segment));
            ++num;
            if (newLineCount > 0 && num >= newLineCount)
            {
                stringBuilder.Append(Environment.NewLine);
                num = 0L;
            }
        }
        if (segment != char.MinValue && stringBuilder.Length > 1 && stringBuilder[stringBuilder.Length - 1] == segment)
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 字符串数据转化成16进制表示的字符串
    /// </summary>
    /// <param name="InString">输入的字符串数据</param>
    public static string ByteToHexString(string InString) => DataTransUtil.ByteToHexString(Encoding.Unicode.GetBytes(InString));

    /// <summary>根据当前的位偏移地址及读取位长度信息，计算出实际的字节索引，字节数，字节位偏移</summary>
    /// <param name="addressStart">起始地址</param>
    /// <param name="length">读取的长度</param>
    /// <param name="newStart">返回的新的字节的索引，仍然按照位单位</param>
    /// <param name="byteLength">字节长度</param>
    /// <param name="offset">当前偏移的信息</param>
    public static void CalculateStartBitIndexAndLength(
      int addressStart,
      ushort length,
      out int newStart,
      out ushort byteLength,
      out int offset)
    {
        byteLength = (ushort)(((addressStart + length - 1) / 8) - (addressStart / 8) + 1);
        offset = addressStart % 8;
        newStart = addressStart - offset;
    }

    /// <summary>
    /// 获取BCD值
    /// </summary>
    public static byte GetBcdCodeFromChar(char value, BcdFormat format)
    {
        return format switch
        {
            BcdFormat.C8421 => value switch
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
            BcdFormat.C5421 => value switch
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
            BcdFormat.C2421 => value switch
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
            BcdFormat.C3 => value switch
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
            BcdFormat.Gray => value switch
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
    /// 获取BCD值
    /// </summary>
    public static string GetBcdFromByte(int value, BcdFormat format)
    {
        return format switch
        {
            BcdFormat.C8421 => value switch
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
            BcdFormat.C5421 => value switch
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
            BcdFormat.C2421 => value switch
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
            BcdFormat.C3 => value switch
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
            BcdFormat.Gray => value switch
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
    /// 获取BCD值
    /// </summary>
    public static string GetBCDValue(byte[] buffer, BcdFormat format)
    {
        StringBuilder stringBuilder = new();
        for (int index = 0; index < buffer.Length; ++index)
        {
            int num1 = buffer[index] & 15;
            int num2 = buffer[index] >> 4;
            stringBuilder.Append(GetBcdFromByte(num2, format));
            stringBuilder.Append(GetBcdFromByte(num1, format));
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 获取BCD值
    /// </summary>
    public static byte[] GetBytesFromBCD(string value, BcdFormat format)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new byte[0];
        }

        int length = value.Length % 2 == 1 ? (value.Length / 2) + 1 : value.Length / 2;
        byte[] bytesFromBcd = new byte[length];
        for (int index = 0; index < length; ++index)
        {
            bytesFromBcd[index] = (byte)(bytesFromBcd[index] | ((uint)GetBcdCodeFromChar(value[2 * index], format) << 4));
            if ((2 * index) + 1 < value.Length)
            {
                bytesFromBcd[index] = (byte)(bytesFromBcd[index] | (uint)GetBcdCodeFromChar(value[(2 * index) + 1], format));
            }
        }
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
    /// 拼接任意个泛型数组为一个总的泛型数组对象，采用深度拷贝实现。
    /// </summary>
    /// <typeparam name="T">数组的类型信息</typeparam>
    /// <param name="arrays">任意个长度的数组</param>
    /// <returns>拼接之后的最终的结果对象</returns>
    public static T[] SpliceArray<T>(params T[][] arrays)
    {
        int length = 0;
        for (int index = 0; index < arrays.Length; ++index)
        {
            T[] array = arrays[index];
            if (array != null && array.Length != 0)
                length += arrays[index].Length;
        }
        int index1 = 0;
        T[] objArray = new T[length];
        for (int index2 = 0; index2 < arrays.Length; ++index2)
        {
            T[] array = arrays[index2];
            if (array != null && array.Length != 0)
            {
                arrays[index2].CopyTo(objArray, index1);
                index1 += arrays[index2].Length;
            }
        }
        return objArray;
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

    /// <summary>
    /// 切割当前的地址数据信息，根据读取的长度来分割成多次不同的读取内容，需要指定地址，总的读取长度，切割读取长度
    /// </summary>
    public static KeyValuePair<int[], int[]> SplitReadLength(
      int address,
      ushort length,
      ushort max)
    {
        int[] array = SplitIntegerToArray(length, max);
        int[] numArray = new int[array.Length];
        for (int index = 0; index < numArray.Length; ++index)
        {
            numArray[index] = index != 0 ? numArray[index - 1] + array[index - 1] : address;
        }
        return new(numArray, array);
    }
}