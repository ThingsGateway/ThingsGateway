/// <summary>
/// 有一部分参考来自Hsl7或TouchSocket
/// </summary>
public class DataHelper
{
    /// <summary>
    /// 字节数据转化成16进制表示的字符串
    /// </summary>
    /// <param name="InBytes">字节数组</param>
    /// <returns>返回的字符串</returns>
    public static string ByteToHexString(byte[] InBytes) => DataHelper.ByteToHexString(InBytes, char.MinValue);

    /// <summary>
    /// 字节数据转化成16进制表示的字符串
    /// </summary>
    /// <param name="InBytes">字节数组</param>
    /// <param name="segment">分割符</param>
    /// <returns>返回的字符串</returns>
    public static string ByteToHexString(byte[] InBytes, char segment) => DataHelper.ByteToHexString(InBytes, segment, 0);

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
        StringBuilder stringBuilder = new StringBuilder();
        long num = 0;
        foreach (byte inByte in InBytes)
        {
            if (segment == char.MinValue)
                stringBuilder.Append(string.Format("{0:X2}", (object)inByte));
            else
                stringBuilder.Append(string.Format("{0:X2}{1}", (object)inByte, (object)segment));
            ++num;
            if (newLineCount > 0 && num >= (long)newLineCount)
            {
                stringBuilder.Append(Environment.NewLine);
                num = 0L;
            }
        }
        if (segment != char.MinValue && stringBuilder.Length > 1 && (int)stringBuilder[stringBuilder.Length - 1] == (int)segment)
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
        return stringBuilder.ToString();
    }

    /// <summary>
    /// 字符串数据转化成16进制表示的字符串
    /// </summary>
    /// <param name="InString">输入的字符串数据</param>
    public static string ByteToHexString(string InString) => DataHelper.ByteToHexString(Encoding.Unicode.GetBytes(InString));

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
        switch (format)
        {
            case BcdFormat.C8421:
                switch (value)
                {
                    case '0':
                        return 0;

                    case '1':
                        return 1;

                    case '2':
                        return 2;

                    case '3':
                        return 3;

                    case '4':
                        return 4;

                    case '5':
                        return 5;

                    case '6':
                        return 6;

                    case '7':
                        return 7;

                    case '8':
                        return 8;

                    case '9':
                        return 9;

                    default:
                        return byte.MaxValue;
                }
            case BcdFormat.C5421:
                switch (value)
                {
                    case '0':
                        return 0;

                    case '1':
                        return 1;

                    case '2':
                        return 2;

                    case '3':
                        return 3;

                    case '4':
                        return 4;

                    case '5':
                        return 8;

                    case '6':
                        return 9;

                    case '7':
                        return 10;

                    case '8':
                        return 11;

                    case '9':
                        return 12;

                    default:
                        return byte.MaxValue;
                }
            case BcdFormat.C2421:
                switch (value)
                {
                    case '0':
                        return 0;

                    case '1':
                        return 1;

                    case '2':
                        return 2;

                    case '3':
                        return 3;

                    case '4':
                        return 4;

                    case '5':
                        return 11;

                    case '6':
                        return 12;

                    case '7':
                        return 13;

                    case '8':
                        return 14;

                    case '9':
                        return 15;

                    default:
                        return byte.MaxValue;
                }
            case BcdFormat.C3:
                switch (value)
                {
                    case '0':
                        return 3;

                    case '1':
                        return 4;

                    case '2':
                        return 5;

                    case '3':
                        return 6;

                    case '4':
                        return 7;

                    case '5':
                        return 8;

                    case '6':
                        return 9;

                    case '7':
                        return 10;

                    case '8':
                        return 11;

                    case '9':
                        return 12;

                    default:
                        return byte.MaxValue;
                }
            case BcdFormat.Gray:
                switch (value)
                {
                    case '0':
                        return 0;

                    case '1':
                        return 1;

                    case '2':
                        return 3;

                    case '3':
                        return 2;

                    case '4':
                        return 6;

                    case '5':
                        return 7;

                    case '6':
                        return 5;

                    case '7':
                        return 4;

                    case '8':
                        return 12;

                    case '9':
                        return 8;

                    default:
                        return byte.MaxValue;
                }
            default:
                return byte.MaxValue;
        }
    }
    /// <summary>
    /// 获取BCD值
    /// </summary>
    public static string GetBcdFromByte(int value, BcdFormat format)
    {
        switch (format)
        {
            case BcdFormat.C8421:
                switch (value)
                {
                    case 0:
                        return "0";

                    case 1:
                        return "1";

                    case 2:
                        return "2";

                    case 3:
                        return "3";

                    case 4:
                        return "4";

                    case 5:
                        return "5";

                    case 6:
                        return "6";

                    case 7:
                        return "7";

                    case 8:
                        return "8";

                    case 9:
                        return "9";

                    default:
                        return "*";
                }
            case BcdFormat.C5421:
                switch (value)
                {
                    case 0:
                        return "0";

                    case 1:
                        return "1";

                    case 2:
                        return "2";

                    case 3:
                        return "3";

                    case 4:
                        return "4";

                    case 8:
                        return "5";

                    case 9:
                        return "6";

                    case 10:
                        return "7";

                    case 11:
                        return "8";

                    case 12:
                        return "9";

                    default:
                        return "*";
                }
            case BcdFormat.C2421:
                switch (value)
                {
                    case 0:
                        return "0";

                    case 1:
                        return "1";

                    case 2:
                        return "2";

                    case 3:
                        return "3";

                    case 4:
                        return "4";

                    case 11:
                        return "5";

                    case 12:
                        return "6";

                    case 13:
                        return "7";

                    case 14:
                        return "8";

                    case 15:
                        return "9";

                    default:
                        return "*";
                }
            case BcdFormat.C3:
                switch (value)
                {
                    case 3:
                        return "0";

                    case 4:
                        return "1";

                    case 5:
                        return "2";

                    case 6:
                        return "3";

                    case 7:
                        return "4";

                    case 8:
                        return "5";

                    case 9:
                        return "6";

                    case 10:
                        return "7";

                    case 11:
                        return "8";

                    case 12:
                        return "9";

                    default:
                        return "*";
                }
            case BcdFormat.Gray:
                switch (value)
                {
                    case 0:
                        return "0";

                    case 1:
                        return "1";

                    case 2:
                        return "3";

                    case 3:
                        return "2";

                    case 4:
                        return "7";

                    case 5:
                        return "6";

                    case 6:
                        return "4";

                    case 7:
                        return "5";

                    case 8:
                        return "9";

                    case 12:
                        return "8";

                    default:
                        return "*";
                }
            default:
                return "*";
        }
    }
    /// <summary>
    /// 获取BCD值
    /// </summary>
    public static string GetBCDValue(byte[] buffer, BcdFormat format)
    {
        StringBuilder stringBuilder = new StringBuilder();
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
        switch (offset)
        {
            case 0:
                return 1;

            case 1:
                return 2;

            case 2:
                return 4;

            case 3:
                return 8;

            case 4:
                return 16;

            case 5:
                return 32;

            case 6:
                return 64;

            case 7:
                return 128;

            default:
                return 0;
        }
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
                arrays[index2].CopyTo((Array)objArray, index1);
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
    public static OperResult<int[], int[]> SplitReadLength(
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

        return OperResult.CreateSuccessResult<int[], int[]>(numArray, array);
    }
}