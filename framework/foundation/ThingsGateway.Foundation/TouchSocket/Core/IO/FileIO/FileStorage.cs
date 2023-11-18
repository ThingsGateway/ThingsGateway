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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 文件存储器。在该存储器中，读写线程安全。
    /// </summary>
    public partial class FileStorage
    {
        internal volatile int m_reference;
        private readonly ReaderWriterLockSlim m_lockSlim;
        private bool m_disposedValue;
        private byte[] m_fileData;

        /// <summary>
        /// 初始化一个文件存储器。在该存储器中，读写线程安全。
        /// </summary>
        internal FileStorage(FileInfo fileInfo, FileAccess fileAccess) : this()
        {
            this.FileAccess = fileAccess;
            this.FileInfo = fileInfo;
            this.Path = fileInfo.FullName;
            this.m_reference = 0;
            this.FileStream = fileInfo.Open(FileMode.OpenOrCreate, fileAccess, FileShare.ReadWrite);
            this.m_lockSlim = new ReaderWriterLockSlim();
        }

        private FileStorage()
        {
            this.AccessTime = DateTime.Now;
            this.AccessTimeout = TimeSpan.FromSeconds(60);
        }

        /// <summary>
        /// 最后访问时间。
        /// </summary>
        public DateTime AccessTime { get; private set; }

        /// <summary>
        /// 访问超时时间。默认60s
        /// </summary>
        public TimeSpan AccessTimeout { get; set; }

        /// <summary>
        /// 是否为缓存型。为false时，意味着该文件句柄正在被该程序占用。
        /// </summary>
        public bool Cache { get; private set; }

        /// <summary>
        /// 访问属性
        /// </summary>
        public FileAccess FileAccess { get; private set; }

        /// <summary>
        /// 文件信息
        /// </summary>
        public FileInfo FileInfo { get; private set; }

        /// <summary>
        /// 文件流。
        /// 一般情况下，请不要直接访问该对象。否则有可能会产生不可预测的错误。
        /// </summary>
        public FileStream FileStream { get; private set; }

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length => this.FileStream.Length;

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 引用次数。
        /// </summary>
        public int Reference => this.m_reference;

        /// <summary>
        /// 创建一个只读的、已经缓存的文件信息。该操作不会占用文件句柄。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileStorage"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool TryCreateCacheFileStorage(string path, out FileStorage fileStorage, out string msg)
        {
            path = System.IO.Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                fileStorage = null;
                msg = TouchSocketCoreResource.FileNotExists.GetDescription(path);
                return false;
            }
            try
            {
                fileStorage = new FileStorage()
                {
                    Cache = true,
                    FileAccess = FileAccess.Read,
                    FileInfo = new FileInfo(path),
                    Path = path,
                    m_reference = 0,
                    m_fileData = File.ReadAllBytes(path)
                };
                msg = null;
                return true;
            }
            catch (Exception ex)
            {
                fileStorage = null;
                msg = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 写入时清空缓存区
        /// </summary>
        public void Flush()
        {
            this.AccessTime = DateTime.Now;
            this.FileStream.Flush();
        }

        /// <summary>
        /// 从指定位置，读取数据到缓存区。线程安全。
        /// </summary>
        /// <param name="stratPos"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(long stratPos, byte[] buffer, int offset, int length)
        {
            this.AccessTime = DateTime.Now;
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                if (this.m_disposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.FileAccess == FileAccess.Write)
                {
                    throw new Exception("该流不允许读取。");
                }
                if (this.Cache)
                {
                    var r = (int)Math.Min(this.m_fileData.Length - stratPos, length);
                    Array.Copy(this.m_fileData, stratPos, buffer, offset, r);
                    return r;
                }
                else
                {
                    this.FileStream.Position = stratPos;
                    return this.FileStream.Read(buffer, offset, length);
                }
            }
        }

        /// <summary>
        /// 减少引用次数，并尝试释放流。
        /// </summary>
        /// <param name="delayTime">延迟释放时间。当设置为0时，立即释放,单位毫秒。</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public Result TryReleaseFile(int delayTime = 0)
        {
            return FilePool.TryReleaseFile(this.Path, delayTime);
        }

        /// <summary>
        /// 从指定位置，写入数据到存储区。线程安全。
        /// </summary>
        /// <param name="stratPos"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Write(long stratPos, byte[] buffer, int offset, int length)
        {
            this.AccessTime = DateTime.Now;
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                if (this.m_disposedValue)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.FileAccess == FileAccess.Read)
                {
                    throw new Exception("该流不允许写入。");
                }
                this.FileStream.Position = stratPos;
                this.FileStream.Write(buffer, offset, length);
            }
        }

        internal void Dispose()
        {
            if (this.m_disposedValue)
            {
                return;
            }
            using (var writeLock = new WriteLock(this.m_lockSlim))
            {
                this.m_disposedValue = true;
                this.FileStream.SafeDispose();
                this.m_fileData = null;
            }
        }
    }
}