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

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.Foundation.Extension.String;

/// <inheritdoc/>
public static class StringExtensions
{
    /// <summary>
    /// 转换布尔值，注意    1，0，on，off  ，true，false     都会对应转换
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool? ToBool(this string value)
    {
        switch (value)
        {
            case null:
                return null;
            case "1":
                return true;
            case "0":
                return false;
        }
        value = value.ToUpper();
        switch (value)
        {
            case "TRUE":
                return true;
            case "FALSE":
                return false;
            case "ON":
                return true;
            case "OFF":
                return false;
            default:
                if (bool.TryParse(value, out bool parseBool))
                {
                    return parseBool;
                }
                else
                {
                    return null;
                }
        }
    }
    /// <summary>
    /// 转换布尔值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue">默认值</param>
    /// <returns></returns>
    public static bool ToBool(this string value, bool defaultValue)
    {
        return ToBool(value) ?? defaultValue;
    }

    /// <summary>
    /// 根据<see cref="Type"/> 数据类型转化返回值类型
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <param name="objResult"></param>
    /// <returns></returns>
    public static bool GetTypeValue(this Type propertyType, string value, out object objResult)
    {
        if (propertyType == typeof(bool))
            objResult = value.ToBool(false);
        else if (propertyType == typeof(byte))
            objResult = byte.Parse(value);
        else if (propertyType == typeof(sbyte))
            objResult = sbyte.Parse(value);
        else if (propertyType == typeof(short))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = short.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = short.Parse(value);
        }
        else if (propertyType == typeof(ushort))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = ushort.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = ushort.Parse(value);
        }
        else if (propertyType == typeof(int))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = int.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = int.Parse(value);
        }
        else if (propertyType == typeof(uint))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = uint.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = uint.Parse(value);
        }
        else if (propertyType == typeof(long))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = long.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = long.Parse(value);
        }
        else if (propertyType == typeof(ulong))
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                objResult = ulong.Parse(value.Substring(2), NumberStyles.HexNumber);
            else
                objResult = ulong.Parse(value);
        }
        else if (propertyType == typeof(float))
            objResult = float.Parse(value);
        else if (propertyType == typeof(double))
            objResult = double.Parse(value);
        else if (propertyType == typeof(decimal))
            objResult = decimal.Parse(value);
        else if (propertyType == typeof(DateTime))
            objResult = DateTime.Parse(value);
        else if (propertyType == typeof(DateTimeOffset))
            objResult = DateTimeOffset.Parse(value);
        else if (propertyType == typeof(string))
            objResult = value;
        else if (propertyType == typeof(IPAddress))
            objResult = IPAddress.Parse(value);
        else if (propertyType.IsEnum)
            objResult = Enum.Parse(propertyType, value);
        else
        {
            objResult = null;
            return false;
        }
        return true;
    }
    /// <summary>
    /// 根据<see cref="Type"/> 数据类型转化返回值类型
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <param name="objResult"></param>
    /// <returns></returns>
    public static bool GetTypeStringValue(this Type propertyType, object value, out string objResult)
    {
        if (propertyType == typeof(bool))
            objResult = value.ToString();
        else if (propertyType == typeof(byte))
            objResult = value.ToString();
        else if (propertyType == typeof(sbyte))
            objResult = value.ToString();
        else if (propertyType == typeof(short))
            objResult = value.ToString();
        else if (propertyType == typeof(ushort))
            objResult = value.ToString();
        else if (propertyType == typeof(int))
            objResult = value.ToString();
        else if (propertyType == typeof(uint))
            objResult = value.ToString();
        else if (propertyType == typeof(long))
            objResult = value.ToString();
        else if (propertyType == typeof(ulong))
            objResult = value.ToString();
        else if (propertyType == typeof(float))
            objResult = value.ToString();
        else if (propertyType == typeof(double))
            objResult = value.ToString();
        else if (propertyType == typeof(decimal))
            objResult = value.ToString();
        else if (propertyType == typeof(DateTime))
            objResult = value.ToString();
        else if (propertyType == typeof(DateTimeOffset))
            objResult = value.ToString();
        else if (propertyType == typeof(string))
            objResult = value.ToString();
        else if (propertyType == typeof(IPAddress))
            objResult = value.ToString();
        else if (propertyType.IsEnum)
            objResult = value.ToString();
        else
        {
            objResult = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 将字符串数组转换成字符串
    /// </summary>
    public static string ArrayToString(this string[] strArray, string spitStr = "")
    {
        StringBuilder str = new();
        for (int i = 0; i < strArray.Length; i++)
        {
            if (i > 0)
            {
                //分割符可根据需要自行修改
                str.Append(spitStr);
            }
            str.Append(strArray[i]);
        }
        return str.ToString();
    }

    /// <summary>
    /// 从Base64转到数组。
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ByBase64ToBytes(this string value)
    {
        return Convert.FromBase64String(value);
    }

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

    /// <summary>
    /// <inheritdoc cref="Path.Combine(string[])"/> <br></br>
    /// 并把\转为/
    /// </summary>
    public static string CombinePathOS(this string path, params string[] ps)
    {
        if (ps == null || ps.Length == 0)
        {
            return path;
        }

        path ??= string.Empty;

        foreach (string text in ps)
        {
            if (!text.IsNullOrEmpty())
            {
                path = Path.Combine(path, text).Replace("\\", "/");
            }
        }

        return path;
    }

    /// <summary>
    /// 返回字符串首字符的大写字母
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string FirstCharToUpper(this string input) => input.IsNullOrEmpty() ? input : input.First().ToString().ToUpper();

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
    /// 返回字符串尾字符的大写字母
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string LastCharToUpper(this string input) => input.IsNullOrEmpty() ? input : input.Last().ToString().ToUpper();

    /// <summary>
    /// 匹配邮箱格式
    /// </summary>
    /// <param name="s">源字符串</param>
    /// <returns>是否匹配成功</returns>
    public static bool MatchEmail(this string s) => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

    /// <summary>
    /// 匹配手机号码
    /// </summary>
    /// <param name="s">源字符串</param>
    /// <returns>是否匹配成功</returns>
    public static bool MatchPhoneNumber(this string s) => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"^1[3456789][0-9]{9}$");

    /// <summary>
    /// 根据英文小数点进行切割字符串，去除空白的字符
    /// </summary>
    public static string[] SplitStringByDelimiter(this string str)
    {
        return str.Split(new char[1]
{
  '.'
}, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 根据英文分号割字符串，去除空白的字符
    /// </summary>
    public static string[] SplitStringBySemicolon(this string str)
    {
        return str.Split(new char[1]
{
  ';'
}, StringSplitOptions.RemoveEmptyEntries);
    }


    /// <summary>
    /// 只按第一个匹配项分割
    /// </summary>
    /// <param name="str"></param>
    /// <param name="split"></param>
    /// <returns></returns>
    public static string[] SplitFirst(this string str, char split)
    {
        List<string> s = new();
        int index = str.IndexOf(split);
        if (index > 0)
        {
            s.Add(str.Substring(0, index).Trim());
            s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
        }

        return s.ToArray();
    }

    /// <summary>
    /// 返回List,无其他处理
    /// </summary>
    public static List<string> StringToList(this string str)
    {
        return new List<string>() { str };
    }

    /// <summary>
    /// ToDecimal
    /// </summary>
    /// <returns></returns>
    public static decimal ToDecimal(this string value, int defaultValue = 0) => value.IsNullOrEmpty() ? defaultValue : Decimal.TryParse(value, out var n) ? n : defaultValue;

    /// <summary>
    /// ToInt
    /// </summary>
    /// <returns></returns>
    public static int ToInt(this string value, int defaultValue = 0)
    {
        return value.IsNullOrEmpty() ? defaultValue : int.TryParse(value, out int n) ? n : defaultValue;
    }
    /// <summary>
    /// ToUInt
    /// </summary>
    /// <returns></returns>
    public static uint ToUInt(this string value, uint defaultValue = 0)
    {
        return value.IsNullOrEmpty() ? defaultValue : uint.TryParse(value, out uint n) ? n : defaultValue;
    }

    /// <summary>
    /// ToLong
    /// </summary>
    /// <returns></returns>
    public static long ToLong(this string value, long defaultValue = 0)
    {
        return value.IsNullOrEmpty() ? defaultValue : long.TryParse(value, out long n) ? n : defaultValue;
    }

    /// <summary>
    /// ToUShort
    /// </summary>
    /// <returns></returns>
    public static ushort ToUShort(this string value, ushort defaultValue = 0)
    {
        return value.IsNullOrEmpty() ? defaultValue : ushort.TryParse(value, out var n) ? n : defaultValue;
    }
    /// <summary>
    /// ToDouble
    /// </summary>
    /// <returns></returns>
    public static double ToDouble(this object value, double defaultValue = 0)
    {
        if (value is Double d)
        {
            return Double.IsNaN(d) ? defaultValue : (Double)d;
        }
        var str = value?.ToString();
        if (str.IsNullOrEmpty())
        {
            return (double)defaultValue;
        }
        else
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0;
            }
            return (double)(double.TryParse(str, out var n) ? n : defaultValue);
        }
    }

    /// <summary>
    /// ToDecimal
    /// </summary>
    /// <returns></returns>
    public static decimal ToDecimal(this object value, int defaultValue = 0)
    {
        if (value is Double d)
        {
            return Double.IsNaN(d) ? defaultValue : (Decimal)d;
        }
        var str = value?.ToString();
        if (str.IsNullOrEmpty())
        {
            return defaultValue;
        }
        else
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0;
            }
            return Decimal.TryParse(str, out var n) ? n : defaultValue;
        }
    }
    /// <summary>
    /// ToInt
    /// </summary>
    /// <returns></returns>
    public static int ToInt(this object value, int defaultValue = 0)
    {
        if (value == null || value.ToString().IsNullOrEmpty())
        {
            return defaultValue;
        }
        else
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1 : 0;
            }
            return int.TryParse(value.ToString(), out int n) ? n : defaultValue;
        }
    }

    /// <summary>
    /// 用 正则表达式 判断字符是不是汉字
    /// </summary>
    /// <param name="text">待判断字符或字符串</param>
    /// <returns>真：是汉字；假：不是</returns>
    private static bool IsChinese(string text) => Regex.IsMatch(text, @"[\u4e00-\u9fbb]");
}