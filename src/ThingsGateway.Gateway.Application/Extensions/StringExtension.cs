
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

using TouchSocket.Core;

namespace ThingsGateway.Gateway.Application;

/// <inheritdoc/>
public static class StringExtension
{
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
}