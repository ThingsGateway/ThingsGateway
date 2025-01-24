using System.Text.RegularExpressions;

using TouchSocket.Core;
// ------------------------------------------------------------------------------
// 此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
// 此代码版权（除特别声明外的代码）归作者本人Diego所有
// 源代码使用协议遵循本仓库的开源协议及附加协议
// Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
// Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
// 使用文档：https://thingsgateway.cn/
// QQ群：605534569
// ------------------------------------------------------------------------------

namespace ThingsGateway.Gateway.Application;

public static class VariableMethodExtensions
{
    public static List<string> SplitOS(this string input)
    {
        var results = new List<string>();
        if (input.IsNullOrEmpty())
        {
            return results;
        }
        input = input?.Trim()?.TrimEnd(',');
        // 正则表达式解析
        var matches = Regex.Matches(input, "\"([^\"]*)\"|([^,]+)");

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success)
            {
                // 如果匹配的是引号内的内容
                results.Add(match.Groups[1].Value);
            }
            else
            {
                // 如果匹配的是普通内容
                results.Add(match.Groups[2].Value);
            }
        }

        return results;
    }
}