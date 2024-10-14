//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Globalization;
using System.Net;

using ThingsGateway.NewLife.Extension;

namespace ThingsGateway.Foundation.Extension.String;

/// <inheritdoc/>
public static class StringExtensions
{
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
    /// 根据<see cref="Type"/> 数据类型转化常见类型，如果不成功，返回false
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="value"></param>
    /// <param name="objResult"></param>
    /// <returns></returns>
    public static bool GetTypeStringValue(this Type propertyType, object value, out string? objResult)
    {
        if (propertyType == typeof(bool))
            objResult = value.ToString();
        else if (propertyType == typeof(char))
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
    /// 根据<see cref="Type"/> 数据类型转化常见类型，如果不成功，返回false
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
            objResult = value.ToBoolean(false);
        else if (propertyType == typeof(char))
            objResult = char.Parse(value);
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

    /// <see cref="DataTransUtil.HexStringToBytes(string)"/>
    public static byte[] HexStringToBytes(this string str) => DataTransUtil.HexStringToBytes(str);

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
    /// 只按第一个匹配项分割字符串
    /// </summary>
    /// <param name="str">要分割的字符串</param>
    /// <param name="split">分割字符</param>
    /// <returns>包含分割结果的列表</returns>
    public static List<string> SplitFirst(this string str, char split)
    {
        List<string> result = new List<string>();

        // 寻找第一个分割字符的位置
        int index = str.IndexOf(split);
        if (index >= 0)
        {
            // 将第一个分割字符之前的部分添加到结果列表
            result.Add(str.Substring(0, index).Trim());
            // 将第一个分割字符之后的部分添加到结果列表
            result.Add(str.Substring(index + 1).Trim());
        }

        return result;
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
    /// 返回List,无其他处理
    /// </summary>
    public static List<string> StringToList(this string str)
    {
        return new List<string>() { str };
    }
}
