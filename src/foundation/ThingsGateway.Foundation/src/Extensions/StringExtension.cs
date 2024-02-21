//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Globalization;
using System.Net;

namespace ThingsGateway.Foundation.Extension.String;

/// <inheritdoc/>
public static class StringExtension
{
    /// <see cref="DataTransUtil.HexStringToBytes(string)"/>
    public static byte[] HexStringToBytes(this string str) => DataTransUtil.HexStringToBytes(str);

    /// <summary>
    /// 转换布尔值，注意    1，0，on，off  ，true，false     都会对应转换，如果都不是，返回null
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
    /// 根据<see cref="Type"/> 数据类型转化返回值类型，如果不成功，返回false
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <param name="objResult"></param>
    /// <returns></returns>
    public static bool GetTypeValue(this Type propertyType, string value, out object? objResult)
    {
        if (value == null)
        {
            if (propertyType.IsNullableType())
            {
                objResult = null;
                return true;
            }
        }
        if (propertyType.IsNullableType())
        {
            propertyType = propertyType.GetGenericArguments()[0];
        }
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
    /// 根据<see cref="Type"/> 数据类型转化返回值类型，如果不成功，返回false
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <param name="objResult"></param>
    /// <returns></returns>
    public static bool GetTypeStringValue(this Type propertyType, object value, out string? objResult)
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
    public static string ArrayToString(this string[] strArray, string separator = "")
    {
        if (strArray == null)
            return string.Empty;
        return string.Join(separator, strArray);
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
            if (!string.IsNullOrEmpty(text))
            {
                path = Path.Combine(path, text).Replace("\\", "/");
            }
        }

        return path;
    }

    /// <summary>
    /// 根据英文小数点进行分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitStringByDelimiter(this string? str)
    {
        return str?.Split(new char[1]
{
  '.'
}, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 根据英文小数点进行分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitStringByComma(this string? str)
    {
        return str?.Split(new char[1]
{
  ','
}, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 根据英文分号分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitStringBySemicolon(this string? str)
    {
        return str.Split(new char[1]
{
  ';'
}, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 根据英文逗号分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitAndTrim(this string? str)
    {
        return str.Split(new char[1]
{
  ','
}, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// 根据-符号分割字符串，去除空白的字符
    /// </summary>
    public static string[]? SplitByHyphen(this string? str)
    {
        return str.Split(new char[1]
{
  '-'
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
    public static decimal ToDecimal(this string value, int defaultValue = 0) => string.IsNullOrEmpty(value) ? defaultValue : Decimal.TryParse(value, out var n) ? n : defaultValue;

    /// <summary>
    /// ToInt
    /// </summary>
    /// <returns></returns>
    public static int ToInt(this string value, int defaultValue = 0)
    {
        return string.IsNullOrEmpty(value) ? defaultValue : int.TryParse(value, out int n) ? n : defaultValue;
    }

    /// <summary>
    /// ToUInt
    /// </summary>
    /// <returns></returns>
    public static uint ToUInt(this string value, uint defaultValue = 0)
    {
        return string.IsNullOrEmpty(value) ? defaultValue : uint.TryParse(value, out uint n) ? n : defaultValue;
    }

    /// <summary>
    /// ToLong
    /// </summary>
    /// <returns></returns>
    public static long ToLong(this string value, long defaultValue = 0)
    {
        return string.IsNullOrEmpty(value) ? defaultValue : long.TryParse(value, out long n) ? n : defaultValue;
    }

    /// <summary>
    /// ToUShort
    /// </summary>
    /// <returns></returns>
    public static ushort ToUShort(this string value, ushort defaultValue = 0)
    {
        return string.IsNullOrEmpty(value) ? defaultValue : ushort.TryParse(value, out var n) ? n : defaultValue;
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
        if (string.IsNullOrEmpty(str))
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
        if (string.IsNullOrEmpty(str))
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
}