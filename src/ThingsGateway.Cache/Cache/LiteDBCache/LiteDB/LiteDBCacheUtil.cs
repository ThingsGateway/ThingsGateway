//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

using Furion;
using Furion.Logging.Extensions;

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
    public static LiteDBCache<T>? GetDB<T>(string id, string typeName, bool isInsert, bool isDeleteRule = true) where T : IPrimaryIdEntity
    {
        lock (_dictObject)
        {
            var dir = GetFilePath(id);
            var fileStart = GetFileStartName(typeName);
            var maxNum = GetMaxNumFileName(id, typeName);
            var fullName = dir.CombinePath($"{fileStart}_{maxNum}{ex}");
            if (isDeleteRule)
            {
                //磁盘使用率限制
                {
                    string currentPath = Directory.GetCurrentDirectory();
                    DriveInfo drive = new(Path.GetPathRoot(currentPath));
                    var driveUsage = (100 - (drive.TotalFreeSpace * 100.00 / drive.TotalSize));
                    if (driveUsage > LiteDBCacheUtil.config.MaxDriveUsage)
                    {
                        $"磁盘使用率超限，将删除缓存文件".LogInformation();
                        //删除全部文件夹中旧文件
                        string[] dirs = Directory.GetDirectories(GetFileBasePath());
                        //遍历全部文件夹，删除90%的文件
                        foreach (var d in dirs)
                        {
                            string[] files = Directory.GetFiles(d);
                            //如果文件数量小于4的，退出循环
                            if (files.Length < 4)
                            {
                                break;
                            }
                            //数量超限就删除旧文件
                            //按文件更改时间排序
                            var sortedFiles = files.OrderBy(file => File.GetLastWriteTime(file)).ToArray();

                            // 需要删除的文件数量
                            int filesToDeleteCount = files.Length - Math.Max(2, (int)(files.Length * 0.1));

                            // 删除较旧的文件
                            for (int i = 0; i < filesToDeleteCount; i++)
                            {
                                var fileName = sortedFiles[i];
                                if (_dict.TryGetValue(fileName, out object cache1))
                                {
                                    DisposeAndDeleteFile(fileName, cache1);
                                }
                                else
                                {
                                    DeleteFile(fileName);
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
                        $"{dir}缓存文件数量超限，将删除文件".LogInformation();
                        //数量超限就删除旧文件
                        //按文件更改时间降序排序
                        var sortedFiles = files.OrderBy(file => File.GetLastWriteTime(file)).ToArray();

                        // 需要删除的文件数量
                        int filesToDeleteCount = files.Length - LiteDBCacheUtil.config.MaxFileCount;

                        // 删除较旧的文件
                        for (int i = 0; i < filesToDeleteCount; i++)
                        {
                            var fileName = sortedFiles[i];
                            if (_dict.TryGetValue(fileName, out object cache1))
                            {
                                DisposeAndDeleteFile(fileName, cache1);
                            }
                            else
                            {
                                DeleteFile(fileName);
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

                if (isInsert && mb1 > LiteDBCacheUtil.config.MaxFileLength)
                {
                    $"{fullName}缓存文件大小超限，将产生新文件".LogInformation();
                    //大小超限就返回新的文件
                    var newFullName = dir.CombinePath($"{fileStart}_{maxNum + 1}{ex}");
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
                        var connect = (LiteDBCache<T>)cache1;
                        if (maxNum > 1 && !isInsert)
                        {
                            if (connect.GetPage(1, 1).Count == 0)
                            {
                                //无内容时，删除文件
                                DisposeAndDeleteFile(fullName, cache1);
                                return GetDB<T>(id, typeName, isInsert, isDeleteRule);
                            }
                        }
                        else
                        {
                            if (maxNum == 1)
                            {
                                if (isDeleteRule)
                                {
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
                                        connect.InitDb(true);
                                    }
                                }
                            }
                        }
                        return connect;
                    }
                    catch (Exception)
                    {
                        //可能类型变换导致错误，此时释放连接
                        DisposeAndDeleteFile(fullName, cache1);

                        return GetDB<T>(id, typeName, isInsert, isDeleteRule);
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
            $"删除{file}缓存文件".LogInformation();
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
        string[] files = Directory.GetFiles(dir, $"{fileStart}_*.ldb");
        int maxNumber = 1;

        Regex regex = new Regex(@"_(\d+)\.ldb$");

        foreach (var file in files)
        {
            Match match = regex.Match(file);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
            {
                if (number > maxNumber)
                {
                    maxNumber = number;
                }
            }
        }
        return maxNumber;
        //搜索全部符合条件的文件
        //if (!File.Exists(dir.CombinePath($"{fileStart}_1{ex}")))
        //{
        //    return 1;
        //}
        //var index = 2;
        //while (true)
        //{
        //    var newFileName = dir.CombinePath($"{fileStart}_{index}{ex}");
        //    if (System.IO.File.Exists(newFileName))
        //    {
        //        index++;
        //    }
        //    else
        //    {
        //        return (index - 1);
        //    }
        //}
    }
}