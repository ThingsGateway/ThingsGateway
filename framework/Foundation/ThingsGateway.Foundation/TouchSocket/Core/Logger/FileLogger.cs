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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// 文件日志记录器
    /// <para>会在指定目录下，生成logs文件夹，然后按[yyyy-MM-dd].log的形式，每日生成日志</para>
    /// </summary>
    public class FileLogger : LoggerBase, IDisposable
    {
        private readonly object m_lock = new object();
        private readonly string m_rootPath;
        private bool m_disposedValue;
        private FileStorageWriter m_writer;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rootPath">日志根目录</param>
        public FileLogger(string rootPath = "logs")
        {
            if (rootPath is null)
            {
                throw new ArgumentNullException(nameof(rootPath));
            }
            this.m_rootPath = rootPath;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~FileLogger()
        {
            this.Dispose(disposing: false);
        }

        /// <summary>
        /// 最大日志尺寸
        /// </summary>
        public int MaxSize { get; set; } = 1024 * 1024;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.m_disposedValue)
            {
                if (disposing)
                {
                }

                this.m_writer?.Dispose();
                this.m_disposedValue = true;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="source"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        protected override void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ffff"));
            stringBuilder.Append(" | ");
            stringBuilder.Append(logLevel.ToString());
            stringBuilder.Append(" | ");
            stringBuilder.Append(message);

            if (exception != null)
            {
                stringBuilder.Append(" | ");
                stringBuilder.Append($"【异常消息】：{exception.Message}");
                stringBuilder.Append($"【堆栈】：{(exception == null ? "未知" : exception.StackTrace)}");
            }
            stringBuilder.AppendLine();

            this.Print(stringBuilder.ToString());
        }

        private void Print(string logString)
        {
            try
            {
                lock (this.m_lock)
                {
                    if (this.m_writer == null || this.m_writer.DisposedValue)
                    {
                        var dir = Path.Combine(this.m_rootPath, DateTime.Now.ToString("[yyyy-MM-dd]"));
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        var count = 0;
                        string path = null;
                        while (true)
                        {
                            path = Path.Combine(dir, $"{count:0000}.log");
                            if (!File.Exists(path))
                            {
                                this.m_writer = FilePool.GetWriter(path);
                                this.m_writer.FileStorage.AccessTimeout = TimeSpan.MaxValue;
                                break;
                            }
                            count++;
                        }
                    }
                    this.m_writer.Write(Encoding.UTF8.GetBytes(logString));
                    this.m_writer.FileStorage.Flush();
                    if (this.m_writer.FileStorage.Length > this.MaxSize)
                    {
                        //this.m_writer.FileStorage.Flush();
                        this.m_writer.SafeDispose();
                        this.m_writer = null;
                    }
                }
            }
            catch
            {
                this.m_writer.SafeDispose();
                this.m_writer = null;
            }
        }
    }
}