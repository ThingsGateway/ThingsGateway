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

namespace ThingsGateway.Foundation.Core
{
    /// <summary>
    /// BytesExtension
    /// </summary>
    public static class BytesExtension
    {
        #region 字节数组扩展

        /// <summary>
        /// 转Base64。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToBase64(this byte[] data)
        {
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// 索引包含数组。
        /// <para>
        /// 例如：在{0,1,2,3,1,2,3}中搜索{1,2}，则会返回list:[2,5]，均为最后索引的位置。
        /// </para>
        /// </summary>
        /// <param name="srcByteArray"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="subByteArray"></param>
        /// <returns></returns>
        public static List<int> IndexOfInclude(this byte[] srcByteArray, int offset, int length, byte[] subByteArray)
        {
            var subByteArrayLen = subByteArray.Length;
            var indexes = new List<int>();
            if (length < subByteArrayLen)
            {
                return indexes;
            }
            var hitLength = 0;
            for (var i = offset; i < length; i++)
            {
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
                else
                {
                    hitLength = 0;
                    if (srcByteArray[i] == subByteArray[hitLength])
                    {
                        hitLength++;
                    }
                }

                if (hitLength == subByteArray.Length)
                {
                    hitLength = 0;
                    indexes.Add(i);
                }
            }

            return indexes;
        }

        /// <summary>
        /// 索引第一个包含数组的索引位置，例如：在{0,1,2,3,1,2,3}中索引{2,3}，则返回3。
        /// <para>如果目标数组为null或长度为0，则直接返回offset的值</para>
        /// </summary>
        /// <param name="srcByteArray"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="subByteArray"></param>
        /// <returns></returns>
        public static int IndexOfFirst(this byte[] srcByteArray, int offset, int length, byte[] subByteArray)
        {
            if (length < subByteArray.Length)
            {
                return -1;
            }
            if (subByteArray == null || subByteArray.Length == 0)
            {
                return offset;
            }
            var hitLength = 0;
            for (var i = offset; i < length + offset; i++)
            {
                if (srcByteArray[i] == subByteArray[hitLength])
                {
                    hitLength++;
                }
                else
                {
                    hitLength = 0;
                    if (srcByteArray[i] == subByteArray[hitLength])
                    {
                        hitLength++;
                    }
                }

                if (hitLength == subByteArray.Length)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 字节数组转16进制字符
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="splite"></param>
        /// <returns></returns>
        public static string ByBytesToHexString(this byte[] buffer, int offset, int length, string splite = default)
        {
            return string.IsNullOrEmpty(splite)
                ? BitConverter.ToString(buffer, offset, length).Replace("-", string.Empty)
                : BitConverter.ToString(buffer, offset, length).Replace("-", splite);
        }

        #endregion 字节数组扩展
    }
}