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

namespace ThingsGateway.Foundation.Core;

/// <summary>
/// HSL摘录，用于CRC16验证的类
/// </summary>
public class EasyCRC16
{
    #region Public Methods

    /// <summary>
    /// 来校验对应的接收数据的CRC校验码，默认多项式码为0xA001
    /// </summary>
    /// <param name="value">需要校验的数据，带CRC校验码</param>
    /// <returns>返回校验成功与否</returns>
    public static bool CheckCRC16(byte[] value)
    {
        return CheckCRC16(value, 160, 1);
    }

    /// <summary>
    /// 指定多项式码来校验对应的接收数据的CRC校验码
    /// </summary>
    /// <param name="value">需要校验的数据，带CRC校验码</param>
    /// <param name="CH">多项式码高位</param>
    /// <param name="CL">多项式码低位</param>
    /// <returns>返回校验成功与否</returns>
    public static bool CheckCRC16(byte[] value, byte CH, byte CL)
    {
        if (value == null || value.Length < 2)
        {
            return false;
        }

        int length = value.Length;
        byte[] destinationArray = new byte[length - 2];
        Array.Copy(value, 0, destinationArray, 0, destinationArray.Length);
        byte[] numArray = CRC16(destinationArray, CH, CL);
        return numArray[length - 2] == value[length - 2] && numArray[length - 1] == value[length - 1];
    }

    /// <summary>
    /// 获取对应的数据的CRC校验码，默认多项式码为0xA001
    /// </summary>
    /// <param name="value">需要校验的数据，不包含CRC字节</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] CRC16(byte[] value)
    {
        return CRC16(value, 160, 1);
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="value">需要校验的数据，不包含CRC字节</param>
    /// <param name="CL">多项式码地位</param>
    /// <param name="CH">多项式码高位</param>
    /// <param name="preH">预置的高位值</param>
    /// <param name="preL">预置的低位值</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] CRC16(byte[] value, byte CH, byte CL, byte preH = 255, byte preL = 255)
    {
        byte[] numArray = new byte[value.Length + 2];
        value.CopyTo(numArray, 0);
        byte num1 = preL;
        byte num2 = preH;
        foreach (byte num3 in value)
        {
            num1 ^= num3;
            for (int index = 0; index <= 7; ++index)
            {
                byte num4 = num2;
                byte num5 = num1;
                num2 >>= 1;
                num1 >>= 1;
                if ((num4 & 1) == 1)
                {
                    num1 |= 128;
                }

                if ((num5 & 1) == 1)
                {
                    num2 ^= CH;
                    num1 ^= CL;
                }
            }
        }
        numArray[numArray.Length - 2] = num1;
        numArray[numArray.Length - 1] = num2;
        return numArray;
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="value">需要校验的数据，不包含CRC字节</param>
    /// <param name="index">计算的起始字节索引</param>
    /// <param name="length">计算的字节长度</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] CRC16Only(byte[] value,
      int index,
      int length)
    {
        return CRC16Only(value, index, length, 160, 1);
    }

    /// <summary>
    /// 通过指定多项式码来获取对应的数据的CRC校验码
    /// </summary>
    /// <param name="value">需要校验的数据，不包含CRC字节</param>
    /// <param name="index">计算的起始字节索引</param>
    /// <param name="length">计算的字节长度</param>
    /// <param name="CL">多项式码地位</param>
    /// <param name="CH">多项式码高位</param>
    /// <param name="preH">预置的高位值</param>
    /// <param name="preL">预置的低位值</param>
    /// <returns>返回带CRC校验码的字节数组，可用于串口发送</returns>
    public static byte[] CRC16Only(
      byte[] value,
      int index,
      int length,
      byte CH,
      byte CL,
      byte preH = 255,
      byte preL = 255)
    {
        byte num1 = preL;
        byte num2 = preH;
        for (int index1 = index; index1 < index + length; ++index1)
        {
            num1 ^= value[index1];
            for (int index2 = 0; index2 <= 7; ++index2)
            {
                byte num3 = num2;
                byte num4 = num1;
                num2 >>= 1;
                num1 >>= 1;
                if ((num3 & 1) == 1)
                    num1 |= 128;
                if ((num4 & 1) == 1)
                {
                    num2 ^= CH;
                    num1 ^= CL;
                }
            }
        }
        return new byte[2] { num1, num2 };
    }

    #endregion Public Methods
}