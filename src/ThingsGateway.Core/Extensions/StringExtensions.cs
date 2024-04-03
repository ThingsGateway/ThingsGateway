//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace ThingsGateway.Core.Extension;

public static class StringExtensions
{
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
    /// 返回字符串尾字符的大写字母
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string LastCharToUpper(this string input) => string.IsNullOrEmpty(input) ? input : input.Last().ToString().ToUpper();

    public static string Format(this string str, params object[] args)
    {
        return args == null || args.Length == 0 ? str : string.Format(str, args);
    }

    /// <summary>
    /// 切割骆驼命名式字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string[] SplitCamelCase(this string str)
    {
        if (str == null) return Array.Empty<string>();

        if (string.IsNullOrWhiteSpace(str)) return new string[] { str };
        if (str.Length == 1) return new string[] { str };

        return Regex.Split(str, @"(?=\p{Lu}\p{Ll})|(?<=\p{Ll})(?=\p{Lu})")
            .Where(u => u.Length > 0)
            .ToArray();
    }

    /// <summary>
    /// 清除字符串前后缀
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="pos">0：前后缀，1：后缀，-1：前缀</param>
    /// <param name="affixes">前后缀集合</param>
    /// <returns></returns>
    internal static string ClearStringAffixes(this string str, int pos = 0, params string[] affixes)
    {
        // 空字符串直接返回
        if (string.IsNullOrWhiteSpace(str)) return str;

        // 空前后缀集合直接返回
        if (affixes == null || affixes.Length == 0) return str;

        var startCleared = false;
        var endCleared = false;

        string? tempStr = null;
        foreach (var affix in affixes)
        {
            if (string.IsNullOrWhiteSpace(affix)) continue;

            if (pos != 1 && !startCleared && str.StartsWith(affix, StringComparison.OrdinalIgnoreCase))
            {
                tempStr = str[affix.Length..];
                startCleared = true;
            }
            if (pos != -1 && !endCleared && str.EndsWith(affix, StringComparison.OrdinalIgnoreCase))
            {
                var _tempStr = !string.IsNullOrWhiteSpace(tempStr) ? tempStr : str;
                tempStr = _tempStr[..^affix.Length];
                endCleared = true;
            }
            if (startCleared && endCleared) break;
        }

        return !string.IsNullOrWhiteSpace(tempStr) ? tempStr : str;
    }

    /// <summary>
    /// 首字母小写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToLowerCamelCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        return string.Concat(str.First().ToString().ToLower(), str.AsSpan(1));
    }

    /// <summary>
    /// 首字母大写
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToUpperCamelCase(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return str;

        return string.Concat(str.First().ToString().ToUpper(), str.AsSpan(1));
    }

    /// <summary>
    /// 匹配邮箱格式
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool MatchEmail(this string s)
    {
        if (string.IsNullOrEmpty(s) || s.Length < 7)
        {
            return false;
        }
        Match match = Regex.Match(s, "[^@ \\t\\r\\n]+@[^@ \\t\\r\\n]+\\.[^@ \\t\\r\\n]+");
        bool flag = match.Success;

        return flag;
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