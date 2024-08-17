//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

namespace ThingsGateway.ASPNetCore;

/// <inheritdoc/>
public static class CacheExtensions
{
    private static int? _age;

    private static List<string>? _files;

    public static void ProcessCache(this StaticFileResponseContext context, IConfiguration configuration)
    {
        if (context.CanCache(configuration, out var age))
        {
            context.Context.Response.Headers[HeaderNames.CacheControl] = $"public, max-age={age}";
        }
    }

    private static bool CanCache(this StaticFileResponseContext context, IConfiguration configuration, out int age)
    {
        var ret = false;
        age = 0;

        var files = configuration.GetFiles();
        if (files.Any(i => context.CanCache(i)))
        {
            ret = true;
            age = configuration.GetAge();
        }
        return ret;
    }

    private static bool CanCache(this StaticFileResponseContext context, string file)
    {
        var ext = Path.GetExtension(context.File.PhysicalPath) ?? "";
        bool ret = file.Equals(ext, StringComparison.OrdinalIgnoreCase);
        if (ret && ext.Equals(".js", StringComparison.OrdinalIgnoreCase))
        {
            // process javascript file
            ret = false;
            if (context.Context.Request.QueryString.HasValue)
            {
                var paras = QueryHelpers.ParseQuery(context.Context.Request.QueryString.Value);
                ret = paras.ContainsKey("v");
            }
        }
        return ret;
    }

    private static int GetAge(this IConfiguration configuration)
    {
        _age ??= GetAge();
        return _age.Value;

        int GetAge()
        {
            var cacheSection = configuration.GetSection("Cache-Control");
            return cacheSection.GetValue<int>("Max-Age", 1000 * 60 * 10);
        }
    }

    private static List<string> GetFiles(this IConfiguration configuration)
    {
        _files ??= GetFiles();
        return _files;

        List<string> GetFiles()
        {
            var cacheSection = configuration.GetSection("Cache-Control");
            return cacheSection.GetSection("Files").Get<List<string>>() ?? new();
        }
    }
}
