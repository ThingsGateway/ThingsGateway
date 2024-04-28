
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------




namespace ThingsGateway.Foundation.Extension.String;

/// <inheritdoc/>
public static class StringExtensions
{
    /// <see cref="DataTransUtil.HexStringToBytes(string)"/>
    public static byte[] HexStringToBytes(this string str) => DataTransUtil.HexStringToBytes(str);

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
    /// 返回List,无其他处理
    /// </summary>
    public static List<string> StringToList(this string str)
    {
        return new List<string>() { str };
    }
}