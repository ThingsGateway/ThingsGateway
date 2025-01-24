//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway;

using System;
using System.Text;

/// <summary>
/// 随机数
/// </summary>
[ThingsGateway.DependencyInjection.SuppressSniffer]
public class RandomHelper
{
    /// <summary>
    /// 生成随机纯字母随机数
    /// </summary>
    /// <param name="Length">生成长度</param>
    /// <returns></returns>
    public static string CreateLetter(int Length)
    {

        char[] Pattern = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        string result = "";
        int n = Pattern.Length;
        System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
        for (int i = 0; i < Length; i++)
        {
            int rnd = random.Next(0, n);
            result += Pattern[rnd];
        }
        return result;
    }


    /// <summary>
    /// 生成随机字母和数字随机数
    /// </summary>
    /// <param name="Length">生成长度</param>
    /// <returns></returns>
    public static string CreateLetterAndNumber(int Length)
    {

        char[] Pattern = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        string result = "";
        int n = Pattern.Length;
        System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
        for (int i = 0; i < Length; i++)
        {
            int rnd = random.Next(0, n);
            result += Pattern[rnd];
        }
        return result;
    }

    /// <summary>
    /// 生成随机小写字母和数字随机数
    /// </summary>
    /// <param name="Length">生成长度</param>
    /// <returns></returns>
    public static string CreateLetterAndNumberLower(int Length)
    {

        char[] Pattern = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        string result = "";
        int n = Pattern.Length;
        System.Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
        for (int i = 0; i < Length; i++)
        {
            int rnd = random.Next(0, n);
            result += Pattern[rnd];
        }
        return result;
    }



    /// <summary>
    /// 生成随机字符串
    /// </summary>
    /// <param name="length">字符串的长度</param>
    /// <returns></returns>
    public static string CreateRandomString(int length)
    {
        // 创建一个StringBuilder对象存储密码
        StringBuilder sb = new StringBuilder();
        //使用for循环把单个字符填充进StringBuilder对象里面变成14位密码字符串
        for (int i = 0; i < length; i++)
        {
            Random random = new Random(Guid.NewGuid().GetHashCode());
            //随机选择里面其中的一种字符生成
            switch (random.Next(3))
            {
                case 0:
                    //调用生成生成随机数字的方法
                    sb.Append(CreateNum());
                    break;
                case 1:
                    //调用生成生成随机小写字母的方法
                    sb.Append(CreateSmallAbc());
                    break;
                case 2:
                    //调用生成生成随机大写字母的方法
                    sb.Append(CreateBigAbc());
                    break;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 生成单个随机数字
    /// </summary>
    public static int CreateNum()
    {
        Random random = new Random(Guid.NewGuid().GetHashCode());
        int num = random.Next(10);
        return num;
    }

    /// <summary>
    /// 生成指定长度的随机数字字符串
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string CreateNum(int length = 1)
    {
        Random random = new Random(Guid.NewGuid().GetHashCode());
        var result = "";
        for (int i = 0; i < length; i++)
        {
            result += random.Next(10);
        }
        return result;
    }

    /// <summary>
    /// 生成单个大写随机字母
    /// </summary>
    public static string CreateBigAbc()
    {
        //A-Z的 ASCII值为65-90
        Random random = new Random(Guid.NewGuid().GetHashCode());
        int num = random.Next(65, 91);
        string abc = Convert.ToChar(num).ToString();
        return abc;
    }

    /// <summary>
    /// 生成单个小写随机字母
    /// </summary>
    public static string CreateSmallAbc()
    {
        //a-z的 ASCII值为97-122
        Random random = new Random(Guid.NewGuid().GetHashCode());
        int num = random.Next(97, 123);
        string abc = Convert.ToChar(num).ToString();
        return abc;
    }

}

