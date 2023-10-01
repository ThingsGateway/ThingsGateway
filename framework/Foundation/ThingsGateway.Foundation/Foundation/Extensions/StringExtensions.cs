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

using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.Foundation.Extension.String;

/// <inheritdoc/>
public static class StringExtensions
{
    /// <summary>
    /// 转换布尔值
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool GetBoolValue(this string value)
    {
        switch (value)
        {
            case null:
                return false;
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
            default:
                return (!(value == "OFF")) && (bool.TryParse(value, out bool parseBool) && parseBool);
        }
    }

    /// <summary>
    /// 是否布尔值，对于TRUE/FALSE/ON/OFF也返回True
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsBoolValue(this string value)
    {
        if (value == "1")
            return true;
        if (value == "0")
            return true;
        value = value.ToUpper();
        if (value == "TRUE")
            return true;
        if (value == "FALSE")
            return true;
        if (value == "ON")
            return true;
        if (value == "OFF")
            return true;
        return bool.TryParse(value, out _);
    }
    /// <summary>
    /// 获取Double/Bool/String
    /// </summary>
    /// <returns></returns>
    public static object GetObjectData(this string value)
    {
        //判断数值类型
        Regex regex = new("^[-+]?[0-9]*\\.?[0-9]+$");
        bool match = regex.IsMatch(value);
        if (match)
        {
            if (value.ToDouble() == 0 && Convert.ToInt64(value) != 0)
            {
                throw new("转换失败");
            }
            return value.ToDouble();
        }
        else if (value.IsBoolValue())
        {
            return value.GetBoolValue();
        }
        else
        {
            return value;
        }
    }

    /// <summary>
    /// 根据<see cref="Type"/> 数据类型转化返回值类型
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object ObjToTypeValue(this Type propertyType, string value)
    {
        object _value = null;
        if (propertyType == typeof(bool))
            _value = value.GetBoolValue();
        else if (propertyType == typeof(byte))
            _value = byte.Parse(value);
        else if (propertyType == typeof(sbyte))
            _value = sbyte.Parse(value);
        else if (propertyType == typeof(short))
            _value = short.Parse(value);
        else if (propertyType == typeof(ushort))
            _value = ushort.Parse(value);
        else if (propertyType == typeof(int))
            _value = int.Parse(value);
        else if (propertyType == typeof(uint))
            _value = uint.Parse(value);
        else if (propertyType == typeof(long))
            _value = long.Parse(value);
        else if (propertyType == typeof(ulong))
            _value = ulong.Parse(value);
        else if (propertyType == typeof(float))
            _value = float.Parse(value);
        else if (propertyType == typeof(double))
            _value = double.Parse(value);
        else if (propertyType == typeof(decimal))
            _value = decimal.Parse(value);
        else if (propertyType == typeof(DateTime))
            _value = DateTime.Parse(value);
        else if (propertyType == typeof(DateTimeOffset))
            _value = DateTimeOffset.Parse(value);
        else if (propertyType == typeof(string))
            _value = value;
        else if (propertyType == typeof(IPAddress))
            _value = IPAddress.Parse(value);
        else if (propertyType.IsEnum)
            _value = Enum.Parse(propertyType, value);
        return _value;

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
    /// <inheritdoc cref="Path.Combine(string[])"/>
    /// 并把\\转为/
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
    /// 根据英文小数点进行切割字符串，去除空白的字符<br />
    /// </summary>
    /// <param name="str">字符串本身</param>
    public static string[] SplitDot(this string str)
    {
        return str.Split(new char[1]
{
  '.'
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
    /// 转换布尔值
    /// </summary>
    /// <returns></returns>
    public static bool ToBoolean(this string value, bool defaultValue = false) => value?.ToUpper() switch
    {
        "0" or "FALSE" => false,
        "1" or "TRUE" => true,
        _ => defaultValue,
    };

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
    /// 用 正则表达式 判断字符是不是汉字
    /// </summary>
    /// <param name="text">待判断字符或字符串</param>
    /// <returns>真：是汉字；假：不是</returns>
    private static bool IsChinese(string text) => Regex.IsMatch(text, @"[\u4e00-\u9fbb]");
}