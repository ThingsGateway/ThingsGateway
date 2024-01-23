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

using System.Runtime.InteropServices;
using System.Text;

namespace ThingsGateway.Foundation
{
    /// <summary>
    /// 日志数据
    /// </summary>
    public class LogData
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string LogTime { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常
        /// </summary>
        public string ExceptionString { get; set; }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class LastLogResult : OperResult<IEnumerable<LogData>>
    {
        /// <inheritdoc/>
        public LastLogResult() : base()
        {
        }

        /// <inheritdoc/>
        public LastLogResult(Exception ex) : base(ex)
        {
        }

        /// <summary>
        /// 流位置
        /// </summary>
        public long Position { set; get; }
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class LogInfoResult : OperResult
    {
        /// <summary>
        /// 流位置
        /// </summary>
        public long Length { set; get; }

        /// <summary>
        /// 全名称
        /// </summary>
        public string FullName { set; get; }
    }

    /// <summary>日志文本文件倒序读取</summary>
    public class TextFileReader
    {
        /// <summary>
        /// 获取目录下所有文件信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<LogInfoResult> GetFile(string path)
        {
            List<LogInfoResult> list = new();
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                if (files == null) return list;
                var data = files.Select(a => new FileInfo(a)).OrderByDescending(x => x.LastWriteTime)
                     .Select(x => new LogInfoResult() { FullName = x.FullName, Length = x.Length }).ToList();
                return data;
            }
            return null;
        }

        /// <summary>
        /// 倒序读取文本文件
        /// </summary>
        /// <param name="file">文件路径</param>
        /// <param name="position">读取流的起始位置</param>
        /// <param name="lineCount">读取行数</param>
        /// <returns></returns>
        public static LastLogResult LastLog(string file, long position, int lineCount = 200)
        {
            LastLogResult result = new();
            try
            {
                if (!File.Exists(file)) { result.OperCode = 999; result.ErrorMessage = "文件路径无效！"; return result; }
                List<string> txt = new();
                long ps = position;
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (ps <= 0)
                        ps = fs.Length;
                    //读取200行
                    for (int i = 0; i < lineCount; i++)
                    {
                        ps = InverseReadRow(fs, ps, txt);
                        if (ps <= 0)
                            break;
                    }
                }
                result.Content = txt.Select(a =>
                {
                    var data = ParseCSV(a);
                    if (data.Count > 2)
                    {
                        var log = new LogData();
                        log.LogTime = data[0].Trim();
                        log.LogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), data[1].Trim());
                        log.Message = data[2].Trim();
                        log.ExceptionString = data.Count > 3 ? data[3].Trim() : null;
                        return log;
                    }
                    else
                    {
                        return null;
                    }
                }).Where(a => a != null).ToList();
                result.Position = ps;
                result.OperCode = 0;
                return result;
            }
            catch (Exception ex)
            {
                result = new LastLogResult(ex);
                return result;
            }
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

        /// <summary>
        /// 从后向前按行读取文本文件
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="position"></param>
        /// <param name="strs"></param>
        /// <param name="maxRead">默认每次最多读取1kb数据</param>
        /// <returns>返回读取位置</returns>
        private static long InverseReadRow(FileStream fs, long position, List<string> strs, int maxRead = 1024)
        {
            byte n = 0xD;//换行符
            byte a = 0xA;//回车符
            byte s = 0x22;//双引号
            if (fs.Length == 0) return 0;
            var newPos = position - 1;
            try
            {

                int curVal = 0;
                var readLength = 0;
                List<byte> arr = new List<byte>();
                while (true)//读取一行后跳出
                {
                    readLength++;
                    if (newPos <= 0)
                        newPos = 0;

                    fs.Position = newPos;
                    curVal = fs.ReadByte();
                    if (curVal == -1) break;

                    arr.Insert(0, (byte)curVal);

                    if (newPos <= 0)
                        break;
                    if (readLength == maxRead)
                    {
                        arr.RemoveRange(arr.Count - maxRead / 2, maxRead / 2);
                        arr.Add((byte)s);
                        readLength = arr.Count;
                    }

                    if (curVal == n || curVal == s || curVal == a)
                    {
                        if (arr.Last() == s || arr.Last() == n || arr.Last() == a)
                        {
#if !NETFRAMEWORK
                            //双引号判定
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                //时间筛选
                                if (arr.Count > 20 && arr[2] >= 0x30)
                                {
                                    string content = Encoding.UTF8.GetString(arr.GetRange(2, 4).ToArray(), 0, 4);
                                    if (int.TryParse(content, out var year))
                                        if (year >= 1900 && year <= DateTime.Now.Year + 1)
                                            break;
                                    newPos--;
                                }
                                else
                                    newPos--;
                            }
                            else
                            {
                                if (arr.Count > 20 && arr[1] >= 0x30)
                                {
                                    string content = Encoding.UTF8.GetString(arr.GetRange(1, 4).ToArray(), 0, 4);
                                    if (int.TryParse(content, out var year))
                                        if (year >= 1900 && year <= DateTime.Now.Year + 1)
                                            break;

                                    newPos--;
                                }
                                else
                                    newPos--;
                            }

#else
                            //时间筛选
                            if (arr.Count > 20 && arr[2] >= 0x30)
                            {
                                string content = Encoding.UTF8.GetString(arr.GetRange(2, 4).ToArray(), 0, 4);
                                if (int.TryParse(content, out var year))
                                    if (year >= 1900 && year <= DateTime.Now.Year + 1)
                                        break;
                                newPos--;
                            }
                            else
                                newPos--;
#endif
                        }
                        else
                            newPos--;
                    }
                    else
                    {
                        newPos--;
                    }
                }
                if (arr.Count >= 5)
                {
                    //如果行数据小于5，不需要添加
                    if (arr[0] == 0x0D || arr[0] == 0x0A)
                        arr.RemoveAt(0);
                    if (arr[0] == 0x0D || arr[0] == 0x0A)
                        arr.RemoveAt(0);
                    var str = Encoding.UTF8.GetString(arr.ToArray());
                    strs.Add(str);
                }
                else
                {
                }

                return newPos;
            }
            finally
            {

            }
        }
    }
}