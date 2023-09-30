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

namespace ThingsGateway.Foundation.Extension;
/// <summary>
/// 对象拓展类
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// 转换布尔值
    /// </summary>
    /// <returns></returns>
    public static bool ToBoolean(this object value, bool defaultValue = false) => value?.ToString().ToUpper() switch
    {
        "0" or "FALSE" => false,
        "1" or "TRUE" => true,
        _ => defaultValue,
    };

    /// <summary>
    /// ToLong
    /// </summary>
    /// <returns></returns>
    public static long ToLong(this object value, long defaultValue = 0)
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
            return Int64.TryParse(value.ToString(), out var n) ? n : defaultValue;
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
    /// ToDecimal
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
}
