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

namespace ThingsGateway.Foundation;

/// <inheritdoc/>
public class LastLogResult : OperResultClass<IEnumerable<LogData>>
{
    /// <inheritdoc/>
    public LastLogResult() : base()
    {
    }

    /// <inheritdoc/>
    public LastLogResult(IOperResult operResult) : base(operResult)
    {
    }

    /// <inheritdoc/>
    public LastLogResult(string msg) : base(msg)
    {
    }

    /// <inheritdoc/>
    public LastLogResult(Exception ex) : base(ex)
    {
    }

    /// <inheritdoc/>
    public LastLogResult(string msg, Exception ex) : base(msg, ex)
    {
    }

    /// <summary>
    /// 流位置
    /// </summary>
    public long Position { set; get; }
}

/// <summary>
/// 日志数据
/// </summary>
public class LogData
{
    /// <summary>
    /// 异常
    /// </summary>
    public string? ExceptionString { get; set; }

    /// <summary>
    /// 级别
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    public string LogTime { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// <inheritdoc/>
/// </summary>
public class LogInfoResult : OperResultClass
{
    /// <summary>
    /// 全名称
    /// </summary>
    public string FullName { set; get; }

    /// <summary>
    /// 流位置
    /// </summary>
    public long Length { set; get; }
}

/// <summary>日志文本文件倒序读取</summary>
public class TextFileReader
{
    /// <summary>
    /// 获取指定目录下所有文件信息
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>包含文件信息的列表</returns>
    public static List<LogInfoResult> GetFiles(string directoryPath)
    {
        // 检查目录是否存在
        if (!Directory.Exists(directoryPath))
        {
            return new List<LogInfoResult>();
        }

        // 获取目录下所有文件路径
        var files = Directory.GetFiles(directoryPath);

        // 如果文件列表为空，则返回空列表
        if (files == null || files.Length == 0)
        {
            return new List<LogInfoResult>();
        }

        // 获取文件信息并按照最后写入时间降序排序
        var fileInfos = files.Select(filePath => new FileInfo(filePath))
                             .OrderByDescending(x => x.LastWriteTime)
                             .Select(x => new LogInfoResult() { FullName = x.FullName, Length = x.Length })
                             .ToList();

        return fileInfos;
    }

    /// <summary>
    /// 从指定位置开始倒序读取文本文件，并解析日志数据
    /// </summary>
    /// <param name="file">文件路径</param>
    /// <param name="position">读取流的起始位置，如果为0，则从文件末尾开始读取</param>
    /// <param name="lineCount">读取行数，默认为200行</param>
    /// <returns>返回最后读取的日志内容、新的起始位置和操作状态</returns>
    public static LastLogResult LastLog(string file, long position, int lineCount = 200)
    {
        LastLogResult result = new(); // 初始化结果对象
        try
        {
            if (!File.Exists(file)) // 检查文件是否存在
            {
                result.OperCode = 999;
                result.ErrorMessage = "The file path is invalid";
                return result;
            }

            List<string> txt = new(); // 存储读取的文本内容
            long ps = position; // 保存起始位置

            using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (ps <= 0) // 如果起始位置小于等于0，将起始位置设置为文件长度
                    ps = fs.Length;

                // 循环读取指定行数的文本内容
                for (int i = 0; i < lineCount; i++)
                {
                    ps = InverseReadRow(fs, ps, txt); // 调用方法逆向读取一行文本并存储
                    if (ps <= 0) // 如果已经读取到文件开头则跳出循环
                        break;
                }
            }

            // 解析读取的文本为日志数据
            result.Content = txt.Select(a =>
            {
                var data = ParseCSV(a); // 解析CSV格式的文本
                if (data.Count > 2) // 如果解析出的数据列数大于2，则认为是有效日志数据
                {
                    var log = new LogData(); // 创建日志数据对象
                    log.LogTime = data[0].Trim(); // 日志时间
                    log.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), data[1].Trim()); // 日志级别
                    log.Message = data[2].Trim(); // 日志消息
                    log.ExceptionString = data.Count > 3 ? data[3].Trim() : null; // 异常信息（如果有）
                    return log; // 返回解析后的日志数据
                }
                else
                {
                    return null; // 数据列数不足2则返回空
                }
            }).Where(a => a != null).ToList()!; // 过滤空值并转换为列表

            result.Position = ps; // 更新起始位置
            result.OperCode = 0; // 操作状态设为成功
            return result; // 返回解析结果
        }
        catch (Exception ex) // 捕获异常
        {
            result = new LastLogResult(ex); // 创建包含异常信息的结果对象
            return result; // 返回异常结果
        }
    }

    /// <summary>
    /// 从后向前按行读取文本文件，并根据特定规则筛选行数据
    /// </summary>
    /// <param name="fs">文件流</param>
    /// <param name="position">读取位置</param>
    /// <param name="strs">存储读取的文本行</param>
    /// <param name="maxRead">每次最多读取字节数，默认为10kb</param>
    /// <returns>返回新的读取位置</returns>
    private static long InverseReadRow(FileStream fs, long position, List<string> strs, int maxRead = 10240)
    {
        byte n = 0xD; // 换行符
        byte a = 0xA; // 回车符

        if (fs.Length == 0) return 0; // 若文件长度为0，则直接返回0作为新的位置

        var newPos = position - 1; // 新的位置从指定位置减一开始

        try
        {
            int curVal;
            var readLength = 0;
            LinkedList<byte> arr = new LinkedList<byte>(); // 存储读取的字节数据

            while (true) // 循环读取一行数据，TextFileLogger.Separator行判定
            {
                readLength++;
                if (newPos <= 0)
                    newPos = 0;

                fs.Position = newPos;
                curVal = fs.ReadByte();
                if (curVal == -1) break; // 到达文件开头时跳出循环

                arr.AddFirst((byte)curVal); // 将读取的字节插入列表头部

                if (curVal == n || curVal == a)//判断当前字符是换行符 // TextFileLogger.Separator
                {
                    if (MatchSeparator(arr))
                    {
                        // 去掉匹配的指定字符串
                        for (int i = 0; i < TextFileLogger.SeparatorBytes.Length; i++)
                        {
                            arr.RemoveFirst();
                        }
                        break;
                    }
                }

                if (readLength == maxRead) // 达到最大读取限制时，直接放弃
                {
                    arr = new();
                    readLength = arr.Count;
                }
                newPos--;
                if (newPos <= -1)
                    break;
            }

            if (arr.Count >= 5) // 处理完整的行数据
            {
                var str = Encoding.UTF8.GetString(arr.ToArray()); // 转换为字符串
                strs.Add(str); // 存储有效行数据
            }

            return newPos; // 返回新的读取位置
        }
        finally
        {
        }
    }

    private static bool MatchSeparator(LinkedList<byte> arr)
    {
        if (arr.Count < TextFileLogger.SeparatorBytes.Length)
        {
            return false;
        }

        var currentNode = arr.First; // 从头节点开始
        for (int i = 0; i < TextFileLogger.SeparatorBytes.Length; i++)
        {
            if (currentNode.Value != TextFileLogger.SeparatorBytes[i])
            {
                return false;
            }

            currentNode = currentNode.Next; // 移动到下一个节点
        }

        return true;
    }

    private static List<string> ParseCSV(string data)
    {
        List<string> items = new List<string>();

        int i = 0;
        while (i < data.Length)
        {
            // 当前字符不是逗号，开始解析一个新的数据项
            if (data[i] != ',')
            {
                int j = i;
                bool inQuotes = false;

                // 解析到一个未闭合的双引号时，继续读取下一个数据项
                while (j < data.Length && (inQuotes || data[j] != ','))
                {
                    if (data[j] == '\"')
                    {
                        inQuotes = !inQuotes;
                    }
                    j++;
                }

                // 去掉前后的双引号并将当前数据项加入列表中
                items.Add(RemoveQuotes(data.Substring(i, j - i)));

                // 跳过当前数据项结尾的逗号
                if (j < data.Length && data[j] == ',')
                {
                    j++;
                }

                i = j;
            }
            // 当前字符是逗号，跳过它
            else
            {
                i++;
            }
        }

        return items;
    }

    private static string RemoveQuotes(string data)
    {
        if (data.Length >= 2 && data[0] == '\"' && data[data.Length - 1] == '\"')
        {
            return data.Substring(1, data.Length - 2);
        }
        else
        {
            return data;
        }
    }
}
