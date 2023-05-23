#region copyright
//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway/
//  QQ群：605534569
//------------------------------------------------------------------------------
#endregion

using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ThingsGateway.Core
{
    public static partial class StringExtensions
    {
        /// <summary>
        /// 转成非null
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string AsNotNull(this string s)
        {
            return string.IsNullOrEmpty(s) ? "" : s;
        }

        /// <summary>
        /// 转成非null
        /// </summary>
        /// <param name="s"></param>
        /// <param name="value">为空时的替换值</param>
        /// <returns></returns>
        public static string IfNullOrEmpty(this string s, string value)
        {
            return string.IsNullOrEmpty(s) ? value : s;
        }

        /// <summary>
        /// 转成非null
        /// </summary>
        /// <param name="s"></param>
        /// <param name="valueFactory">为空时的替换值函数</param>
        /// <returns></returns>
        public static string IfNullOrEmpty(this string s, Func<string> valueFactory)
        {
            return string.IsNullOrEmpty(s) ? valueFactory() : s;
        }

        public static string Join(this IEnumerable<string> strs, string separate = ", ", bool removeEmptyEntry = false) => string.Join(separate, removeEmptyEntry ? strs.Where(s => !string.IsNullOrEmpty(s)) : strs);

        public static string Join<T>(this IEnumerable<T> strs, string separate = ", ", bool removeEmptyEntry = false) => string.Join(separate, removeEmptyEntry ? strs.Where(t => t != null) : strs);

        /// <summary>
        /// 字符串掩码
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="mask">掩码符</param>
        /// <returns></returns>
        public static string Mask(this string s, char mask = '*')
        {
            if (string.IsNullOrWhiteSpace(s?.Trim()))
            {
                return s;
            }
            s = s.Trim();
            string masks = mask.ToString().PadLeft(4, mask);
            return s.Length switch
            {
                >= 11 => Regex.Replace(s, "(.{3}).*(.{4})", $"$1{masks}$2"),
                10 => Regex.Replace(s, "(.{3}).*(.{3})", $"$1{masks}$2"),
                9 => Regex.Replace(s, "(.{2}).*(.{3})", $"$1{masks}$2"),
                8 => Regex.Replace(s, "(.{2}).*(.{2})", $"$1{masks}$2"),
                7 => Regex.Replace(s, "(.{1}).*(.{2})", $"$1{masks}$2"),
                6 => Regex.Replace(s, "(.{1}).*(.{1})", $"$1{masks}$2"),
                _ => Regex.Replace(s, "(.{1}).*", $"$1{masks}")
            };
        }

        /// <summary>
        /// 根据正则替换
        /// </summary>
        /// <param name="input"></param>
        /// <param name="regex">正则表达式</param>
        /// <param name="replacement">新内容</param>
        /// <returns></returns>
        public static string Replace(this string input, Regex regex, string replacement)
        {
            return regex.Replace(input, replacement);
        }

        /// <summary>
        /// 取字符串前{length}个字
        /// </summary>
        /// <param name="s"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Take(this string s, int length)
        {
            return s.Length > length ? s.Substring(0, length) : s;
        }

        /// <summary>
        /// 转换成字节数组
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string @this)
        {
            return Encoding.UTF8.GetBytes(@this);
        }

        /// <summary>
        /// 字符串转时间
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string value)
        {
            DateTime.TryParse(value, out var result);
            return result;
        }

        /// <summary>
        /// 字符串转Guid
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Guid ToGuid(this string s)
        {
            return Guid.Parse(s);
        }

        #region 检测字符串中是否包含列表中的关键词

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词(快速匹配)
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="keys">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool Contains(this string s, IEnumerable<string> keys, bool ignoreCase = true)
        {
            if (keys is not ICollection<string> array)
            {
                array = keys.ToArray();
            }

            if (array.Count == 0 || string.IsNullOrEmpty(s))
            {
                return false;
            }

            return ignoreCase ? array.Any(item => s.IndexOf(item, StringComparison.InvariantCultureIgnoreCase) >= 0) : array.Any(s.Contains);
        }

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词(安全匹配)
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="keys">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool ContainsSafety(this string s, IEnumerable<string> keys, bool ignoreCase = true)
        {
            if (keys is not ICollection<string> array)
            {
                array = keys.ToArray();
            }

            if (array.Count == 0 || string.IsNullOrEmpty(s))
            {
                return false;
            }

            bool flag = false;
            if (ignoreCase)
            {
                foreach (var item in array)
                {
                    if (s.Contains(item))
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                foreach (var item in array)
                {
                    if (s.IndexOf(item, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        flag = true;
                    }
                }
            }

            return flag;
        }

        /// <summary>
        /// 检测字符串中是否以列表中的关键词结尾
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="keys">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool EndsWith(this string s, IEnumerable<string> keys, bool ignoreCase = true)
        {
            if (keys is not ICollection<string> array)
            {
                array = keys.ToArray();
            }

            if (array.Count == 0 || string.IsNullOrEmpty(s))
            {
                return false;
            }

            var pattern = $"({array.Select(Regex.Escape).Join("|")})$";
            return ignoreCase ? Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase) : Regex.IsMatch(s, pattern);
        }

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="regex">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool RegexMatch(this string s, string regex, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(regex) || string.IsNullOrEmpty(s))
            {
                return false;
            }

            if (ignoreCase)
            {
                return Regex.IsMatch(s, regex, RegexOptions.IgnoreCase);
            }

            return Regex.IsMatch(s, regex);
        }

        /// <summary>
        /// 检测字符串中是否包含列表中的关键词
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="regex">关键词列表</param>
        /// <returns></returns>
        public static bool RegexMatch(this string s, Regex regex) => !string.IsNullOrEmpty(s) && regex.IsMatch(s);

        /// <summary>
        /// 检测字符串中是否以列表中的关键词开始
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="keys">关键词列表</param>
        /// <param name="ignoreCase">忽略大小写</param>
        /// <returns></returns>
        public static bool StartsWith(this string s, IEnumerable<string> keys, bool ignoreCase = true)
        {
            if (keys is not ICollection<string> array)
            {
                array = keys.ToArray();
            }

            if (array.Count == 0 || string.IsNullOrEmpty(s))
            {
                return false;
            }

            var pattern = $"^({array.Select(Regex.Escape).Join("|")})";
            return ignoreCase ? Regex.IsMatch(s, pattern, RegexOptions.IgnoreCase) : Regex.IsMatch(s, pattern);
        }

        #endregion 检测字符串中是否包含列表中的关键词

        #region Email

        /// <summary>
        /// 邮箱掩码
        /// </summary>
        /// <param name="s">邮箱</param>
        /// <param name="mask">掩码</param>
        /// <returns></returns>
        public static string MaskEmail(this string s, char mask = '*')
        {
            var index = s.LastIndexOf("@");
            var oldValue = s.Substring(0, index);
            return !MatchEmail(s).isMatch ? s : s.Replace(oldValue, Mask(oldValue, mask));
        }

        /// <summary>
        /// 匹配Email
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="valid">是否验证有效性</param>
        /// <returns>匹配对象；是否匹配成功，若返回true，则会得到一个Match对象，否则为null</returns>
        public static (bool isMatch, Match match) MatchEmail(this string s, bool valid = false)
        {
            if (string.IsNullOrEmpty(s) || s.Length < 7)
            {
                return (false, null);
            }

            var match = Regex.Match(s, @"[^@ \t\r\n]+@[^@ \t\r\n]+\.[^@ \t\r\n]+");
            var isMatch = match.Success;

            return (isMatch, match);
        }

        #endregion Email

        #region 匹配完整的URL

        /// <summary>
        /// 匹配完整格式的URL
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static Uri MatchUrl(this string s, out bool isMatch)
        {
            try
            {
                var uri = new Uri(s);
                isMatch = Dns.GetHostAddresses(uri.Host).Any(ip => !ip.IsPrivateIP());
                return uri;
            }
            catch
            {
                isMatch = false;
                return null;
            }
        }

        /// <summary>
        /// 匹配完整格式的URL
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchUrl(this string s)
        {
            MatchUrl(s, out var isMatch);
            return isMatch;
        }

        #endregion 匹配完整的URL

        #region 权威校验身份证号码

        private static readonly string[] ChinaIDProvinceCodes = {
             "11", "12", "13", "14", "15",
             "21", "22", "23",
             "31", "32", "33", "34", "35", "36", "37",
             "41", "42", "43", "44", "45", "46",
             "50", "51", "52", "53", "54",
             "61", "62", "63", "64", "65",
             "71",
             "81", "82",
             "91"
        };

        /// <summary>
        /// 根据GB11643-1999标准权威校验中国身份证号码的合法性
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchIdentifyCard(this string s)
        {
            return s.Length switch
            {
                18 => CheckChinaID18(s),
                15 => CheckChinaID15(s),
                _ => false
            };
        }

        private static bool CheckChinaID15(string ID)
        {
            Match m = Regex.Match(ID, @"\d{15}", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return false;
            }
            if (!ChinaIDProvinceCodes.Contains(ID.Substring(0, 2)))
            {
                return false;
            }
            CultureInfo zhCN = new CultureInfo("zh-CN", true);
            if (!DateTime.TryParseExact("19" + ID.Substring(6, 6), "yyyyMMdd", zhCN, DateTimeStyles.None, out DateTime birthday))
            {
                return false;
            }
            return birthday.In(new DateTime(1800, 1, 1), new DateTime(2000, 1, 1));
        }

        private static bool CheckChinaID18(string ID)
        {
            ID = ID.ToUpper();
            Match m = Regex.Match(ID, @"\d{17}[\dX]", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return false;
            }
            if (!ChinaIDProvinceCodes.Contains(ID.Substring(0, 2)))
            {
                return false;
            }
            CultureInfo zhCN = new CultureInfo("zh-CN", true);
            if (!DateTime.TryParseExact(ID.Substring(6, 8), "yyyyMMdd", zhCN, DateTimeStyles.None, out DateTime birthday))
            {
                return false;
            }
            if (!birthday.In(new DateTime(1800, 1, 1), DateTime.Today))
            {
                return false;
            }
            int[] factors = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += (ID[i] - '0') * factors[i];
            }
            int n = (12 - (sum % 11)) % 11;
            return n < 10 ? ID[17] - '0' == n : ID[17].Equals('X');
        }

        #endregion 权威校验身份证号码

        #region IP地址

        /// <summary>
        /// 判断IP地址在不在某个IP地址段
        /// </summary>
        /// <param name="input">需要判断的IP地址</param>
        /// <param name="begin">起始地址</param>
        /// <param name="ends">结束地址</param>
        /// <returns></returns>
        public static bool IpAddressInRange(this string input, string begin, string ends)
        {
            uint current = input.IPToID();
            return current >= begin.IPToID() && current <= ends.IPToID();
        }

        /// <summary>
        /// 判断IP地址在不在某个IP地址段
        /// </summary>
        /// <param name="input">需要判断的IP地址</param>
        /// <param name="begin">起始地址</param>
        /// <param name="ends">结束地址</param>
        /// <returns></returns>
        public static bool IpAddressInRange(this IPAddress input, IPAddress begin, IPAddress ends)
        {
            uint current = input.ToUInt32();
            return current >= begin.ToUInt32() && current <= ends.ToUInt32();
        }

        /// <summary>
        /// IP地址转换成数字
        /// </summary>
        /// <param name="addr">IP地址</param>
        /// <returns>数字,输入无效IP地址返回0</returns>
        public static uint IPToID(this string addr)
        {
            if (!IPAddress.TryParse(addr, out var ip))
            {
                return 0;
            }

            byte[] bInt = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bInt);
            }

            return BitConverter.ToUInt32(bInt, 0);
        }

        /// <summary>
        /// 判断IP是否是私有地址
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsPrivateIP(this string ip)
        {
            var address = MatchInetAddress(ip, out var b);
            return b && address.IsPrivateIP();
        }

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static IPAddress MatchInetAddress(this string s, out bool isMatch)
        {
            isMatch = IPAddress.TryParse(s, out var ip);
            return ip;
        }

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchInetAddress(this string s)
        {
            MatchInetAddress(s, out var success);
            return success;
        }

        /// <summary>
        /// IP地址转换成数字
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <returns>数字,输入无效IP地址返回0</returns>
        public static uint ToUInt32(this IPAddress ip)
        {
            byte[] bInt = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bInt);
            }

            return BitConverter.ToUInt32(bInt, 0);
        }

        #endregion IP地址

        #region 校验手机号码的正确性

        /// <summary>
        /// 匹配手机号码
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchPhoneNumber(this string s)
        {
            return !string.IsNullOrEmpty(s) && s[0] == '1' && (s[1] > '2' || s[1] <= '9');
        }

        #endregion 校验手机号码的正确性

        #region Url

        /// <summary>
        /// 判断url是否是外部地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsExternalAddress(this string url)
        {
            var uri = new Uri(url);
            switch (uri.HostNameType)
            {
                case UriHostNameType.Dns:
                    var ipHostEntry = Dns.GetHostEntry(uri.DnsSafeHost);
                    if (ipHostEntry.AddressList.Where(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork).Any(ipAddress => !ipAddress.IsPrivateIP()))
                    {
                        return true;
                    }
                    break;

                case UriHostNameType.IPv4:
                    return !IPAddress.Parse(uri.DnsSafeHost).IsPrivateIP();
            }
            return false;
        }

        #endregion Url

        #region 权威校验中国专利申请号/专利号

        /// <summary>
        /// 中国专利申请号（授权以后就是专利号）由两种组成
        /// 2003年9月30号以前的9位（不带校验位是8号），校验位之前可能还会有一个点，例如：00262311, 002623110 或 00262311.0
        /// 2003年10月1号以后的13位（不带校验位是12号），校验位之前可能还会有一个点，例如：200410018477, 2004100184779 或200410018477.9
        /// http://www.sipo.gov.cn/docs/pub/old/wxfw/zlwxxxggfw/hlwzljsxt/hlwzljsxtsyzn/201507/P020150713610193194682.pdf
        /// 上面的文档中均不包括校验算法，但是下面的校验算法没有问题
        /// </summary>
        /// <param name="patnum">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchCNPatentNumber(this string patnum)
        {
            Regex patnumWithCheckbitPattern = new Regex(@"^
(?<!\d)
(?<patentnum>
    (?<basenum>
        (?<year>(?<old>8[5-9]|9[0-9]|0[0-3])|(?<new>[2-9]\d{3}))
        (?<sn>
            (?<patenttype>[12389])
            (?(old)\d{5}|(?(new)\d{7}))
        )
    )
    (?:
    \.?
    (?<checkbit>[0-9X])
    )?
)
(?!\d)
$", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            Match m = patnumWithCheckbitPattern.Match(patnum);
            if (!m.Success)
            {
                return false;
            }
            bool isPatnumTrue = true;
            patnum = patnum.ToUpper().Replace(".", "");
            if (patnum.Length == 9 || patnum.Length == 8)
            {
                byte[] factors8 = new byte[] { 2, 3, 4, 5, 6, 7, 8, 9 };
                int year = Convert.ToUInt16(patnum.Substring(0, 2));
                year += (year >= 85) ? (ushort)1900u : (ushort)2000u;
                if (year >= 1985 || year <= 2003)
                {
                    int sum = 0;
                    for (byte i = 0; i < 8; i++)
                    {
                        sum += factors8[i] * (patnum[i] - '0');
                    }
                    char checkbit = "0123456789X"[sum % 11];
                    if (patnum.Length == 9)
                    {
                        if (checkbit != patnum[8])
                        {
                            isPatnumTrue = false;
                        }
                    }
                    else
                    {
                        patnum += checkbit;
                    }
                }
                else
                {
                    isPatnumTrue = false;
                }
            }
            else if (patnum.Length == 13 || patnum.Length == 12)
            {
                byte[] factors12 = new byte[12] { 2, 3, 4, 5, 6, 7, 8, 9, 2, 3, 4, 5 };
                int year = Convert.ToUInt16(patnum.Substring(0, 4));
                if (year >= 2003 && year <= DateTime.UtcNow.Year)
                {
                    int sum = 0;
                    for (byte i = 0; i < 12; i++)
                    {
                        sum += factors12[i] * (patnum[i] - '0');
                    }
                    char checkbit = "0123456789X"[sum % 11];
                    if (patnum.Length == 13)
                    {
                        if (checkbit != patnum[12])
                        {
                            isPatnumTrue = false;
                        }
                    }
                    else
                    {
                        patnum += checkbit;
                    }
                }
                else
                {
                    isPatnumTrue = false;
                }
            }
            else
            {
                isPatnumTrue = false;
            }
            return isPatnumTrue;
        }

        #endregion 权威校验中国专利申请号/专利号
    }
}