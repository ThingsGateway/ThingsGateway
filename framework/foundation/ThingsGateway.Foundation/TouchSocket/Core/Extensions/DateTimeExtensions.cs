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
using System.Runtime.CompilerServices;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// DateExtensions
    /// </summary>
    public static class DateTimeExtensions
    {
        private static readonly DateTime m_utc_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly DateTimeOffset m_utc1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private static readonly DateTime _dt1970 = new(1970, 1, 1);

        /// <summary>
        /// 系统默认使用的当前时间
        /// </summary>
        public static DateTime CurrentDateTime => DateTime.Now;

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
            //return new DateTimeOffset(dt, offset).ToString("yyyy-MM-dd HH:mm:ss:fff zz");
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
        /// 返回yyyy-MM-dd HH:mm:ss:fff zz时间格式字符串
        /// </summary>
        public static string ToFileDateTimeFormat(this in DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH-mm-ss-fff zz");
        }

        /// <summary>
        /// 计算2个时间差，返回文字描述
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>时间差</returns>
        public static string GetDiffTime(this in DateTime beginTime, in DateTime endTime)
        {
            string strResout = string.Empty;

            //获得2时间的时间间隔秒计算
            TimeSpan span = endTime.Subtract(beginTime);
            int sec = Convert.ToInt32(span.TotalSeconds);
            int minutes = 1 * 60;
            int hours = minutes * 60;
            int day = hours * 24;
            int month = day * 30;
            int year = month * 12;

            //提醒时间,到了返回1,否则返回0
            if (sec > year)
            {
                strResout += (sec / year) + "年";
                sec %= year; //剩余
            }

            if (sec > month)
            {
                strResout += (sec / month) + "月";
                sec %= month;
            }

            if (sec > day)
            {
                strResout += (sec / day) + "天";
                sec %= day;
            }

            if (sec > hours)
            {
                strResout += (sec / hours) + "小时";
                sec %= hours;
            }

            if (sec > minutes)
            {
                strResout += (sec / minutes) + "分";
                sec %= minutes;
            }

            strResout += sec + "秒";
            return strResout;
        }

        /// <summary>
        /// 计算2个时间差，返回文字描述
        /// </summary>
        /// <param name="beginTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns>时间差</returns>
        public static string GetDiffTime(this in DateTimeOffset beginTime, in DateTimeOffset endTime)
        {
            string strResout = string.Empty;

            //获得2时间的时间间隔秒计算
            TimeSpan span = endTime.Subtract(beginTime);
            int sec = Convert.ToInt32(span.TotalSeconds);
            int minutes = 1 * 60;
            int hours = minutes * 60;
            int day = hours * 24;
            int month = day * 30;
            int year = month * 12;

            //提醒时间,到了返回1,否则返回0
            if (sec > year)
            {
                strResout += (sec / year) + "年";
                sec %= year; //剩余
            }

            if (sec > month)
            {
                strResout += (sec / month) + "月";
                sec %= month;
            }

            if (sec > day)
            {
                strResout += (sec / day) + "天";
                sec %= day;
            }

            if (sec > hours)
            {
                strResout += (sec / hours) + "小时";
                sec %= hours;
            }

            if (sec > minutes)
            {
                strResout += (sec / minutes) + "分";
                sec %= minutes;
            }

            strResout += sec + "秒";
            return strResout;
        }

        /// <summary>
        /// ToLong
        /// </summary>
        /// <returns></returns>
        public static long ToLong(this DateTime value, long defaultValue = 0)
        {
            // 特殊处理时间，转Unix毫秒
            if (value == DateTime.MinValue) return 0;

            //// 先转UTC时间再相减，以得到绝对时间差
            //return (Int32)(dt.ToUniversalTime() - _dt1970).TotalSeconds;
            return (Int64)(value - _dt1970).TotalMilliseconds;
        }

        /// <summary>
        /// 将时间转为毫秒级别的短整形
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertTime(this in DateTime time)
        {
            return (uint)(Convert.ToInt64(time.Subtract(m_utc_time).TotalMilliseconds) & 0xffffffff);
        }

        /// <summary>
        /// 将时间转为毫秒级别的短整形
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertTime(this in DateTimeOffset time)
        {
            return (uint)(Convert.ToInt64(time.Subtract(m_utc1970).TotalMilliseconds) & 0xffffffff);
        }
    }
}