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

//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 文件操作
    /// </summary>

    public static partial class FileUtility
    {
        /// <summary>
        /// 获取不重复文件名。
        /// <para>例如：New.txt已存在时，会返回New(1).txt</para>
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetDuplicateFileName(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return fileName;
            }

            var index = 0;
            while (true)
            {
                index++;
                var newPath = Path.Combine(Path.GetDirectoryName(fileName), $"{Path.GetFileNameWithoutExtension(fileName)}({index}){Path.GetExtension(fileName)}");
                if (!File.Exists(newPath))
                {
                    return newPath;
                }
            }
        }

        /// <summary>
        /// 获取不重复文件夹名称.
        /// <para>例如：NewDir已存在时，会返回NewDir(1)</para>
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        public static string GetDuplicateDirectoryName(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                return dirName;
            }

            var index = 0;
            while (true)
            {
                index++;
                var newPath = Path.Combine(Path.GetDirectoryName(dirName), $"{Path.GetFileNameWithoutExtension(dirName)}({index})");
                if (!System.IO.Directory.Exists(newPath))
                {
                    return newPath;
                }
            }
        }

        /// <summary>
        /// 转化为文件大小的字符串，类似10B，10Kb，10Mb，10Gb。
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ToFileLengthString(long length)
        {
            if (length < 1024)
            {
                return $"{length}B";
            }
            else
            {
                return length < 1024 * 1024
                    ? $"{(length / 1024.0).ToString("0.00")}Kb"
                    : length < 1024 * 1024 * 1024
                                    ? $"{(length / (1024.0 * 1024)).ToString("0.00")}Mb"
                                    : $"{(length / (1024.0 * 1024 * 1024)).ToString("0.00")}Gb";
            }
        }

        /// <summary>
        /// 获取文件MD5
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileMD5(string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return GetStreamMD5(fileStream);
            }
        }

        /// <summary>
        /// 获取流MD5
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public static string GetStreamMD5(Stream fileStream)
        {
            using (HashAlgorithm hash = System.Security.Cryptography.MD5.Create())
            {
                return GetStreamHash(fileStream, hash);
            }
        }

        /// <summary>
        /// 获得文件Hash值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static string GetFileHash256(string filePath)
        {
            try
            {
                HashAlgorithm hash = SHA256.Create();
                using (var fileStream = File.OpenRead(filePath))
                {
                    var HashValue = hash.ComputeHash(fileStream);
                    return BitConverter.ToString(HashValue).Replace("-", "");
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获得流Hash值
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetStreamHash256(Stream stream)
        {
            try
            {
                HashAlgorithm hash = SHA256.Create();
                var HashValue = hash.ComputeHash(stream);
                return BitConverter.ToString(HashValue).Replace("-", "");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获得文件Hash值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string GetFileHash(string filePath, HashAlgorithm hash)
        {
            try
            {
                using (var fileStream = File.OpenRead(filePath))
                {
                    var HashValue = hash.ComputeHash(fileStream);
                    return BitConverter.ToString(HashValue).Replace("-", "");
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获得流Hash值
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static string GetStreamHash(Stream stream, HashAlgorithm hash)
        {
            try
            {
                var HashValue = hash.ComputeHash(stream);
                return BitConverter.ToString(HashValue).Replace("-", "");
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取仅当前文件夹中包含的文件名称，不含全路径。
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static string[] GetIncludeFileNames(string dirPath)
        {
            return Directory.GetFiles(dirPath).Select(s => Path.GetFileName(s)).ToArray();
        }

        /// <summary>
        /// 获取相对路径。
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string GetRelativePath(string relativeTo, string path)
        {
            if (string.IsNullOrEmpty(relativeTo)) throw new ArgumentNullException(nameof(relativeTo));
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            var fromUri = new Uri(relativeTo);
            var toUri = new Uri(path);

            if (fromUri.Scheme != toUri.Scheme)
            {
                // 不是同一种路径，无法转换成相对路径。
                return path;
            }

            if (fromUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase)
                && !relativeTo.EndsWith("/", StringComparison.OrdinalIgnoreCase)
                && !relativeTo.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                // 如果是文件系统，则视来源路径为文件夹。
                fromUri = new Uri(relativeTo + Path.DirectorySeparatorChar);
            }

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        /// <summary>
        /// 删除路径文件
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path)
        {
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }
        }

# if NET45_OR_GREATER

        [DllImport("kernel32.dll")]
        private static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int OF_READWRITE = 2;

        private const int OF_SHARE_DENY_NONE = 0x40;

        private static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        /// <summary>
        /// 判断文件是否被已打开
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public static bool FileIsOpen(string fileFullName)
        {
            if (!File.Exists(fileFullName))
            {
                return false;
            }

            var handle = _lopen(fileFullName, OF_READWRITE | OF_SHARE_DENY_NONE);

            if (handle == HFILE_ERROR)
            {
                return true;
            }

            CloseHandle(handle);

            return false;
        }

#endif
    }
}