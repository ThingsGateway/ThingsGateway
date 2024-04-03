//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using NewLife;

using System.Text.RegularExpressions;

using ThingsGateway.Core;

namespace ThingsGateway.Admin.Application;

public class LiteDBCacheUtil
{
    private const string ex = ".ldb";
    private static object _dictObject = new();

    /// <summary>
    /// 获取缓存链接
    /// </summary>
    public static LiteDBCache<T> GetCache<T>(string folder, string name) where T : IPrimaryIdEntity
    {
        lock (_dictObject)
        {
            var dir = GetFilePath(folder);
            var fileStart = GetFileName(name);
            var fullName = dir.CombinePathWithOs($"{fileStart}{ex}");
            var cache = new LiteDBCache<T>(fullName);
            return cache;
        }
    }

    public static string GetFilePath(string folderName)
    {
        var dir = GetFileBasePath().CombinePathWithOs(folderName);
        //创建文件夹
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void DeleteFile(string file)
    {
        if (File.Exists(file))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }
    }

    private static void DisposeAndDeleteFile(string file, object cache)
    {
        cache.TryDispose();
        //删除旧文件
        DeleteFile(file);
    }

    public static string GetFileBasePath()
    {
        var dir = Path.Combine(App.ContentRootPath!, "businessCache");
        //创建文件夹
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static double GetFileLength(string fullName)
    {
        long? length1;
        if (!File.Exists(fullName))
        {
            length1 = 0;
        }
        else
        {
            length1 = new FileInfo(fullName).Length;
        }
        var mb1 = Math.Round((double)length1 / (double)1024 / (double)1024, 2);
        return mb1;
    }

    private static string GetFileName(string typeName)
    {
        var fileStart = $"{Regex.Replace($"{typeName}", "[^a-zA-Z0-9]", "_")}";
        return fileStart;
    }
}