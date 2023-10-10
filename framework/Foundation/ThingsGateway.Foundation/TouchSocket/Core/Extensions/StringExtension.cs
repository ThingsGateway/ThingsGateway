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
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// StringExtension
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        /// IsNullOrEmpty
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// IsNullOrWhiteSpace
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// 当不为null，且不为空。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool HasValue(this string str)
        {
            return !string.IsNullOrWhiteSpace(str);
        }

        ///<summary>
        ///  将字符串格式化成指定的数据类型
        ///</summary>
        ///<param name="str"></param>
        ///<param name="type"></param>
        /// <param name="value"></param>
        /// <param name="splits"></param>
        ///   <returns></returns>
        public static bool TryParseToType(string str, Type type, out object value, char[] splits = default)
        {
            if (string.IsNullOrEmpty(str))
            {
                value = default;
                return true;
            }

            if (type == null)
            {
                value = str;
                return true;
            }
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var strs = splits == null ? str.Split(new char[] { ' ', ';', '-', '/' }) : str.Split(splits);
                var array = Array.CreateInstance(elementType, strs.Length);
                for (int i = 0, c = strs.Length; i < c; ++i)
                {
                    if (ConvertSimpleType(strs[i], elementType, out var o))
                    {
                        array.SetValue(o, i);
                    }
                    else
                    {
                        value = default;
                        return false;
                    }
                }
                value = array;
                return true;
            }
            return ConvertSimpleType(str, type, out value);
        }

        private static bool ConvertSimpleType(string value, Type destinationType, out object returnValue)
        {
            if ((value == null) || destinationType.IsInstanceOfType(value))
            {
                returnValue = value;
                return true;
            }

            if (string.IsNullOrEmpty(value))
            {
                returnValue = default;
                return true;
            }
            var converter = TypeDescriptor.GetConverter(destinationType);
            var flag = converter.CanConvertFrom(value.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!flag && !converter.CanConvertTo(destinationType))
            {
                returnValue = default;
                return false;
            }
            try
            {
                returnValue = flag ? converter.ConvertFrom(null, null, value) : converter.ConvertTo(null, null, value, destinationType);
            }
            catch
            {
                returnValue = default;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断字符串compare 在 input字符串中出现的次数
        /// </summary>
        /// <param name="input">源字符串</param>
        /// <param name="compare">用于比较的字符串</param>
        /// <returns>字符串compare 在 input字符串中出现的次数</returns>
        public static int HitStringCount(this string input, string compare)
        {
            var index = input.IndexOf(compare);
            return index != -1 ? 1 + HitStringCount(input.Substring(index + compare.Length), compare) : 0;
        }

        /// <summary>
        /// 将字符转换为对应的基础类型类型。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType">目标类型必须为基础类型</param>
        /// <returns></returns>
        public static object ParseToType(this string value, Type destinationType)
        {
            object returnValue;
            if ((value == null) || destinationType.IsInstanceOfType(value))
            {
                return value;
            }
            var str = value;
            if ((str != null) && (str.Length == 0))
            {
                return null;
            }
            var converter = TypeDescriptor.GetConverter(destinationType);
            var flag = converter.CanConvertFrom(value.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }
            if (!flag && !converter.CanConvertTo(destinationType))
            {
                throw new InvalidOperationException("无法转换成类型：" + value.ToString() + "==>" + destinationType);
            }
            try
            {
                returnValue = flag ? converter.ConvertFrom(null, null, value) : converter.ConvertTo(null, null, value, destinationType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(" 类型转换出错：" + value.ToString() + "==>" + destinationType, e);
            }
            return returnValue;
        }

        /// <summary>
        /// 只按第一个匹配项分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string[] SplitFirst(this string str, char split)
        {
            var s = new List<string>();
            var index = str.IndexOf(split);
            if (index > 0)
            {
                s.Add(str.Substring(0, index).Trim());
                s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
            }

            return s.ToArray();
        }

        /// <summary>
        /// 按字符串分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string[] Split(this string str, string pattern)
        {
            return Regex.Split(str, pattern);
        }

        /// <summary>
        /// 只按最后一个匹配项分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string[] SplitLast(this string str, char split)
        {
            var s = new List<string>();
            var index = str.LastIndexOf(split);
            if (index > 0)
            {
                s.Add(str.Substring(0, index).Trim());
                s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
            }

            return s.ToArray();
        }

        /// <summary>
        /// 按格式填充
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static string Format(this string str, params object[] ps)
        {
            if (ps == null || ps.Length == 0)
            {
                return str;
            }
            try
            {
                return string.Format(str, ps);
            }
            catch
            {
                return str;
            }
        }

        /// <summary>
        /// 转换为SHA1。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] ToSha1(this string value, Encoding encoding)
        {
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(encoding.GetBytes(value));
            }
        }

        /// <summary>
        /// 转换为UTF-8数据，效果等于<see cref="Encoding.UTF8"/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ToUTF8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// 将16进制的字符转换为数组。
        /// </summary>
        /// <param name="hexString"></param>
        /// <param name="splite"></param>
        /// <returns></returns>
        public static byte[] ByHexStringToBytes(this string hexString, string splite = default)
        {
            if (!string.IsNullOrEmpty(splite))
            {
                hexString = hexString.Replace(splite, string.Empty);
            }

            if ((hexString.Length % 2) != 0)
            {
                hexString += " ";
            }
            var returnBytes = new byte[hexString.Length / 2];
            for (var i = 0; i < returnBytes.Length; i++)
            {
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return returnBytes;
        }

        /// <summary>
        /// 将16进制的字符转换为int32。
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static int ByHexStringToInt32(this string hexString)
        {
            return string.IsNullOrEmpty(hexString) ? default : int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// 从Base64转到数组。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] ByBase64ToBytes(this string value)
        {
            return Convert.FromBase64String(value);
        }
    }
}