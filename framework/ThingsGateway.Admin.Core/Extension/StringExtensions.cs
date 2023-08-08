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

using Furion.DependencyInjection;

using System.Text.RegularExpressions;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// 对象拓展
/// </summary>
[SuppressSniffer]
public static class StringExtensions
{
    /// <summary>
    /// 返回List,无其他处理
    /// </summary>
    public static List<string> StringToList(this string str)
    {
        return new List<string>() { str };
    }

    /// <summary>
    /// 用 正则表达式 判断字符是不是汉字
    /// </summary>
    /// <param name="text">待判断字符或字符串</param>
    /// <returns>真：是汉字；假：不是</returns>
    private static bool IsChinese(string text) => Regex.IsMatch(text, @"[\u4e00-\u9fbb]");

    /// <summary>
    /// 获取字符串中的两个字符作为名称简述
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetNameLen2(this string name)
    {
        if (name.IsNullOrEmpty())
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
                if (nameWritten.IsNullOrEmpty())
                {
                    nameWritten = name.FirstCharToUpper() + name.LastCharToUpper();
                }
            }
        }

        return nameWritten;
    }

    /// <summary>
    /// 字符串是 null 或者 空
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string value) => value == null || value!.Length <= 0;

    /// <summary>
    /// 返回字符串首字符的大写字母
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string FirstCharToUpper(this string input) => input.IsNullOrEmpty() ? input : input.First().ToString().ToUpper();

    /// <summary>
    /// 返回字符串尾字符的大写字母
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string LastCharToUpper(this string input) => input.IsNullOrEmpty() ? input : input.Last().ToString().ToUpper();

    /// <summary>
    /// 转换布尔值
    /// </summary>
    /// <returns></returns>
    public static bool ToBoolean(this string value, bool defaultValue = false) => value?.ToUpper() switch
    {
        "1" or "TRUE" => true,
        _ => defaultValue,
    };

    /// <summary>
    /// ToLong
    /// </summary>
    /// <returns></returns>
    public static long ToLong(this string value, long defaultValue = 0) => value.IsNullOrEmpty() ? defaultValue : Int64.TryParse(value, out var n) ? n : defaultValue;

    /// <summary>
    /// ToInt
    /// </summary>
    /// <returns></returns>
    public static int ToInt(this string value, int defaultValue = 0) => value.IsNullOrEmpty() ? defaultValue : Int32.TryParse(value, out var n) ? n : defaultValue;

    /// <summary>
    /// ToDecimal
    /// </summary>
    /// <returns></returns>
    public static decimal ToDecimal(this string value, int defaultValue = 0) => value.IsNullOrEmpty() ? defaultValue : Decimal.TryParse(value, out var n) ? n : defaultValue;


    /// <summary>
    /// 匹配手机号码
    /// </summary>
    /// <param name="s">源字符串</param>
    /// <returns>是否匹配成功</returns>
    public static bool MatchPhoneNumber(this string s) => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^1[3456789][0-9]{9}$");

    /// <summary>
    /// 匹配邮箱格式
    /// </summary>
    /// <param name="s">源字符串</param>
    /// <returns>是否匹配成功</returns>
    public static bool MatchEmail(this string s) => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

    /// <summary>合并多段路径</summary>
    /// <param name="path"></param>
    /// <param name="ps"></param>
    /// <returns></returns>
    public static string CombinePath(this string path, params string[] ps)
    {
        if (ps == null || ps.Length <= 0) return path;
        path ??= string.Empty;

        foreach (var item in ps)
        {
            if (!item.IsNullOrEmpty()) path = Path.Combine(path, item);
        }
        return path;
    }
}