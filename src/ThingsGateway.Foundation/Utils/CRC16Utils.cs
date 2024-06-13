//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Foundation;

/// <summary>
/// CRC16验证
/// </summary>
public class CRC16Utils
{
    /// <summary>
    /// 来校验对应的接收数据的CRC校验码，默认多项式码为0xA001
    /// </summary>
    /// <param name="value">需要校验的数据，带CRC校验码</param>
    /// <returns>返回校验成功与否</returns>
    public static bool CheckCRC16(byte[] value)
    {
        return CheckCRC16(value, 0, value.Length);
    }

    /// <summary>
    /// 指定多项式码来校验对应的接收数据的CRC校验码
    /// </summary>
    /// <param name="value">需要校验的数据，带CRC校验码</param>
    /// <param name="index">计算的起始字节索引</param>
    /// <param name="length">计算的字节长度</param>
    /// <returns>返回校验成功与否</returns>
    public static bool CheckCRC16(byte[] value, int index, int length)
    {
        if (value == null || value.Length < 2)
        {
            return false;
        }

        byte[] destinationArray = new byte[length - 2];
        Array.Copy(value, index, destinationArray, 0, destinationArray.Length);
        byte[] numArray = Crc16Only(destinationArray);
        return numArray[0] == value[length - 2] && numArray[1] == value[length - 1];
    }

    /// <summary>
    /// 获取对应的数据的CRC校验码，默认多项式码为0xA001
    /// </summary>
    /// <param name="value">需要校验的数据，不包含CRC字节</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] Crc16(byte[] value)
    {
        return Crc16(value, 0, value.Length);
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="value">需要校验的数据，不包含CRC字节</param>
    /// <param name="index">计算的起始字节索引</param>
    /// <param name="length">计算的字节长度</param>
    public static byte[] Crc16(byte[] value, int index, int length)
    {
        byte[] numArray = new byte[value.Length + 2];
        Array.Copy(value, index, numArray, 0, length);
        var crc = Crc16Only(value, index, length);
        Array.Copy(crc, 0, numArray, numArray.Length - 2, 2);
        return numArray;
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="value">需要校验的数据，不包含CRC字节</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] Crc16Only(byte[] value)
    {
        return Crc16Only(value, 0, value.Length);
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="value">需要校验的数据，不包含CRC字节</param>
    /// <param name="index">计算的起始字节索引</param>
    /// <param name="length">计算的字节长度</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] Crc16Only(byte[] value,
      int index,
      int length)
    {
        return Crc16Only(value, index, length, 0xA001);
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="data">需要校验的数据，不包含CRC字节</param>
    /// <param name="index">计算的起始字节索引</param>
    /// <param name="length">计算的字节长度</param>
    /// <param name="xdapoly">多项式</param>
    /// <param name="crc16">crc16</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] Crc16Only(byte[] data, int index, int length, int xdapoly, bool crc16 = true)
    {
        int num = 65535;
        for (int i = 0; i < length; i++)
        {
            num = (num >> ((!crc16) ? 8 : 0)) ^ data[i];
            for (int j = 0; j < 8; j++)
            {
                int num2 = num & 1;
                num >>= 1;
                if (num2 == 1)
                {
                    num ^= xdapoly;
                }
            }
        }

        return (!crc16) ? new byte[2]
        {
            (byte)(num >> 8),
            (byte)((uint)num & 0xFFu)
        } : new byte[2]
        {
            (byte)((uint)num & 0xFFu),
            (byte)(num >> 8)
        };
    }
}
