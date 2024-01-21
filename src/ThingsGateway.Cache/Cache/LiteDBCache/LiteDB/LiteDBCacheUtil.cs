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

using Furion;

using NewLife;

using System.Text.RegularExpressions;

using ThingsGateway.Core;

namespace ThingsGateway.Cache;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

public static class LiteDBCacheUtil
{
    private const string ex = ".ldb";
    private static LiteDBConfig config = App.GetConfig<LiteDBConfig>("LiteDBConfig");
    private static Dictionary<string, object> _dict = new();
    private static object _dictObject = new();

    /// <summary>
    /// 根据Id和typeName，获取数据库链接
    /// 如果当前文件的大小超限，则返回新的链接
    /// 如果当前文件的数量超限，则删除部分旧文件
    /// </summary>
    public static LiteDBCache<T>? GetDB<T>(string id, string typeName, bool isDeleteRule = true) where T : IPrimaryIdEntity
    {
        lock (_dictObject)
        {
            var dir = GetFilePath(id);
            var fileStart = GetFileStartName(typeName);
            var maxNum = GetMaxNumFileName(id, typeName);
            var fullName = dir.CombinePath($"{fileStart}{maxNum}{ex}");
            if (isDeleteRule)
            {
                //磁盘使用率限制
                {
                    string currentPath = Directory.GetCurrentDirectory();
                    DriveInfo drive = new(Path.GetPathRoot(currentPath));
                    var driveUsage = (100 - (drive.TotalFreeSpace * 100.00 / drive.TotalSize));
                    if (driveUsage > LiteDBCacheUtil.config.MaxDriveUsage)
                    {
                        //删除全部文件夹中旧文件
                        string[] dirs = Directory.GetDirectories(GetFileBasePath());
                        //遍历全部文件夹，删除90%的文件
                        foreach (var d in dirs)
                        {
                            string[] files = Directory.GetFiles(d);
                            //数量超限就删除旧文件
                            //按文件更改时间降序排序
                            var sortedFiles = files.OrderByDescending(file => File.GetLastWriteTime(file));

                            // 需要删除的文件数量
                            int filesToDeleteCount = files.Length - Math.Max(2, (int)(files.Length * 0.1));

                            // 删除较旧的文件
                            for (int i = 0; i < filesToDeleteCount; i++)
                            {
                                var fileName = sortedFiles.ElementAt(i);
                                if (_dict.TryGetValue(fileName, out object cache1))
                                {
                                    DisposeAndDeleteFile(fileName, cache1);
                                }
                                else
                                {
                                    DeleteFile(fullName);
                                }
                            }
                        }
                    }
                }
                //文件数量限制
                {
                    string searchPattern = "*.ldb"; // 文件名匹配模式
                    string[] files = Directory.GetFiles(dir, searchPattern);
                    if (files.Length > LiteDBCacheUtil.config.MaxFileCount)
                    {
                        //数量超限就删除旧文件
                        //按文件更改时间降序排序
                        var sortedFiles = files.OrderByDescending(file => File.GetLastWriteTime(file));

                        // 需要删除的文件数量
                        int filesToDeleteCount = files.Length - LiteDBCacheUtil.config.MaxFileCount;

                        // 删除较旧的文件
                        for (int i = 0; i < filesToDeleteCount; i++)
                        {
                            var fileName = sortedFiles.ElementAt(i);
                            if (_dict.TryGetValue(fileName, out object cache1))
                            {
                                DisposeAndDeleteFile(fileName, cache1);
                            }
                            else
                            {
                                DeleteFile(fullName);
                            }
                        }
                    }
                }

                //文件大小限制
                long? length1 = null;
                if (!File.Exists(fullName))
                {
                    length1 = 0;
                }
                else
                {
                    length1 = new FileInfo(fullName).Length;
                }
                var mb1 = Math.Round((double)length1 / (double)1024 / (double)1024, 2);

                if (mb1 > LiteDBCacheUtil.config.MaxFileLength)
                {
                    //大小超限就返回新的文件
                    var newFullName = dir.CombinePath($"{fileStart}{maxNum + 1}{ex}");
                    {
                        if (_dict.TryGetValue(fullName, out object cache1))
                        {
                            //注销原连接，释放文件句柄
                            cache1.TryDispose();
                            //取消字典
                            _dict.Remove(fullName);
                        }
                    }
                    {
                        if (_dict.TryGetValue(newFullName, out object cache1))
                        {
                            try
                            {
                                return (LiteDBCache<T>)cache1;
                            }
                            catch (Exception)
                            {
                                //可能类型变换导致错误，此时返回null,并释放连接
                                DisposeAndDeleteFile(newFullName, cache1);
                                var cache = new LiteDBCache<T>(id, typeName, newFullName);
                                _dict.TryAdd(newFullName, cache);
                                return cache;
                            }
                        }
                        else
                        {
                            var cache = new LiteDBCache<T>(id, typeName, newFullName);
                            _dict.TryAdd(newFullName, cache);
                            return cache;
                        }
                    }
                }
            }

            {
                if (_dict.TryGetValue(fullName, out object cache1))
                {
                    //返回原连接
                    try
                    {
                        return (LiteDBCache<T>)cache1;
                    }
                    catch (Exception)
                    {
                        //可能类型变换导致错误，此时返回null,并释放连接
                        DisposeAndDeleteFile(fullName, cache1);

                        var cache = new LiteDBCache<T>(id, typeName, fullName);
                        _dict.TryAdd(fullName, cache);
                        return cache;
                    }
                }
                {
                    var cache = new LiteDBCache<T>(id, typeName, fullName);
                    _dict.TryAdd(fullName, cache);
                    return cache;
                }
            }
        }
    }

    private static void DeleteFile(string file)
    {
        if (File.Exists(file))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }
    }

    private static void DisposeAndDeleteFile(string file, object cache1)
    {
        cache1.TryDispose();
        //取消字典
        _dict.Remove(file);
        //删除旧文件
        DeleteFile(file);
    }

    public static string GetFileBasePath()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "ListDBCache");
        //创建文件夹
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string GetFilePath(string id)
    {
        var dir = GetFileBasePath().CombinePath(id);
        //创建文件夹
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static string GetFileStartName(string typeName)
    {
        var fileStart = $"{Regex.Replace($"{typeName}", "[^a-zA-Z0-9]", "_")}";
        return fileStart;
    }

    /// <summary>
    /// 获取最大的后缀
    /// <para>例如：dir/xx.ldb，dir/xx2.ldb，会返回2</para>
    /// </summary>
    private static int? GetMaxNumFileName(string id, string typeName)
    {
        var dir = GetFilePath(id);
        var fileStart = GetFileStartName(typeName);

        //搜索全部符合条件的文件
        if (!File.Exists(dir.CombinePath($"{fileStart}{ex}")))
        {
            return null;
        }
        var index = 1;
        while (true)
        {
            var newFileName = dir.CombinePath($"{fileStart}{index}{ex}");
            if (System.IO.File.Exists(newFileName))
            {
                index++;
            }
            else
            {
                return (index == 1 ? null : index - 1);
            }
        }
    }
}