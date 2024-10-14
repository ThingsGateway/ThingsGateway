//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://thingsgateway.cn/
//  QQ群：605534569
//------------------------------------------------------------------------------

using System.Text;

using ThingsGateway.NewLife.Caching;

namespace ThingsGateway.Foundation;

/// <summary>
/// 文本文件日志类。提供向文本文件写日志的能力
/// </summary>
public class TextFileLogger : ThingsGateway.NewLife.Log.TextFileLog, TouchSocket.Core.ILog, IDisposable
{
    private static string separator = Environment.NewLine + "-----ThingsGateway-Log-Separator-----" + Environment.NewLine;

    /// <summary>
    /// 分隔符
    /// </summary>
    public static string Separator
    {
        get
        {
            return separator;
        }
        set
        {
            separator = value;
            separatorBytes = Encoding.UTF8.GetBytes(separator);
        }
    }
    private static byte[] separatorBytes = Encoding.UTF8.GetBytes(Environment.NewLine + "-----ThingsGateway-Log-Separator-----" + Environment.NewLine);

    internal static byte[] SeparatorBytes
    {
        get
        {
            return separatorBytes;
        }
    }


    private static readonly MemoryCache cache = new MemoryCache();

    /// <summary>
    ///  文本日志记录器
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="isfile">单文件</param>
    /// <param name="fileFormat">文件名称格式</param>
    private TextFileLogger(string path, bool isfile, string? fileFormat = null) : base(path, isfile, fileFormat)
    {
        _headEnable = false;
    }

    /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
    /// <param name="path">日志目录或日志文件路径</param>
    /// <param name="fileFormat"></param>
    /// <returns></returns>
    public static TextFileLogger CreateTextLogger(String path, String? fileFormat = null)
    {
        //if (path.IsNullOrEmpty()) path = XTrace.LogPath;
        if (path.IsNullOrEmpty()) path = "Log";

        var key = (path + fileFormat).ToLower();
        return cache.GetOrAdd(key, k => new TextFileLogger(path, false, fileFormat));
    }

    /// <summary>每个目录的日志实例应该只有一个，所以采用静态创建</summary>
    /// <param name="path">日志目录或日志文件路径</param>
    /// <returns></returns>
    public static TextFileLogger CreateTextFileLogger(String path)
    {
        if (path.IsNullOrEmpty()) throw new ArgumentNullException(nameof(path));

        return cache.GetOrAdd(path, k => new TextFileLogger(k, true));
    }


    /// <summary>
    /// TimeFormat
    /// </summary>
    public const string TimeFormat = "yyyy-MM-dd HH:mm:ss:ffff zz";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="source"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    protected void WriteLog(LogLevel logLevel, object source, string message, Exception exception)
    {
        if (!Check()) return;

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(DateTime.Now.ToString(TimeFormat));
        stringBuilder.Append(",");
        stringBuilder.Append(logLevel.ToString());
        stringBuilder.Append(",");
        stringBuilder.Append($"\"{message}\"");

        if (exception != null)
        {
            stringBuilder.Append(",");
            stringBuilder.Append($"\"{exception}\"");
        }

        //自定义的分割符，用于读取文件时的每一行判断，而不是单纯换行符
        stringBuilder.Append(Separator);

        // 推入队列
        Enqueue(stringBuilder.ToString());

        WriteLog();
    }

    /// <inheritdoc/>
    public LogLevel LogLevel { get; set; } = LogLevel.Trace;
    /// <inheritdoc/>
    public void Log(LogLevel logLevel, object source, string message, Exception exception)
    {
        if (logLevel < LogLevel)
        {
            return;
        }
        WriteLog(logLevel, source, message, exception);
    }

}
