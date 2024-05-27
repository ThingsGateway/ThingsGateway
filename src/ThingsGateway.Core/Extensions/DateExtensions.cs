//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://kimdiego2098.github.io/
//  QQ群：605534569
//------------------------------------------------------------------------------

namespace ThingsGateway.Core.Extension;

public static class DateExtensions
{
    /// <summary>
    /// 返回yyyy-MM-dd HH:mm:ss:fff zz时间格式字符串
    /// </summary>
    public static string ToDefaultDateTimeFormat(this in DateTime dt, TimeSpan offset)
    {
        if (dt.Kind == DateTimeKind.Utc)
            return new DateTimeOffset(dt.ToLocalTime(), offset).ToString("yyyy-MM-dd HH:mm:ss:fff zz");
        else if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
            return dt.ToString("yyyy-MM-dd HH:mm:ss:fff zz");
        else
        {
            if (offset == TimeSpan.Zero)
            {
                return dt.ToString("yyyy-MM-dd HH:mm:ss:fff zz");
            }
            else if (dt.Kind != DateTimeKind.Local)
                return new DateTimeOffset(dt, offset).ToString("yyyy-MM-dd HH:mm:ss:fff zz");
        }
        return dt.ToString("yyyy-MM-dd HH:mm:ss:fff zz");
    }

    /// <summary>
    /// 返回yyyy-MM-dd HH:mm:ss:fff zz时间格式字符串
    /// </summary>
    public static string ToDefaultDateTimeFormat(this in DateTime dt)
    {
        return dt.ToString("yyyy-MM-dd HH:mm:ss:fff zz");
    }

    /// <summary>
    /// 返回yyyy-MM-dd HH-mm-ss-fff zz时间格式字符串
    /// </summary>
    public static string ToFileDateTimeFormat(this in DateTime dt)
    {
        return ToDefaultDateTimeFormat(dt).Replace(":", "-");
    }

    /// <summary>
    /// 返回yyyy-MM-dd HH-mm-ss-fff zz时间格式字符串
    /// </summary>
    public static string ToFileDateTimeFormat(this in DateTime dt, TimeSpan offset)
    {
        return ToDefaultDateTimeFormat(dt, offset).Replace(":", "-");
    }

    /// <summary>
    /// 将 DateTimeOffset 转换成本地 DateTime
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime ConvertToDateTime(this DateTimeOffset dateTime)
    {
        if (dateTime.Offset.Equals(TimeSpan.Zero))
            return dateTime.UtcDateTime;
        if (dateTime.Offset.Equals(TimeZoneInfo.Local.GetUtcOffset(dateTime.DateTime)))
            return dateTime.ToLocalTime().DateTime;
        else
            return dateTime.DateTime;
    }

    /// <summary>
    /// 将 DateTimeOffset? 转换成本地 DateTime?
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTime? ConvertToDateTime(this DateTimeOffset? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ConvertToDateTime() : null;
    }

    /// <summary>
    /// 将 DateTime 转换成 DateTimeOffset
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset ConvertToDateTimeOffset(this DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
    }

    /// <summary>
    /// 将 DateTime? 转换成 DateTimeOffset?
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static DateTimeOffset? ConvertToDateTimeOffset(this DateTime? dateTime)
    {
        return dateTime.HasValue ? dateTime.Value.ConvertToDateTimeOffset() : null;
    }

    /// <summary>
    /// 将时间戳转换为 DateTime
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    internal static DateTime ConvertToDateTime(this long timestamp)
    {
        var timeStampDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var digitCount = (int)Math.Floor(Math.Log10(timestamp) + 1);

        if (digitCount != 13 && digitCount != 10)
        {
            throw new ArgumentException("Data is not a valid timestamp format.");
        }

        return (digitCount == 13
            ? timeStampDateTime.AddMilliseconds(timestamp)  // 13 位时间戳
            : timeStampDateTime.AddSeconds(timestamp)).ToLocalTime();   // 10 位时间戳
    }

    /// <summary>
    /// 计算2个时间差，返回文字描述
    /// </summary>
    /// <param name="beginTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>时间差</returns>
    public static string GetDiffTime(this in DateTime beginTime, in DateTime endTime)
    {
        TimeSpan timeDifference = endTime - beginTime;
        if (timeDifference.TotalDays >= 1)
        {
            return $"{(int)timeDifference.TotalDays} d {(int)timeDifference.Hours} H";
        }
        else if (timeDifference.TotalHours >= 1)
        {
            return $"{(int)timeDifference.TotalHours} H {(int)timeDifference.Minutes} m";
        }
        else
        {
            return $"{(int)timeDifference.TotalMinutes} m";
        }
    }

    /// <summary>
    /// 计算2个时间差，返回文字描述
    /// </summary>
    /// <param name="beginTime">开始时间</param>
    /// <param name="endTime">结束时间</param>
    /// <returns>时间差</returns>
    public static string GetDiffTime(this in DateTimeOffset beginTime, in DateTimeOffset endTime)
    {
        TimeSpan timeDifference = endTime - beginTime;
        if (timeDifference.TotalDays >= 1)
        {
            return $"{(int)timeDifference.TotalDays} d {(int)timeDifference.Hours} H";
        }
        else if (timeDifference.TotalHours >= 1)
        {
            return $"{(int)timeDifference.TotalHours} H {(int)timeDifference.Minutes} m";
        }
        else
        {
            return $"{(int)timeDifference.TotalMinutes} m";
        }
    }
}
