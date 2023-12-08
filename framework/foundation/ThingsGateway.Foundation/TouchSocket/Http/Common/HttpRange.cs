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

namespace ThingsGateway.Foundation.Http
{
    /// <summary>
    /// Range: bytes=0-499 表示第 0-499 字节范围的内容
    /// Range: bytes=500-999 表示第 500-999 字节范围的内容
    /// Range: bytes=-500 表示最后 500 字节的内容
    /// Range: bytes=500- 表示从第 500 字节开始到文件结束部分的内容
    /// Range: bytes=0-0,-1 表示第一个和最后一个字节
    /// Range: bytes=500-600,601-999 同时指定几个范围
    /// </summary>
    public class HttpRange
    {
        /// <summary>
        /// 转换获取的集合
        /// </summary>
        /// <param name="rangeStr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static HttpRange[] GetRanges(string rangeStr, long size)
        {
            var ranges = rangeStr.Split('=');
            if (ranges.Length != 2)
            {
                return new HttpRange[0];
            }
            rangeStr = ranges[1];
            if (string.IsNullOrEmpty(rangeStr))
            {
                return new HttpRange[0];
            }
            ranges = rangeStr.Split(',');
            var httpRanges = new List<HttpRange>();
            foreach (var range in ranges)
            {
                var httpRange = new HttpRange();
                ranges = range.Split('-');
                if (ranges.Length == 2)
                {
                    if (range.StartsWith("-"))
                    {
                        httpRange.Length = Convert.ToInt64(ranges[1]);
                        httpRange.Start = size - httpRange.Length;
                    }
                    else if (range.EndsWith("-"))
                    {
                        httpRange.Start = Convert.ToInt64(ranges[0]);
                        httpRange.Length = size - httpRange.Start;
                    }
                    else
                    {
                        httpRange.Start = Convert.ToInt64(ranges[0]);
                        httpRange.Length = Convert.ToInt64(ranges[1]) - httpRange.Start + 1;
                    }
                }
                else
                {
                    continue;
                }
                httpRanges.Add(httpRange);
            }
            return httpRanges.ToArray();
        }

        /// <summary>
        /// 转换获取的集合
        /// </summary>
        /// <param name="rangeStr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static HttpRange GetRange(string rangeStr, long size)
        {
            var ranges = rangeStr.Split('=');
            if (ranges.Length != 2)
            {
                return null;
            }
            rangeStr = ranges[1];
            if (string.IsNullOrEmpty(rangeStr))
            {
                return null;
            }
            ranges = rangeStr.Split(',');
            foreach (var range in ranges)
            {
                var httpRange = new HttpRange();
                ranges = range.Split('-');
                if (ranges.Length == 2)
                {
                    if (range.StartsWith("-"))
                    {
                        httpRange.Length = Convert.ToInt64(ranges[1]);
                        httpRange.Start = size - httpRange.Length;
                    }
                    else if (range.EndsWith("-"))
                    {
                        httpRange.Start = Convert.ToInt64(ranges[0]);
                        httpRange.Length = size - httpRange.Start;
                    }
                    else
                    {
                        httpRange.Start = Convert.ToInt64(ranges[0]);
                        httpRange.Length = Convert.ToInt64(ranges[1]) - httpRange.Start + 1;
                    }
                }
                else
                {
                    continue;
                }
                return httpRange;
            }
            return null;
        }

        /// <summary>
        /// 起始位置
        /// </summary>
        public long Start { get; set; } = -1;

        /// <summary>
        /// 长度
        /// </summary>
        public long Length { get; set; } = -1;
    }
}