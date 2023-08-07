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

namespace ThingsGateway.Application.Extensions;
/// <summary>
/// 扩展方法
/// </summary>
public static class StringExtensions
{
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

}
