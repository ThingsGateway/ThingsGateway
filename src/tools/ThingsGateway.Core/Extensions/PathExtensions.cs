//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

public static class PathExtensions
{
    /// <summary>
    /// 处理 Windows 和 Linux 路径分隔符不一致问题
    /// </summary>
    /// <param name="path"></param>
    /// <param name="ps"></param>
    /// <returns></returns>
    public static string CombinePathWithOs(this string? path, params string[] ps)
    {
        if (path == null)
        {
            path = string.Empty;
        }

        if (ps == null || ps.Length == 0)
        {
            return path;
        }

        foreach (string text in ps)
        {
            if (!string.IsNullOrEmpty(text))
            {
                path = Path.Combine(path, text);
            }
        }
        // 处理路径分隔符，兼容Windows和Linux
        var sep = Path.DirectorySeparatorChar;
        var sep2 = sep == '/' ? '\\' : '/';
        path = path.Replace(sep2, sep);
        return path;
    }
}
