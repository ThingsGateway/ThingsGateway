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
using NewLife.Threading;

using System.Collections.Concurrent;
using System.Text;

namespace ThingsGateway.Foundation
{
    /// <summary>文本文件日志类。提供向文本文件写日志的能力</summary>
    /// <remarks>
    /// 两大用法：
    /// 1，Create(path, fileFormat) 指定日志目录和文件名格式
    /// 2，CreateFile(path) 指定文件，一直往里面写
    /// </remarks>
    public class TextFileLogger : LoggerBase, IDisposable
    {
        /// <summary>日志文件上限。超过上限后拆分新日志文件，默认10MB，0表示不限制大小</summary>
        public static Int32 FileMaxBytes { get; set; } = 20;

        /// <summary>日志文件备份。超过备份数后，最旧的文件将被删除，默认100，0表示不限制个数</summary>
        public static Int32 FileBackups { get; set; } = 5;

        #region 属性

        /// <summary>日志目录</summary>
        public String LogPath { get; set; } = "";

        /// <summary>日志文件格式。默认{0:yyyy_MM_dd}.log</summary>
        public String FileFormat { get; set; } = "{0:yyyy_MM_dd}.log";

        /// <summary>日志文件上限。超过上限后拆分新日志文件，默认10MB，0表示不限制大小</summary>
        public Int32 MaxBytes { get; set; } = 10;

        /// <summary>日志文件备份。超过备份数后，最旧的文件将被删除，默认100，0表示不限制个数</summary>
        public Int32 Backups { get; set; } = 100;

        private readonly Boolean _isFile = false;

        #endregion 属性

        #region 构造

        /// <summary>该构造函数没有作用，为了继承而设置</summary>
        public TextFileLogger()
        { }

        internal TextFileLogger(String path, Boolean isfile, String fileFormat = null)
        {
            LogPath = path;
            _isFile = isfile;

            if (!string.IsNullOrEmpty(fileFormat))
                FileFormat = fileFormat;

            MaxBytes = FileMaxBytes;
            Backups = FileBackups;

            _Timer = new TimerX(DoWriteAndClose, null, 0_000, 5_000) { Async = true };
        }

        private static readonly ConcurrentDictionary<String, TextFileLogger> cache = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
        /// <param name="path">日志目录或日志文件路径</param>
        /// <param name="fileFormat"></param>
        /// <returns></returns>
        public static TextFileLogger Create(String path, String fileFormat = null)
        {
            if (string.IsNullOrEmpty(path)) path = "Log";

            var key = (path + fileFormat).ToLower();
            return cache.GetOrAdd(key, k => new TextFileLogger(path, false, fileFormat));
        }

        /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
        /// <param name="path">日志目录或日志文件路径</param>
        /// <returns></returns>
        public static TextFileLogger CreateFile(String path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            return cache.GetOrAdd(path, k => new TextFileLogger(k, true));
        }

        /// <summary>销毁</summary>
        public void Dispose()
        { Dispose(true); GC.SuppressFinalize(this); }

        /// <summary>销毁</summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(Boolean disposing)
        {
            _Timer.SafeDispose();

            // 销毁前把队列日志输出
            if (Interlocked.CompareExchange(ref _writing, 1, 0) == 0) WriteAndClose(DateTime.MinValue);
        }

        #endregion 构造

        #region 内部方法

        private StreamWriter LogWriter;
        private String CurrentLogFile;
        private Int32 _logFileError;

