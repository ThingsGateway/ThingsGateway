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

using System.Collections.Concurrent;

namespace ThingsGateway.Foundation.Dmtp.FileTransfer
{
    /// <summary>
    /// 文件资源控制器。
    /// </summary>
    public class FileResourceController : DisposableObject, IFileResourceController
    {
        private readonly Timer m_timer;

        /// <summary>
        /// 文件资源控制器
        /// </summary>
        public FileResourceController()
        {
            this.Timeout = TimeSpan.FromSeconds(60);
            this.m_timer = new Timer((o) =>
            {
                var ints = new List<int>();
                foreach (var item in this.FileResourceStore)
                {
                    if (DateTime.Now - item.Value.LastActiveTime > this.Timeout)
                    {
                        ints.Add(item.Key);
                    }
                }

                foreach (var item in ints)
                {
                    if (this.FileResourceStore.TryRemove(item, out var locator))
                    {
                        locator.SafeDispose();
                    }
                }
            }, null, 1000, 1000);
        }

        /// <summary>
        /// 文件资源控制器析构函数
        /// </summary>
        ~FileResourceController()
        {
            this.Dispose(false);
        }

        /// <inheritdoc/>
        public ConcurrentDictionary<int, FileResourceLocator> FileResourceStore { get; } = new ConcurrentDictionary<int, FileResourceLocator>();

        /// <summary>
        /// 超时时间。默认60秒。
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <inheritdoc/>
        public virtual string GetFullPath(string root, string path)
        {
            this.ThrowIfDisposed();
            return path.IsNullOrEmpty() ? string.Empty : Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(root, path));
        }

        /// <inheritdoc/>
        public virtual FileResourceLocator LoadFileResourceLocatorForRead(string path, int fileSectionSize)
        {
            this.ThrowIfDisposed();
            var fileResourceInfo = new FileResourceInfo(new FileInfo(path), fileSectionSize);
            var fileResourceLocator = new FileResourceLocator(fileResourceInfo);
            this.FileResourceStore.TryAdd(fileResourceInfo.ResourceHandle, fileResourceLocator);
            return fileResourceLocator;
        }

        /// <inheritdoc/>
        public virtual FileResourceLocator LoadFileResourceLocatorForWrite(string savePath, FileResourceInfo fileResourceInfo)
        {
            this.ThrowIfDisposed();
            if (this.FileResourceStore.TryGetValue(fileResourceInfo.ResourceHandle, out var fileResourceLocator))
            {
                return fileResourceLocator;
            }
            fileResourceInfo.ResetResourceHandle(fileResourceInfo.GetHashCode());
            fileResourceLocator = new FileResourceLocator(fileResourceInfo, savePath);
            this.FileResourceStore.TryAdd(fileResourceInfo.ResourceHandle, fileResourceLocator);
            return fileResourceLocator;
        }

        /// <inheritdoc/>
        public virtual int ReadAllBytes(FileInfo fileInfo, byte[] buffer)
        {
            this.ThrowIfDisposed();
            using (var reader = FilePool.GetReader(fileInfo))
            {
                return reader.Read(buffer, 0, buffer.Length);
            }
        }

        /// <inheritdoc/>
        public virtual bool TryGetFileResourceLocator(int resourceHandle, out FileResourceLocator fileResourceLocator)
        {
            this.ThrowIfDisposed();
            return this.FileResourceStore.TryGetValue(resourceHandle, out fileResourceLocator);
        }

        /// <inheritdoc/>
        public virtual bool TryRelaseFileResourceLocator(int resourceHandle, out FileResourceLocator locator)
        {
            this.ThrowIfDisposed();
            if (this.FileResourceStore.TryRemove(resourceHandle, out locator))
            {
                locator.SafeDispose();
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public virtual void WriteAllBytes(string path, byte[] buffer, int offset, int length)
        {
            this.ThrowIfDisposed();
            using (var writer = FilePool.GetWriter(path))
            {
                writer.Write(buffer, offset, length);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_timer.SafeDispose();
            foreach (var item in this.FileResourceStore.Keys.ToArray())
            {
                if (this.FileResourceStore.TryRemove(item, out var locator))
                {
                    locator.SafeDispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}