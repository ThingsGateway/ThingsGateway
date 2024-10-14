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

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public static class ByteExtensions
{
    /// <summary>
    /// 获取byte数据类型的第offset位，是否为True<br />
    /// </summary>
    /// <param name="value">byte数值</param>
    /// <param name="offset">索引位置</param>
    /// <returns>结果</returns>
    public static bool BoolOnByteIndex(this byte value, int offset)
    {
        if (offset < 0 || offset > 7)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset value must be between 0 and 7.");
        }

        byte mask = (byte)(1 << offset);
        return (value & mask) == mask;
    }

    /// <summary>
    /// 数组内容分别相加某个数字
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] BytesAdd(this byte[] bytes, int value)
    {
        if (bytes == null || bytes.Length == 0)
        {
            throw new ArgumentException("Input byte array is null or empty");
        }

        byte[] result = new byte[bytes.Length];
        for (int index = 0; index < bytes.Length; index++)
        {
            result[index] = (byte)(bytes[index] + value);
        }

        return result;
    }

    /// <summary>
    /// 数组内容分别相加某个数字
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ReadOnlySpan<byte> BytesAdd(this ReadOnlySpan<byte> bytes, int value)
    {
        byte[] result = new byte[bytes.Length];
        for (int index = 0; index < bytes.Length; index++)
        {
            result[index] = (byte)(bytes[index] + value);
        }

        return result;
    }

    /// <summary>
    /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐<br />
    /// </summary>
    /// <param name="inBytes">输入的字节信息</param>
    /// <returns>反转后的数据</returns>
    public static byte[] BytesReverseByWord(this byte[] inBytes)
    {
        if (inBytes.Length == 0)
        {
            return Array.Empty<byte>();
        }
        // 创建新数组进行补齐
        byte[] paddedBytes = inBytes.CopyArray<byte>().ArrayExpandToLengthEven();
        // 进行双字节反转
        for (int index = 0; index < paddedBytes.Length; index += 2)
        {
            byte temp = paddedBytes[index];
            paddedBytes[index] = paddedBytes[index + 1];
            paddedBytes[index + 1] = temp;
        }

        return paddedBytes;
    }

    /// <summary>
    /// 从字节数组中提取位数组，length 代表位数
    /// </summary>
    /// <param name="inBytes">原始的字节数组</param>
    /// <param name="length">想要转换的位数，如果超出字节数组长度 * 8，则自动缩小为数组最大长度</param>
    /// <returns>转换后的布尔数组</returns>
    public static bool[] ByteToBoolArray(this byte[] inBytes, int length)
    {
        // 计算字节数组能够提供的最大位数
        int maxBitLength = inBytes.Length * 8;

        // 如果指定长度超出最大位数，则将长度缩小为最大位数
        if (length > maxBitLength)
        {
            length = maxBitLength;
        }

        // 创建对应长度的布尔数组
        bool[] boolArray = new bool[length];

        // 从字节数组中提取位信息并转换为布尔值存储到布尔数组中
        for (int index = 0; index < length; ++index)
        {
            boolArray[index] = inBytes[index / 8].BoolOnByteIndex(index % 8);
        }

        return boolArray;
    }
    /// <summary>
    /// 从字节数组中提取位数组，length 代表位数
    /// </summary>
    /// <param name="inBytes">原始的字节数组</param>
    /// <param name="length">想要转换的位数，如果超出字节数组长度 * 8，则自动缩小为数组最大长度</param>
    /// <returns>转换后的布尔数组</returns>
    public static bool[] ByteToBoolArray(this Span<byte> inBytes, int length)
    {
        // 计算字节数组能够提供的最大位数
        int maxBitLength = inBytes.Length * 8;

        // 如果指定长度超出最大位数，则将长度缩小为最大位数
        if (length > maxBitLength)
        {
            length = maxBitLength;
        }

        // 创建对应长度的布尔数组
        bool[] boolArray = new bool[length];

        // 从字节数组中提取位信息并转换为布尔值存储到布尔数组中
        for (int index = 0; index < length; ++index)
        {
            boolArray[index] = inBytes[index / 8].BoolOnByteIndex(index % 8);
        }

        return boolArray;
    }

    /// <summary>
    /// 获取异或校验
    /// </summary>
    /// <param name="data"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static byte[] GetAsciiXOR(this byte[] data, int left, int right)
    {
        if (data == null || left < 0 || right < 0 || left >= data.Length || right >= data.Length || left > right)
        {
            throw new ArgumentException("Invalid input parameters");
        }

        byte tmp = data[left];
        for (int i = left + 1; i <= right; i++)
        {
            tmp ^= data[i];
        }

        return Encoding.ASCII.GetBytes(tmp.ToString("X2"));
    }

    /// <summary>
    /// 获取Byte数组的第 boolIndex 偏移的bool值，这个偏移值可以为 10，就是第 1 个字节的 第3位 <br />
    /// </summary>
    /// <param name="bytes">字节数组信息</param>
    /// <param name="boolIndex">指定字节的位偏移</param>
    /// <returns>bool值</returns>
    public static bool GetBoolByIndex(this byte[] bytes, int boolIndex)
    {
        return bytes[boolIndex / 8].BoolOnByteIndex(boolIndex % 8);
    }

    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static string ToHexString(this ArraySegment<byte> buffer, char splite = ' ')
    {
        return DataTransUtil.ByteToHexString(buffer, splite);
    }

    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static string ToHexString(this ReadOnlySpan<byte> buffer, char splite = ' ')
    {
        return DataTransUtil.ByteToHexString(buffer, splite);
    }

    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="splite"></param>
    /// <returns></returns>
    public static string ToHexString(this byte[] buffer, char splite = default)
    {
        return DataTransUtil.ByteToHexString(buffer, splite);
    }

    /// <summary>
    /// 字节数组默认转16进制字符
    /// </summary>
    /// <returns></returns>
    public static string ToHexString(this byte[] buffer, int offset, int length, char splite = ' ', int newLineCount = 0)
    {
        return DataTransUtil.ByteToHexString(buffer, offset, length, splite, newLineCount);
    }
}
