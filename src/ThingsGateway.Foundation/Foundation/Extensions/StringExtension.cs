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

using System.IO;

namespace ThingsGateway.Foundation.Extension;

/// <inheritdoc/>
public static class StringExtension
{
    /// <summary>
    /// 将字符串数组转换成字符串
    /// </summary>
    public static string ArrayToString(this string[] strArray, string spitStr = "")
    {
        StringBuilder str = new StringBuilder();
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


    /// <summary>
    /// 将16进制的字符转换为int32。
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public static int ByHexStringToInt32(this string hexString)
    {
        if (string.IsNullOrEmpty(hexString))
        {
            return default;
        }
        return int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
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

        if (path == null)
        {
            path = string.Empty;
        }

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
        List<string> s = new List<string>();
        int index = str.IndexOf(split);
        if (index > 0)
        {
            s.Add(str.Substring(0, index).Trim());
            s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
        }

        return s.ToArray();
    }
}