        /// <summary>初始化日志记录文件</summary>
        private StreamWriter? InitLog(String logfile)
        {
            try
            {
                logfile.EnsureDirectory(true);
                var stream = new FileStream(logfile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                var writer = new StreamWriter(stream, Encoding.UTF8);
                _logFileError = 0;
                return LogWriter = writer;
            }
            catch (Exception ex)
            {
                _logFileError++;
                Console.WriteLine("创建日志文件失败：{0}", ex.Message);
                return null;
            }
        }

        /// <summary>获取日志文件路径</summary>
        /// <returns></returns>
        public String GetLogFile()
        {
            // 单日志文件
            if (_isFile) return LogPath.GetBasePath();

            // 目录多日志文件
            var logfile = LogPath.CombinePath(String.Format(FileFormat, TimerX.Now)).GetBasePath();

            // 是否限制文件大小
            if (MaxBytes == 0) return logfile;

            // 找到今天第一个未达到最大上限的文件
            var max = MaxBytes * 1024L * 1024L;
            var ext = Path.GetExtension(logfile);
            var name = logfile.TrimEnd(ext);
            for (var i = 1; i < 1024; i++)
            {
                if (i > 1) logfile = $"{name}_{i}{ext}";

                var fi = logfile.AsFile();
                if (!fi.Exists || fi.Length < max) return logfile;
            }

            return null;
        }

        #endregion 内部方法

        #region 异步写日志

        private readonly TimerX _Timer;
        private readonly ConcurrentQueue<String> _Logs = new();
        private volatile Int32 _logCount;
        private Int32 _writing;
        private DateTime _NextClose;

        /// <summary>写文件</summary>
        protected virtual void WriteFile()
        {
            var writer = LogWriter;

            var now = TimerX.Now;
            var logFile = GetLogFile();
            if (!_isFile && logFile != CurrentLogFile)
            {
                writer.TryDispose();
                writer = null;

                CurrentLogFile = logFile;
                _logFileError = 0;
            }

            // 错误过多时不再尝试创建日志文件。下一天更换日志文件名后，将会再次尝试
            if (writer == null && _logFileError >= 3) return;

            // 初始化日志读写器
            if (writer == null)
                writer = InitLog(logFile);
            if (writer == null) return;

            // 依次把队列日志写入文件
            while (_Logs.TryDequeue(out var str))
            {
                Interlocked.Decrement(ref _logCount);
                // 写日志。TextWriter.WriteLine内需要拷贝，浪费资源
                //writer.WriteLine(str);
                writer.Write(str);
                writer.WriteLine();
            }
            // 写完一批后，刷一次磁盘
            writer?.Flush();

            // 连续5秒没日志，就关闭
            _NextClose = now.AddSeconds(5);
        }

        /// <summary>关闭文件</summary>
        private void DoWriteAndClose(Object? state)
        {
            // 同步写日志
            if (Interlocked.CompareExchange(ref _writing, 1, 0) == 0) WriteAndClose(_NextClose);

            // 检查文件是否超过上限
            if (!_isFile && Backups > 0)
            {
                // 判断日志目录是否已存在
                var di = LogPath.GetBasePath().AsDirectory();
                if (di.Exists)
                {
                    // 删除*.del
                    try
                    {
                        var dels = di.GetFiles("*.del");
                        if (dels != null && dels.Length > 0)
                        {
                            foreach (var item in dels)
                            {
                                item.Delete();
                            }
                        }
                    }
                    catch { }

                    var ext = Path.GetExtension(FileFormat);
                    var fis = di.GetFiles("*" + ext);
                    if (fis != null && fis.Length > Backups)
                    {
                        // 删除最旧的文件
                        var retain = fis.Length - Backups;
                        fis = fis.OrderBy(e => e.LastAccessTime).OrderByDescending(a => a.Name.Length).Take(retain).ToArray();
                        foreach (var item in fis)
                        {
                            WriteLog(LogLevel.Info, null, string.Format("日志文件达到上限 {0}，删除 {1}，大小 {2:n0}Byte", Backups, item.Name, item.Length), null);
                            try
                            {
                                item.Delete();
                            }
                            catch
                            {
                                try
                                {
                                    item.MoveTo(item.FullName + ".del");
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>写入队列日志并关闭文件</summary>
        protected virtual void WriteAndClose(DateTime closeTime)
        {
            try
            {
                // 处理残余
                var writer = LogWriter;
                if (!_Logs.IsEmpty) WriteFile();

                // 连续5秒没日志，就关闭
                if (writer != null && closeTime < TimerX.Now)
                {
                    writer.TryDispose();
                    LogWriter = null;
                }
            }
            finally
            {
                _writing = 0;
            }
        }

        #endregion 异步写日志

        #region 写日志

        /// <summary>
        /// TimeFormat
        /// </summary>
        public const string Format = "yyyy-MM-dd HH:mm:ss:fff zz";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected override void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
        {
            // 据@夏玉龙反馈，如果不给Log目录写入权限，日志队列积压将会导致内存暴增
            if (_logCount > 200) return;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DateTimeUtil.Now.ToString(Format));
            stringBuilder.Append(",");
            stringBuilder.Append(logLevel.ToString());
            stringBuilder.Append(",");
            stringBuilder.Append($"\"{message}\"");

            if (exception != null)
            {
                stringBuilder.Append(",");
                stringBuilder.Append($"\"{exception}\"");
            }
            // 推入队列
            _Logs.Enqueue(stringBuilder.ToString());
            Interlocked.Increment(ref _logCount);

            // 异步写日志，实时。即使这里错误，定时器那边仍然会补上
            if (Interlocked.CompareExchange(ref _writing, 1, 0) == 0)
            {
                // 调试级别 或 致命错误 同步写日志
                if (LogLevel <= LogLevel.Debug || LogLevel >= LogLevel.Error)
                {
                    try
                    {
                        WriteFile();
                    }
                    finally
                    {
                        _writing = 0;
                    }
                }
                else
                {
                    ThreadPool.UnsafeQueueUserWorkItem(s =>
                    {
                        try
                        {
                            WriteFile();
                        }
                        catch { }
                        finally
                        {
                            _writing = 0;
                        }
                    }, null);
                }
            }
        }

        #endregion 写日志

        #region 辅助

        /// <summary>已重载。</summary>
        /// <returns></returns>
        public override String ToString() => $"{GetType().Name} {LogPath}";

        #endregion 辅助
    }
}