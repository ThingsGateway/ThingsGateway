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
    /// MD5相关操作类
    /// </summary>
    public static class MD5
    {
        /// <summary>
        /// 从字符串获取MD5值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMD5Hash(string str)
        {
            var sb = new StringBuilder();
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                var length = data.Length;
                for (var i = 0; i < length; i++)
                    sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 从流中获取MD5值。
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetMD5Hash(Stream stream)
        {
            var sb = new StringBuilder();
            using (var crypto = System.Security.Cryptography.MD5.Create())
            {
                var data = crypto.ComputeHash(stream);
                var length = data.Length;
                for (var i = 0; i < length; i++)
                    sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 从字节获取MD5值
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetMD5Hash(byte[] buffer, int offset, int length)
        {
            var sb = new StringBuilder();
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var data = md5.ComputeHash(buffer, offset, length);
                var count = data.Length;
                for (var i = 0; i < count; i++)
                    sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// 从字节获取MD5值
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string GetMD5Hash(byte[] buffer)
        {
            return GetMD5Hash(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 验证MD5值。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static bool VerifyMD5Hash(string str, string hash)
        {
            var hashOfInput = GetMD5Hash(str);
            return hashOfInput.CompareTo(hash) == 0;
        }
    }
}