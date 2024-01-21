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

using System.Text.RegularExpressions;

namespace ThingsGateway.Core.Extension;

/// <summary>
/// StringExtension
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// 首字母小写
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string FirstCharToLower(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }
        return input.First().ToString().ToLower() + input.Substring(1);
    }

    /// <summary>
    /// 返回字符串首字符的大写字母
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string FirstCharToUpper(this string input) => string.IsNullOrEmpty(input) ? input : input.First().ToString().ToUpper();

    /// <summary>
    /// 获取字符串中的两个字符作为名称简述
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetNameLen2(this string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;
        var nameLength = name.Length;//获取姓名长度
        string nameWritten = name;//需要绘制的文字
        if (nameLength > 2)//如果名字长度超过2个
        {
            // 如果用户输入的姓名大于等于3个字符，截取后面两位
            string firstName = name.Substring(0, 1);
            if (IsChinese(firstName))
            {
                // 截取倒数两位汉字
                nameWritten = name.Substring(name.Length - 2);
            }
            else
            {
                // 截取第一个英文字母和第二个大写的字母
                var data = Regex.Match(name, @"[A-Z]?[a-z]+([A-Z])").Value;
                nameWritten = data.FirstCharToUpper() + data.LastCharToUpper();
                if (string.IsNullOrEmpty(nameWritten))
                {
                    nameWritten = name.FirstCharToUpper() + name.LastCharToUpper();
                }
            }
        }

        return nameWritten;
    }

    /// <summary>
    /// 返回字符串尾字符的大写字母
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string LastCharToUpper(this string input) => string.IsNullOrEmpty(input) ? input : input.Last().ToString().ToUpper();

    /// <summary>
    /// 匹配邮箱格式
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static (bool isMatch, Match match) MatchEmail(this string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length < 7)
        {
            return (false, null);
        }

        Match match = Regex.Match(s, "[^@ \\t\\r\\n]+@[^@ \\t\\r\\n]+\\.[^@ \\t\\r\\n]+");
        bool flag = match.Success;

        return (flag, match);
    }

    /// <summary>
    /// 匹配手机号码
    /// </summary>
    /// <param name="s">源字符串</param>
    /// <returns>是否匹配成功</returns>
    public static bool MatchPhoneNumber(this string s) => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^1[3456789][0-9]{9}$");

    /// <summary>
    /// 用 正则表达式 判断字符是不是汉字
    /// </summary>
    /// <param name="text">待判断字符或字符串</param>
    /// <returns>真：是汉字；假：不是</returns>
    private static bool IsChinese(string text) => Regex.IsMatch(text, @"[\u4e00-\u9fbb]");
}