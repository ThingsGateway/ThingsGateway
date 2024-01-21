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

using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;

using System.Net;

namespace ThingsGateway.Admin.Core.Utils;

public class ZipUtils
{
    /// <summary>
    /// 压缩单个文件
    /// </summary>
    /// <param name="fileToZip">要压缩的文件</param>
    /// <param name="zipedFile">压缩后的文件</param>
    /// <param name="compressionLevel">压缩等级</param>
    /// <param name="blockSize">每次写入大小</param>
    public static void ZipFile(string fileToZip, string zipedFile, int compressionLevel, int blockSize)
    {
        //如果文件没有找到，则报错
        if (!System.IO.File.Exists(fileToZip))
        {
            throw new System.IO.FileNotFoundException("指定要压缩的文件: " + fileToZip + " 不存在!");
        }

        using (System.IO.FileStream ZipFile = System.IO.File.Create(zipedFile))
        {
            using (ZipOutputStream ZipStream = new ZipOutputStream(ZipFile))
            {
                using (System.IO.FileStream StreamToZip = new System.IO.FileStream(fileToZip, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    string fileName = fileToZip.Substring(fileToZip.LastIndexOf("\\") + 1);

                    ZipEntry ZipEntry = new ZipEntry(fileName);

                    ZipStream.PutNextEntry(ZipEntry);

                    ZipStream.SetLevel(compressionLevel);

                    byte[] buffer = new byte[blockSize];

                    int sizeRead = 0;

                    do
                    {
                        sizeRead = StreamToZip.Read(buffer, 0, buffer.Length);
                        ZipStream.Write(buffer, 0, sizeRead);
                    }
                    while (sizeRead > 0);

                    StreamToZip.Close();
                }

                ZipStream.Finish();
                ZipStream.Close();
            }

            ZipFile.Close();
        }
    }

    /// <summary>
    /// 压缩单个文件
    /// </summary>
    /// <param name="fileToZip">要进行压缩的文件名</param>
    /// <param name="zipedFile">压缩后生成的压缩文件名</param>
    public static void ZipFile(string fileToZip, string zipedFile)
    {
        //如果文件没有找到，则报错
        if (!File.Exists(fileToZip))
        {
            throw new System.IO.FileNotFoundException("指定要压缩的文件: " + fileToZip + " 不存在!");
        }

        using (FileStream fs = File.OpenRead(fileToZip))
        {
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();

            using (FileStream ZipFile = File.Create(zipedFile))
            {
                using (ZipOutputStream ZipStream = new ZipOutputStream(ZipFile))
                {
                    string fileName = fileToZip.Substring(fileToZip.LastIndexOf("\\") + 1);
                    ZipEntry ZipEntry = new ZipEntry(fileName);
                    ZipStream.PutNextEntry(ZipEntry);
                    ZipStream.SetLevel(5);

                    ZipStream.Write(buffer, 0, buffer.Length);
                    ZipStream.Finish();
                    ZipStream.Close();
                }
            }
        }
    }

    /// <summary>
    /// 压缩多个文件到指定路径
    /// </summary>
    /// <param name="sourceFileNames">压缩到哪个路径</param>
    /// <param name="zipFileName">压缩文件名称</param>
    public static void ZipFile(List<string> sourceFileNames, string zipFileName)
    {
        //压缩文件打包
        using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFileName)))
        {
            s.SetLevel(9);
            byte[] buffer = new byte[4096];
            foreach (string file in sourceFileNames)
            {
                if (Directory.Exists(file))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                {
                    string pPath = "";
                    pPath += Path.GetFileName(file);
                    pPath += "\\";
                    ZipSetp(file, s, pPath, sourceFileNames);
                }
                else // 否则直接压缩文件
                {
                    ZipEntry entry = new ZipEntry(Path.GetFileName(file));
                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);
                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
            }
            s.Finish();
            s.Close();
        }
    }

    /// <summary>
    /// 压缩多层目录
    /// </summary>
    /// <param name="strDirectory">待压缩目录</param>
    /// <param name="zipedFile">压缩后生成的压缩文件名，绝对路径</param>
    public static void ZipFileDirectory(string strDirectory, string zipedFile)
    {
        using (System.IO.FileStream ZipFile = System.IO.File.Create(zipedFile))
        {
            using (ZipOutputStream s = new ZipOutputStream(ZipFile))
            {
                s.SetLevel(9);
                ZipSetp(strDirectory, s, "");
            }
        }
    }

    /// <summary>
    /// 压缩多层目录
    /// </summary>
    /// <param name="strDirectory">待压缩目录</param>
    /// <param name="zipedFile">压缩后生成的压缩文件名，绝对路径</param>
    /// <param name="files">指定要压缩的文件列表(完全路径)</param>
    public static void ZipFileDirectory(string strDirectory, string zipedFile, List<string> files)
    {
        using (System.IO.FileStream ZipFile = System.IO.File.Create(zipedFile))
        {
            using (ZipOutputStream s = new ZipOutputStream(ZipFile))
            {
                s.SetLevel(9);
                ZipSetp(strDirectory, s, "", files);
            }
        }
    }

    /// <summary>
    /// 递归遍历目录
    /// </summary>
    /// <param name="strDirectory">需遍历的目录</param>
    /// <param name="s">压缩输出流对象</param>
    /// <param name="parentPath">The parent path.</param>
    /// <param name="files">需要压缩的文件</param>
    private static void ZipSetp(string strDirectory, ZipOutputStream s, string parentPath, List<string> files = null)
    {
        if (strDirectory[strDirectory.Length - 1] != Path.DirectorySeparatorChar)
        {
            strDirectory += Path.DirectorySeparatorChar;
        }

        string[] filenames = Directory.GetFileSystemEntries(strDirectory);

        byte[] buffer = new byte[4096];
        foreach (string file in filenames)// 遍历所有的文件和目录
        {
            if (files != null && !files.Contains(file))
            {
                continue;
            }
            if (Directory.Exists(file))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
            {
                string pPath = parentPath;
                pPath += Path.GetFileName(file);
                pPath += "\\";
                ZipSetp(file, s, pPath, files);
            }
            else // 否则直接压缩文件
            {
                //打开压缩文件
                string fileName = parentPath + Path.GetFileName(file);
                ZipEntry entry = new ZipEntry(fileName);

                entry.DateTime = DateTime.Now;

                s.PutNextEntry(entry);
                using (FileStream fs = File.OpenRead(file))
                {
                    int sourceBytes;
                    do
                    {
                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                        s.Write(buffer, 0, sourceBytes);
                    } while (sourceBytes > 0);
                }
            }
        }
    }

    /// <summary>
    /// 解压缩一个 zip 文件。
    /// </summary>
    /// <param name="zipedFile">压缩文件</param>
    /// <param name="strDirectory">解压目录</param>
    /// <param name="password">zip 文件的密码。</param>
    /// <param name="overWrite">是否覆盖已存在的文件。</param>
    public static void UnZip(string zipedFile, string strDirectory, bool overWrite, string password)
    {
        if (strDirectory == "")
            strDirectory = Directory.GetCurrentDirectory();
        if (!strDirectory.EndsWith("\\"))
            strDirectory = strDirectory + "\\";

        using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipedFile)))
        {
            if (password != null)
            {
                s.Password = password;
            }
            ZipEntry theEntry;

            while ((theEntry = s.GetNextEntry()) != null)
            {
                string directoryName = "";
                string pathToZip = "";
                pathToZip = theEntry.Name;

                if (pathToZip != "")
                    directoryName = Path.GetDirectoryName(pathToZip) + "\\";

                string fileName = Path.GetFileName(pathToZip);

                Directory.CreateDirectory(strDirectory + directoryName);

                if (fileName != "")
                {
                    if ((File.Exists(strDirectory + directoryName + fileName) && overWrite) || (!File.Exists(strDirectory + directoryName + fileName)))
                    {
                        using (FileStream streamWriter = File.Create(strDirectory + directoryName + fileName))
                        {
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);

                                if (size > 0)
                                    streamWriter.Write(data, 0, size);
                                else
                                    break;
                            }
                            streamWriter.Close();
                        }
                    }
                }
            }

            s.Close();
        }
    }

    /// <summary>
    /// 解压缩一个 zip 文件。
    /// </summary>
    /// <param name="zipedFile">压缩文件</param>
    /// <param name="strDirectory">解压目录</param>
    /// <param name="overWrite">是否覆盖已存在的文件。</param>
    public static void UnZip(string zipedFile, string strDirectory, bool overWrite)
    {
        UnZip(zipedFile, strDirectory, overWrite, null);
    }

    /// <summary>
    /// 解压缩一个 zip 文件。
    /// 覆盖已存在的文件。
    /// </summary>
    /// <param name="zipedFile">压缩文件</param>
    /// <param name="strDirectory">解压目录</param>
    public static void UnZip(string zipedFile, string strDirectory)
    {
        UnZip(zipedFile, strDirectory, true);
    }

    /// <summary>
    /// 获取压缩文件中指定类型的文件
    /// </summary>
    /// <param name="zipedFile">压缩文件</param>
    /// <param name="fileExtension">文件类型(.txt|.exe)</param>
    /// <returns>文件名称列表(包含子目录)</returns>
    public static List<string> GetFiles(string zipedFile, List<string> fileExtension)
    {
        List<string> files = new List<string>();
        if (!File.Exists(zipedFile))
        {
            //return files;
            throw new FileNotFoundException(zipedFile);
        }

        using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipedFile)))
        {
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                if (theEntry.IsFile)
                {
                    //Console.WriteLine("Name : {0}", theEntry.Name);
                    if (fileExtension != null)
                    {
                        if (fileExtension.Contains(Path.GetExtension(theEntry.Name)))
                        {
                            files.Add(theEntry.Name);
                        }
                    }
                    else
                    {
                        files.Add(theEntry.Name);
                    }
                }
            }
            s.Close();
        }

        return files;
    }

    /// <summary>
    /// 获取压缩文件中的所有文件
    /// </summary>
    /// <param name="zipedFile">压缩文件</param>
    /// <returns>文件名称列表(包含子目录)</returns>
    public static List<string> GetFiles(string zipedFile)
    {
        return GetFiles(zipedFile, null);
    }

    /// <summary>
    /// 打包线上线下文件
    /// </summary>
    /// <param name="zipName">压缩文件名称</param>
    /// <param name="fileList">文件列表</param>
    /// <param name="error">保存路径</param>
    /// <param name="isLocal">是否本地</param>
    public static string ZipFiles(string zipName, List<FileItem> fileList, out string error, bool isLocal = true)
    {
        error = string.Empty;

        string path = string.Format("/ZipFiles/{0}/{1}/{2}/", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        //文件保存目录
        string directory = App.WebHostEnvironment.WebRootPath + path;

        string url = App.Configuration["FileHostUrl"].TrimEnd('/') + path + zipName;
        string savePath = directory + zipName;
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(savePath)))
            {
                zipStream.SetLevel(9);   //压缩级别0-9

                foreach (var item in fileList)
                {
                    byte[] buffer = null;
                    if (isLocal)
                    {
                        FileStream stream = new FileInfo(item.FilePath).OpenRead();
                        buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, Convert.ToInt32(stream.Length));
                    }
                    else
                    {
#pragma warning disable SYSLIB0014 // 类型或成员已过时
                        buffer = new WebClient().DownloadData(item.FilePath);//取消
#pragma warning restore SYSLIB0014 // 类型或成员已过时
                    }
                    ZipEntry entry = new ZipEntry(item.FileName);
                    entry.DateTime = DateTime.Now;
                    entry.Size = buffer.Length;
                    zipStream.PutNextEntry(entry);
                    zipStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
        catch (Exception ex)
        {
            error = "文件打包失败：" + ex.Message;
        }
        return url;
    }

    /// 压缩文件夹
    /// 要打包的文件夹
    /// 是否删除原文件夹
    public static string CompressDirectory(string dirPath, bool deleteDir)
    {
        //压缩文件路径
        string pCompressPath = dirPath + ".zip";
        if (File.Exists(pCompressPath))
            File.Delete(pCompressPath);
        //创建压缩文件
        FileStream pCompressFile = new FileStream(pCompressPath, FileMode.Create);
        using (ZipOutputStream zipoutputstream = new ZipOutputStream(pCompressFile))
        {
            Crc32 crc = new Crc32();
            Dictionary<string, DateTime> fileList = GetAllFies(dirPath);
            foreach (KeyValuePair<string, DateTime> item in fileList)
            {
                FileStream fs = new FileStream(item.Key.ToString(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                // FileStream fs = File.OpenRead(item.Key.ToString());
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                ZipEntry entry = new ZipEntry(item.Key.Substring(dirPath.Length));
                entry.DateTime = item.Value;
                entry.Size = fs.Length;
                fs.Close();
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;
                zipoutputstream.PutNextEntry(entry);
                zipoutputstream.Write(buffer, 0, buffer.Length);
            }
        }
        if (deleteDir)
        {
            Directory.Delete(dirPath, true);
        }
        return pCompressPath;
    }

    ///
    /// 获取所有文件
    ///
    ///
    private static Dictionary<string, DateTime> GetAllFies(string dir)
    {
        Dictionary<string, DateTime> FilesList = new Dictionary<string, DateTime>();
        DirectoryInfo fileDire = new DirectoryInfo(dir);
        if (!fileDire.Exists)
        {
            throw new System.IO.FileNotFoundException("目录:" + fileDire.FullName + "没有找到!");
        }
        GetAllDirFiles(fileDire, FilesList);
        GetAllDirsFiles(fileDire.GetDirectories(), FilesList);
        return FilesList;
    }

    ///
    /// 获取一个文件夹下的所有文件夹里的文件
    ///
    ///
    ///
    private static void GetAllDirsFiles(DirectoryInfo[] dirs, Dictionary<string, DateTime> filesList)
    {
        foreach (DirectoryInfo dir in dirs)
        {
            foreach (FileInfo file in dir.GetFiles("."))
            {
                filesList.Add(file.FullName, file.LastWriteTime);
            }
            GetAllDirsFiles(dir.GetDirectories(), filesList);
        }
    }

    ///
    /// 获取一个文件夹下的文件
    ///
    /// 目录名称
    /// 文件列表HastTable
    private static void GetAllDirFiles(DirectoryInfo dir, Dictionary<string, DateTime> filesList)
    {
        foreach (FileInfo file in dir.GetFiles())
        {
            filesList.Add(file.FullName, file.LastWriteTime);
        }
    }
}

/// <summary>
/// 文件对象
/// </summary>
public class FileItem
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; }
}