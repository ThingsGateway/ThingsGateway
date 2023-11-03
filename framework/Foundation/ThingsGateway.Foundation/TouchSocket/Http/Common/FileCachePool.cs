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

using System.Text;
using System.Web;

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// 静态文件缓存池
    /// </summary>
    public class FileCachePool : DisposableObject
    {
        /// <summary>
        /// 添加委托
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public delegate bool InsertHandler(FileCachePool cache, string key, byte[] value, TimeSpan timeout);

        #region Cache items access

        /// <summary>
        /// Is the file cache empty?
        /// </summary>
        public bool Empty => this.entriesByKey.Count == 0;

        /// <summary>
        /// Get the file cache size
        /// </summary>
        public int Size => this.entriesByKey.Count;

        /// <summary>
        /// Add a new cache value with the given timeout into the file cache
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="timeout">Cache timeout (default is 0 - no timeout)</param>
        /// <returns>'true' if the cache value was added, 'false' if the given key was not added</returns>
        public bool Add(string key, byte[] value, TimeSpan timeout = new TimeSpan())
        {
            using (new WriteLock(this.lockEx))
            {
                // Try to find and remove the previous key
                this.entriesByKey.Remove(key);

                // Update the cache entry
                this.entriesByKey.Add(key, new MemCacheEntry(value, timeout));

                return true;
            }
        }

        /// <summary>
        /// Try to find the cache value by the given key
        /// </summary>
        /// <param name="key">Key to find</param>
        /// <param name="data"></param>
        /// <returns>'true' and cache value if the cache value was found, 'false' if the given key was not found</returns>
        public bool Find(string key, out byte[] data)
        {
            using (new ReadLock(this.lockEx))
            {
                // Try to find the given key
                if (!this.entriesByKey.TryGetValue(key, out var cacheValue))
                {
                    data = null;
                    return false;
                }
                data = cacheValue.Value;
                return true;
            }
        }

        /// <summary>
        /// Remove the cache value with the given key from the file cache
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <returns>'true' if the cache value was removed, 'false' if the given key was not found</returns>
        public bool Remove(string key)
        {
            using (new WriteLock(this.lockEx))
            {
                return this.entriesByKey.Remove(key);
            }
        }

        #endregion Cache items access

        #region Cache management methods

        /// <summary>
        /// Insert a new cache path with the given timeout into the file cache
        /// </summary>
        /// <param name="path">Path to insert</param>
        /// <param name="prefix">Cache prefix (default is "/")</param>
        /// <param name="filter">Cache filter (default is "*.*")</param>
        /// <param name="timeout">Cache timeout (default is 0 - no timeout)</param>
        /// <param name="handler">Cache insert handler (default is 'return cache.Add(key, value, timeout)')</param>
        /// <returns>'true' if the cache path was setup, 'false' if failed to setup the cache path</returns>
        public bool InsertPath(string path, string prefix = "/", string filter = "*.*", TimeSpan timeout = new TimeSpan(), InsertHandler handler = null)
        {
            handler ??= (FileCachePool cache, string key, byte[] value, TimeSpan timespan) => cache.Add(key, value, timespan);

            // Try to find and remove the previous path
            this.RemovePathInternal(path);

            using (new WriteLock(this.lockEx))
            {
                // Add the given path to the cache
                this.pathsByKey.Add(path, new FileCacheEntry(this, prefix, path, filter, handler, timeout));
                // Create entries by path map
                this.entriesByPath[path] = new HashSet<string>();
            }

            // Insert the cache path
            return this.InsertPathInternal(path, path, prefix, timeout, handler);
        }

        /// <summary>
        /// Try to find the cache path
        /// </summary>
        /// <param name="path">Path to find</param>
        /// <returns>'true' if the cache path was found, 'false' if the given path was not found</returns>
        public bool FindPath(string path)
        {
            using (new ReadLock(this.lockEx))
            {
                // Try to find the given key
                return this.pathsByKey.ContainsKey(path);
            }
        }

        /// <summary>
        /// Remove the cache path from the file cache
        /// </summary>
        /// <param name="path">Path to remove</param>
        /// <returns>'true' if the cache path was removed, 'false' if the given path was not found</returns>
        public bool RemovePath(string path)
        {
            return this.RemovePathInternal(path);
        }

        /// <summary>
        /// Clear the memory cache
        /// </summary>
        public void Clear()
        {
            using (new WriteLock(this.lockEx))
            {
                // Stop all file system watchers
                foreach (var fileCacheEntry in this.pathsByKey)
                    fileCacheEntry.Value.StopWatcher();

                // Clear all cache entries
                this.entriesByKey.Clear();
                this.entriesByPath.Clear();
                this.pathsByKey.Clear();
            }
        }

        #endregion Cache management methods

        #region Cache implementation

        private readonly ReaderWriterLockSlim lockEx = new ReaderWriterLockSlim();
        private readonly Dictionary<string, MemCacheEntry> entriesByKey = new Dictionary<string, MemCacheEntry>();
        private readonly Dictionary<string, HashSet<string>> entriesByPath = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, FileCacheEntry> pathsByKey = new Dictionary<string, FileCacheEntry>();

        private class MemCacheEntry
        {
            public byte[] Value { get; }
            public TimeSpan Timespan { get; private set; }

            public MemCacheEntry(byte[] value, TimeSpan timespan = new TimeSpan())
            {
                this.Value = value;
                this.Timespan = timespan;
            }

            public MemCacheEntry(string value, TimeSpan timespan = new TimeSpan())
            {
                this.Value = Encoding.UTF8.GetBytes(value);
                this.Timespan = timespan;
            }
        };

        private class FileCacheEntry
        {
            private readonly string _prefix;
            private readonly string _path;
            private readonly InsertHandler _handler;
            private readonly TimeSpan _timespan;
            private readonly FileSystemWatcher _watcher;

            public FileCacheEntry(FileCachePool cache, string prefix, string path, string filter, InsertHandler handler, TimeSpan timespan)
            {
                this._prefix = prefix;
                this._path = path;
                this._handler = handler;
                this._timespan = timespan;
                this._watcher = new FileSystemWatcher();

                // Start the filesystem watcher
                this.StartWatcher(cache, path, filter);
            }

            private void StartWatcher(FileCachePool cache, string path, string filter)
            {
                var entry = this;

                // Initialize a new filesystem watcher
                this._watcher.Created += (sender, e) => OnCreated(sender, e, cache, entry);
                this._watcher.Changed += (sender, e) => OnChanged(sender, e, cache, entry);
                this._watcher.Deleted += (sender, e) => OnDeleted(sender, e, cache, entry);
                this._watcher.Renamed += (sender, e) => OnRenamed(sender, e, cache, entry);
                this._watcher.Path = path;
                this._watcher.IncludeSubdirectories = true;
                this._watcher.Filter = filter;
                this._watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                this._watcher.EnableRaisingEvents = true;
            }

            public void StopWatcher()
            {
                this._watcher.Dispose();
            }

            private static bool IsDirectory(string path)
            {
                try
                {
                    // Skip directory updates
                    if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
                        return true;
                }
                catch (Exception) { }

                return false;
            }

            private static void OnCreated(object sender, FileSystemEventArgs e, FileCachePool cache, FileCacheEntry entry)
            {
                var key = e.FullPath.Replace(entry._path, entry._prefix);
                var file = e.FullPath;

                // Skip missing files
                if (!File.Exists(file))
                    return;
                // Skip directory updates
                if (IsDirectory(file))
                    return;

                cache.InsertFileInternal(entry._path, file, key, entry._timespan, entry._handler);
            }

            private static void OnChanged(object sender, FileSystemEventArgs e, FileCachePool cache, FileCacheEntry entry)
            {
                if (e.ChangeType != WatcherChangeTypes.Changed)
                    return;

                var key = e.FullPath.Replace(entry._path, entry._prefix);
                var file = e.FullPath;

                // Skip missing files
                if (!File.Exists(file))
                    return;
                // Skip directory updates
                if (IsDirectory(file))
                    return;

                cache.InsertFileInternal(entry._path, file, key, entry._timespan, entry._handler);
            }

            private static void OnDeleted(object sender, FileSystemEventArgs e, FileCachePool cache, FileCacheEntry entry)
            {
                var key = e.FullPath.Replace(entry._path, entry._prefix);
                var file = e.FullPath;

                cache.RemoveFileInternal(entry._path, key);
            }

            private static void OnRenamed(object sender, RenamedEventArgs e, FileCachePool cache, FileCacheEntry entry)
            {
                var oldKey = e.OldFullPath.Replace(entry._path, entry._prefix);
                var oldFile = e.OldFullPath;
                var newKey = e.FullPath.Replace(entry._path, entry._prefix);
                var newFile = e.FullPath;

                // Skip missing files
                if (!File.Exists(newFile))
                    return;
                // Skip directory updates
                if (IsDirectory(newFile))
                    return;

                cache.RemoveFileInternal(entry._path, oldKey);
                cache.InsertFileInternal(entry._path, newFile, newKey, entry._timespan, entry._handler);
            }
        };

        private bool InsertFileInternal(string path, string file, string key, TimeSpan timeout, InsertHandler handler)
        {
            try
            {
                key = key.Replace('\\', '/');
                file = file.Replace('\\', '/');

                // Load the cache file content
                var content = File.ReadAllBytes(file);
                if (!handler(this, key, content, timeout))
                    return false;

                using (new WriteLock(this.lockEx))
                {
                    // Update entries by path map
                    this.entriesByPath[path].Add(key);
                }

                return true;
            }
            catch (Exception) { return false; }
        }

        private bool RemoveFileInternal(string path, string key)
        {
            try
            {
                key = key.Replace('\\', '/');

                using (new WriteLock(this.lockEx))
                {
                    // Update entries by path map
                    this.entriesByPath[path].Remove(key);
                }

                return this.Remove(key);
            }
            catch (Exception) { return false; }
        }

        private bool InsertPathInternal(string root, string path, string prefix, TimeSpan timeout, InsertHandler handler)
        {
            try
            {
                var keyPrefix = (string.IsNullOrEmpty(prefix) || (prefix == "/")) ? "/" : (prefix + "/");

                // Iterate through all directory entries
                foreach (var item in Directory.GetDirectories(path))
                {
                    var key = keyPrefix + HttpUtility.UrlDecode(Path.GetFileName(item));

                    // Recursively insert sub-directory
                    if (!this.InsertPathInternal(root, item, key, timeout, handler))
                        return false;
                }

                foreach (var item in Directory.GetFiles(path))
                {
                    var key = keyPrefix + HttpUtility.UrlDecode(Path.GetFileName(item));

                    // Insert file into the cache
                    if (!this.InsertFileInternal(root, item, key, timeout, handler))
                        return false;
                }

                return true;
            }
            catch (Exception) { return false; }
        }

        private bool RemovePathInternal(string path)
        {
            using (new WriteLock(this.lockEx))
            {
                // Try to find the given path
                if (!this.pathsByKey.TryGetValue(path, out var cacheValue))
                    return false;

                // Stop the file system watcher
                cacheValue.StopWatcher();

                // Remove path entries
                foreach (var entryKey in this.entriesByPath[path])
                    this.entriesByKey.Remove(entryKey);
                this.entriesByPath.Remove(path);

                // Remove cache path
                this.pathsByKey.Remove(path);

                return true;
            }
        }

        #endregion Cache implementation

        #region IDisposable implementation

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.Clear();
            base.Dispose(disposing);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~FileCachePool()
        {
            // Simply call Dispose(false).
            this.Dispose(false);
        }

        #endregion IDisposable implementation
    }
}