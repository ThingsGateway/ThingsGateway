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

public class LogDataCache
{
    public List<LogData> LogDatas { get; set; }
    public long Length { get; set; }
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


/// <summary>日志文本文件倒序读取</summary>
public class TextFileReader
{
    /// <summary>
    /// 获取指定目录下所有文件信息
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>包含文件信息的列表</returns>
    public static OperResult<List<string>> GetFiles(string directoryPath)
    {
        OperResult<List<string>> result = new(); // 初始化结果对象
        // 检查目录是否存在
        if (!Directory.Exists(directoryPath))
        {
            result.OperCode = 999;
            result.ErrorMessage = "Directory not exists";
            return result;
        }

        // 获取目录下所有文件路径
        var files = Directory.GetFiles(directoryPath);

        // 如果文件列表为空，则返回空列表
        if (files == null || files.Length == 0)
        {
            result.OperCode = 999;
            result.ErrorMessage = "Canot found files";
            return result;
        }

        // 获取文件信息并按照最后写入时间降序排序
        var fileInfos = files.Select(filePath => new FileInfo(filePath))
                             .OrderByDescending(x => x.LastWriteTime)
                             .Select(x => x.FullName)
                             .ToList();
        result.OperCode = 0;
        result.Content = fileInfos;
        return result;
    }

    static MemoryCache _cache = new() { Expire = 30 };
    public static OperResult<List<LogData>> LastLog(string file, int lineCount = 200)
    {
        lock (_cache)
        {

            OperResult<List<LogData>> result = new(); // 初始化结果对象
            try
            {
                if (!File.Exists(file)) // 检查文件是否存在
                {
                    result.OperCode = 999;
                    result.ErrorMessage = "The file path is invalid";
                    return result;
                }

                List<string> txt = new(); // 存储读取的文本内容
                long ps = 0; // 保存起始位置
                var key = $"{nameof(TextFileReader)}_{nameof(LastLog)}_{file})";
                long length = 0;
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    length = fs.Length;
                    var dataCache = _cache.Get<LogDataCache>(key);
                    if (dataCache != null && dataCache.Length == length)
                    {
                        result.Content = dataCache.LogDatas;
                        result.OperCode = 0; // 操作状态设为成功
                        return result; // 返回解析结果
                    }

                    if (ps <= 0) // 如果起始位置小于等于0，将起始位置设置为文件长度
                        ps = length - 1;

                    // 循环读取指定行数的文本内容
                    for (int i = 0; i < lineCount; i++)
                    {
                        ps = InverseReadRow(fs, ps, out var value); // 使用逆序读取
                        txt.Add(value);
                        if (ps <= 0) // 如果已经读取到文件开头则跳出循环
                            break;
                    }
                }

                // 使用单次 LINQ 操作进行过滤和解析
                result.Content = txt
                    .Select(a => ParseCSV(a))
                    .Where(data => data.Count >= 3)
                    .Select(data =>
                    {
                        var log = new LogData
                        {
                            LogTime = data[0].Trim(),
                            LogLevel = Enum.TryParse(data[1].Trim(), out LogLevel level) ? level : LogLevel.Info,
                            Message = data[2].Trim(),
                            ExceptionString = data.Count > 3 ? data[3].Trim() : null
                        };
                        return log;
                    })
                    .ToList();

                result.OperCode = 0; // 操作状态设为成功
                var data = _cache.Set<LogDataCache>(key, new LogDataCache() { Length = length, LogDatas = result.Content });

                return result; // 返回解析结果
            }
            catch (Exception ex) // 捕获异常
            {
                result = new(ex); // 创建包含异常信息的结果对象
                return result; // 返回异常结果
            }
        }
    }

    private static long InverseReadRow(FileStream fs, long position, out string value, int maxRead = 102400)
    {
        byte n = 0xD; // 换行符
        byte a = 0xA; // 回车符
        value = string.Empty;
        if (fs.Length == 0) return 0; // 若文件长度为0，则直接返回0作为新的位置

        var newPos = position;
        List<byte> buffer = new List<byte>(maxRead); // 缓存读取的数据

        try
        {
            var readLength = 0;

            while (true) // 循环读取一行数据，TextFileLogger.Separator行判定
            {
                readLength++;
                if (newPos <= 0)
                    newPos = 0;

                fs.Position = newPos;
                int byteRead = fs.ReadByte();

                if (byteRead == -1) break; // 到达文件开头时跳出循环

                buffer.Add((byte)byteRead);

                if (byteRead == n || byteRead == a)//判断当前字符是换行符 // TextFileLogger.Separator
                {
                    if (MatchSeparator(buffer))
                    {
                        // 去掉匹配的指定字符串
                        buffer.RemoveRange(buffer.Count - TextFileLogger.SeparatorBytes.Length, TextFileLogger.SeparatorBytes.Length);
                        break;
                    }
                }

                if (buffer.Count > maxRead) // 超过最大字节数限制时丢弃数据
                {
                    newPos = -1;
                    return newPos;
                }
                newPos--;
                if (newPos <= -1)
                    break;
            }

            if (buffer.Count >= 10)
            {
                buffer.Reverse();
                value = Encoding.UTF8.GetString(buffer.ToArray()); // 转换为字符串
            }

            return newPos; // 返回新的读取位置
        }
        finally
        {
        }
    }


    private static bool MatchSeparator(List<byte> arr)
    {
        if (arr.Count < TextFileLogger.SeparatorBytes.Length)
        {
            return false;
        }
        var pos = arr.Count - 1;
        for (int i = 0; i < TextFileLogger.SeparatorBytes.Length; i++)
        {
            if (arr[pos] != TextFileLogger.SeparatorBytes[i])
            {
                return false;
            }
            pos--;
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
