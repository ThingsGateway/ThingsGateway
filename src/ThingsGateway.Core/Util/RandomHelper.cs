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

namespace ThingsGateway.Core;

/// <summary>
/// 随机数
/// </summary>
public class RandomUtil
{
    /// <summary>
    /// 生成随机纯字母随机数
    /// </summary>
    /// <param name="Length">生成长度</param>
    /// <returns></returns>
    public static string CreateLetter(int Length)
    {
        char[] array = new char[52]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
            'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z'
        };
        string text = "";
        int maxValue = array.Length;
        Random random = new Random((int)(~DateTime.Now.Ticks));
        for (int i = 0; i < Length; i++)
        {
            int num = random.Next(0, maxValue);
            text += array[num];
        }

        return text;
    }

    /// <summary>
    /// 生成随机字母和数字随机数
    /// </summary>
    /// <param name="Length">生成长度</param>
    /// <returns></returns>
    public static string CreateLetterAndNumber(int Length)
    {
        char[] array = new char[62]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
            'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
            'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9'
        };
        string text = "";
        int maxValue = array.Length;
        Random random = new Random((int)(~DateTime.Now.Ticks));
        for (int i = 0; i < Length; i++)
        {
            int num = random.Next(0, maxValue);
            text += array[num];
        }

        return text;
    }

    /// <summary>
    /// 生成随机小写字母和数字随机数
    /// </summary>
    /// <param name="Length">生成长度</param>
    /// <returns></returns>
    public static string CreateLetterAndNumberLower(int Length)
    {
        char[] array = new char[36]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
            'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't',
            'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3',
            '4', '5', '6', '7', '8', '9'
        };
        string text = "";
        int maxValue = array.Length;
        Random random = new Random((int)(~DateTime.Now.Ticks));
        for (int i = 0; i < Length; i++)
        {
            int num = random.Next(0, maxValue);
            text += array[num];
        }

        return text;
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    /// <param name="length">生成长度</param>
    /// <returns></returns>
    public static string CreateRandomString(int length)
    {
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            switch (new Random(Guid.NewGuid().GetHashCode()).Next(3))
            {
                case 0:
                    stringBuilder.Append(CreateNum());
                    break;

                case 1:
                    stringBuilder.Append(CreateSmallAbc());
                    break;

                case 2:
                    stringBuilder.Append(CreateBigAbc());
                    break;
            }
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// 生成单个随机数字
    /// </summary>
    /// <returns></returns>
    public static int CreateNum()
    {
        return new Random(Guid.NewGuid().GetHashCode()).Next(10);
    }

    /// <summary>
    /// 生成指定长度的随机数字字符串
    /// </summary>
    /// <returns></returns>
    public static string CreateNum(int length = 1)
    {
        Random random = new Random(Guid.NewGuid().GetHashCode());
        string text = "";
        for (int i = 0; i < length; i++)
        {
            text += random.Next(10);
        }

        return text;
    }

    /// <summary>
    /// 生成单个大写随机字母
    /// </summary>
    /// <returns></returns>
    public static string CreateBigAbc()
    {
        return Convert.ToChar(new Random(Guid.NewGuid().GetHashCode()).Next(65, 91)).ToString();
    }

    /// <summary>
    /// 生成单个小写随机字母
    /// </summary>
    /// <returns></returns>
    public static string CreateSmallAbc()
    {
        return Convert.ToChar(new Random(Guid.NewGuid().GetHashCode()).Next(97, 123)).ToString();
    }
}